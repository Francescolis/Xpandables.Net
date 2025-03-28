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

using System.ComponentModel.DataAnnotations;

using Xpandables.Net.Executions.Tasks;
using Xpandables.Net.Repositories;
using Xpandables.Net.Repositories.Filters;

namespace Xpandables.Net.Executions.Domains;

/// <summary>
/// Represents a store for aggregates that handles appending and peeking operations.
/// it uses a GUID as an identifier.
/// </summary>
/// <typeparam name="TAggregate">The type of the aggregate.</typeparam>
/// <remarks>
/// Initializes a new instance of the 
/// <see cref="AggregateStore{TAggregate}"/> class.
/// The <see cref="IUnitOfWork"/> must be registered with the key "Aggregate".
/// </remarks>
/// <param name="eventStore">The event store.</param>
/// <param name="publisher">The event publisher.</param>
public sealed class AggregateStore<TAggregate>(
    IEventStore eventStore,
    IPublisher publisher) :
    IAggregateStore<TAggregate>
    where TAggregate : class, IAggregate, new()
{
    private readonly IEventStore _eventStore = eventStore;
    private readonly IPublisher _publisher = publisher;

    /// <inheritdoc/>
    public async Task AppendAsync(
        TAggregate aggregate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            IReadOnlyCollection<IEventDomain> uncommittedEvents =
                aggregate.GetUncommittedEvents();

            if (uncommittedEvents.Count == 0)
            {
                return;
            }

            await _eventStore
                .AppendAsync(uncommittedEvents, cancellationToken)
                .ConfigureAwait(false);

            Task[] tasks = [.. uncommittedEvents
                .Select(async @event => await _publisher
                    .PublishAsync(@event, cancellationToken)
                    .ConfigureAwait(false))];

            await Task.WhenAll(tasks).ConfigureAwait(false);

            aggregate.MarkEventsAsCommitted();
        }
        catch (Exception exception)
            when (exception is not ValidationException and not InvalidOperationException)
        {
            throw new InvalidOperationException(
                "Unable to append the object. See inner exception for details.",
                exception);
        }
    }

    /// <inheritdoc/>
    public async Task<TAggregate> PeekAsync(
        Guid keyId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            IEventFilter filter = new EventEntityFilterDomain
            {
                Predicate = x => x.AggregateId == keyId,
                OrderBy = x => x.OrderBy(o => o.EventVersion)
            };

            TAggregate aggregate = new();

            List<IEventDomain> events = await _eventStore
                .FetchAsync(filter, cancellationToken)
                .OfType<IEventDomain>()
                .OrderBy(x => x.EventVersion)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            aggregate.LoadFromHistory(events);

            if (aggregate.IsEmpty)
            {
                throw new ValidationException(new ValidationResult(
                    "The object was not found.",
                    [nameof(keyId)]), null, keyId);
            }

            return aggregate;
        }
        catch (Exception exception)
            when (exception is not ValidationException and not InvalidOperationException)
        {
            throw new InvalidOperationException(
                "Unable to peek the object. See inner exception for details.",
                exception);
        }
    }
}
