
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
using System.Runtime.CompilerServices;

using Microsoft.EntityFrameworkCore;

using Xpandables.Net.Events;
using Xpandables.Net.Repositories;
using Xpandables.Net.Repositories.Converters;
using Xpandables.Net.Text;

namespace Xpandables.Net.Repositories;

/// <summary>
/// EF Core-backed event store.
/// - Stream appends do not call SaveChanges to allow unit-of-work to commit atomically.
/// - Reads are immediate and use AsNoTracking.
/// </summary>
public sealed class EventStore<TDataContext>(TDataContext context) : AsyncDisposable, IEventStore
    where TDataContext : DataContext
{
    private readonly TDataContext _db = context;

    /// <inheritdoc/>
    public async Task<AppendResult> AppendToStreamAsync(
        Guid aggregateId,
        string aggregateName,
        long expectedVersion,
        IEnumerable<IDomainEvent> events,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(events);

        var batch = events as IDomainEvent[] ?? [.. events];
        if (batch.Length == 0)
        {
            return AppendResult.Create(expectedVersion + 1, expectedVersion);
        }

        long current = await GetStreamVersionCoreAsync(aggregateId, cancellationToken).ConfigureAwait(false);
        if (current != expectedVersion)
        {
            // You can choose to throw early, or let DB uniqueness enforce at commit. Early throw is helpful.
            throw new InvalidOperationException(
                $"Concurrency violation for aggregate {aggregateId}. " +
                $"Expected version {expectedVersion} but found {current}.");
        }

        var converter = EventConverter.GetConverterFor<IDomainEvent>();
        var entities = new List<EntityDomainEvent>(capacity: batch.Length);
        long next = expectedVersion;

        foreach (var @event in batch)
        {
            next++;

            var nextEvent = @event
                .WithAggregateId(aggregateId)
                .WithStreamVersion(next)
                .WithAggregateName(aggregateName);

            var entity = (EntityDomainEvent)converter.ConvertTo(nextEvent, DefaultSerializerOptions.Defaults);

            entities.Add(entity);
        }

        await _db.AddRangeAsync(entities, cancellationToken).ConfigureAwait(false);
        // Defer SaveChanges to the UnitOfWork

        Guid[] ids = [.. entities.ConvertAll(e => e.KeyId)];
        return AppendResult.Create(ids, expectedVersion + 1, next);
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<EventEnvelope> ReadStreamAsync(
        Guid aggregateId,
        long fromVersion = -1,
        int maxCount = int.MaxValue,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(fromVersion, -1);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxCount);

        var query = _db.Set<EntityDomainEvent>()
            .AsNoTracking()
            .Where(e => e.AggregateId == aggregateId && e.StreamVersion > fromVersion) // exclusive lower bound
            .OrderBy(e => e.StreamVersion)
            .Take(maxCount);

        await foreach (var entity in query.AsAsyncEnumerable().WithCancellation(cancellationToken))
        {
            var @event = EventConverter.ConvertFromCached(entity, DefaultSerializerOptions.Defaults);
            yield return new EventEnvelope
            {
                EventId = entity.KeyId,
                EventType = entity.Name,
                EventFullName = entity.FullName,
                OccurredOn = entity.CreatedOn,
                Event = @event,
                GlobalPosition = entity.Sequence,
                AggregateId = entity.AggregateId,
                AggregateName = entity.AggregateName,
                StreamVersion = entity.StreamVersion
            };
        }
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<EventEnvelope> ReadAllAsync(
        long fromPosition = 0,
        int maxCount = 4096,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(fromPosition);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxCount);

        var query = _db.Set<EntityDomainEvent>()
            .AsNoTracking()
            .Where(e => e.Sequence > fromPosition)
            .OrderBy(e => e.Sequence)
            .Take(maxCount);

        await foreach (var entity in query.AsAsyncEnumerable().WithCancellation(cancellationToken))
        {
            var @event = EventConverter.ConvertFromCached(entity, DefaultSerializerOptions.Defaults);

            Guid? aggregateId = null;
            string? aggregateName = null;
            long? streamVersion = null;

            if (entity is EntityDomainEvent de)
            {
                aggregateId = de.AggregateId;
                aggregateName = de.AggregateName;
                streamVersion = de.StreamVersion;
            }

            yield return new EventEnvelope
            {
                EventId = entity.KeyId,
                EventType = entity.Name,
                EventFullName = entity.FullName,
                OccurredOn = entity.CreatedOn,
                Event = @event,
                GlobalPosition = entity.Sequence,
                AggregateId = aggregateId,
                AggregateName = aggregateName,
                StreamVersion = streamVersion
            };
        }
    }

    /// <inheritdoc/>
    public Task<long> GetStreamVersionAsync(
        Guid aggregateId, CancellationToken cancellationToken = default) =>
        GetStreamVersionCoreAsync(aggregateId, cancellationToken);

    /// <inheritdoc/>
    public async Task AppendSnapshotAsync(
        Guid aggregateId, ISnapshotEvent snapshot, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        var converter = EventConverter.GetConverterFor<ISnapshotEvent>();

        snapshot.WithOwnerId(aggregateId);
        var entity = (EntitySnapshotEvent)converter.ConvertTo(snapshot, DefaultSerializerOptions.Defaults);

        await _db.AddAsync(entity, cancellationToken).ConfigureAwait(false);
        // Defer SaveChanges to the UnitOfWork
    }

    /// <inheritdoc/>
    public async Task<EventEnvelope?> ReadLatestSnapshotAsync(
        Guid aggregateId, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Set<EntitySnapshotEvent>()
            .AsNoTracking()
            .Where(e => e.OwnerId == aggregateId)
            .OrderByDescending(e => e.Sequence)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (entity is null) return null;

        var @event = EventConverter.ConvertFromCached(entity, DefaultSerializerOptions.Defaults);
        return new EventEnvelope
        {
            EventId = entity.KeyId,
            EventType = entity.Name,
            EventFullName = entity.FullName,
            OccurredOn = entity.CreatedOn,
            Event = @event,
            GlobalPosition = entity.Sequence,
            AggregateId = aggregateId,
            AggregateName = null,
            StreamVersion = null
        };
    }

    private async Task<long> GetStreamVersionCoreAsync(
        Guid aggregateId, CancellationToken cancellationToken)
    {
        var last = await _db.Set<EntityDomainEvent>()
            .AsNoTracking()
            .Where(e => e.AggregateId == aggregateId)
            .OrderByDescending(e => e.StreamVersion)
            .Select(e => (long?)e.StreamVersion)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        return last ?? -1;
    }
}