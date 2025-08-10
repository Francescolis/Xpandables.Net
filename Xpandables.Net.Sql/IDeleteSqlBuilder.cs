using System.Linq.Expressions;

namespace Xpandables.Net.Sql;

/// <summary>
/// Defines the contract for DELETE SQL builders.
/// </summary>
/// <typeparam name="TEntity">The entity type to delete from.</typeparam>
public interface IDeleteSqlBuilder<TEntity> : ISqlBuilder where TEntity : class
{
    /// <summary>
    /// Adds a WHERE condition for the deletion.
    /// </summary>
    /// <param name="predicate">The WHERE condition expression.</param>
    /// <returns>A DELETE SQL builder for method chaining.</returns>
    IDeleteSqlBuilder<TEntity> Where(Expression<Func<TEntity, bool>> predicate);
}
