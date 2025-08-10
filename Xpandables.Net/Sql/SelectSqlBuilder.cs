using System.Data;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

using Microsoft.Data.SqlClient;

namespace Xpandables.Net.Sql;
/// <summary>
/// Implementation of SELECT SQL builder with fluent API.
/// </summary>
/// <typeparam name="TEntity">The main entity type.</typeparam>
internal sealed class SelectSqlBuilder<TEntity> : ISelectSqlBuilder<TEntity> where TEntity : class
{
    private readonly List<string> _selectColumns = [];
    private readonly List<string> _fromClauses = [];
    private readonly List<string> _joinClauses = [];
    private readonly List<string> _whereClauses = [];
    private readonly List<string> _groupByClauses = [];
    private readonly List<string> _havingClauses = [];
    private readonly List<string> _orderByClauses = [];
    private readonly List<string> _cteClauses = [];
    private readonly List<string> _unionClauses = [];
    private readonly SqlExpressionVisitor _expressionVisitor = new();
    private readonly Dictionary<Type, string> _registeredTypes = [];
    private readonly List<IDbDataParameter> _additionalParameters = [];
    private readonly string _mainTableAlias;
    private int? _skipCount;
    private int? _takeCount;
    private int _rawParameterIndex;

    public SelectSqlBuilder(string? alias = null)
    {
        _mainTableAlias = alias ?? GetDefaultAlias<TEntity>(0);
        RegisterType<TEntity>(0);

        var tableName = GetTableName<TEntity>();
        _fromClauses.Add($"[{tableName}] AS [{_mainTableAlias}]");
    }

    public ISelectSqlBuilder<TEntity> Select(Expression<Func<TEntity, object>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);

        _expressionVisitor.RegisterParameterAliases(selector);
        var sql = _expressionVisitor.VisitAndGenerateSql(selector.Body);
        _selectColumns.Add(sql);
        return this;
    }

    public ISelectSqlBuilder<TEntity> Select<TJoin>(Expression<Func<TEntity, TJoin, object>> selector)
        where TJoin : class
    {
        ArgumentNullException.ThrowIfNull(selector);

        _expressionVisitor.RegisterParameterAliases(selector);
        var sql = _expressionVisitor.VisitAndGenerateSql(selector.Body);
        _selectColumns.Add(sql);
        return this;
    }

    public ISelectSqlBuilder<TEntity> Select<TJoin, RJoin>(Expression<Func<TEntity, TJoin, RJoin, object>> selector)
        where TJoin : class
        where RJoin : class
    {
        ArgumentNullException.ThrowIfNull(selector);

        _expressionVisitor.RegisterParameterAliases(selector);
        var sql = _expressionVisitor.VisitAndGenerateSql(selector.Body);
        _selectColumns.Add(sql);
        return this;
    }

    public ISelectSqlBuilder<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        _expressionVisitor.RegisterParameterAliases(predicate);
        var sql = _expressionVisitor.VisitAndGenerateSql(predicate.Body);
        _whereClauses.Add(sql);
        return this;
    }

    public ISelectSqlBuilder<TEntity> Where<TJoin>(Expression<Func<TEntity, TJoin, bool>> predicate)
        where TJoin : class
    {
        ArgumentNullException.ThrowIfNull(predicate);

        _expressionVisitor.RegisterParameterAliases(predicate);
        var sql = _expressionVisitor.VisitAndGenerateSql(predicate.Body);
        _whereClauses.Add(sql);
        return this;
    }

    public ISelectSqlBuilder<TEntity> WhereRaw(string rawSql, params object[] parameters)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rawSql);
        ArgumentNullException.ThrowIfNull(parameters);

        var parameterizedSql = rawSql;

        // Replace parameter placeholders with actual parameter names
        for (int i = 0; i < parameters.Length; i++)
        {
            var paramName = $"@raw{_rawParameterIndex++}";
            parameterizedSql = parameterizedSql.Replace($"{{{i}}}", paramName, StringComparison.InvariantCulture);
            _additionalParameters.Add(new SqlParameter(paramName, parameters[i] ?? DBNull.Value));
        }

        _whereClauses.Add($"({parameterizedSql})");
        return this;
    }

    public ISelectSqlBuilder<TEntity> InnerJoin<TJoin>(Expression<Func<TEntity, TJoin, bool>> joinCondition)
        where TJoin : class
    {
        return AddJoin<TJoin>("INNER JOIN", joinCondition);
    }

    public ISelectSqlBuilder<TEntity> LeftJoin<TJoin>(Expression<Func<TEntity, TJoin, bool>> joinCondition)
        where TJoin : class
    {
        return AddJoin<TJoin>("LEFT JOIN", joinCondition);
    }

    public ISelectSqlBuilder<TEntity> RightJoin<TJoin>(Expression<Func<TEntity, TJoin, bool>> joinCondition)
        where TJoin : class
    {
        return AddJoin<TJoin>("RIGHT JOIN", joinCondition);
    }

    public ISelectSqlBuilder<TEntity> FullOuterJoin<TJoin>(Expression<Func<TEntity, TJoin, bool>> joinCondition)
        where TJoin : class
    {
        return AddJoin<TJoin>("FULL OUTER JOIN", joinCondition);
    }

    public ISelectSqlBuilder<TEntity> CrossJoin<TJoin>()
        where TJoin : class
    {
        // Register the join type if not already registered
        if (!_registeredTypes.ContainsKey(typeof(TJoin)))
        {
            RegisterType<TJoin>(_registeredTypes.Count);
        }

        var joinTableName = GetTableName<TJoin>();
        var joinAlias = _registeredTypes[typeof(TJoin)];

        _joinClauses.Add($"CROSS JOIN [{joinTableName}] AS [{joinAlias}]");
        return this;
    }

    public ISelectSqlBuilder<TEntity> GroupBy(Expression<Func<TEntity, object>> groupSelector)
    {
        ArgumentNullException.ThrowIfNull(groupSelector);

        _expressionVisitor.RegisterParameterAliases(groupSelector);
        var sql = _expressionVisitor.VisitAndGenerateSql(groupSelector.Body);
        _groupByClauses.Add(sql);
        return this;
    }

    public ISelectSqlBuilder<TEntity> GroupBy<TJoin>(Expression<Func<TEntity, TJoin, object>> groupSelector)
        where TJoin : class
    {
        ArgumentNullException.ThrowIfNull(groupSelector);

        _expressionVisitor.RegisterParameterAliases(groupSelector);
        var sql = _expressionVisitor.VisitAndGenerateSql(groupSelector.Body);
        _groupByClauses.Add(sql);
        return this;
    }

    public ISelectSqlBuilder<TEntity> Having(Expression<Func<TEntity, bool>> havingCondition)
    {
        ArgumentNullException.ThrowIfNull(havingCondition);

        _expressionVisitor.RegisterParameterAliases(havingCondition);
        var sql = _expressionVisitor.VisitAndGenerateSql(havingCondition.Body);
        _havingClauses.Add(sql);
        return this;
    }

    public ISelectSqlBuilder<TEntity> Having<TJoin>(Expression<Func<TEntity, TJoin, bool>> havingCondition)
        where TJoin : class
    {
        ArgumentNullException.ThrowIfNull(havingCondition);

        _expressionVisitor.RegisterParameterAliases(havingCondition);
        var sql = _expressionVisitor.VisitAndGenerateSql(havingCondition.Body);
        _havingClauses.Add(sql);
        return this;
    }

    public ISelectSqlBuilder<TEntity> OrderBy<TKey>(Expression<Func<TEntity, TKey>> orderSelector)
    {
        ArgumentNullException.ThrowIfNull(orderSelector);

        _expressionVisitor.RegisterParameterAliases(orderSelector);
        var sql = _expressionVisitor.VisitAndGenerateSql(orderSelector.Body);
        _orderByClauses.Add($"{sql} ASC");
        return this;
    }

    public ISelectSqlBuilder<TEntity> OrderByDescending<TKey>(Expression<Func<TEntity, TKey>> orderSelector)
    {
        ArgumentNullException.ThrowIfNull(orderSelector);

        _expressionVisitor.RegisterParameterAliases(orderSelector);
        var sql = _expressionVisitor.VisitAndGenerateSql(orderSelector.Body);
        _orderByClauses.Add($"{sql} DESC");
        return this;
    }

    public ISelectSqlBuilder<TEntity> Skip(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        _skipCount = count;
        return this;
    }

    public ISelectSqlBuilder<TEntity> Take(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);
        _takeCount = count;
        return this;
    }

    public ISelectSqlBuilder<TEntity> WithCte(string cteName, ISqlBuilder cteQuery)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cteName);
        ArgumentNullException.ThrowIfNull(cteQuery);

        var cteResult = cteQuery.Build();
        _cteClauses.Add($"[{cteName}] AS ({cteResult.Sql})");

        // Add CTE parameters to our additional parameters collection
        _additionalParameters.AddRange(cteResult.Parameters);

        return this;
    }

    public ISelectSqlBuilder<TEntity> Union(ISelectSqlBuilder<TEntity> unionQuery)
    {
        ArgumentNullException.ThrowIfNull(unionQuery);

        var unionResult = unionQuery.Build();
        _unionClauses.Add($"UNION ({unionResult.Sql})");

        // Add union parameters to our additional parameters collection
        _additionalParameters.AddRange(unionResult.Parameters);

        return this;
    }

    public ISelectSqlBuilder<TEntity> UnionAll(ISelectSqlBuilder<TEntity> unionQuery)
    {
        ArgumentNullException.ThrowIfNull(unionQuery);

        var unionResult = unionQuery.Build();
        _unionClauses.Add($"UNION ALL ({unionResult.Sql})");

        // Add union parameters to our additional parameters collection
        _additionalParameters.AddRange(unionResult.Parameters);

        return this;
    }

    public SqlQueryResult Build()
    {
        var sql = new StringBuilder();

        // Add CTEs
        if (_cteClauses.Count > 0)
        {
            sql.AppendLine(CultureInfo.InvariantCulture, $"WITH {string.Join(", ", _cteClauses)}");
        }

        // SELECT clause
        if (_selectColumns.Count == 0)
        {
            sql.Append(CultureInfo.InvariantCulture, $"SELECT [{_mainTableAlias}].*");
        }
        else
        {
            sql.Append(CultureInfo.InvariantCulture, $"SELECT {string.Join(", ", _selectColumns)}");
        }

        // FROM clause
        sql.AppendLine();
        sql.Append(CultureInfo.InvariantCulture, $"FROM {string.Join(", ", _fromClauses)}");

        // JOIN clauses
        if (_joinClauses.Count > 0)
        {
            sql.AppendLine();
            sql.Append(string.Join(Environment.NewLine, _joinClauses));
        }

        // WHERE clause
        if (_whereClauses.Count > 0)
        {
            sql.AppendLine();
            sql.Append(CultureInfo.InvariantCulture, $"WHERE {string.Join(" AND ", _whereClauses)}");
        }

        // GROUP BY clause
        if (_groupByClauses.Count > 0)
        {
            sql.AppendLine();
            sql.Append(CultureInfo.InvariantCulture, $"GROUP BY {string.Join(", ", _groupByClauses)}");
        }

        // HAVING clause
        if (_havingClauses.Count > 0)
        {
            sql.AppendLine();
            sql.Append(CultureInfo.InvariantCulture, $"HAVING {string.Join(" AND ", _havingClauses)}");
        }

        // ORDER BY clause
        if (_orderByClauses.Count > 0)
        {
            sql.AppendLine();
            sql.Append(CultureInfo.InvariantCulture, $"ORDER BY {string.Join(", ", _orderByClauses)}");
        }

        // OFFSET/FETCH (SQL Server) for pagination
        if (_skipCount.HasValue || _takeCount.HasValue)
        {
            if (_orderByClauses.Count == 0)
            {
                // ORDER BY is required for OFFSET/FETCH in SQL Server
                sql.AppendLine();
                sql.Append(CultureInfo.InvariantCulture, $"ORDER BY [{_mainTableAlias}].[Id]");
            }

            sql.AppendLine();
            if (_skipCount.HasValue)
            {
                sql.Append(CultureInfo.InvariantCulture, $"OFFSET {_skipCount.Value} ROWS");
                if (_takeCount.HasValue)
                {
                    sql.Append(CultureInfo.InvariantCulture, $" FETCH NEXT {_takeCount.Value} ROWS ONLY");
                }
            }
            else if (_takeCount.HasValue)
            {
                sql.Append(CultureInfo.InvariantCulture, $"OFFSET 0 ROWS FETCH NEXT {_takeCount.Value} ROWS ONLY");
            }
        }

        // Add UNION clauses at the end
        if (_unionClauses.Count > 0)
        {
            sql.AppendLine();
            sql.Append(string.Join(Environment.NewLine, _unionClauses));
        }

        // Combine all parameters
        var allParameters = _expressionVisitor.Parameters.Concat(_additionalParameters).ToList();

        return new SqlQueryResult(sql.ToString(), allParameters);
    }

#pragma warning disable CA1859 // Use concrete types when possible for improved performance
    private ISelectSqlBuilder<TEntity> AddJoin<TJoin>(
#pragma warning restore CA1859 // Use concrete types when possible for improved performance
        string joinType,
        Expression<Func<TEntity, TJoin, bool>> joinCondition)
        where TJoin : class
    {
        ArgumentNullException.ThrowIfNull(joinCondition);

        // Register the join type if not already registered
        if (!_registeredTypes.ContainsKey(typeof(TJoin)))
        {
            RegisterType<TJoin>(_registeredTypes.Count);
        }

        var joinTableName = GetTableName<TJoin>();
        var joinAlias = _registeredTypes[typeof(TJoin)];

        _expressionVisitor.RegisterParameterAliases(joinCondition);
        var conditionSql = _expressionVisitor.VisitAndGenerateSql(joinCondition.Body);
        _joinClauses.Add($"{joinType} [{joinTableName}] AS [{joinAlias}] ON {conditionSql}");

        return this;
    }

    private void RegisterType<T>(int index)
    {
        var alias = GetDefaultAlias<T>(index);
        _registeredTypes[typeof(T)] = alias;
    }

#pragma warning disable CA1308 // Normalize strings to uppercase
    private static string GetDefaultAlias<T>(int index)
    {
        // Generate single character aliases: u, o, c, etc.
        var typeName = typeof(T).Name;
        if (index == 0) return typeName.ToLowerInvariant()[0].ToString();
        return $"{typeName.ToLowerInvariant()[0]}{index}";
    }
#pragma warning restore CA1308 // Normalize strings to uppercase

    private static string GetTableName<T>()
    {
        var type = typeof(T);
        var tableAttribute = type.GetCustomAttribute<System.ComponentModel.DataAnnotations.Schema.TableAttribute>();
        return tableAttribute?.Name ?? type.Name;
    }
}