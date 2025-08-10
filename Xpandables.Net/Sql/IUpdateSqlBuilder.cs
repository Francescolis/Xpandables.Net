using System.Linq.Expressions;

namespace Xpandables.Net.Sql;

/// <summary>
/// Defines the contract for UPDATE SQL builders.
/// </summary>
/// <typeparam name="TEntity">The entity type to update.</typeparam>
public interface IUpdateSqlBuilder<TEntity> : ISqlBuilder where TEntity : class
{
    /// <summary>
    /// Specifies the columns and values to update.
    /// </summary>
    /// <typeparam name="TValues">The type containing the new values.</typeparam>
    /// <param name="setSelector">Expression defining the columns and values to update.</param>
    /// <returns>An UPDATE SQL builder for method chaining.</returns>
    IUpdateSqlBuilder<TEntity> Set<TValues>(Expression<Func<TEntity, TValues>> setSelector);

    /// <summary>
    /// Adds a WHERE condition for the update.
    /// </summary>
    /// <param name="predicate">The WHERE condition expression.</param>
    /// <returns>An UPDATE SQL builder for method chaining.</returns>
    IUpdateSqlBuilder<TEntity> Where(Expression<Func<TEntity, bool>> predicate);
}
