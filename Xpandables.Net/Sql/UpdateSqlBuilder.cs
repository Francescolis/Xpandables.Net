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

using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Xpandables.Net.Sql;

/// <summary>
/// Implementation of UPDATE SQL builder with fluent API.
/// </summary>
/// <typeparam name="TEntity">The entity type to update.</typeparam>
internal sealed class UpdateSqlBuilder<TEntity> : IUpdateSqlBuilder<TEntity> where TEntity : class
{
    private readonly List<string> _setClauses = [];
    private readonly List<string> _whereClauses = [];
    private readonly List<string> _outputColumns = [];
    private readonly SqlExpressionVisitor _expressionVisitor = new();

    public UpdateSqlBuilder()
    {
    }

    public IUpdateSqlBuilder<TEntity> Set<TValues>(Expression<Func<TEntity, TValues>> setSelector)
    {
        ArgumentNullException.ThrowIfNull(setSelector);

        ExtractSetClauses(setSelector);
        return this;
    }

    public IUpdateSqlBuilder<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        _expressionVisitor.RegisterParameterAliases(predicate);
        var sql = _expressionVisitor.VisitAndGenerateSql(predicate.Body);
        if (sql.StartsWith('('))
        {
            sql = sql[1..^1]; // Remove outer parentheses
        }
        _whereClauses.Add($"({RemoveAliasFromWhereClause(sql)})");
        return this;
    }

    public IUpdateSqlBuilder<TEntity> Output<TOutput>(Expression<Func<TEntity, TOutput>> outputSelector)
    {
        ArgumentNullException.ThrowIfNull(outputSelector);

        _expressionVisitor.RegisterParameterAliases(outputSelector);
        var outputSql = _expressionVisitor.VisitAndGenerateSql(outputSelector.Body);
        _outputColumns.Add(outputSql);
        return this;
    }

    public SqlQueryResult Build()
    {
        if (_setClauses.Count == 0)
            throw new InvalidOperationException("No SET clauses specified for UPDATE operation.");

        var sql = new StringBuilder();
        var tableName = GetTableName<TEntity>();

        // Use table name (no alias) for UPDATE
        sql.Append(CultureInfo.InvariantCulture, $"UPDATE [{tableName}] SET {string.Join(", ", _setClauses)}");

        // OUTPUT clause (SQL Server specific)
        if (_outputColumns.Count > 0)
        {
            sql.AppendLine();
            sql.Append(CultureInfo.InvariantCulture, $"OUTPUT {string.Join(", ", _outputColumns.Select(c => $"INSERTED.{c}"))}");
        }

        // WHERE clause
        if (_whereClauses.Count > 0)
        {
            sql.AppendLine();
            sql.Append(CultureInfo.InvariantCulture, $"WHERE {string.Join(" AND ", _whereClauses)}");
        }

        return new SqlQueryResult(sql.ToString(), _expressionVisitor.Parameters);
    }

    private void ExtractSetClauses<TValues>(Expression<Func<TEntity, TValues>> setSelector)
    {
        _expressionVisitor.RegisterParameterAliases(setSelector);

        if (setSelector.Body is NewExpression newExpression)
        {
            // Handle anonymous type: new { entity.Property1, entity.Property2 }
            for (int i = 0; i < newExpression.Arguments.Count; i++)
            {
                if (newExpression.Arguments[i] is MemberExpression memberExpr &&
                    newExpression.Members?[i] != null)
                {
                    var columnName = GetColumnName(newExpression.Members[i]);
                    var valueSql = _expressionVisitor.VisitAndGenerateSql(memberExpr);
                    _setClauses.Add($"[{columnName}] = {valueSql}");
                }
                else if (newExpression.Members?[i] != null)
                {
                    // Handle direct values: new { Property1 = value1, Property2 = value2 }
                    var columnName = GetColumnName(newExpression.Members[i]);
                    var valueSql = _expressionVisitor.VisitAndGenerateSql(newExpression.Arguments[i]);
                    _setClauses.Add($"[{columnName}] = {valueSql}");
                }
            }
        }
        else if (setSelector.Body is MemberInitExpression memberInitExpression)
        {
            // Handle object initialization: new Entity { Property1 = value1, Property2 = value2 }
            foreach (var binding in memberInitExpression.Bindings.OfType<MemberAssignment>())
            {
                var columnName = GetColumnName(binding.Member);
                var valueSql = _expressionVisitor.VisitAndGenerateSql(binding.Expression);
                _setClauses.Add($"[{columnName}] = {valueSql}");
            }
        }
    }

    private static string RemoveAliasFromWhereClause(string sql)
    {
        // Removes patterns like [alias].[Column] => [Column]
        // Handles multiple aliases and nested brackets
        return System.Text.RegularExpressions.Regex.Replace(sql, @"\[[a-zA-Z0-9_]+\]\.\[([a-zA-Z0-9_]+)\]", "[$1]");
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
}