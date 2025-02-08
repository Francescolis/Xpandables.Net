
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

using Microsoft.Extensions.Options;

using Xpandables.Net.Repositories.Filters;
using Xpandables.Net.States;

namespace Xpandables.Net.Events.Aggregates;

/// <summary>
/// Represents a store for aggregate snapshots.
/// </summary>
/// <typeparam name="TAggregate">The type of the aggregate.</typeparam>
public sealed class AggregateSnapshotStore<TAggregate>(
    IAggregateStore<TAggregate> aggregateStore,
    IEventStore eventStore,
    IOptions<EventOptions> options) :
    IAggregateStore<TAggregate>
    where TAggregate : class, IAggregate, IOriginator, new()
{
    private readonly IAggregateStore<TAggregate> _aggregateStore = aggregateStore;
    private readonly IEventStore _eventStore = eventStore;
    private readonly EventOptions _options = options.Value;

    /// <inheritdoc/>
    public async Task AppendAsync(
        TAggregate aggregate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (CanSnapshot(aggregate))
            {
                IMemento memento = aggregate.Save();

                EventSnapshot @event = new()
                {
                    EventId = Guid.CreateVersion7(),
                    EventVersion = aggregate.Version,
                    Memento = memento,
                    OwnerId = aggregate.KeyId
                };

                await _eventStore
                    .AppendAsync([@event], cancellationToken)
                    .ConfigureAwait(false);
            }

            await _aggregateStore
                .AppendAsync(aggregate, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception)
            when (exception is not InvalidOperationException)
        {
            throw new InvalidOperationException(
                "Unable to append the aggregate. See inner exception for details.",
                exception);
        }
    }

    /// <inheritdoc/>
    public async Task<TAggregate> PeekAsync(
        Guid keyId,
        CancellationToken cancellationToken = default)
    {
        if (!_options.IsSnapshotEnabled)
        {
            return await _aggregateStore
                .PeekAsync(keyId, cancellationToken)
                .ConfigureAwait(false);
        }

        try
        {
            IEventFilter filter = new EventEntityFilterSnapshot
            {
                Predicate = x => x.OwnerId == keyId,
                OrderBy = x => x.OrderByDescending(x => x.EventVersion),
                PageIndex = 1,
                PageSize = 1
            };

            IEventSnapshot? @event = await _eventStore
                .FetchAsync(filter, cancellationToken)
                .OfType<IEventSnapshot>()
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            TAggregate aggregate = new();

            if (@event is not null)
            {
                aggregate.Restore(@event.Memento);
            }
            else
            {
                return await _aggregateStore
                    .PeekAsync(keyId, cancellationToken)
                    .ConfigureAwait(false);
            }

            // because the snapshot is not always up to date, we need to fetch the events
            // after the snapshot version to get the latest events.

            filter = new EventEntityFilterDomain
            {
                Predicate = x => x.AggregateId == keyId
                    && x.EventVersion > aggregate.Version,
                OrderBy = x => x.OrderBy(x => x.EventVersion)
            };

            List<IEventDomain> events = await _eventStore
                .FetchAsync(filter, cancellationToken)
                .OfType<IEventDomain>()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            aggregate.LoadFromHistory(events);

            if (aggregate.IsEmpty)
            {
                throw new ValidationException(new ValidationResult(
                    "The aggregate was not found.",
                    [nameof(keyId)]), null, keyId);
            }

            return aggregate;
        }
        catch (Exception exception)
            when (exception is not InvalidOperationException)
        {
            throw new InvalidOperationException(
                "Unable to peek the aggregate. See inner exception for details.",
                exception);
        }
    }

    private bool CanSnapshot(IAggregate aggregate) =>
      _options.IsSnapshotEnabled
          && aggregate.Version % _options.SnapshotFrequency == 0
          && aggregate.Version >= _options.SnapshotFrequency;
}
