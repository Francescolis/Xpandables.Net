
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
using System.Text.Json;

using Xpandables.Net.Executions.Domains;
using Xpandables.Net.Repositories;

namespace Xpandables.Net.Repositories.Filters;

/// <summary>
/// Represents a filter for events that can be applied to a queryable 
/// collection of events.
/// </summary>
public interface IEventFilter : IEntityFilter
{
    /// <summary>
    /// Gets the event type to filter.
    /// </summary>
    /// <remarks>e.g. <see cref="IEventDomain"/>, <see cref="IEventIntegration"/>
    /// or <see cref="IEventSnapshot"/>.</remarks>
    Type EventType { get; }

    /// <summary>
    /// Gets the predicate expression used to filter event data.
    /// </summary>
    /// <remarks>Only supported by PostgreSQL.</remarks>
    Expression<Func<JsonDocument, bool>>? EventDataPredicate { get; }

    /// <summary>
    /// Asynchronously fetches event entities from the specified queryable collection.
    /// </summary>
    /// <param name="queryable">The queryable collection of entities.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An asynchronous enumerable of event entities.</returns>
    public IAsyncEnumerable<IEventEntity> FetchAsync(
        IQueryable queryable,
        CancellationToken cancellationToken = default) =>
        Apply(queryable).OfType<IEventEntity>().ToAsyncEnumerable();

    [EditorBrowsable(EditorBrowsableState.Never)]
    IAsyncEnumerable<TResult> IEntityFilter.FetchAsync<TResult>(
        IQueryable queryable,
        CancellationToken cancellationToken) =>
        FetchAsync(queryable, cancellationToken)
        .OfType<TResult>();
}

/// <summary>
/// Represents a filter for events that can be applied to a queryable 
/// collection of events with specific entity.
/// </summary>
/// <typeparam name="TEventEntity">The type of the event entity.</typeparam>
public interface IEventFilter<TEventEntity> :
    IEventFilter,
    IEntityFilter<TEventEntity>
    where TEventEntity : class, IEventEntity
{
    /// <summary>
    /// Applies the filter to the specified queryable collection of 
    /// event entities.
    /// </summary>
    /// <param name="queryable">The queryable collection of event entities.</param>
    /// <returns>A queryable collection of filtered results.</returns>
    public new IQueryable Apply(IQueryable queryable)
    {
        IQueryable<TEventEntity> query = (IQueryable<TEventEntity>)queryable;

        if (Predicate is not null)
        {
            query = query.Where(Predicate);
        }

        if (EventDataPredicate is not null)
        {
            Expression<Func<TEventEntity, bool>> expression =
                RepositoryExtensions.Compose<TEventEntity, JsonDocument, bool>(
                    x => x.EventData, EventDataPredicate);

            query = query.Where(expression);
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
        => Apply((IQueryable<TEventEntity>)queryable);
}