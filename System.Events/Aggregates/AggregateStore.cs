/*******************************************************************************
 * Copyright (C) 2025 Kamersoft
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
using System.Events.Domain;

namespace System.Events.Aggregates;

/// <summary>
/// Provides an implementation of an aggregate store that loads and saves aggregates using an event store and manages
/// pending domain events.
/// </summary>
/// <remarks>This class is typically used in domain-driven design architectures to manage the persistence and
/// reconstruction of aggregates from event streams. It ensures that aggregates are loaded from their event history and
/// that uncommitted domain events are properly tracked and dispatched after saving.</remarks>
/// <typeparam name="TAggregate">The type of aggregate managed by the store. Must implement <see cref="IAggregate"/> and have a parameterless
/// constructor.</typeparam>
/// <param name="eventStore">The event store used to persist and retrieve aggregate events.</param>
/// <param name="domainEvents">The collection used to track and dispatch pending domain events after aggregates are saved.</param>
public sealed class AggregateStore<TAggregate>(
    IEventStore eventStore,
    IPendingDomainEventsBuffer domainEvents) : IAggregateStore<TAggregate>
    where TAggregate : class, IAggregate, IAggregateFactory<TAggregate>
{
    private readonly IEventStore _eventStore = eventStore;

    /// <inheritdoc />
    public async Task<TAggregate> LoadAsync(
        Guid streamId,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(streamId, Guid.Empty);

        ReadStreamRequest request = new()
        {
            StreamId = streamId,
            FromVersion = -1,
            MaxCount = int.MaxValue
        };

        var history = await _eventStore
            .ReadStreamAsync(request, cancellationToken)
            .Select(e => e.Event)
            .OfType<IDomainEvent>()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var aggregate = TAggregate.Initialize();

        aggregate.Replay(history);

        if (aggregate.IsEmpty)
        {
            throw new ValidationException(new ValidationResult(
                $"The {typeof(TAggregate).Name.SplitTypeName()} was not found.",
                [nameof(streamId)]), null, streamId);
        }

        return aggregate;
    }

    /// <inheritdoc />
    public async Task SaveAsync(
        TAggregate aggregate,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(aggregate);

        var pending = aggregate.DequeueUncommittedEvents();
        if (pending.Count == 0) return;

        // The aggregate.StreamVersion has already been advanced by pending.Count.
        // expectedVersion must reflect the persisted version before those events.
        var expectedVersion = aggregate.StreamVersion - pending.Count;

        AppendRequest request = new()
        {
            StreamId = aggregate.StreamId,
            Events = pending,
            ExpectedVersion = expectedVersion
        };

        await _eventStore.AppendToStreamAsync(request, cancellationToken).ConfigureAwait(false);

        domainEvents.AddRange(pending, aggregate.MarkEventsAsCommitted);
    }
}