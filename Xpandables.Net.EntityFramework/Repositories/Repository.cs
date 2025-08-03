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

using Microsoft.EntityFrameworkCore;

namespace Xpandables.Net.Repositories;

/// <summary>
/// Represents an abstract base class for a repository that provides asynchronous operations for managing entities in a
/// data store. This class must be inherited to implement specific data access logic.
/// </summary>
/// <remarks>The <see cref="Repository"/> class defines a set of virtual methods for common data operations such
/// as insert, update, delete, and fetch. Derived classes should override these methods to provide specific
/// implementations for interacting with a particular data source.</remarks>
public abstract class Repository : AsyncDisposable, IRepository
{
    /// <summary>
    /// when overridden in derived classes, deletes entities from the repository.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity to delete. Must implement <see cref="IEntity"/>.</typeparam>
    /// <param name="filter">A function to filter the entities to be deleted. 
    /// The function takes an <see cref="IQueryable{TEntity}"/> and
    /// returns a filtered <see cref="IQueryable{TEntity}"/>.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    public virtual Task DeleteAsync<TEntity>(
        Func<IQueryable<TEntity>, IQueryable<TEntity>> filter,
        CancellationToken cancellationToken)
        where TEntity : class, IEntity => Task.CompletedTask;

    /// <summary>
    /// When overridden in derived classes, fetches a sequence of results asynchronously based on the provided filter.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity to query, which must implement <see cref="IEntity"/>.</typeparam>
    /// <typeparam name="TResult">The type of the result to return.</typeparam>
    /// <param name="filter">A function that defines the query to apply to the <see cref="IQueryable{TEntity}"/> to produce a sequence of
    /// <see cref="IQueryable{TResult}"/>.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. 
    /// The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>An asynchronous sequence of <typeparamref name="TResult"/> that represents the query results.</returns>
    public virtual IAsyncEnumerable<TResult> FetchAsync<TEntity, TResult>(
        Func<IQueryable<TEntity>, IQueryable<TResult>> filter,
        CancellationToken cancellationToken = default)
        where TEntity : class, IEntity => AsyncEnumerable.Empty<TResult>();

    /// <summary>
    /// When overridden in derived classes, adds or updates a collection of entities in the data store asynchronously.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entities to add or update. Must implement <see cref="IEntity"/>.</typeparam>
    /// <param name="entities">The collection of entities to add or update. Cannot be <see langword="null"/> or empty.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public virtual Task AddOrUpdateAsync<TEntity>(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken)
        where TEntity : class, IEntity => Task.CompletedTask;

}

/// <summary>
/// Represents a repository that provides data access functionality for a
/// specific data context.
/// </summary>
public class Repository<TDataContext> : Repository, IRepository<TDataContext>
    where TDataContext : DataContext
{
    /// <summary>
    /// Gets the data context associated with this repository.
    /// </summary>
    public required TDataContext Context { get; set; }

    /// <summary>
    /// Asynchronously deletes entities from the database that match the specified filter.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entities to delete.</typeparam>
    /// <param name="filter">A function to filter the entities to be deleted. Cannot be <see langword="null"/>.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    public override async Task DeleteAsync<TEntity>(
        Func<IQueryable<TEntity>, IQueryable<TEntity>> filter,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(filter);

        IQueryable<TEntity> query = filter(Context.Set<TEntity>());

        Context.RemoveRange(query);

        await Task.CompletedTask.ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously fetches a sequence of results from the database based on the specified filter.
    /// </summary>
    /// <remarks>The method uses asynchronous streaming to yield results as they are retrieved from the
    /// database. If <typeparamref name="TEntity"/> and <typeparamref name="TResult"/> are different types, the query is
    /// executed with no tracking to improve performance.</remarks>
    /// <typeparam name="TEntity">The type of the entity to query.</typeparam>
    /// <typeparam name="TResult">The type of the result to return.</typeparam>
    /// <param name="filter">A function to apply a filter to the queryable entity set.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An asynchronous sequence of results of type <typeparamref name="TResult"/>.</returns>
    public override IAsyncEnumerable<TResult> FetchAsync<TEntity, TResult>(
        Func<IQueryable<TEntity>, IQueryable<TResult>> filter,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        IQueryable<TEntity> queryable = typeof(TEntity) != typeof(TResult)
            ? Context.Set<TEntity>().AsNoTracking()
            : Context.Set<TEntity>();

        IQueryable<TResult> query = filter(queryable);
        return query.AsAsyncEnumerable();
    }

    /// <summary>
    /// Asynchronously adds or updates a collection of entities in the database.
    /// </summary>
    /// <remarks>This method uses the underlying database context to add the specified entities. The operation
    /// is performed asynchronously and can be cancelled using the provided <paramref
    /// name="cancellationToken"/>.</remarks>
    /// <typeparam name="TEntity">The type of the entities to add or update. Must implement <see cref="IEntity"/>.</typeparam>
    /// <param name="entities">The collection of entities to add or update. Cannot be <see langword="null"/> or empty.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public override async Task AddOrUpdateAsync<TEntity>(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entities);
        if (entities is not ICollection<TEntity> collection || collection.Count == 0)
        {
            return;
        }

        Context.UpdateRange(entities);

        await Task.CompletedTask.ConfigureAwait(false);
    }
}

/// <summary>
/// Represents a repository that provides persistent data operations for a specified data context.
/// </summary>
/// <remarks>This class extends the functionality of <see cref="Repository{TDataContext}"/> by ensuring that
/// changes made to the data context are persisted to the database. It provides methods for deleting entities and adding
/// or updating entities, with changes automatically committed to the underlying data source.</remarks>
/// <typeparam name="TDataContext">The type of the data context used by the repository. 
/// Must derive from <see cref="DataContext"/>.</typeparam>
public class RepositoryPersistent<TDataContext> : Repository<TDataContext>
    where TDataContext : DataContext
{
    /// <summary>
    /// Deletes entities from the data source that match the specified filter.
    /// </summary>
    /// <remarks>This method deletes the entities that match the specified filter and commits the changes to
    /// the database.</remarks>
    /// <typeparam name="TEntity">The type of the entities to delete.</typeparam>
    /// <param name="filter">A function that applies a queryable filter to select the entities to delete. The function should return a
    /// filtered <see cref="IQueryable{T}"/> of the entities to be removed.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the operation to complete.</param>
    /// <returns></returns>
    public override async Task DeleteAsync<TEntity>(
        Func<IQueryable<TEntity>, IQueryable<TEntity>> filter,
        CancellationToken cancellationToken)
    {
        await base.DeleteAsync(filter, cancellationToken).ConfigureAwait(false);
        await Context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Adds new entities or updates existing entities in the database asynchronously.
    /// </summary>
    /// <remarks>This method ensures that changes are persisted to the database by calling <see
    /// cref="DbContext.SaveChangesAsync(CancellationToken)"/>.</remarks>
    /// <typeparam name="TEntity">The type of the entities to add or update.</typeparam>
    /// <param name="entities">The collection of entities to be added or updated. Cannot be null.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns></returns>
    public override async Task AddOrUpdateAsync<TEntity>(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken)
    {
        await base.AddOrUpdateAsync(entities, cancellationToken).ConfigureAwait(false);
        await Context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}