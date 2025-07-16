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
using System.Text.Json;

using Xpandables.Net.Executions.Domains;
using Xpandables.Net.Executions.Tasks;

namespace Xpandables.Net.Repositories.Filters;

/// <summary>
/// Represents a filter for events that can be applied to a queryable
/// collection of events with specific entity.
/// </summary>
/// <typeparam name="TEventEntity">The type of the event entity.</typeparam>
/// <typeparam name="TEvent"> The type of the event.</typeparam>
public interface IEventFilter<TEventEntity, TEvent> : IEntityFilter<TEventEntity, TEvent>
    where TEventEntity : class, IEntityEvent
    where TEvent : class, IEvent
{
    /// <summary>
    /// Gets the event type to filter.
    /// </summary>
    /// <remarks>
    /// e.g. <see cref="IDomainEvent" />, <see cref="IIntegrationEvent" />
    /// or <see cref="ISnapshotEvent" />.
    /// </remarks>
    public Type EventType => typeof(TEvent);

    /// <summary>
    /// Gets the predicate expression used to filter event data.
    /// </summary>
    /// <remarks>Only supported by PostgreSQL.</remarks>
    Expression<Func<JsonDocument, bool>>? EventDataWhere { get; }

    IQueryable<TEvent> IEntityFilter<TEventEntity, TEvent>.Apply(IQueryable<TEventEntity> queryable) =>
        Apply(queryable);

    /// <summary>
    /// Applies the filter to the specified queryable collection of
    /// event entities.
    /// </summary>
    /// <param name="queryable">The queryable collection of event entities.</param>
    /// <returns>A queryable collection of filtered results.</returns>
    public new IQueryable<TEvent> Apply(IQueryable<TEventEntity> queryable)
    {
        ArgumentNullException.ThrowIfNull(queryable);
        IQueryable<TEventEntity> query = queryable;

        if (Where is not null)
        {
            query = query.Where(Where);
        }

        if (EventDataWhere is not null)
        {
            Expression<Func<TEventEntity, bool>> eventDataExpression =
                CreateEventDataExpression(EventDataWhere);

            query = query.Where(eventDataExpression);
        }

        if (OrderBy is not null)
        {
            query = OrderBy(query);
        }

        SetTotalCount(query);

        if (PageIndex > 0 && PageSize > 0)
        {
            query = query
                .Skip((PageIndex - 1) * PageSize)
                .Take(PageSize);
        }

        return query.Select(Selector);
    }

    private static Expression<Func<TEventEntity, bool>> CreateEventDataExpression(
        Expression<Func<JsonDocument, bool>> eventDataWhere)
    {
        // Create the property access expression: entity => entity.EventData
        Expression<Func<TEventEntity, JsonDocument>> eventDataSelector =
            entity => entity.EventData;

        // Use RepositoryExtensions.Compose to combine the expressions
        // This creates: entity => eventDataWhere(entity.EventData)
        return eventDataSelector.Compose(eventDataWhere);
    }
}