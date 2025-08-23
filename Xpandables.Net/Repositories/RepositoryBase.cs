
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
using Xpandables.Net.Collections;

namespace Xpandables.Net.Repositories;

/// <summary>
/// Represents an abstract base class for a repository that provides asynchronous operations for managing entities in a
/// data store. This class must be inherited to implement specific data access logic.
/// </summary>
/// <remarks>The <see cref="RepositoryBase"/> class defines a set of virtual methods for common data operations such
/// as insert, update, delete, and fetch. Derived classes should override these methods to provide specific
/// implementations for interacting with a particular data source.</remarks>
public abstract class RepositoryBase : AsyncDisposable, IRepository
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
        where TEntity : class => Task.CompletedTask;

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
    public virtual IAsyncPagedEnumerable<TResult> FetchAsync<TEntity, TResult>(
        Func<IQueryable<TEntity>, IQueryable<TResult>> filter,
        CancellationToken cancellationToken = default)
        where TEntity : class =>
        AsyncEnumerable.Empty<TResult>().WithPagination();

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
        where TEntity : class => Task.CompletedTask;
}

/// <summary>
/// Represents a base class for repositories that operate on a specific data context.
/// </summary>
/// <remarks>This class provides a foundation for implementing repositories that interact with a specific data
/// context. Derived classes should define the specific operations and behaviors for the repository.</remarks>
/// <typeparam name="TDataContext">The type of the data context associated with the repository. This must be a reference type.</typeparam>
/// <param name="context"> The data context instance to be used by the repository. This 
/// parameter is required and must not be <see langword="null"/>.</param>
public abstract class RepositoryBase<TDataContext>(TDataContext context) : RepositoryBase
    where TDataContext : class
{
    /// <summary>
    /// Gets or sets the data context associated with this store.
    /// </summary>
    protected TDataContext Context { get; init; } = context;
}