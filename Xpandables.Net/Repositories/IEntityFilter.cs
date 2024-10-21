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
using System.ComponentModel;
using System.Linq.Expressions;

namespace Xpandables.Net.Repositories;
/// <summary>
/// Represents a filter for entities with pagination support.
/// </summary>
public interface IEntityFilter
{
    /// <summary>
    /// Gets or sets the index of the page.
    /// </summary>
    ushort PageIndex { get; }

    /// <summary>
    /// Gets or sets the size of the page.
    /// </summary>
    ushort PageSize { get; }

    /// <summary>
    /// Applies the filter to the given queryable.
    /// </summary>
    /// <param name="queryable">The queryable to apply the filter to.</param>
    /// <returns>The filtered queryable.</returns>
    IQueryable Apply(IQueryable queryable);

    /// <summary>
    /// Fetches a collection of entities from the given queryable.
    /// </summary>
    /// <param name="queryable">The queryable to fetch entities from.</param>
    /// <returns>A collection of entities.</returns>
    public IEnumerable<IEntity> Fetch(IQueryable queryable)
        => Apply(queryable).OfType<IEntity>();

    /// <summary>
    /// Fetches a collection of entities asynchronously from the given queryable.
    /// </summary>
    /// <param name="queryable">The queryable to fetch entities from.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>An asynchronous collection of entities.</returns>
    public IAsyncEnumerable<IEntity> FetchAsync(
        IQueryable queryable,
        CancellationToken cancellationToken = default)
        => Apply(queryable).OfType<IEntity>().ToAsyncEnumerable();
}

/// <summary>
/// Represents a filter for entities with pagination support and specific 
/// selection criteria.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TResult">The type of the result.</typeparam>
public interface IEntityFilter<TEntity, TResult> : IEntityFilter
    where TEntity : class, IEntity
{
    /// <summary>
    /// Gets the selector expression for the entity.
    /// </summary>
    Expression<Func<TEntity, TResult>> Selector { get; }

    /// <summary>
    /// Gets the predicate expression for filtering entities.
    /// </summary>
    Expression<Func<TEntity, bool>>? Predicate { get; }

    /// <summary>
    /// Gets the function for ordering the entities.
    /// </summary>
    Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? OrderBy { get; }

    /// <summary>
    /// Fetches a collection of entities from the given queryable.
    /// </summary>
    /// <param name="queryable">The queryable to fetch entities from.</param>
    /// <returns>A collection of entities.</returns>
    public IEnumerable<TResult> Fetch(IQueryable<TEntity> queryable)
        => Apply(queryable);

    /// <summary>
    /// Fetches a collection of entities asynchronously from the given queryable.
    /// </summary>
    /// <param name="queryable">The queryable to fetch entities from.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>An asynchronous collection of entities.</returns>
    public IAsyncEnumerable<TResult> FetchAsync(
        IQueryable<TEntity> queryable,
        CancellationToken cancellationToken = default)
        => Apply(queryable).ToAsyncEnumerable();

    /// <summary>
    /// Applies the filter to the given queryable.
    /// </summary>
    /// <param name="queryable">The queryable to apply the filter to.</param>
    /// <returns>The filtered queryable.</returns>
    public IQueryable<TResult> Apply(IQueryable<TEntity> queryable)
    {
        IQueryable<TEntity> query = queryable;

        if (Predicate is not null)
        {
            query = query.Where(Predicate);
        }

        if (OrderBy is not null)
        {
            query = OrderBy(query);
        }

        if (PageIndex > 0 && PageSize > 0)
        {
            query = query
                .Skip((PageIndex - 1) * PageSize)
                .Take(PageSize);
        }

        return query.Select(Selector);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    IQueryable IEntityFilter.Apply(IQueryable queryable)
        => Apply(queryable.OfType<TEntity>());
}

/// <summary>
/// Represents a filter for entities with pagination support and specific 
/// selection criteria.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public interface IEntityFilter<TEntity> : IEntityFilter<TEntity, TEntity>
    where TEntity : class, IEntity
{ }
