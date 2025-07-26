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

using System.Runtime.CompilerServices;

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
    /// When overridden in derived classes, inserts a collection of entities into the repository.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entities to be inserted. Must implement <see cref="IEntity"/>.</typeparam>
    /// <param name="entities">The collection of entities to insert. Cannot be null.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous insert operation.</returns>
    public virtual Task InsertAsync<TEntity>(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken)
        where TEntity : class, IEntity => Task.CompletedTask;

    /// <summary>
    /// When overridden in derived classes, updates a collection of entities in the data store.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entities to update. Must implement <see cref="IEntity"/>.</typeparam>
    /// <param name="entities">The collection of entities to be updated. Cannot be null.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous update operation.</returns>
    public virtual Task UpdateAsync<TEntity>(
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
    public override Task DeleteAsync<TEntity>(
        Func<IQueryable<TEntity>, IQueryable<TEntity>> filter,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(filter);

        IQueryable<TEntity> query = filter(Context.Set<TEntity>());

        Context.RemoveRange(query);

        return Task.CompletedTask;
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
    public override async IAsyncEnumerable<TResult> FetchAsync<TEntity, TResult>(
        Func<IQueryable<TEntity>, IQueryable<TResult>> filter,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        IQueryable<TEntity> queryable = typeof(TEntity) != typeof(TResult)
            ? Context.Set<TEntity>().AsNoTracking()
            : Context.Set<TEntity>();

        IQueryable<TResult> query = filter(queryable);

        await foreach (TResult result in query.AsAsyncEnumerable()
            .WithCancellation(cancellationToken))
        {
            yield return result;
        }
    }

    /// <summary>
    /// Asynchronously inserts a collection of entities into the database context.
    /// </summary>
    /// <remarks>This method uses the underlying database context to add the specified entities. The operation
    /// is performed asynchronously and can be cancelled using the provided <paramref
    /// name="cancellationToken"/>.</remarks>
    /// <typeparam name="TEntity">The type of the entities to be inserted.</typeparam>
    /// <param name="entities">The collection of entities to insert. Cannot be null.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous insert operation.</returns>
    public override async Task InsertAsync<TEntity>(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken) =>
        await Context
            .AddRangeAsync(entities, cancellationToken)
            .ConfigureAwait(false);

    /// <summary>
    /// Updates the specified collection of entities in the context asynchronously.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entities to update.</typeparam>
    /// <param name="entities">The collection of entities to be updated. Cannot be <see langword="null"/>.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous update operation.</returns>
    public override Task UpdateAsync<TEntity>(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entities);
        Context.UpdateRange(entities);
        return Task.CompletedTask;
    }
}