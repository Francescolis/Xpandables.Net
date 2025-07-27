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
namespace Xpandables.Net.Repositories;

/// <summary>
/// Represents a repository that provides read and write operations for entities.
/// </summary>
public interface IRepository : IAsyncDisposable
{
    /// <summary>
    /// Fetches results from the repository using a query builder function.
    /// This method allows full control over query composition including filtering, 
    /// ordering, including, pagination, and projection.
    /// <code>
    /// // Example usage:
    /// var results = await repository.FetchAsync&lt;User, object&gt;(
    ///     query => query
    ///         .Where(u => u.IsActive)
    ///         .Include(u => u.Profile)
    ///         .OrderBy(u => u.LastName)
    ///         .Select(u => new { u.Id, u.FirstName, u.LastName, u.Email })
    ///         .Skip(10)
    ///         .Take(20)
    /// ).ToListAsync();
    /// </code>
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="filter">A filter function that takes an IQueryable&lt;TEntity&gt; and returns an IQueryable&lt;TResult&gt;.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An asynchronous enumerable of the result type.</returns>
    IAsyncEnumerable<TResult> FetchAsync<TEntity, TResult>(
        Func<IQueryable<TEntity>, IQueryable<TResult>> filter,
        CancellationToken cancellationToken = default)
        where TEntity : class, IEntity;

    /// <summary>
    /// Inserts a collection of entities into the repository.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="entities">The entities to insert.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task InsertAsync<TEntity>(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default)
        where TEntity : class, IEntity;

    /// <summary>
    /// Asynchronously updates a collection of entities in the data store.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entities to update. Must implement <see cref="IEntity"/>.</typeparam>
    /// <param name="entities">The collection of entities to be updated. Cannot be null or empty.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. 
    /// The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous update operation.</returns>
    Task UpdateAsync<TEntity>(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default)
        where TEntity : class, IEntity;

    /// <summary>
    /// Deletes entities from the repository based on a filter.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="filter">The filter to apply to the entities to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task DeleteAsync<TEntity>(
        Func<IQueryable<TEntity>, IQueryable<TEntity>> filter,
        CancellationToken cancellationToken = default)
        where TEntity : class, IEntity;
}

/// <summary>
/// Defines a repository interface for data operations within a specified data context type.
/// </summary>
/// <typeparam name="TDataContext">The type of the data context within which the repository operates. Must be a reference type.</typeparam>
public interface IRepository<in TDataContext> : IRepository
    where TDataContext : class
{
}