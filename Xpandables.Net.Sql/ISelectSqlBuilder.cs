using System.Linq.Expressions;

namespace Xpandables.Net.Sql;

/// <summary>
/// Defines the contract for SELECT SQL builders.
/// </summary>
/// <typeparam name="TEntity">The main entity type.</typeparam>
public interface ISelectSqlBuilder<TEntity> : ISqlBuilder where TEntity : class
{
    /// <summary>
    /// Specifies the columns to select from the main entity.
    /// </summary>
    /// <param name="selector">Expression defining which columns to select.</param>
    /// <returns>A SELECT SQL builder for method chaining.</returns>
    ISelectSqlBuilder<TEntity> Select(Expression<Func<TEntity, object>> selector);

    /// <summary>
    /// Specifies the columns to select from joined entities.
    /// </summary>
    /// <typeparam name="TJoin">The joined entity type.</typeparam>
    /// <param name="selector">Expression defining which columns to select from joined entities.</param>
    /// <returns>A SELECT SQL builder for method chaining.</returns>
    ISelectSqlBuilder<TEntity> Select<TJoin>(Expression<Func<TEntity, TJoin, object>> selector) where TJoin : class;

    /// <summary>
    /// Specifies the columns to select from multiple joined entities.
    /// </summary>
    /// <typeparam name="TJoin">The first joined entity type.</typeparam>
    /// <typeparam name="RJoin">The second joined entity type.</typeparam>
    /// <param name="selector">Expression defining which columns to select from joined entities.</param>
    /// <returns>A SELECT SQL builder for method chaining.</returns>
#pragma warning disable CA1715 // Identifiers should have correct prefix
    ISelectSqlBuilder<TEntity> Select<TJoin, RJoin>(Expression<Func<TEntity, TJoin, RJoin, object>> selector)
#pragma warning restore CA1715 // Identifiers should have correct prefix
        where TJoin : class
        where RJoin : class;

    /// <summary>
    /// Adds a WHERE condition to the query.
    /// </summary>
    /// <param name="predicate">The WHERE condition expression.</param>
    /// <returns>A SELECT SQL builder for method chaining.</returns>
    ISelectSqlBuilder<TEntity> Where(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// Adds a WHERE condition involving a joined entity.
    /// </summary>
    /// <typeparam name="TJoin">The joined entity type.</typeparam>
    /// <param name="predicate">The WHERE condition expression.</param>
    /// <returns>A SELECT SQL builder for method chaining.</returns>
    ISelectSqlBuilder<TEntity> Where<TJoin>(Expression<Func<TEntity, TJoin, bool>> predicate) where TJoin : class;

    /// <summary>
    /// Adds a custom WHERE clause using raw SQL.
    /// </summary>
    /// <param name="rawSql">The raw SQL condition. Use {0}, {1}, etc. for parameter placeholders.</param>
    /// <param name="parameters">Parameters for the raw SQL.</param>
    /// <returns>A SELECT SQL builder for method chaining.</returns>
    /// <example>
    /// <code>
    /// query.WhereRaw("YEAR({0}) > {1}", "u.BirthDate", 1990)
    /// // Generates: WHERE (YEAR(@raw0) > @raw1)
    /// </code>
    /// </example>
    ISelectSqlBuilder<TEntity> WhereRaw(string rawSql, params object[] parameters);

    /// <summary>
    /// Adds an INNER JOIN to another table.
    /// </summary>
    /// <typeparam name="TJoin">The entity type to join.</typeparam>
    /// <param name="joinCondition">The join condition expression.</param>
    /// <returns>A SELECT SQL builder for method chaining.</returns>
    ISelectSqlBuilder<TEntity> InnerJoin<TJoin>(Expression<Func<TEntity, TJoin, bool>> joinCondition) where TJoin : class;

    /// <summary>
    /// Adds a LEFT JOIN to another table.
    /// </summary>
    /// <typeparam name="TJoin">The entity type to join.</typeparam>
    /// <param name="joinCondition">The join condition expression.</param>
    /// <returns>A SELECT SQL builder for method chaining.</returns>
    ISelectSqlBuilder<TEntity> LeftJoin<TJoin>(Expression<Func<TEntity, TJoin, bool>> joinCondition) where TJoin : class;

    /// <summary>
    /// Adds a RIGHT JOIN to another table.
    /// </summary>
    /// <typeparam name="TJoin">The entity type to join.</typeparam>
    /// <param name="joinCondition">The join condition expression.</param>
    /// <returns>A SELECT SQL builder for method chaining.</returns>
    ISelectSqlBuilder<TEntity> RightJoin<TJoin>(Expression<Func<TEntity, TJoin, bool>> joinCondition) where TJoin : class;

    /// <summary>
    /// Adds a FULL OUTER JOIN to another table.
    /// </summary>
    /// <typeparam name="TJoin">The entity type to join.</typeparam>
    /// <param name="joinCondition">The join condition expression.</param>
    /// <returns>A SELECT SQL builder for method chaining.</returns>
    ISelectSqlBuilder<TEntity> FullOuterJoin<TJoin>(Expression<Func<TEntity, TJoin, bool>> joinCondition) where TJoin : class;

    /// <summary>
    /// Adds a CROSS JOIN to another table.
    /// </summary>
    /// <typeparam name="TJoin">The entity type to join.</typeparam>
    /// <returns>A SELECT SQL builder for method chaining.</returns>
    ISelectSqlBuilder<TEntity> CrossJoin<TJoin>() where TJoin : class;

    /// <summary>
    /// Adds GROUP BY columns.
    /// </summary>
    /// <param name="groupSelector">Expression defining the grouping columns.</param>
    /// <returns>A SELECT SQL builder for method chaining.</returns>
    ISelectSqlBuilder<TEntity> GroupBy(Expression<Func<TEntity, object>> groupSelector);

    /// <summary>
    /// Adds GROUP BY columns from joined entities.
    /// </summary>
    /// <typeparam name="TJoin">The joined entity type.</typeparam>
    /// <param name="groupSelector">Expression defining the grouping columns.</param>
    /// <returns>A SELECT SQL builder for method chaining.</returns>
    ISelectSqlBuilder<TEntity> GroupBy<TJoin>(Expression<Func<TEntity, TJoin, object>> groupSelector) where TJoin : class;

    /// <summary>
    /// Adds HAVING condition for grouped results.
    /// </summary>
    /// <param name="havingCondition">The HAVING condition expression.</param>
    /// <returns>A SELECT SQL builder for method chaining.</returns>
    ISelectSqlBuilder<TEntity> Having(Expression<Func<TEntity, bool>> havingCondition);

    /// <summary>
    /// Adds HAVING condition for grouped results.
    /// </summary>
    /// <typeparam name="TJoin">The joined entity type.</typeparam>
    /// <param name="havingCondition">The HAVING condition expression.</param>
    /// <returns>A SELECT SQL builder for method chaining.</returns>
    ISelectSqlBuilder<TEntity> Having<TJoin>(Expression<Func<TEntity, TJoin, bool>> havingCondition) where TJoin : class;

    /// <summary>
    /// Adds ORDER BY clause in ascending order.
    /// </summary>
    /// <typeparam name="TKey">The type of the ordering key.</typeparam>
    /// <param name="orderSelector">Expression defining the ordering column.</param>
    /// <returns>A SELECT SQL builder for method chaining.</returns>
    ISelectSqlBuilder<TEntity> OrderBy<TKey>(Expression<Func<TEntity, TKey>> orderSelector);

    /// <summary>
    /// Adds ORDER BY clause in descending order.
    /// </summary>
    /// <typeparam name="TKey">The type of the ordering key.</typeparam>
    /// <param name="orderSelector">Expression defining the ordering column.</param>
    /// <returns>A SELECT SQL builder for method chaining.</returns>
    ISelectSqlBuilder<TEntity> OrderByDescending<TKey>(Expression<Func<TEntity, TKey>> orderSelector);

    /// <summary>
    /// Adds SKIP (OFFSET) clause for pagination.
    /// </summary>
    /// <param name="count">Number of rows to skip.</param>
    /// <returns>A SELECT SQL builder for method chaining.</returns>
    ISelectSqlBuilder<TEntity> Skip(int count);

    /// <summary>
    /// Adds TAKE (LIMIT) clause for pagination.
    /// </summary>
    /// <param name="count">Number of rows to take.</param>
    /// <returns>A SELECT SQL builder for method chaining.</returns>
    ISelectSqlBuilder<TEntity> Take(int count);

    /// <summary>
    /// Adds a Common Table Expression (CTE) to the query.
    /// </summary>
    /// <param name="cteName">The name of the CTE.</param>
    /// <param name="cteQuery">The CTE query builder.</param>
    /// <returns>A SELECT SQL builder for method chaining.</returns>
    ISelectSqlBuilder<TEntity> WithCte(string cteName, ISqlBuilder cteQuery);

    /// <summary>
    /// Adds a UNION operation to combine results from another query.
    /// </summary>
    /// <param name="unionQuery">The query to union with.</param>
    /// <returns>A SELECT SQL builder for method chaining.</returns>
    ISelectSqlBuilder<TEntity> Union(ISelectSqlBuilder<TEntity> unionQuery);

    /// <summary>
    /// Adds a UNION ALL operation to combine results from another query.
    /// </summary>
    /// <param name="unionQuery">The query to union all with.</param>
    /// <returns>A SELECT SQL builder for method chaining.</returns>
    ISelectSqlBuilder<TEntity> UnionAll(ISelectSqlBuilder<TEntity> unionQuery);
}