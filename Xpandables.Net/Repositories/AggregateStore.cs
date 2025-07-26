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

using System.ComponentModel.DataAnnotations;

using Xpandables.Net.Executions.Domains;
using Xpandables.Net.Executions.Tasks;

namespace Xpandables.Net.Repositories;

/// <summary>
/// Represents a store for managing aggregate by providing methods to append and resolve aggregates.
/// This class is designed for use with event sourcing and domain-driven design concepts.
/// </summary>
/// <typeparam name="TAggregate">
/// The type of the aggregate, which must inherit from <see cref="Aggregate"/> and have a parameterless constructor.
/// </typeparam>
/// <remarks>
/// This class depends on an implementation of <see cref="IEventStore"/> for storing events and <see cref="IPublisher"/> for publishing those events.
/// An <see cref="IUnitOfWork"/> must be registered with the key "Aggregate" to ensure proper transactional support.
/// </remarks>
public sealed class AggregateStore<TAggregate>(
    IEventStore eventStore,
    IPublisher publisher) :
    IAggregateStore<TAggregate>
    where TAggregate : Aggregate, new()
{
    private readonly IEventStore _eventStore = eventStore;
    private readonly IPublisher _publisher = publisher;

    /// <inheritdoc />
    public async Task AppendAsync(
        TAggregate aggregate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            IReadOnlyCollection<IDomainEvent> uncommittedEvents =
                aggregate.GetUncommittedEvents();

            if (uncommittedEvents.Count == 0)
            {
                return;
            }

            await _eventStore
                .AppendAsync(uncommittedEvents, cancellationToken)
                .ConfigureAwait(false);

            Task[] tasks =
            [
                .. uncommittedEvents
                    .Select(async @event => await _publisher
                        .PublishAsync(@event, cancellationToken)
                        .ConfigureAwait(false))
            ];

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

    /// <inheritdoc />
    public async Task<TAggregate> ResolveAsync(
        Guid keyId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Func<IQueryable<EntityDomainEvent>, IQueryable<EntityDomainEvent>> domainFilterFunc = query =>
                query.Where(w => w.AggregateId == keyId)
                    .OrderBy(o => o.EventVersion);

            TAggregate aggregate = new();

            await foreach (IDomainEvent @event in _eventStore
                .FetchAsync(domainFilterFunc, cancellationToken)
                .AsEventsAsync(cancellationToken)
                .OfType<IDomainEvent>()
                .OrderBy(x => x.EventVersion)
                .ConfigureAwait(false))
            {
                aggregate.LoadFromHistory(@event);
            }

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