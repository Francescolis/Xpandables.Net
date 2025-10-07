
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

using Xpandables.Net;
using Xpandables.Net.Events;

namespace Xpandables.Net.Events;

/// <summary>
/// Represents a store for managing aggregate by providing methods to append and resolve aggregates.
/// This class is designed for use with event sourcing and domain-driven design concepts.
/// </summary>
/// <typeparam name="TAggregate"></typeparam>
/// <param name="eventStore"></param>
/// <param name="domainEvents"></param>
public sealed class AggregateStore<TAggregate>(
    IEventStore eventStore,
    IPendingDomainEvents domainEvents) : IAggregateStore<TAggregate>
    where TAggregate : class, IAggregate, new()
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

        var aggregate = new TAggregate();

        aggregate.Replay(history);

        if (aggregate.IsEmpty)
        {
            throw new ValidationException(new ValidationResult(
                "The aggregate was not found.",
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

        await _eventStore.AppendAsync(request, cancellationToken).ConfigureAwait(false);

        domainEvents.AddRange(pending, aggregate.MarkEventsAsCommitted);
    }
}