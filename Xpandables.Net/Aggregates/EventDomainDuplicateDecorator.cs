﻿/*******************************************************************************
 * Copyright (C) 2023 Francis-Black EWANE
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

using Xpandables.Net.Operations;
using Xpandables.Net.Primitives;

namespace Xpandables.Net.Aggregates;

/// <summary>
/// A marker interface that defines a domain event that cannot be duplicated.
/// </summary>
public interface IEventDomainDuplicate
{
    /// <summary>
    /// Gets the criteria to check for duplicate domain events.
    /// </summary>
    Expression<Func<JsonDocument, bool>>? Criteria { get; }

    /// <summary>
    /// Gets the operation result to return when the domain event is duplicated.
    /// </summary>
    IOperationResult OnFailure { get; }
}

/// <summary>
/// Defines a domain event handler that checks for duplicate domain events.
/// </summary>
/// <typeparam name="TEventDomain"></typeparam>
/// <typeparam name="TAggragateId"></typeparam>
/// <param name="eventStore"></param>
/// <param name="decoratee"></param>
public sealed class EventDomainDuplicateDecorator<TEventDomain, TAggragateId>(
    IDomainEventStore eventStore,
    IDomainEventHandler<TEventDomain, TAggragateId> decoratee) :
    IDomainEventHandler<TEventDomain, TAggragateId>
    where TEventDomain : notnull, IEventDomain<TAggragateId>, IEventDomainDuplicate
    where TAggragateId : struct, IAggregateId<TAggragateId>
{
    /// <summary>
    /// Checks for duplicate domain events before handling the domain event.
    /// </summary>
    /// <param name="event">The domain event to handle.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async ValueTask<IOperationResult> HandleAsync(
        TEventDomain @event,
        CancellationToken cancellationToken = default)
    {
        IEventFilter eventFilter = new EventFilter()
        {
            AggregateIdTypeName = typeof(TAggragateId).Name,
            EventTypeName = @event.GetType().Name,
            Pagination = Pagination.With(0, 1),
            DataCriteria = @event.Criteria
        };

        return eventStore
            .ReadAsync<TAggragateId>(eventFilter, cancellationToken)
            .ToBlockingEnumerable(cancellationToken)
            .Any() switch
        {
            true => @event.OnFailure,
            _ => await decoratee
                .HandleAsync(@event, cancellationToken)
                .ConfigureAwait(false)
        };

    }
}
