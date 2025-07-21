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
using System.Linq.Expressions;

using Xpandables.Net.Repositories.Filters;

namespace Xpandables.Net.Repositories;

/// <summary>
/// Represents a repository that provides read and write operations for entities.
/// </summary>
public interface IRepository : IAsyncDisposable
{
    /// <summary>
    /// Fetches entities from the repository based on the specified filter.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="filter">The filter to apply to the entities.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An asynchronous enumerable of the result type.</returns>
    IAsyncEnumerable<TResult> FetchAsync<TEntity, TResult>(
        IEntityFilter<TEntity, TResult> filter,
        CancellationToken cancellationToken = default)
        where TEntity : class, IEntity;

    /// <summary>
    /// Fetches projected results from the repository using a selector expression.
    /// This method is particularly useful for anonymous type projections.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TResult">The type of the result (can be anonymous).</typeparam>
    /// <param name="selector">The projection expression to apply to entities.</param>
    /// <param name="where">The predicate to filter entities.</param>
    /// <param name="orderBy">Optional ordering function.</param>
    /// <param name="includes">Optional function to include related entities.</param>
    /// <param name="pageIndex">Optional page index (1-based, 0 to disable pagination).</param>
    /// <param name="pageSize">Optional page size (0 to disable pagination).</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An asynchronous enumerable of the result type.</returns>
    IAsyncEnumerable<TResult> FetchAsync<TEntity, TResult>(
        Expression<Func<TEntity, bool>> where,
        Expression<Func<TEntity, TResult>> selector,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? includes = null,
        ushort pageIndex = 0,
        ushort pageSize = 0,
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
        IEntityFilter<TEntity> filter,
        CancellationToken cancellationToken = default)
        where TEntity : class, IEntity;
}
