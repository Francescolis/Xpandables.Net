/*******************************************************************************
 * Copyright (C) 2025 Kamersoft
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
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace System.Data;

/// <summary>
/// Provides a base implementation of <see cref="IDataSqlBuilder"/> with common SQL generation logic.
/// </summary>
/// <remarks>
/// <para>
/// This base class implements expression translation for predicates, ordering, and property updates.
/// Derived classes must implement dialect-specific formatting such as identifier quoting and pagination.
/// </para>
/// <para>
/// Supported expression types:
/// <list type="bullet">
/// <item>Binary comparisons (==, !=, &lt;, &gt;, &lt;=, &gt;=)</item>
/// <item>Logical operators (&amp;&amp;, ||, !)</item>
/// <item>Member access (property access)</item>
/// <item>Constant values</item>
/// <item>String methods (Contains, StartsWith, EndsWith)</item>
/// <item>Nullable comparisons</item>
/// </list>
/// </para>
/// </remarks>
#pragma warning disable CA1062 // Validate arguments of public methods - protected methods are called from validated public methods
#pragma warning disable CA1002 // Do not expose generic lists - internal implementation detail
[RequiresDynamicCode("Expression compilation requires dynamic code generation.")]
public abstract class DataSqlBuilderBase : IDataSqlBuilder
{
    private int _parameterIndex;

    /// <inheritdoc />
    public abstract SqlDialect Dialect { get; }

    /// <inheritdoc />
    public abstract string ParameterPrefix { get; }

    /// <inheritdoc />
    public abstract string QuoteIdentifier(string identifier);

    /// <summary>
    /// Gets the SQL keyword for limiting results (e.g., "TOP" for SQL Server, "LIMIT" for PostgreSQL/MySQL).
    /// </summary>
    protected abstract string LimitKeyword { get; }

    /// <summary>
    /// Gets a value indicating whether the limit clause comes before the columns (SQL Server TOP).
    /// </summary>
    protected abstract bool LimitBeforeColumns { get; }

    /// <inheritdoc />
    public virtual string GetTableName<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TEntity>()
        where TEntity : class
    {
        var type = typeof(TEntity);
        var tableAttr = type.GetCustomAttribute<TableAttribute>();

        if (tableAttr != null)
        {
            var schema = string.IsNullOrEmpty(tableAttr.Schema) ? string.Empty : $"{QuoteIdentifier(tableAttr.Schema)}.";
            return $"{schema}{QuoteIdentifier(tableAttr.Name)}";
        }

        // Default: use type name as table name
        return QuoteIdentifier(type.Name);
    }

    /// <inheritdoc />
    public virtual IReadOnlyDictionary<string, string> GetColumnMappings<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TEntity>()
        where TEntity : class
    {
        var type = typeof(TEntity);
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var mappings = new Dictionary<string, string>();

        foreach (var property in properties)
        {
            // Skip properties marked as not mapped
            if (property.GetCustomAttribute<NotMappedAttribute>() != null)
                continue;

            var columnAttr = property.GetCustomAttribute<ColumnAttribute>();
            var columnName = columnAttr?.Name ?? property.Name;
            mappings[property.Name] = columnName;
        }

        return mappings;
    }

    /// <inheritdoc />
    public virtual SqlQueryResult BuildSelect<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TEntity, TResult>(
        IDataSpecification<TEntity, TResult> specification)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(specification);

        ResetParameterIndex();
        var parameters = new List<SqlParameter>();
        var sql = new StringBuilder();

        var tableName = GetTableName<TEntity>();
        var baseColumns = GetColumnMappings<TEntity>();
        var bindings = BuildTableBindings<TEntity, TResult>(specification, baseColumns);
        var selectorParameters = BuildParameterBindings(specification.Selector, bindings);
        var columns = BuildSelectColumns(specification.Selector, selectorParameters, parameters);

        // SELECT
        sql.Append("SELECT ");

        if (specification.IsDistinct)
            sql.Append("DISTINCT ");

        // Handle TOP for SQL Server
        if (LimitBeforeColumns && specification.Take.HasValue && !specification.Skip.HasValue)
            sql.Append(CultureInfo.InvariantCulture, $"{LimitKeyword} {specification.Take.Value} ");

        sql.Append(columns);
        sql.Append(" FROM ");
        sql.Append(tableName);
        sql.Append(' ');
        sql.Append(bindings[0].Alias);

        AppendJoins(sql, specification.Joins, bindings, parameters);

        // WHERE
        if (specification.Predicate != null)
        {
            sql.Append(" WHERE ");
            var predicateBindings = BuildParameterBindings(specification.Predicate, bindings);
            var whereClause = TranslateExpression(specification.Predicate.Body, predicateBindings, parameters);
            sql.Append(whereClause);
        }

        if (specification.GroupBy.Count > 0)
        {
            sql.Append(" GROUP BY ");
            var groupClauses = specification.GroupBy.Select(group =>
            {
                var groupBindings = BuildParameterBindings(group, bindings);
                return TranslateExpression(group.Body, groupBindings, parameters);
            });
            sql.Append(string.Join(", ", groupClauses));
        }

        if (specification.Having != null)
        {
            sql.Append(" HAVING ");
            var havingBindings = BuildParameterBindings(specification.Having, bindings);
            var havingClause = TranslateExpression(specification.Having.Body, havingBindings, parameters);
            sql.Append(havingClause);
        }

        // ORDER BY
        if (specification.OrderBy.Count > 0)
        {
            sql.Append(" ORDER BY ");
            var orderClauses = specification.OrderBy.Select(o => BuildOrderClause(o, bindings, parameters));
            sql.Append(string.Join(", ", orderClauses));
        }

        // OFFSET/FETCH or LIMIT/OFFSET (depending on dialect)
        AppendPaging(sql, specification.Skip, specification.Take);

        return new SqlQueryResult(sql.ToString(), parameters);
    }

    /// <inheritdoc />
    public virtual SqlQueryResult BuildCount<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TEntity, TResult>(
        IDataSpecification<TEntity, TResult> specification)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(specification);

        ResetParameterIndex();
        var parameters = new List<SqlParameter>();
        var sql = new StringBuilder();

        var tableName = GetTableName<TEntity>();
        var baseColumns = GetColumnMappings<TEntity>();
        var bindings = BuildTableBindings<TEntity, TResult>(specification, baseColumns);

        var requiresSubquery = specification.GroupBy.Count > 0 || specification.IsDistinct;

        if (requiresSubquery)
        {
            sql.Append("SELECT COUNT(*) FROM (");
        }

        sql.Append("SELECT ");
        sql.Append(requiresSubquery ? "1" : "COUNT(*)");
        sql.Append(" FROM ");
        sql.Append(tableName);
        sql.Append(' ');
        sql.Append(bindings[0].Alias);
        AppendJoins(sql, specification.Joins, bindings, parameters);

        if (specification.Predicate != null)
        {
            sql.Append(" WHERE ");
            var predicateBindings = BuildParameterBindings(specification.Predicate, bindings);
            var whereClause = TranslateExpression(specification.Predicate.Body, predicateBindings, parameters);
            sql.Append(whereClause);
        }

        if (specification.GroupBy.Count > 0)
        {
            sql.Append(" GROUP BY ");
            var groupClauses = specification.GroupBy.Select(group =>
            {
                var groupBindings = BuildParameterBindings(group, bindings);
                return TranslateExpression(group.Body, groupBindings, parameters);
            });
            sql.Append(string.Join(", ", groupClauses));
        }

        if (specification.Having != null)
        {
            sql.Append(" HAVING ");
            var havingBindings = BuildParameterBindings(specification.Having, bindings);
            var havingClause = TranslateExpression(specification.Having.Body, havingBindings, parameters);
            sql.Append(havingClause);
        }

        if (requiresSubquery)
        {
            sql.Append(") AS CountQuery");
        }

        return new SqlQueryResult(sql.ToString(), parameters);
    }

    /// <inheritdoc />
    public virtual SqlQueryResult BuildInsert<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TEntity>(
        TEntity entity)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(entity);

        ResetParameterIndex();
        var parameters = new List<SqlParameter>();
        var sql = new StringBuilder();

        var tableName = GetTableName<TEntity>();
        var columnMappings = GetColumnMappings<TEntity>();
        var type = typeof(TEntity);

        var columns = new List<string>();
        var values = new List<string>();

        foreach (var (propertyName, columnName) in columnMappings)
        {
            var property = type.GetProperty(propertyName);
            if (property == null || !property.CanRead)
                continue;

            if (IsDatabaseGeneratedIdentity(property))
                continue;

            var value = property.GetValue(entity);
            var paramName = NextParameterName();

            columns.Add(QuoteIdentifier(columnName));
            values.Add($"{ParameterPrefix}{paramName}");
            parameters.Add(new SqlParameter(paramName, value));
        }

        sql.Append("INSERT INTO ");
        sql.Append(tableName);
        sql.Append(" (");
        sql.Append(string.Join(", ", columns));
        sql.Append(") VALUES (");
        sql.Append(string.Join(", ", values));
        sql.Append(')');

        return new SqlQueryResult(sql.ToString(), parameters);
    }

    /// <inheritdoc />
    public virtual SqlQueryResult BuildInsertBatch<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TEntity>(
        IEnumerable<TEntity> entities)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(entities);

        var entityList = entities.ToList();
        if (entityList.Count == 0)
            return SqlQueryResult.Empty;

        if (entityList.Count == 1)
            return BuildInsert(entityList[0]);

        ResetParameterIndex();
        var parameters = new List<SqlParameter>();
        var sql = new StringBuilder();

        var tableName = GetTableName<TEntity>();
        var columnMappings = GetColumnMappings<TEntity>();
        var type = typeof(TEntity);

        // Get property info once
        var properties = columnMappings
            .Select(m => (Mapping: m, Property: type.GetProperty(m.Key)))
            .Where(p => p.Property?.CanRead == true && !IsDatabaseGeneratedIdentity(p.Property))
            .ToList();

        var columns = properties.Select(p => QuoteIdentifier(p.Mapping.Value));

        sql.Append("INSERT INTO ");
        sql.Append(tableName);
        sql.Append(" (");
        sql.Append(string.Join(", ", columns));
        sql.Append(") VALUES ");

        var valuesClauses = new List<string>();
        foreach (var entity in entityList)
        {
            var values = new List<string>();
            foreach (var (mapping, property) in properties)
            {
                var value = property!.GetValue(entity);
                var paramName = NextParameterName();

                values.Add($"{ParameterPrefix}{paramName}");
                parameters.Add(new SqlParameter(paramName, value));
            }
            valuesClauses.Add($"({string.Join(", ", values)})");
        }

        sql.Append(string.Join(", ", valuesClauses));

        return new SqlQueryResult(sql.ToString(), parameters);
    }

    /// <inheritdoc />
    public virtual SqlQueryResult BuildUpdate<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TEntity>(
        IDataSpecification<TEntity, TEntity> specification,
        DataUpdater<TEntity> updater)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(specification);
        ArgumentNullException.ThrowIfNull(updater);

        if (updater.Updates.Count == 0)
            throw new InvalidOperationException("The updater must contain at least one property update.");

        ResetParameterIndex();
        var parameters = new List<SqlParameter>();
        var sql = new StringBuilder();

        var tableName = GetTableName<TEntity>();
        var columnMappings = GetColumnMappings<TEntity>();

        sql.Append("UPDATE ");
        sql.Append(tableName);
        sql.Append(" SET ");

        var setClauses = new List<string>();
        foreach (var update in updater.Updates)
        {
            var setClause = BuildSetClause(update, columnMappings, parameters);
            setClauses.Add(setClause);
        }
        sql.Append(string.Join(", ", setClauses));

        if (specification.Predicate != null)
        {
            sql.Append(" WHERE ");
            var whereClause = TranslateExpression(specification.Predicate.Body, columnMappings, parameters);
            sql.Append(whereClause);
        }

        return new SqlQueryResult(sql.ToString(), parameters);
    }

    /// <inheritdoc />
    public virtual SqlQueryResult BuildDelete<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TEntity>(
        IDataSpecification<TEntity, TEntity> specification)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(specification);

        ResetParameterIndex();
        var parameters = new List<SqlParameter>();
        var sql = new StringBuilder();

        var tableName = GetTableName<TEntity>();
        var columnMappings = GetColumnMappings<TEntity>();

        sql.Append("DELETE FROM ");
        sql.Append(tableName);

        if (specification.Predicate != null)
        {
            sql.Append(" WHERE ");
            var whereClause = TranslateExpression(specification.Predicate.Body, columnMappings, parameters);
            sql.Append(whereClause);
        }

        return new SqlQueryResult(sql.ToString(), parameters);
    }

    /// <summary>
    /// Determines whether the specified property is configured to have a database-generated identity value.
    /// </summary>
    /// <remarks>This method inspects the DatabaseGeneratedAttribute applied to the property to determine its
    /// configuration. It specifically checks if the DatabaseGeneratedOption is set to Identity, which indicates that
    /// the database will generate the value for this property.</remarks>
    /// <param name="property">The property to check for database generation configuration. This should be a valid PropertyInfo object
    /// representing a property of an entity.</param>
    /// <returns>true if the property is configured to use a database-generated identity; otherwise, false.</returns>
    [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "<Pending>")]
    protected virtual bool IsDatabaseGeneratedIdentity(PropertyInfo property)
    {
        ArgumentNullException.ThrowIfNull(property);
        var attribute = property.GetCustomAttribute<DatabaseGeneratedAttribute>();
        return attribute?.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity;
    }

    /// <summary>
    /// Appends the paging clause (OFFSET/FETCH or LIMIT/OFFSET) to the SQL.
    /// </summary>
    /// <param name="sql">The SQL builder.</param>
    /// <param name="skip">The number of rows to skip.</param>
    /// <param name="take">The number of rows to take.</param>
    protected abstract void AppendPaging(StringBuilder sql, int? skip, int? take);

    /// <summary>
    /// Represents a binding between a CLR type and a SQL table alias.
    /// </summary>
    protected sealed record TableBinding
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TableBinding"/> record.
        /// </summary>
        public TableBinding(Type entityType, string alias, IReadOnlyDictionary<string, string> columns)
        {
            EntityType = entityType;
            Alias = alias;
            Columns = columns;
        }

        /// <summary>
        /// Gets the CLR type represented by the binding.
        /// </summary>
        public Type EntityType { get; }

        /// <summary>
        /// Gets the SQL alias for the bound table.
        /// </summary>
        public string Alias { get; }

        /// <summary>
        /// Gets the column mappings for the bound type.
        /// </summary>
        public IReadOnlyDictionary<string, string> Columns { get; }
    }

    /// <summary>
    /// Builds table bindings for the base entity and joined entities.
    /// </summary>
    protected virtual List<TableBinding> BuildTableBindings<TEntity, TResult>(
        IDataSpecification<TEntity, TResult> specification,
        IReadOnlyDictionary<string, string> baseColumns)
        where TEntity : class
    {
        var bindings = new List<TableBinding>
        {
            new(typeof(TEntity), "t0", baseColumns)
        };

        for (var i = 0; i < specification.Joins.Count; i++)
        {
            var join = specification.Joins[i];
            var alias = join.TableAlias ?? $"t{i + 1}";
            var columns = GetColumnMappingsForType(join.RightType);
            bindings.Add(new TableBinding(join.RightType, alias, columns));
        }

        return bindings;
    }

    /// <summary>
    /// Maps lambda parameters to table bindings.
    /// </summary>
    protected virtual IReadOnlyDictionary<ParameterExpression, TableBinding> BuildParameterBindings(
        LambdaExpression expression,
        IReadOnlyList<TableBinding> bindings,
        IJoinSpecification? join = null)
    {
        var map = new Dictionary<ParameterExpression, TableBinding>();

        if (join != null && expression.Parameters.Count == 2)
        {
            var leftBinding = bindings.FirstOrDefault(b => b.EntityType == join.LeftType);
            var rightBinding = bindings.FirstOrDefault(b => b.EntityType == join.RightType);

            if (leftBinding != null && rightBinding != null)
            {
                map[expression.Parameters[0]] = leftBinding;
                map[expression.Parameters[1]] = rightBinding;
                return map;
            }
        }

        if (expression.Parameters.Count > bindings.Count)
            throw new InvalidOperationException("Not enough bindings available to map expression parameters.");

        for (var i = 0; i < expression.Parameters.Count; i++)
        {
            map[expression.Parameters[i]] = bindings[i];
        }

        return map;
    }

    /// <summary>
    /// Appends join clauses to the SQL statement.
    /// </summary>
    protected virtual void AppendJoins(
        StringBuilder sql,
        IReadOnlyList<IJoinSpecification> joins,
        IReadOnlyList<TableBinding> bindings,
        List<SqlParameter> parameters)
    {
        for (var i = 0; i < joins.Count; i++)
        {
            var join = joins[i];
            var binding = bindings[i + 1];
            var joinKeyword = join.JoinType switch
            {
                SqlJoinType.Inner => "INNER JOIN",
                SqlJoinType.Left => "LEFT JOIN",
                SqlJoinType.Right => "RIGHT JOIN",
                SqlJoinType.Full => "FULL OUTER JOIN",
                SqlJoinType.Cross => "CROSS JOIN",
                _ => throw new NotSupportedException($"Join type '{join.JoinType}' is not supported.")
            };

            sql.Append(' ');
            sql.Append(joinKeyword);
            sql.Append(' ');
            sql.Append(GetTableNameForType(binding.EntityType));
            sql.Append(' ');
            sql.Append(binding.Alias);

            if (join.JoinType != SqlJoinType.Cross)
            {
                if (join.OnExpression is null)
                    throw new InvalidOperationException("Join predicate is required for non-cross joins.");

                var onBindings = BuildParameterBindings(join.OnExpression, bindings, join);
                var onClause = TranslateExpression(join.OnExpression.Body, onBindings, parameters);
                sql.Append(" ON ");
                sql.Append(onClause);
            }
        }
    }

    /// <summary>
    /// Resolves a member expression to a qualified SQL column.
    /// </summary>
    protected virtual string ResolveMemberColumn(
        MemberExpression member,
        IReadOnlyDictionary<ParameterExpression, TableBinding> bindings)
    {
        if (member.Expression is not ParameterExpression parameter)
            throw new InvalidOperationException("Member expressions must target a query parameter.");

        if (!bindings.TryGetValue(parameter, out var binding))
            throw new InvalidOperationException("Parameter binding could not be resolved.");

        var propertyName = member.Member.Name;
        if (!binding.Columns.TryGetValue(propertyName, out var columnName))
            throw new InvalidOperationException($"Property '{propertyName}' is not mapped to a column.");

        return $"{binding.Alias}.{QuoteIdentifier(columnName)}";
    }

    /// <summary>
    /// Builds a SELECT projection expression with an optional alias.
    /// </summary>
    protected virtual string BuildSelectColumnExpression(
        Expression expression,
        IReadOnlyDictionary<ParameterExpression, TableBinding> bindings,
        List<SqlParameter> parameters,
        string? columnAlias)
    {
        var column = expression is MemberExpression member
            ? ResolveMemberColumn(member, bindings)
            : TranslateExpression(expression, bindings, parameters);

        if (!string.IsNullOrWhiteSpace(columnAlias))
            return $"{column} AS {QuoteIdentifier(columnAlias)}";

        return column;
    }

    /// <summary>
    /// Resolves a table name for a runtime type.
    /// </summary>
    [UnconditionalSuppressMessage("Trimming", "IL2075:Requires unreferenced code",
        Justification = "Runtime join binding requires reflection to resolve table names.")]
    [UnconditionalSuppressMessage("Trimming", "IL2060:Requires unreferenced code",
        Justification = "Runtime join binding requires reflection to resolve table names.")]
    protected virtual string GetTableNameForType(Type entityType)
    {
        var method = GetType().GetMethod(nameof(GetTableName), BindingFlags.Public | BindingFlags.Instance);
        var generic = method?.MakeGenericMethod(entityType)
            ?? throw new InvalidOperationException("Table name resolution failed.");
        return (string)generic.Invoke(this, null)!;
    }

    /// <summary>
    /// Resolves column mappings for a runtime type.
    /// </summary>
    [UnconditionalSuppressMessage("Trimming", "IL2075:Requires unreferenced code",
        Justification = "Runtime join binding requires reflection to resolve column mappings.")]
    [UnconditionalSuppressMessage("Trimming", "IL2060:Requires unreferenced code",
        Justification = "Runtime join binding requires reflection to resolve column mappings.")]
    protected virtual IReadOnlyDictionary<string, string> GetColumnMappingsForType(Type entityType)
    {
        var method = GetType().GetMethod(nameof(GetColumnMappings), BindingFlags.Public | BindingFlags.Instance);
        var generic = method?.MakeGenericMethod(entityType)
            ?? throw new InvalidOperationException("Column mapping resolution failed.");
        return (IReadOnlyDictionary<string, string>)generic.Invoke(this, null)!;
    }

    /// <summary>
    /// Builds the SELECT column list from a selector expression.
    /// </summary>
    protected virtual string BuildSelectColumns(
        LambdaExpression selector,
        IReadOnlyDictionary<ParameterExpression, TableBinding> bindings,
        List<SqlParameter> parameters)
    {
        if (selector.Body is ParameterExpression parameter && bindings.TryGetValue(parameter, out var binding))
        {
            return string.Join(", ", binding.Columns.Values.Select(column =>
                $"{binding.Alias}.{QuoteIdentifier(column)}"));
        }

        if (selector.Body is MemberExpression memberExpr)
        {
            return ResolveMemberColumn(memberExpr, bindings);
        }

        if (selector.Body is NewExpression newExpr)
        {
            var selectedColumns = new List<string>();
            for (var i = 0; i < newExpr.Arguments.Count; i++)
            {
                var arg = newExpr.Arguments[i];
                var alias = newExpr.Members?[i].Name;
                selectedColumns.Add(BuildSelectColumnExpression(arg, bindings, parameters, alias));
            }
            if (selectedColumns.Count > 0)
                return string.Join(", ", selectedColumns);
        }

        if (selector.Body is MemberInitExpression memberInit)
        {
            var selectedColumns = new List<string>();
            foreach (var bindingInfo in memberInit.Bindings.OfType<MemberAssignment>())
            {
                selectedColumns.Add(BuildSelectColumnExpression(bindingInfo.Expression, bindings, parameters, bindingInfo.Member.Name));
            }
            if (selectedColumns.Count > 0)
                return string.Join(", ", selectedColumns);
        }

        var expressionColumn = TranslateExpression(selector.Body, bindings, parameters);
        return expressionColumn;
    }

    /// <summary>
    /// Builds an ORDER BY clause from an order specification.
    /// </summary>
    protected virtual string BuildOrderClause(
        OrderSpecification orderSpec,
        IReadOnlyList<TableBinding> bindings,
        List<SqlParameter> parameters)
    {
        var bindingMap = BuildParameterBindings(orderSpec.KeySelector, bindings);
        var orderExpression = orderSpec.KeySelector.Body;
        var column = orderExpression is MemberExpression member
            ? ResolveMemberColumn(member, bindingMap)
            : TranslateExpression(orderExpression, bindingMap, parameters);

        var direction = orderSpec.Descending ? "DESC" : "ASC";
        return $"{column} {direction}";
    }

    /// <summary>
    /// Builds a SET clause from a property update.
    /// </summary>
    protected virtual string BuildSetClause<TEntity>(
        IDataPropertyUpdate<TEntity> update,
        IReadOnlyDictionary<string, string> columnMappings,
        List<SqlParameter> parameters)
        where TEntity : class
    {
        // Get the property name from the property expression
        if (update.PropertyExpression.Body is not MemberExpression memberExpr)
            throw new NotSupportedException($"Unsupported property expression: {update.PropertyExpression}");

        var propertyName = memberExpr.Member.Name;
        if (!columnMappings.TryGetValue(propertyName, out var columnName))
            throw new InvalidOperationException($"Property '{propertyName}' is not mapped to a column.");

        var quotedColumn = QuoteIdentifier(columnName);

        if (update.IsConstant)
        {
            // Constant value - extract and parameterize
            var value = ExtractConstantValue(update.ValueExpression);
            var paramName = NextParameterName();
            parameters.Add(new SqlParameter(paramName, value));
            return $"{quotedColumn} = {ParameterPrefix}{paramName}";
        }
        else
        {
            // Computed value - translate the expression
            var valueExpr = update.ValueExpression;
            if (valueExpr is LambdaExpression lambda)
                valueExpr = lambda.Body;

            var translatedValue = TranslateExpression(valueExpr, columnMappings, parameters);
            return $"{quotedColumn} = {translatedValue}";
        }
    }

    /// <summary>
    /// Translates a LINQ expression to SQL using table bindings.
    /// </summary>
    protected virtual string TranslateExpression(
        Expression expression,
        IReadOnlyDictionary<ParameterExpression, TableBinding> bindings,
        List<SqlParameter> parameters)
    {
        return expression switch
        {
            BinaryExpression binary => TranslateBinaryExpression(binary, bindings, parameters),
            UnaryExpression unary => TranslateUnaryExpression(unary, bindings, parameters),
            MemberExpression member => TranslateMemberExpression(member, bindings, parameters),
            ConstantExpression constant => TranslateConstantExpression(constant, parameters),
            MethodCallExpression methodCall => TranslateMethodCallExpression(methodCall, bindings, parameters),
            ParameterExpression => throw new NotSupportedException("Parameter expressions must be accessed through member expressions."),
            _ => throw new NotSupportedException($"Expression type '{expression.NodeType}' is not supported.")
        };
    }

    /// <summary>
    /// Translates a binary expression using table bindings.
    /// </summary>
    protected virtual string TranslateBinaryExpression(
        BinaryExpression binary,
        IReadOnlyDictionary<ParameterExpression, TableBinding> bindings,
        List<SqlParameter> parameters)
    {
        var left = TranslateExpression(binary.Left, bindings, parameters);
        var right = TranslateExpression(binary.Right, bindings, parameters);

        var op = binary.NodeType switch
        {
            ExpressionType.Equal => "=",
            ExpressionType.NotEqual => "<>",
            ExpressionType.GreaterThan => ">",
            ExpressionType.GreaterThanOrEqual => ">=",
            ExpressionType.LessThan => "<",
            ExpressionType.LessThanOrEqual => "<=",
            ExpressionType.AndAlso => "AND",
            ExpressionType.OrElse => "OR",
            ExpressionType.Add => "+",
            ExpressionType.Subtract => "-",
            ExpressionType.Multiply => "*",
            ExpressionType.Divide => "/",
            ExpressionType.Modulo => "%",
            _ => throw new NotSupportedException($"Binary operator '{binary.NodeType}' is not supported.")
        };

        if (right == "NULL" && op == "=")
            return $"({left} IS NULL)";
        if (right == "NULL" && op == "<>")
            return $"({left} IS NOT NULL)";

        return $"({left} {op} {right})";
    }

    /// <summary>
    /// Translates a unary expression using table bindings.
    /// </summary>
    protected virtual string TranslateUnaryExpression(
        UnaryExpression unary,
        IReadOnlyDictionary<ParameterExpression, TableBinding> bindings,
        List<SqlParameter> parameters)
    {
        var operand = TranslateExpression(unary.Operand, bindings, parameters);

        return unary.NodeType switch
        {
            ExpressionType.Not => $"(NOT {operand})",
            ExpressionType.Convert => operand,
            ExpressionType.Quote => operand,
            _ => throw new NotSupportedException($"Unary operator '{unary.NodeType}' is not supported.")
        };
    }

    /// <summary>
    /// Translates a member expression using table bindings.
    /// </summary>
    protected virtual string TranslateMemberExpression(
        MemberExpression member,
        IReadOnlyDictionary<ParameterExpression, TableBinding> bindings,
        List<SqlParameter> parameters)
    {
        if (member.Expression is ParameterExpression)
        {
            return ResolveMemberColumn(member, bindings);
        }

        var value = ExtractValueFromMemberExpression(member);
        var paramName = NextParameterName();
        parameters.Add(new SqlParameter(paramName, value));
        return $"{ParameterPrefix}{paramName}";
    }

    /// <summary>
    /// Translates a method call expression using table bindings.
    /// </summary>
    protected virtual string TranslateMethodCallExpression(
        MethodCallExpression methodCall,
        IReadOnlyDictionary<ParameterExpression, TableBinding> bindings,
        List<SqlParameter> parameters)
    {
        var methodName = methodCall.Method.Name;

        if (methodName == "Contains" && methodCall.Method.DeclaringType != typeof(string))
            return TranslateCollectionContains(methodCall, bindings, parameters);

        if (methodCall.Method.DeclaringType == typeof(string))
        {
            return methodName switch
            {
                "Contains" => TranslateStringContains(methodCall, bindings, parameters),
                "StartsWith" => TranslateStringStartsWith(methodCall, bindings, parameters),
                "EndsWith" => TranslateStringEndsWith(methodCall, bindings, parameters),
                "ToLower" => TranslateStringToLower(methodCall, bindings, parameters),
                "ToUpper" => TranslateStringToUpper(methodCall, bindings, parameters),
                _ => throw new NotSupportedException($"String method '{methodName}' is not supported.")
            };
        }

        throw new NotSupportedException($"Method '{methodCall.Method.DeclaringType?.Name}.{methodName}' is not supported.");
    }

    /// <summary>
    /// Translates a collection Contains method call using table bindings.
    /// </summary>
    protected virtual string TranslateCollectionContains(
        MethodCallExpression methodCall,
        IReadOnlyDictionary<ParameterExpression, TableBinding> bindings,
        List<SqlParameter> parameters)
    {
        var (collectionExpr, itemExpr) = GetCollectionContainsOperands(methodCall);
        var unwrappedItem = UnwrapConversion(itemExpr);

        if (!IsEntityMemberExpression(unwrappedItem))
            throw new NotSupportedException("Collection Contains expects an entity member as the item expression.");

        var column = TranslateExpression(unwrappedItem, bindings, parameters);
        var values = ExtractCollectionValues(collectionExpr);

        if (values.Count == 0)
            return "(1 = 0)";

        var placeholders = new List<string>(values.Count);
        foreach (var value in values)
        {
            var paramName = NextParameterName();
            parameters.Add(new SqlParameter(paramName, value));
            placeholders.Add($"{ParameterPrefix}{paramName}");
        }

        return $"({column} IN ({string.Join(", ", placeholders)}))";
    }

    /// <summary>
    /// Translates string.Contains using table bindings.
    /// </summary>
    protected virtual string TranslateStringContains(
        MethodCallExpression methodCall,
        IReadOnlyDictionary<ParameterExpression, TableBinding> bindings,
        List<SqlParameter> parameters)
    {
        var (columnExpr, valueExpr) = GetStringMethodOperands(methodCall);
        var column = TranslateExpression(columnExpr, bindings, parameters);
        var value = ExtractConstantValue(valueExpr);
        var paramName = NextParameterName();
        parameters.Add(new SqlParameter(paramName, $"%{value}%"));
        return $"({column} LIKE {ParameterPrefix}{paramName})";
    }

    /// <summary>
    /// Translates string.StartsWith using table bindings.
    /// </summary>
    protected virtual string TranslateStringStartsWith(
        MethodCallExpression methodCall,
        IReadOnlyDictionary<ParameterExpression, TableBinding> bindings,
        List<SqlParameter> parameters)
    {
        var (columnExpr, valueExpr) = GetStringMethodOperands(methodCall);
        var column = TranslateExpression(columnExpr, bindings, parameters);
        var value = ExtractConstantValue(valueExpr);
        var paramName = NextParameterName();
        parameters.Add(new SqlParameter(paramName, $"{value}%"));
        return $"({column} LIKE {ParameterPrefix}{paramName})";
    }

    /// <summary>
    /// Translates string.EndsWith using table bindings.
    /// </summary>
    protected virtual string TranslateStringEndsWith(
        MethodCallExpression methodCall,
        IReadOnlyDictionary<ParameterExpression, TableBinding> bindings,
        List<SqlParameter> parameters)
    {
        var (columnExpr, valueExpr) = GetStringMethodOperands(methodCall);
        var column = TranslateExpression(columnExpr, bindings, parameters);
        var value = ExtractConstantValue(valueExpr);
        var paramName = NextParameterName();
        parameters.Add(new SqlParameter(paramName, $"%{value}"));
        return $"({column} LIKE {ParameterPrefix}{paramName})";
    }

    /// <summary>
    /// Translates a LINQ expression to SQL.
    /// </summary>
    protected virtual string TranslateExpression(
        Expression expression,
        IReadOnlyDictionary<string, string> columnMappings,
        List<SqlParameter> parameters)
    {
        return expression switch
        {
            BinaryExpression binary => TranslateBinaryExpression(binary, columnMappings, parameters),
            UnaryExpression unary => TranslateUnaryExpression(unary, columnMappings, parameters),
            MemberExpression member => TranslateMemberExpression(member, columnMappings, parameters),
            ConstantExpression constant => TranslateConstantExpression(constant, parameters),
            MethodCallExpression methodCall => TranslateMethodCallExpression(methodCall, columnMappings, parameters),
            ParameterExpression => throw new NotSupportedException("Parameter expressions must be accessed through member expressions."),
            _ => throw new NotSupportedException($"Expression type '{expression.NodeType}' is not supported.")
        };
    }

    /// <summary>
    /// Translates a binary expression (e.g., ==, !=, &amp;&amp;, ||).
    /// </summary>
    protected virtual string TranslateBinaryExpression(
        BinaryExpression binary,
        IReadOnlyDictionary<string, string> columnMappings,
        List<SqlParameter> parameters)
    {
        var left = TranslateExpression(binary.Left, columnMappings, parameters);
        var right = TranslateExpression(binary.Right, columnMappings, parameters);

        var op = binary.NodeType switch
        {
            ExpressionType.Equal => "=",
            ExpressionType.NotEqual => "<>",
            ExpressionType.GreaterThan => ">",
            ExpressionType.GreaterThanOrEqual => ">=",
            ExpressionType.LessThan => "<",
            ExpressionType.LessThanOrEqual => "<=",
            ExpressionType.AndAlso => "AND",
            ExpressionType.OrElse => "OR",
            ExpressionType.Add => "+",
            ExpressionType.Subtract => "-",
            ExpressionType.Multiply => "*",
            ExpressionType.Divide => "/",
            ExpressionType.Modulo => "%",
            _ => throw new NotSupportedException($"Binary operator '{binary.NodeType}' is not supported.")
        };

        // Handle NULL comparisons
        if (right == "NULL" && op == "=")
            return $"({left} IS NULL)";
        if (right == "NULL" && op == "<>")
            return $"({left} IS NOT NULL)";

        return $"({left} {op} {right})";
    }

    /// <summary>
    /// Translates a unary expression (e.g., !).
    /// </summary>
    protected virtual string TranslateUnaryExpression(
        UnaryExpression unary,
        IReadOnlyDictionary<string, string> columnMappings,
        List<SqlParameter> parameters)
    {
        var operand = TranslateExpression(unary.Operand, columnMappings, parameters);

        return unary.NodeType switch
        {
            ExpressionType.Not => $"(NOT {operand})",
            ExpressionType.Convert => operand, // Ignore type conversions
            ExpressionType.Quote => operand,   // Ignore quotes
            _ => throw new NotSupportedException($"Unary operator '{unary.NodeType}' is not supported.")
        };
    }

    /// <summary>
    /// Translates a member expression (property access).
    /// </summary>
    protected virtual string TranslateMemberExpression(
        MemberExpression member,
        IReadOnlyDictionary<string, string> columnMappings,
        List<SqlParameter> parameters)
    {
        // Check if this is accessing a property on the entity parameter
        if (member.Expression is ParameterExpression)
        {
            var propertyName = member.Member.Name;
            if (columnMappings.TryGetValue(propertyName, out var columnName))
                return QuoteIdentifier(columnName);

            throw new InvalidOperationException($"Property '{propertyName}' is not mapped to a column.");
        }

        // Handle nested member access (e.g., closure variable)
        var value = ExtractValueFromMemberExpression(member);
        var paramName = NextParameterName();
        parameters.Add(new SqlParameter(paramName, value));
        return $"{ParameterPrefix}{paramName}";
    }

    /// <summary>
    /// Translates a constant expression.
    /// </summary>
    protected virtual string TranslateConstantExpression(
        ConstantExpression constant,
        List<SqlParameter> parameters)
    {
        if (constant.Value == null)
            return "NULL";

        var paramName = NextParameterName();
        parameters.Add(new SqlParameter(paramName, constant.Value));
        return $"{ParameterPrefix}{paramName}";
    }

    /// <summary>
    /// Translates a method call expression (e.g., string.Contains).
    /// </summary>
    protected virtual string TranslateMethodCallExpression(
        MethodCallExpression methodCall,
        IReadOnlyDictionary<string, string> columnMappings,
        List<SqlParameter> parameters)
    {
        var methodName = methodCall.Method.Name;

        if (methodName == "Contains" && methodCall.Method.DeclaringType != typeof(string))
            return TranslateCollectionContains(methodCall, columnMappings, parameters);

        // String methods
        if (methodCall.Method.DeclaringType == typeof(string))
        {
            return methodName switch
            {
                "Contains" => TranslateStringContains(methodCall, columnMappings, parameters),
                "StartsWith" => TranslateStringStartsWith(methodCall, columnMappings, parameters),
                "EndsWith" => TranslateStringEndsWith(methodCall, columnMappings, parameters),
                "ToLower" => TranslateStringToLower(methodCall, columnMappings, parameters),
                "ToUpper" => TranslateStringToUpper(methodCall, columnMappings, parameters),
                _ => throw new NotSupportedException($"String method '{methodName}' is not supported.")
            };
        }

        throw new NotSupportedException($"Method '{methodCall.Method.DeclaringType?.Name}.{methodName}' is not supported.");
    }

    /// <summary>
    /// Translates collection Contains to SQL IN clause.
    /// </summary>
    protected virtual string TranslateCollectionContains(
        MethodCallExpression methodCall,
        IReadOnlyDictionary<string, string> columnMappings,
        List<SqlParameter> parameters)
    {
        var (collectionExpr, itemExpr) = GetCollectionContainsOperands(methodCall);
        var unwrappedItem = UnwrapConversion(itemExpr);

        if (!IsEntityMemberExpression(unwrappedItem))
            throw new NotSupportedException("Collection Contains expects an entity member as the item expression.");

        var column = TranslateExpression(unwrappedItem, columnMappings, parameters);
        var values = ExtractCollectionValues(collectionExpr);

        if (values.Count == 0)
            return "(1 = 0)";

        var placeholders = new List<string>(values.Count);
        foreach (var value in values)
        {
            var paramName = NextParameterName();
            parameters.Add(new SqlParameter(paramName, value));
            placeholders.Add($"{ParameterPrefix}{paramName}");
        }

        return $"({column} IN ({string.Join(", ", placeholders)}))";
    }

    private static (Expression Collection, Expression Item) GetCollectionContainsOperands(MethodCallExpression methodCall)
    {
        if (methodCall.Object != null && methodCall.Arguments.Count == 1)
            return (methodCall.Object, methodCall.Arguments[0]);

        if (methodCall.Object == null && methodCall.Arguments.Count == 2)
            return (methodCall.Arguments[0], methodCall.Arguments[1]);

        throw new NotSupportedException("Unsupported Contains signature for collection translation.");
    }

    /// <summary>
    /// Gets the column/value expressions for string method translations.
    /// </summary>
    protected virtual (Expression ColumnExpression, Expression ValueExpression) GetStringMethodOperands(
        MethodCallExpression methodCall)
    {
        var objectExpr = methodCall.Object ?? throw new NotSupportedException("String methods must be instance calls.");
        var argumentExpr = methodCall.Arguments[0];

        var unwrappedObject = UnwrapConversion(objectExpr);
        var unwrappedArgument = UnwrapConversion(argumentExpr);

        if (IsEntityMemberExpression(unwrappedObject))
            return (unwrappedObject, argumentExpr);

        if (IsEntityMemberExpression(unwrappedArgument))
            return (unwrappedArgument, objectExpr);

        return (objectExpr, argumentExpr);
    }

    /// <summary>
    /// Translates string.Contains to LIKE '%value%'.
    /// </summary>
    protected virtual string TranslateStringContains(
        MethodCallExpression methodCall,
        IReadOnlyDictionary<string, string> columnMappings,
        List<SqlParameter> parameters)
    {
        var (columnExpr, valueExpr) = GetStringMethodOperands(methodCall);
        var column = TranslateExpression(columnExpr, columnMappings, parameters);
        var value = ExtractConstantValue(valueExpr);
        var paramName = NextParameterName();
        parameters.Add(new SqlParameter(paramName, $"%{value}%"));
        return $"({column} LIKE {ParameterPrefix}{paramName})";
    }

    /// <summary>
    /// Translates string.StartsWith to LIKE 'value%'.
    /// </summary>
    protected virtual string TranslateStringStartsWith(
        MethodCallExpression methodCall,
        IReadOnlyDictionary<string, string> columnMappings,
        List<SqlParameter> parameters)
    {
        var (columnExpr, valueExpr) = GetStringMethodOperands(methodCall);
        var column = TranslateExpression(columnExpr, columnMappings, parameters);
        var value = ExtractConstantValue(valueExpr);
        var paramName = NextParameterName();
        parameters.Add(new SqlParameter(paramName, $"{value}%"));
        return $"({column} LIKE {ParameterPrefix}{paramName})";
    }

    /// <summary>
    /// Translates string.EndsWith to LIKE '%value'.
    /// </summary>
    protected virtual string TranslateStringEndsWith(
        MethodCallExpression methodCall,
        IReadOnlyDictionary<string, string> columnMappings,
        List<SqlParameter> parameters)
    {
        var (columnExpr, valueExpr) = GetStringMethodOperands(methodCall);
        var column = TranslateExpression(columnExpr, columnMappings, parameters);
        var value = ExtractConstantValue(valueExpr);
        var paramName = NextParameterName();
        parameters.Add(new SqlParameter(paramName, $"%{value}"));
        return $"({column} LIKE {ParameterPrefix}{paramName})";
    }

    /// <summary>
    /// Extracts values from a constant enumerable expression for IN clause usage.
    /// </summary>
    protected virtual IReadOnlyList<object?> ExtractCollectionValues(Expression expression)
    {
        var values = ExtractCollectionValuesInternal(UnwrapConversion(expression));

        if (values.Count == 0)
            return values;

        if (values.Count == 1 && values[0] is string)
            throw new NotSupportedException("String values are not supported for collection Contains translation.");

        return values;
    }

    private IReadOnlyList<object?> ExtractCollectionValuesInternal(Expression expression)
    {
        return expression switch
        {
            ConstantExpression constant => ExtractEnumerableValues(constant.Value),
            MemberExpression member => ExtractEnumerableValues(ExtractValueFromMemberExpression(member)),
            NewArrayExpression newArray => newArray.Expressions.Select(ExtractConstantValue).ToList(),
            ListInitExpression listInit => listInit.Initializers
                .SelectMany(init => init.Arguments)
                .Select(ExtractConstantValue)
                .ToList(),
            MethodCallExpression methodCall when IsReadOnlySpanImplicitConversion(methodCall)
                => ExtractCollectionValuesInternal(methodCall.Arguments[0]),
            MethodCallExpression methodCall when !ContainsEntityParameter(methodCall)
                => ExtractEnumerableValues(EvaluateExpression(methodCall)),
            UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.Quote } unary
                => ExtractCollectionValuesInternal(unary.Operand),
            _ => throw new NotSupportedException("Contains requires a constant enumerable collection expression.")
        };
    }

    private static bool IsReadOnlySpanImplicitConversion(MethodCallExpression methodCall)
    {
        return methodCall.Method.Name == "op_Implicit"
            && methodCall.Method.ReturnType.IsGenericType
            && methodCall.Method.ReturnType.GetGenericTypeDefinition() == typeof(ReadOnlySpan<>)
            && methodCall.Arguments.Count == 1;
    }

    private static object? EvaluateExpression(Expression expression)
    {
        try
        {
            return Expression.Lambda(expression).Compile().DynamicInvoke();
        }
        catch (Exception exception)
        {
            throw new NotSupportedException("Contains requires a constant enumerable collection expression.", exception);
        }
    }

    private static bool ContainsEntityParameter(Expression expression)
        => new ParameterExpressionVisitor().HasParameter(expression);

    private sealed class ParameterExpressionVisitor : ExpressionVisitor
    {
        private bool _hasParameter;

        public bool HasParameter(Expression expression)
        {
            _hasParameter = false;
            Visit(expression);
            return _hasParameter;
        }

        public override Expression? Visit(Expression? node)
        {
            if (node is ParameterExpression)
                _hasParameter = true;

            return base.Visit(node);
        }
    }

    [SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance", Justification = "<Pending>")]
    private static IReadOnlyList<object?> ExtractEnumerableValues(object? value)
    {
        if (value == null)
            return [];

        if (value is string)
            return [value];

        if (value is Collections.IEnumerable enumerable)
        {
            var list = new List<object?>();
            foreach (var item in enumerable)
                list.Add(item);
            return list;
        }

        throw new NotSupportedException("Contains requires an enumerable collection value.");
    }

    private static Expression UnwrapConversion(Expression expression)
    {
        while (expression is UnaryExpression unary
               && (unary.NodeType == ExpressionType.Convert || unary.NodeType == ExpressionType.Quote))
        {
            expression = unary.Operand;
        }

        return expression;
    }

    private static bool IsEntityMemberExpression(Expression expression)
        => expression is MemberExpression { Expression: ParameterExpression };

    /// <summary>
    /// Translates string.ToLower to LOWER(column) using bindings.
    /// </summary>
    protected virtual string TranslateStringToLower(
        MethodCallExpression methodCall,
        IReadOnlyDictionary<ParameterExpression, TableBinding> bindings,
        List<SqlParameter> parameters)
    {
        var column = TranslateExpression(methodCall.Object!, bindings, parameters);
        return $"LOWER({column})";
    }

    /// <summary>
    /// Translates string.ToUpper to UPPER(column) using bindings.
    /// </summary>
    protected virtual string TranslateStringToUpper(
        MethodCallExpression methodCall,
        IReadOnlyDictionary<ParameterExpression, TableBinding> bindings,
        List<SqlParameter> parameters)
    {
        var column = TranslateExpression(methodCall.Object!, bindings, parameters);
        return $"UPPER({column})";
    }

    /// <summary>
    /// Translates string.ToLower to LOWER(column).
    /// </summary>
    protected virtual string TranslateStringToLower(
        MethodCallExpression methodCall,
        IReadOnlyDictionary<string, string> columnMappings,
        List<SqlParameter> parameters)
    {
        var column = TranslateExpression(methodCall.Object!, columnMappings, parameters);
        return $"LOWER({column})";
    }

    /// <summary>
    /// Translates string.ToUpper to UPPER(column).
    /// </summary>
    protected virtual string TranslateStringToUpper(
        MethodCallExpression methodCall,
        IReadOnlyDictionary<string, string> columnMappings,
        List<SqlParameter> parameters)
    {
        var column = TranslateExpression(methodCall.Object!, columnMappings, parameters);
        return $"UPPER({column})";
    }

    /// <summary>
    /// Extracts a constant value from an expression.
    /// </summary>
    protected virtual object? ExtractConstantValue(Expression expression)
    {
        return expression switch
        {
            ConstantExpression constant => constant.Value,
            MemberExpression member => ExtractValueFromMemberExpression(member),
            UnaryExpression { NodeType: ExpressionType.Convert } unary => ExtractConstantValue(unary.Operand),
            _ => Expression.Lambda(expression).Compile().DynamicInvoke()
        };
    }

    /// <summary>
    /// Extracts a value from a member expression (e.g., closure variable).
    /// </summary>
    protected virtual object? ExtractValueFromMemberExpression(MemberExpression member)
    {
        // Handle nested member access
        if (member.Expression is ConstantExpression constant)
        {
            var container = constant.Value;
            return member.Member switch
            {
                FieldInfo field => field.GetValue(container),
                PropertyInfo property => property.GetValue(container),
                _ => throw new NotSupportedException($"Member type '{member.Member.MemberType}' is not supported.")
            };
        }

        if (member.Expression is MemberExpression innerMember)
        {
            var container = ExtractValueFromMemberExpression(innerMember);
            return member.Member switch
            {
                FieldInfo field => field.GetValue(container),
                PropertyInfo property => property.GetValue(container),
                _ => throw new NotSupportedException($"Member type '{member.Member.MemberType}' is not supported.")
            };
        }

        // Static member
        if (member.Expression == null)
        {
            return member.Member switch
            {
                FieldInfo field => field.GetValue(null),
                PropertyInfo property => property.GetValue(null),
                _ => throw new NotSupportedException($"Member type '{member.Member.MemberType}' is not supported.")
            };
        }

        throw new NotSupportedException($"Cannot extract value from member expression: {member}");
    }

    /// <summary>
    /// Resets the parameter index for a new query.
    /// </summary>
    protected void ResetParameterIndex() => _parameterIndex = 0;

    /// <summary>
    /// Gets the next parameter name.
    /// </summary>
    protected string NextParameterName() => $"p{_parameterIndex++}";
}
