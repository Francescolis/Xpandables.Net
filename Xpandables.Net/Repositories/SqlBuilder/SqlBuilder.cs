//=====================================================
// Copyright (c) 2024 Francis-Black EWANE
//=====================================================

using System.Linq.Expressions;
using System.Text;

namespace Xpandables.Net.Repositories.SqlBuilder;

/// <summary>
/// Represents a SQL query with its parameters.
/// </summary>
/// <param name="Sql">The SQL query string.</param>
/// <param name="Parameters">The query parameters.</param>
public sealed record SqlQuery(string Sql, IReadOnlyDictionary<string, object?> Parameters);

/// <summary>
/// A fluent SQL query builder with support for multi-source queries.
/// </summary>
/// <typeparam name="TSource">The primary source type.</typeparam>
public sealed class SqlBuilder<TSource> where TSource : class
{
    private readonly QueryModel _queryModel;
    private static int _aliasCounter = 0;
    
    internal SqlBuilder()
    {
        _queryModel = new QueryModel();
    }
    
    internal SqlBuilder(QueryModel queryModel)
    {
        _queryModel = queryModel ?? throw new ArgumentNullException(nameof(queryModel));
    }
    
    /// <summary>
    /// Specifies the columns to select.
    /// </summary>
    /// <param name="selector">The column selector expression.</param>
    /// <returns>The builder for method chaining.</returns>
    public SqlBuilder<TSource> Select(Expression<Func<TSource, object>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        
        var columnExpression = ExpressionParser.ParseSelectExpression(selector);
        _queryModel.SelectColumns.Clear();
        _queryModel.SelectColumns.Add(columnExpression);
        
        return this;
    }
    
    /// <summary>
    /// Specifies the columns to select from multiple sources.
    /// </summary>
    /// <typeparam name="TSecond">The second source type.</typeparam>
    /// <param name="selector">The column selector expression.</param>
    /// <returns>The builder for method chaining.</returns>
    public SqlBuilder<TSource> Select<TSecond>(Expression<Func<TSource, TSecond, object>> selector)
        where TSecond : class
    {
        ArgumentNullException.ThrowIfNull(selector);
        
        var columnExpression = ExpressionParser.ParseSelectExpression(selector);
        _queryModel.SelectColumns.Clear();
        _queryModel.SelectColumns.Add(columnExpression);
        
        return this;
    }
    
    /// <summary>
    /// Specifies additional columns to select.
    /// </summary>
    /// <param name="selector">The column selector expression.</param>
    /// <returns>The builder for method chaining.</returns>
    public SqlBuilder<TSource> AddSelect(Expression<Func<TSource, object>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        
        var columnExpression = ExpressionParser.ParseSelectExpression(selector);
        _queryModel.SelectColumns.Add(columnExpression);
        
        return this;
    }
    
    /// <summary>
    /// Adds a WHERE condition.
    /// </summary>
    /// <param name="predicate">The WHERE predicate.</param>
    /// <returns>The builder for method chaining.</returns>
    public SqlBuilder<TSource> Where(Expression<Func<TSource, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        
        var whereCondition = ExpressionParser.ParseWhereExpression(predicate, _queryModel);
        _queryModel.WhereConditions.Add(whereCondition);
        
        return this;
    }
    
    /// <summary>
    /// Adds a WHERE condition with multiple sources.
    /// </summary>
    /// <typeparam name="TSecond">The second source type.</typeparam>
    /// <param name="predicate">The WHERE predicate.</param>
    /// <returns>The builder for method chaining.</returns>
    public SqlBuilder<TSource> Where<TSecond>(Expression<Func<TSource, TSecond, bool>> predicate)
        where TSecond : class
    {
        ArgumentNullException.ThrowIfNull(predicate);
        
        var whereCondition = ExpressionParser.ParseWhereExpression(predicate, _queryModel);
        _queryModel.WhereConditions.Add(whereCondition);
        
        return this;
    }
    
    /// <summary>
    /// Adds an INNER JOIN.
    /// </summary>
    /// <typeparam name="TJoin">The type to join.</typeparam>
    /// <param name="joinCondition">The join condition.</param>
    /// <param name="alias">Optional alias for the joined table.</param>
    /// <returns>The builder for method chaining.</returns>
    public SqlBuilder<TSource> InnerJoin<TJoin>(
        Expression<Func<TSource, TJoin, bool>> joinCondition,
        string? alias = null)
        where TJoin : class
    {
        return AddJoin<TJoin>(JoinType.Inner, joinCondition, alias);
    }
    
    /// <summary>
    /// Adds a LEFT JOIN.
    /// </summary>
    /// <typeparam name="TJoin">The type to join.</typeparam>
    /// <param name="joinCondition">The join condition.</param>
    /// <param name="alias">Optional alias for the joined table.</param>
    /// <returns>The builder for method chaining.</returns>
    public SqlBuilder<TSource> LeftJoin<TJoin>(
        Expression<Func<TSource, TJoin, bool>> joinCondition,
        string? alias = null)
        where TJoin : class
    {
        return AddJoin<TJoin>(JoinType.Left, joinCondition, alias);
    }
    
    /// <summary>
    /// Adds a RIGHT JOIN.
    /// </summary>
    /// <typeparam name="TJoin">The type to join.</typeparam>
    /// <param name="joinCondition">The join condition.</param>
    /// <param name="alias">Optional alias for the joined table.</param>
    /// <returns>The builder for method chaining.</returns>
    public SqlBuilder<TSource> RightJoin<TJoin>(
        Expression<Func<TSource, TJoin, bool>> joinCondition,
        string? alias = null)
        where TJoin : class
    {
        return AddJoin<TJoin>(JoinType.Right, joinCondition, alias);
    }
    
    /// <summary>
    /// Adds a FULL OUTER JOIN.
    /// </summary>
    /// <typeparam name="TJoin">The type to join.</typeparam>
    /// <param name="joinCondition">The join condition.</param>
    /// <param name="alias">Optional alias for the joined table.</param>
    /// <returns>The builder for method chaining.</returns>
    public SqlBuilder<TSource> FullJoin<TJoin>(
        Expression<Func<TSource, TJoin, bool>> joinCondition,
        string? alias = null)
        where TJoin : class
    {
        return AddJoin<TJoin>(JoinType.Full, joinCondition, alias);
    }
    
    /// <summary>
    /// Adds a GROUP BY clause.
    /// </summary>
    /// <param name="grouping">The grouping expression.</param>
    /// <returns>The builder for method chaining.</returns>
    public SqlBuilder<TSource> GroupBy(Expression<Func<TSource, object>> grouping)
    {
        ArgumentNullException.ThrowIfNull(grouping);
        
        var groupByExpression = ExpressionParser.ParseGroupByExpression(grouping);
        _queryModel.GroupByColumns.Add(groupByExpression);
        
        return this;
    }
    
    /// <summary>
    /// Adds a GROUP BY clause with multiple sources.
    /// </summary>
    /// <typeparam name="TSecond">The second source type.</typeparam>
    /// <param name="grouping">The grouping expression.</param>
    /// <returns>The builder for method chaining.</returns>
    public SqlBuilder<TSource> GroupBy<TSecond>(Expression<Func<TSource, TSecond, object>> grouping)
        where TSecond : class
    {
        ArgumentNullException.ThrowIfNull(grouping);
        
        var groupByExpression = ExpressionParser.ParseGroupByExpression(grouping);
        _queryModel.GroupByColumns.Add(groupByExpression);
        
        return this;
    }
    
    /// <summary>
    /// Adds a HAVING clause.
    /// </summary>
    /// <param name="having">The having condition.</param>
    /// <returns>The builder for method chaining.</returns>
    public SqlBuilder<TSource> Having(Expression<Func<TSource, bool>> having)
    {
        ArgumentNullException.ThrowIfNull(having);
        
        var havingCondition = ExpressionParser.ParseWhereExpression(having, _queryModel);
        _queryModel.HavingConditions.Add(havingCondition);
        
        return this;
    }
    
    /// <summary>
    /// Adds a HAVING clause with multiple sources.
    /// </summary>
    /// <typeparam name="TSecond">The second source type.</typeparam>
    /// <param name="having">The having condition.</param>
    /// <returns>The builder for method chaining.</returns>
    public SqlBuilder<TSource> Having<TSecond>(Expression<Func<TSource, TSecond, bool>> having)
        where TSecond : class
    {
        ArgumentNullException.ThrowIfNull(having);
        
        var havingCondition = ExpressionParser.ParseWhereExpression(having, _queryModel);
        _queryModel.HavingConditions.Add(havingCondition);
        
        return this;
    }
    
    /// <summary>
    /// Adds an ORDER BY clause.
    /// </summary>
    /// <param name="ordering">The ordering expression.</param>
    /// <returns>The builder for method chaining.</returns>
    public SqlBuilder<TSource> OrderBy(Expression<Func<TSource, object>> ordering)
    {
        ArgumentNullException.ThrowIfNull(ordering);
        
        var orderExpression = ExpressionParser.ParseOrderByExpression(ordering);
        _queryModel.OrderByColumns.Clear();
        _queryModel.OrderByColumns.Add(OrderByModel.Create(orderExpression, OrderDirection.Ascending, 0));
        
        return this;
    }
    
    /// <summary>
    /// Adds an ORDER BY clause with multiple sources.
    /// </summary>
    /// <typeparam name="TSecond">The second source type.</typeparam>
    /// <param name="ordering">The ordering expression.</param>
    /// <returns>The builder for method chaining.</returns>
    public SqlBuilder<TSource> OrderBy<TSecond>(Expression<Func<TSource, TSecond, object>> ordering)
        where TSecond : class
    {
        ArgumentNullException.ThrowIfNull(ordering);
        
        var orderExpression = ExpressionParser.ParseOrderByExpression(ordering);
        _queryModel.OrderByColumns.Clear();
        _queryModel.OrderByColumns.Add(OrderByModel.Create(orderExpression, OrderDirection.Ascending, 0));
        
        return this;
    }
    
    /// <summary>
    /// Adds an ORDER BY DESC clause.
    /// </summary>
    /// <param name="ordering">The ordering expression.</param>
    /// <returns>The builder for method chaining.</returns>
    public SqlBuilder<TSource> OrderByDescending(Expression<Func<TSource, object>> ordering)
    {
        ArgumentNullException.ThrowIfNull(ordering);
        
        var orderExpression = ExpressionParser.ParseOrderByExpression(ordering);
        _queryModel.OrderByColumns.Clear();
        _queryModel.OrderByColumns.Add(OrderByModel.Create(orderExpression, OrderDirection.Descending, 0));
        
        return this;
    }
    
    /// <summary>
    /// Adds an ORDER BY DESC clause with multiple sources.
    /// </summary>
    /// <typeparam name="TSecond">The second source type.</typeparam>
    /// <param name="ordering">The ordering expression.</param>
    /// <returns>The builder for method chaining.</returns>
    public SqlBuilder<TSource> OrderByDescending<TSecond>(Expression<Func<TSource, TSecond, object>> ordering)
        where TSecond : class
    {
        ArgumentNullException.ThrowIfNull(ordering);
        
        var orderExpression = ExpressionParser.ParseOrderByExpression(ordering);
        _queryModel.OrderByColumns.Clear();
        _queryModel.OrderByColumns.Add(OrderByModel.Create(orderExpression, OrderDirection.Descending, 0));
        
        return this;
    }
    
    /// <summary>
    /// Adds a secondary ORDER BY clause.
    /// </summary>
    /// <param name="ordering">The ordering expression.</param>
    /// <returns>The builder for method chaining.</returns>
    public SqlBuilder<TSource> ThenBy(Expression<Func<TSource, object>> ordering)
    {
        ArgumentNullException.ThrowIfNull(ordering);
        
        var orderExpression = ExpressionParser.ParseOrderByExpression(ordering);
        var position = _queryModel.OrderByColumns.Count;
        _queryModel.OrderByColumns.Add(OrderByModel.Create(orderExpression, OrderDirection.Ascending, position));
        
        return this;
    }
    
    /// <summary>
    /// Adds a secondary ORDER BY clause with multiple sources.
    /// </summary>
    /// <typeparam name="TSecond">The second source type.</typeparam>
    /// <param name="ordering">The ordering expression.</param>
    /// <returns>The builder for method chaining.</returns>
    public SqlBuilder<TSource> ThenBy<TSecond>(Expression<Func<TSource, TSecond, object>> ordering)
        where TSecond : class
    {
        ArgumentNullException.ThrowIfNull(ordering);
        
        var orderExpression = ExpressionParser.ParseOrderByExpression(ordering);
        var position = _queryModel.OrderByColumns.Count;
        _queryModel.OrderByColumns.Add(OrderByModel.Create(orderExpression, OrderDirection.Ascending, position));
        
        return this;
    }
    
    /// <summary>
    /// Adds a secondary ORDER BY DESC clause.
    /// </summary>
    /// <param name="ordering">The ordering expression.</param>
    /// <returns>The builder for method chaining.</returns>
    public SqlBuilder<TSource> ThenByDescending(Expression<Func<TSource, object>> ordering)
    {
        ArgumentNullException.ThrowIfNull(ordering);
        
        var orderExpression = ExpressionParser.ParseOrderByExpression(ordering);
        var position = _queryModel.OrderByColumns.Count;
        _queryModel.OrderByColumns.Add(OrderByModel.Create(orderExpression, OrderDirection.Descending, position));
        
        return this;
    }
    
    /// <summary>
    /// Adds a secondary ORDER BY DESC clause with multiple sources.
    /// </summary>
    /// <typeparam name="TSecond">The second source type.</typeparam>
    /// <param name="ordering">The ordering expression.</param>
    /// <returns>The builder for method chaining.</returns>
    public SqlBuilder<TSource> ThenByDescending<TSecond>(Expression<Func<TSource, TSecond, object>> ordering)
        where TSecond : class
    {
        ArgumentNullException.ThrowIfNull(ordering);
        
        var orderExpression = ExpressionParser.ParseOrderByExpression(ordering);
        var position = _queryModel.OrderByColumns.Count;
        _queryModel.OrderByColumns.Add(OrderByModel.Create(orderExpression, OrderDirection.Descending, position));
        
        return this;
    }
    
    /// <summary>
    /// Sets the query to return distinct results.
    /// </summary>
    /// <returns>The builder for method chaining.</returns>
    public SqlBuilder<TSource> Distinct()
    {
        _queryModel.IsDistinct = true;
        return this;
    }
    
    /// <summary>
    /// Skips the specified number of rows.
    /// </summary>
    /// <param name="count">The number of rows to skip.</param>
    /// <returns>The builder for method chaining.</returns>
    public SqlBuilder<TSource> Skip(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        
        _queryModel.Skip = count;
        return this;
    }
    
    /// <summary>
    /// Takes the specified number of rows.
    /// </summary>
    /// <param name="count">The number of rows to take.</param>
    /// <returns>The builder for method chaining.</returns>
    public SqlBuilder<TSource> Take(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);
        
        _queryModel.Take = count;
        return this;
    }
    
    /// <summary>
    /// Builds the final SQL query.
    /// </summary>
    /// <returns>The SQL query with parameters.</returns>
    public SqlQuery Build()
    {
        var sql = new StringBuilder();
        
        // SELECT clause
        BuildSelectClause(sql);
        
        // FROM clause  
        BuildFromClause(sql);
        
        // JOIN clauses
        BuildJoinClauses(sql);
        
        // WHERE clause
        BuildWhereClause(sql);
        
        // GROUP BY clause
        BuildGroupByClause(sql);
        
        // HAVING clause
        BuildHavingClause(sql);
        
        // ORDER BY clause
        BuildOrderByClause(sql);
        
        // Pagination
        BuildPaginationClause(sql);
        
        return new SqlQuery(sql.ToString(), _queryModel.Parameters.AsReadOnly());
    }
    
    private SqlBuilder<TSource> AddJoin<TJoin>(
        JoinType joinType, 
        Expression<Func<TSource, TJoin, bool>> joinCondition, 
        string? alias = null)
        where TJoin : class
    {
        ArgumentNullException.ThrowIfNull(joinCondition);
        
        alias ??= GenerateAlias<TJoin>();
        var table = TableModel.Create<TJoin>(alias);
        var condition = ExpressionParser.ParseWhereExpression(joinCondition, _queryModel);
        var join = JoinModel.Create(joinType, table, condition);
        
        _queryModel.Joins.Add(join);
        _queryModel.RegisterTableAlias<TJoin>(alias);
        
        return this;
    }
    
    private void BuildSelectClause(StringBuilder sql)
    {
        sql.Append("SELECT");
        
        if (_queryModel.IsDistinct)
        {
            sql.Append(" DISTINCT");
        }
        
        if (_queryModel.SelectColumns.Count == 0)
        {
            sql.Append(" *");
        }
        else
        {
            sql.Append(" ");
            sql.Append(string.Join(", ", _queryModel.SelectColumns));
        }
    }
    
    private void BuildFromClause(StringBuilder sql)
    {
        if (_queryModel.FromTable is null)
        {
            var alias = GenerateAlias<TSource>();
            _queryModel.FromTable = TableModel.Create<TSource>(alias);
            _queryModel.RegisterTableAlias<TSource>(alias);
        }
        
        sql.AppendLine();
        sql.Append("FROM ");
        sql.Append(_queryModel.FromTable.TableReference);
    }
    
    private void BuildJoinClauses(StringBuilder sql)
    {
        foreach (var join in _queryModel.Joins)
        {
            sql.AppendLine();
            sql.Append(join.ToSql());
        }
    }
    
    private void BuildWhereClause(StringBuilder sql)
    {
        if (!_queryModel.HasWhere) return;
        
        sql.AppendLine();
        sql.Append("WHERE ");
        sql.Append(string.Join(" AND ", _queryModel.WhereConditions));
    }
    
    private void BuildGroupByClause(StringBuilder sql)
    {
        if (!_queryModel.HasGroupBy) return;
        
        sql.AppendLine();
        sql.Append("GROUP BY ");
        sql.Append(string.Join(", ", _queryModel.GroupByColumns));
    }
    
    private void BuildHavingClause(StringBuilder sql)
    {
        if (!_queryModel.HasHaving) return;
        
        sql.AppendLine();
        sql.Append("HAVING ");
        sql.Append(string.Join(" AND ", _queryModel.HavingConditions));
    }
    
    private void BuildOrderByClause(StringBuilder sql)
    {
        if (!_queryModel.HasOrderBy) return;
        
        sql.AppendLine();
        sql.Append("ORDER BY ");
        
        var orderedColumns = _queryModel.OrderByColumns
            .OrderBy(o => o.Position)
            .Select(o => o.ToSql());
            
        sql.Append(string.Join(", ", orderedColumns));
    }
    
    private void BuildPaginationClause(StringBuilder sql)
    {
        if (!_queryModel.HasPagination) return;
        
        if (_queryModel.Skip.HasValue)
        {
            sql.AppendLine();
            sql.Append($"OFFSET {_queryModel.Skip.Value} ROWS");
        }
        
        if (_queryModel.Take.HasValue)
        {
            if (!_queryModel.Skip.HasValue)
            {
                sql.AppendLine();
                sql.Append("OFFSET 0 ROWS");
            }
            
            sql.AppendLine();
            sql.Append($"FETCH NEXT {_queryModel.Take.Value} ROWS ONLY");
        }
    }
    
    private static string GenerateAlias<T>()
    {
        var typeName = typeof(T).Name;
        var alias = typeName.Length > 1 
            ? typeName.Substring(0, 1).ToLowerInvariant() + (++_aliasCounter).ToString()
            : "t" + (++_aliasCounter).ToString();
            
        return alias;
    }
}

/// <summary>
/// Factory class for creating SQL builders.
/// </summary>
public static class SqlBuilder
{
    /// <summary>
    /// Creates a new SQL builder for the specified type.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="alias">Optional table alias.</param>
    /// <returns>A new SQL builder instance.</returns>
    public static SqlBuilder<T> From<T>(string? alias = null) where T : class
    {
        var builder = new SqlBuilder<T>();
        
        // Initialize the FROM table
        alias ??= GenerateAlias<T>();
        var queryModel = GetQueryModel(builder);
        queryModel.FromTable = TableModel.Create<T>(alias);
        queryModel.RegisterTableAlias<T>(alias);
        
        return builder;
    }
    
    private static QueryModel GetQueryModel<T>(SqlBuilder<T> builder) where T : class
    {
        // Use reflection to access the private _queryModel field
        var field = typeof(SqlBuilder<T>).GetField("_queryModel", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (QueryModel)field!.GetValue(builder)!;
    }
    
    private static string GenerateAlias<T>()
    {
        var typeName = typeof(T).Name;
        return typeName.Length > 1 
            ? typeName.Substring(0, 1).ToLowerInvariant()
            : "t";
    }
}