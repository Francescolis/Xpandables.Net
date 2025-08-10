/*******************************************************************************
 * Copyright (C) 2024 Francis-Black EWANE
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
********************************************************************************/

using System.Data;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

using Microsoft.Data.SqlClient;

namespace Xpandables.Net.Sql;

/// <summary>
/// Implementation of INSERT SQL builder with fluent API.
/// </summary>
/// <typeparam name="TEntity">The entity type to insert.</typeparam>
internal sealed class InsertSqlBuilder<TEntity> : IInsertSqlBuilder<TEntity> where TEntity : class
{
    private readonly List<IDbDataParameter> _parameters = [];
    private readonly List<string> _columns = [];
    private readonly List<string> _valueRows = [];
    private readonly SqlExpressionVisitor _expressionVisitor = new();
    private int _parameterIndex;

    public IInsertSqlBuilder<TEntity> Values<TValues>(Expression<Func<TEntity, TValues>> valuesSelector)
    {
        ArgumentNullException.ThrowIfNull(valuesSelector);

        ExtractColumnsAndValues(valuesSelector);
        return this;
    }

    public IInsertSqlBuilder<TEntity> Values(IEnumerable<TEntity> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);

        var entityList = entities.ToList();
        if (entityList.Count == 0)
            throw new ArgumentException("At least one entity is required.", nameof(entities));

        var properties = typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && IsInsertableProperty(p))
            .ToList();

        // Filter properties that have non-default values in at least one entity
        var propertiesWithValues = properties.Where(p => 
            entityList.Any(entity => HasNonDefaultValue(p, entity))).ToList();

        if (_columns.Count == 0)
        {
            // First time - set up columns
            foreach (var property in propertiesWithValues)
            {
                var columnName = GetColumnName(property);
                _columns.Add($"[{columnName}]");
            }
        }

        foreach (var entity in entityList)
        {
            var valueParameters = new List<string>();
            foreach (var property in propertiesWithValues)
            {
                var value = property.GetValue(entity);
                var parameterName = $"@p{_parameterIndex++}";
                _parameters.Add(new SqlParameter(parameterName, value ?? DBNull.Value));
                valueParameters.Add(parameterName);
            }
            _valueRows.Add($"({string.Join(", ", valueParameters)})");
        }

        return this;
    }

    public SqlQueryResult Build()
    {
        if (_columns.Count == 0)
            throw new InvalidOperationException("No columns specified for INSERT operation.");

        if (_valueRows.Count == 0)
            throw new InvalidOperationException("No values specified for INSERT operation.");

        var sql = new StringBuilder();
        var tableName = GetTableName<TEntity>();

        sql.Append(CultureInfo.InvariantCulture, $"INSERT INTO [{tableName}] ({string.Join(", ", _columns)})");
        sql.AppendLine();
        sql.Append(CultureInfo.InvariantCulture, $"VALUES {string.Join(", ", _valueRows)}");

        // Combine parameters from expression visitor and our parameters
        var allParameters = _parameters.Concat(_expressionVisitor.Parameters).ToList();

        return new SqlQueryResult(sql.ToString(), allParameters);
    }

    private void ExtractColumnsAndValues<TValues>(Expression<Func<TEntity, TValues>> valuesSelector)
    {
        _expressionVisitor.RegisterParameterAliases(valuesSelector);

        if (valuesSelector.Body is NewExpression newExpression)
        {
            // Handle anonymous type: new { FirstName = firstName, LastName = lastName }
            for (int i = 0; i < newExpression.Arguments.Count; i++)
            {
                // Get the column name from the anonymous type member name
                var memberName = newExpression.Members?[i]?.Name ?? $"Column{i}";
                
                // Try to find the corresponding entity property to get the proper column name
                var entityProperty = typeof(TEntity).GetProperty(memberName);
                var columnName = entityProperty != null ? GetColumnName(entityProperty) : memberName;
                
                _columns.Add($"[{columnName}]");

                var parameterName = $"@p{_parameterIndex++}";
                var value = GetExpressionValue(newExpression.Arguments[i]);
                _parameters.Add(new SqlParameter(parameterName, value ?? DBNull.Value));
            }
        }
        else if (valuesSelector.Body is MemberInitExpression memberInitExpression)
        {
            // Handle object initialization: new Entity { Property1 = value1, Property2 = value2 }
            foreach (var binding in memberInitExpression.Bindings.OfType<MemberAssignment>())
            {
                var columnName = GetColumnName(binding.Member);
                _columns.Add($"[{columnName}]");

                var parameterName = $"@p{_parameterIndex++}";
                var value = GetExpressionValue(binding.Expression);
                _parameters.Add(new SqlParameter(parameterName, value ?? DBNull.Value));
            }
        }

        // Create a single value row
        var valueParameters = _parameters.TakeLast(_columns.Count).Select(p => p.ParameterName);
        _valueRows.Add($"({string.Join(", ", valueParameters)})");
    }

    private static object? GetExpressionValue(Expression expression)
    {
#pragma warning disable CA1031 // Do not catch general exception types
        try
        {
            if (expression is ConstantExpression constant)
                return constant.Value;

            var lambda = Expression.Lambda(expression);
            var compiled = lambda.Compile();
            return compiled.DynamicInvoke();
        }
        catch
        {
            return null;
        }
#pragma warning restore CA1031 // Do not catch general exception types
    }

    private static bool IsInsertableProperty(PropertyInfo property)
    {
        // Skip properties that are typically not inserted (like computed columns, identity columns, etc.)
        var keyAttribute = property.GetCustomAttribute<System.ComponentModel.DataAnnotations.KeyAttribute>();
        var databaseGeneratedAttribute = property.GetCustomAttribute<System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedAttribute>();

        return keyAttribute == null &&
               (databaseGeneratedAttribute == null ||
                databaseGeneratedAttribute.DatabaseGeneratedOption == System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.None);
    }

    private static string GetTableName<T>()
    {
        var type = typeof(T);
        var tableAttribute = type.GetCustomAttribute<System.ComponentModel.DataAnnotations.Schema.TableAttribute>();
        return tableAttribute?.Name ?? type.Name;
    }

    private static string GetColumnName(MemberInfo member)
    {
        var columnAttribute = member.GetCustomAttribute<System.ComponentModel.DataAnnotations.Schema.ColumnAttribute>();
        return columnAttribute?.Name ?? member.Name;
    }

    private static bool HasNonDefaultValue(PropertyInfo property, object entity)
    {
        var value = property.GetValue(entity);
        if (value == null) return false;

        // Check if the value is the default for the type
        var defaultValue = property.PropertyType.IsValueType 
            ? Activator.CreateInstance(property.PropertyType) 
            : null;

        return !Equals(value, defaultValue);
    }
}