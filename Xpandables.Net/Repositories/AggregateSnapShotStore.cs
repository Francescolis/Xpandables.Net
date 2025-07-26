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

using Xpandables.Net.Executions.Domains;
using Xpandables.Net.States;

namespace Xpandables.Net.Repositories;

/// <summary>
/// Represents a store for aggregate root snapshots.
/// </summary>
/// <typeparam name="TAggregateRoot">The type of the aggregate root.</typeparam>
public sealed class AggregateSnapShotStore<TAggregateRoot>(
    IAggregateStore<TAggregateRoot> aggregateStore,
    IUnitOfWorkEvent unitOfWork,
    IOptions<SnapShotOptions> options) :
    IAggregateStore<TAggregateRoot>
    where TAggregateRoot : Aggregate, IOriginator, new()
{
    private readonly IAggregateStore<TAggregateRoot> _aggregateStore = aggregateStore;
    private readonly IEventStore _eventStore = unitOfWork.GetEventStore<IEventStore>();
    private readonly SnapShotOptions _options = options.Value;

    /// <inheritdoc />
    public async Task AppendAsync(
        TAggregateRoot aggregate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (CanSnapshot(aggregate))
            {
                IMemento memento = aggregate.Save();

                SnapshotEvent snapshotEvent = new()
                {
                    EventId = Guid.CreateVersion7(),
                    EventVersion = aggregate.Version,
                    Memento = memento,
                    OwnerId = aggregate.KeyId
                };

                await _eventStore
                    .AppendAsync(snapshotEvent, cancellationToken)
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

    /// <inheritdoc />
    public async Task<TAggregateRoot> ResolveAsync(
        Guid keyId,
        CancellationToken cancellationToken = default)
    {
        if (!_options.IsSnapshotEnabled)
        {
            return await _aggregateStore
                .ResolveAsync(keyId, cancellationToken)
                .ConfigureAwait(false);
        }

        try
        {
            Func<IQueryable<EntitySnapshotEvent>, IQueryable<EntitySnapshotEvent>> filter = query =>
                query.Where(w => w.OwnerId == keyId)
                .OrderByDescending(o => o.EventVersion)
                .Take(1);

            ISnapshotEvent? @event = await _eventStore
                .FetchAsync(filter, cancellationToken)
                .AsEventsAsync(cancellationToken)
                .OfType<ISnapshotEvent>()
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            TAggregateRoot aggregateRoot = new();

            if (@event is not null)
            {
                aggregateRoot.Restore(@event.Memento);
            }
            else
            {
                return await _aggregateStore
                    .ResolveAsync(keyId, cancellationToken)
                    .ConfigureAwait(false);
            }

            // because the snapshot is not always up to date, we need to fetch the events
            // after the snapshot version to get the latest events.

            Func<IQueryable<EntityDomainEvent>, IQueryable<EntityDomainEvent>> domainFilterFunc = query =>
                query.Where(w => w.AggregateId == keyId && w.EventVersion > aggregateRoot.Version)
                .OrderBy(o => o.EventVersion);

            await foreach (IDomainEvent ev in _eventStore
                .FetchAsync(domainFilterFunc, cancellationToken)
                .AsEventsAsync(cancellationToken)
                .OfType<IDomainEvent>()
                .ConfigureAwait(false))
            {
                aggregateRoot.LoadFromHistory(ev);
            }

            if (aggregateRoot.IsEmpty)
            {
                throw new ValidationException(new ValidationResult(
                    "The aggregate was not found.",
                    [nameof(keyId)]), null, keyId);
            }

            return aggregateRoot;
        }
        catch (Exception exception)
            when (exception is not InvalidOperationException)
        {
            throw new InvalidOperationException(
                "Unable to peek the aggregate. See inner exception for details.",
                exception);
        }
    }

    private bool CanSnapshot(Aggregate aggregate) =>
        _options.IsSnapshotEnabled
        && aggregate.Version % _options.SnapshotFrequency == 0
        && aggregate.Version >= _options.SnapshotFrequency;
}