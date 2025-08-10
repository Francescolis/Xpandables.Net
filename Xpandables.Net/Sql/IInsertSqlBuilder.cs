using System.Linq.Expressions;

namespace Xpandables.Net.Sql;
/// <summary>
/// Defines the contract for INSERT SQL builders.
/// </summary>
/// <typeparam name="TEntity">The entity type to insert.</typeparam>
public interface IInsertSqlBuilder<TEntity> : ISqlBuilder where TEntity : class
{
    /// <summary>
    /// Specifies the columns and values to insert.
    /// </summary>
    /// <typeparam name="TValues">The type containing the values.</typeparam>
    /// <param name="valuesSelector">Expression defining the columns and values to insert.</param>
    /// <returns>An INSERT SQL builder for method chaining.</returns>
    IInsertSqlBuilder<TEntity> Values<TValues>(Expression<Func<TEntity, TValues>> valuesSelector);

    /// <summary>
    /// Adds multiple rows to insert.
    /// </summary>
    /// <param name="entities">Collection of entities to insert.</param>
    /// <returns>An INSERT SQL builder for method chaining.</returns>
    IInsertSqlBuilder<TEntity> Values(IEnumerable<TEntity> entities);
}
