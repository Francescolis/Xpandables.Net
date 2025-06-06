﻿/*******************************************************************************
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

using Xpandables.Net.Repositories;

namespace Xpandables.Net.Repositories.Filters;
/// <summary>
/// Represents a filter for entities with pagination support.
/// </summary>
public interface IEntityFilter
{
    /// <summary>
    /// A static boolean property that indicates whether to force a total count when applying the filter.
    /// If set to true, the total count will be calculated even if the queryable does not support it.
    /// </summary>
    public static bool ForceTotalCount { get; set; } = true;

    /// <summary>
    /// Gets or sets the index of the page.
    /// </summary>
    ushort PageIndex { get; }

    /// <summary>
    /// Gets or sets the size of the page.
    /// </summary>
    ushort PageSize { get; }

    /// <summary>
    /// Gets the number of elements in a collection. Returns an integer representing the total count.
    /// </summary>
    /// <remarks>This value is set when the filter is applied to a queryable.
    /// You can control the total count calculation using the <see cref="ForceTotalCount"/> property.</remarks>
    int TotalCount { get; set; }

    /// <summary>
    /// Applies the filter to the given queryable.
    /// </summary>
    /// <param name="queryable">The queryable to apply the filter to.</param>
    /// <returns>The filtered queryable.</returns>
    IQueryable Apply(IQueryable queryable);

    /// <summary>
    /// Fetches a collection of results from the given queryable.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="queryable">The queryable to fetch results from.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>An asynchronous collection of results.</returns>
    public IAsyncEnumerable<TResult> FetchAsync<TResult>(
        IQueryable queryable,
        CancellationToken cancellationToken = default) =>
        Apply(queryable).OfType<TResult>().ToAsyncEnumerable();
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
    /// Applies the filter to the given queryable.
    /// </summary>
    /// <param name="queryable">The queryable to apply the filter to.</param>
    /// <returns>The filtered queryable.</returns>
    public new IQueryable Apply(IQueryable queryable)
    {
        IQueryable<TEntity> query = (IQueryable<TEntity>)queryable;

        if (Predicate is not null)
        {
            query = query.Where(Predicate);
        }

        if (OrderBy is not null)
        {
            query = OrderBy(query);
        }

        if (query.TryGetNonEnumeratedCount(out int count))
        {
            TotalCount = count;
        }
        else
        {
            if (ForceTotalCount)
                TotalCount = query.Count();
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
        => Apply((IQueryable<TEntity>)queryable);
}

/// <summary>
/// Represents a filter for entities with pagination support and specific 
/// selection criteria.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public interface IEntityFilter<TEntity> : IEntityFilter<TEntity, TEntity>
    where TEntity : class, IEntity
{ }
