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
using System.Diagnostics.CodeAnalysis;
using System.Events.Domain;
using System.Runtime.CompilerServices;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace System.Events.Data;

/// <summary>
/// Provides an implementation of an event store that persists domain events and snapshots using the specified data
/// context.
/// </summary>
/// <remarks>This class enables appending, reading, and managing event streams and snapshots in an event-sourced
/// system. It is designed to work with a specific data context, allowing integration with various storage backends that
/// support the DataContext abstraction. All operations are asynchronous and support cancellation via CancellationToken.
/// This class is not thread-safe; concurrent usage should be managed externally if required.</remarks>
public sealed class EventStore<[DynamicallyAccessedMembers(EntityEvent.DynamicallyAccessedMemberTypes)] TEntityEventDomain,
    [DynamicallyAccessedMembers(EntityEvent.DynamicallyAccessedMemberTypes)] TEntityEventSnapshot> : IEventStore
    where TEntityEventDomain : class, IEntityEventDomain
    where TEntityEventSnapshot : class, IEntityEventSnapshot
{
    private readonly EventDataContext _db;
    private readonly EventDataContext _outbox;
    private readonly IEventConverterFactory _converterFactory;
    private readonly IEventConverter<TEntityEventDomain, IDomainEvent> _domainConveter;
    private readonly IEventConverter<TEntityEventSnapshot, ISnapshotEvent> _snapshotConverter;

    /// <summary>
    /// Initializes a new instance of the EventStore class with the specified data context factories and event converter
    /// factory.
    /// </summary>
    /// <param name="eventStoreDataContextFactory">The factory used to create data contexts for accessing the event store database. Cannot be null.</param>
    /// <param name="outboxStoreDataContextFactory">The factory used to create data contexts for accessing the outbox store. Cannot be null.</param>
    /// <param name="converterFactory">The factory used to obtain event converters for domain and snapshot events. Cannot be null.</param>
    /// <exception cref="ArgumentNullException">Thrown if converterFactory is null.</exception>
    public EventStore(
        IEventStoreDataContextFactory eventStoreDataContextFactory,
        IOutboxStoreDataContextFactory outboxStoreDataContextFactory,
        IEventConverterFactory converterFactory)
    {
        ArgumentNullException.ThrowIfNull(eventStoreDataContextFactory);
        ArgumentNullException.ThrowIfNull(outboxStoreDataContextFactory);
        ArgumentNullException.ThrowIfNull(converterFactory);

        _db = eventStoreDataContextFactory.Create();
        _outbox = outboxStoreDataContextFactory.Create();

        _converterFactory = converterFactory;
        _domainConveter = converterFactory.GetDomainEventConverter<TEntityEventDomain>();
        _snapshotConverter = converterFactory.GetSnapshotEventConverter<TEntityEventSnapshot>();
    }

    ///<inheritdoc/>
    public async Task<AppendResult> AppendToStreamAsync(
        AppendRequest request,
        CancellationToken cancellationToken = default)
    {
        var batch = request.Events.OfType<IDomainEvent>().ToArray();
        if (batch.Length == 0)
        {
            return AppendResult.Create(request.ExpectedVersion.GetValueOrDefault() + 1, request.ExpectedVersion.GetValueOrDefault());
        }

        long current = await GetStreamVersionCoreAsync(request.StreamId, cancellationToken).ConfigureAwait(false);
        if (request.ExpectedVersion.HasValue && current != request.ExpectedVersion)
        {
            // You can choose to throw early, or let DB uniqueness enforce at commit. Early throw is helpful.
            throw new InvalidOperationException(
                $"Concurrency violation for aggregate {request.StreamId}. " +
                $"Expected version {request.ExpectedVersion} but found {current}.");
        }

        var entities = new List<TEntityEventDomain>(capacity: batch.Length);
        long next = request.ExpectedVersion.GetValueOrDefault();

        foreach (var @event in batch)
        {
            next++;

            var nextEvent = @event
                .WithStreamId(request.StreamId)
                .WithStreamVersion(next)
                .WithStreamName(@event.StreamName);

            var entity = _domainConveter.ConvertEventToEntity(nextEvent, _converterFactory.ConverterContext);
            entities.Add(entity);
        }

        await _db.Set<TEntityEventDomain>().AddRangeAsync(entities, cancellationToken).ConfigureAwait(false);

        Guid[] guids = [.. entities.ConvertAll(e => e.StreamId)];
        return AppendResult.Create(guids, next, request.ExpectedVersion.GetValueOrDefault());
        // defer SaveChanges to FlushEventsAsync
    }

    ///<inheritdoc/>
    public async Task AppendSnapshotAsync(
        ISnapshotEvent snapshotEvent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(snapshotEvent);

        var entity = _snapshotConverter.ConvertEventToEntity(snapshotEvent, _converterFactory.ConverterContext);

        await _db.AddAsync(entity, cancellationToken).ConfigureAwait(false);
        // defer SaveChanges to FlushEventsAsync
    }

    ///<inheritdoc/>
    public async Task DeleteStreamAsync(
        DeleteStreamRequest request,
        CancellationToken cancellationToken = default)
    {
        var events = await _db.Set<TEntityEventDomain>()
            .Where(e => e.StreamId == request.StreamId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (events.Count == 0)
        {
            return;
        }

        if (request.HardDelete)
        {
            // Hard delete: permanently remove events from the database
            _db.RemoveRange(events);
        }
        else
        {
            // Soft delete: mark events as deleted using EntityStatus.DELETED
            foreach (var entity in events)
            {
                entity.SetStatus(EventStatus.DELETED);
            }
            _db.UpdateRange(events);
        }

        // defer SaveChanges to FlushEventsAsync
    }

    ///<inheritdoc/>
    public async Task<EnvelopeResult?> GetLatestSnapshotAsync(
        Guid ownerId,
        CancellationToken cancellationToken = default)
    {
        var last = await _db.Set<TEntityEventSnapshot>()
            .AsNoTracking()
            .Where(e => e.OwnerId == ownerId)
            .OrderByDescending(e => e.Sequence)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (last == null)
            return null;

        return new EnvelopeResult
        {
            Event = _snapshotConverter.ConvertEntityToEvent(last, _converterFactory.ConverterContext),
            EventId = last.KeyId,
            EventName = last.EventName,
            GlobalPosition = last.Sequence,
            OccurredOn = last.CreatedOn,
            StreamId = last.OwnerId,
            StreamName = null,
            StreamVersion = last.Sequence
        };
    }

    ///<inheritdoc/>
    public async Task<long> GetStreamVersionAsync(Guid streamId, CancellationToken cancellationToken = default) =>
        await GetStreamVersionCoreAsync(streamId, cancellationToken).ConfigureAwait(false);

    ///<inheritdoc/>
    public async IAsyncEnumerable<EnvelopeResult> ReadAllStreamsAsync(
        ReadAllStreamsRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var query = _db.Set<TEntityEventDomain>()
            .AsNoTracking()
            .Where(e => e.Sequence > request.FromPosition)
            .OrderBy(e => e.Sequence)
            .Take(request.MaxCount);

        await foreach (var entity in query.AsAsyncEnumerable().ConfigureAwait(false))
        {
            yield return new EnvelopeResult
            {
                Event = _domainConveter.ConvertEntityToEvent(entity, _converterFactory.ConverterContext),
                EventId = entity.KeyId,
                EventName = entity.EventName,
                GlobalPosition = entity.Sequence,
                OccurredOn = entity.CreatedOn,
                StreamId = entity.StreamId,
                StreamName = entity.StreamName,
                StreamVersion = entity.StreamVersion,
                CausationId = entity.CausationId,
                CorrelationId = entity.CorrelationId
            };
        }
    }

    ///<inheritdoc/>
    public async IAsyncEnumerable<EnvelopeResult> ReadStreamAsync(
        ReadStreamRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var query = _db.Set<TEntityEventDomain>()
            .AsNoTracking()
            .Where(e => e.StreamId == request.StreamId && e.StreamVersion > request.FromVersion) // exclusive lower bound
            .OrderBy(e => e.StreamVersion)
            .Take(request.MaxCount);

        await foreach (var entity in query.AsAsyncEnumerable().ConfigureAwait(false))
        {
            yield return new EnvelopeResult
            {
                Event = _domainConveter.ConvertEntityToEvent(entity, _converterFactory.ConverterContext),
                EventId = entity.KeyId,
                EventName = entity.EventName,
                GlobalPosition = entity.Sequence,
                OccurredOn = entity.CreatedOn,
                StreamId = entity.StreamId,
                StreamName = entity.StreamName,
                StreamVersion = entity.StreamVersion,
                CausationId = entity.CausationId,
                CorrelationId = entity.CorrelationId
            };
        }
    }

    ///<inheritdoc/>
    public async Task<bool> StreamExistsAsync(Guid streamId, CancellationToken cancellationToken = default)
    {
        return await _db.Set<TEntityEventDomain>()
            .AsNoTracking()
            .AnyAsync(e => e.StreamId == streamId, cancellationToken)
            .ConfigureAwait(false);
    }

    ///<inheritdoc/>
    public async Task TruncateStreamAsync(
        TruncateStreamRequest request,
        CancellationToken cancellationToken = default)
    {
        // Get the current stream version to determine which events exist
        var currentVersion = await GetStreamVersionCoreAsync(request.StreamId, cancellationToken)
            .ConfigureAwait(false);

        if (currentVersion == -1)
        {
            // Stream doesn't exist
            return;
        }

        // Remove all events from this stream that are before the specified version
        // TruncateBeforeVersion means: remove all events with StreamVersion < TruncateBeforeVersion
        var events = await _db.Set<TEntityEventDomain>()
            .Where(e => e.StreamId == request.StreamId && e.StreamVersion < request.TruncateBeforeVersion)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (events.Count > 0)
        {
            _db.RemoveRange(events);
        }

        // defer SaveChanges to FlushEventsAsync
    }

    ///<inheritdoc/>
    public IAsyncDisposable SubscribeToStream(
        SubscribeToStreamRequest request,
        CancellationToken cancellationToken = default)
    {
        return new StreamSubscription(_db, request, _converterFactory, _domainConveter, cancellationToken);
    }

    ///<inheritdoc/>
    public IAsyncDisposable SubscribeToAllStreams(
        SubscribeToAllStreamsRequest request,
        CancellationToken cancellationToken = default)
    {
        return new AllStreamsSubscription(_db, request, _domainConveter, _converterFactory, cancellationToken);
    }

    ///<inheritdoc/>
    public async Task FlushEventsAsync(CancellationToken cancellationToken = default)
    {
        IExecutionStrategy strategy = _db.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _db.Database
                .BeginTransactionAsync(cancellationToken)
                .ConfigureAwait(false);

            try
            {
                var result = await _db
                    .SaveChangesAsync(cancellationToken)
                    .ConfigureAwait(false);

                await _outbox
                    .SaveChangesAsync(cancellationToken)
                    .ConfigureAwait(false);

                await transaction
                    .CommitAsync(cancellationToken)
                    .ConfigureAwait(false);

                return result;
            }
            catch
            {
                await transaction
                    .RollbackAsync(cancellationToken)
                    .ConfigureAwait(false);

                throw;
            }
        })
            .ConfigureAwait(false);

    }
    private async Task<long> GetStreamVersionCoreAsync(
        Guid streamId, CancellationToken cancellationToken)
    {
        var last = await _db.Set<TEntityEventDomain>()
            .AsNoTracking()
            .Where(e => e.StreamId == streamId)
            .OrderByDescending(e => e.StreamVersion)
            .Select(e => (long?)e.StreamVersion)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        return last ?? -1;
    }

    // Subscription implementation for a single stream
    private sealed class StreamSubscription : IAsyncDisposable
    {
        private readonly EventDataContext _context;
        private readonly SubscribeToStreamRequest _request;
        private readonly IEventConverterFactory _converterFactory;
        private readonly IEventConverter<TEntityEventDomain, IDomainEvent> _domainConverter;
        private readonly CancellationTokenSource _cts;
        private readonly Task _subscriptionTask;

        public StreamSubscription(
            EventDataContext context,
            SubscribeToStreamRequest request,
            IEventConverterFactory converterFactory,
            IEventConverter<TEntityEventDomain, IDomainEvent> domainConverter,
            CancellationToken cancellationToken)
        {
            _context = context;
            _request = request;
            _converterFactory = converterFactory;
            _domainConverter = domainConverter;
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _subscriptionTask = RunSubscriptionAsync();
        }

        private async Task RunSubscriptionAsync()
        {
            long lastProcessedVersion = -1;

            try
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    var events = await _context.Set<TEntityEventDomain>()
                        .AsNoTracking()
                        .Where(e => e.StreamId == _request.StreamId && e.StreamVersion > lastProcessedVersion)
                        .OrderBy(e => e.StreamVersion)
                        .Take(100) // Process in batches
                        .ToListAsync(_cts.Token)
                        .ConfigureAwait(false);

                    foreach (var entity in events)
                    {
                        var domainEvent = _domainConverter.ConvertEntityToEvent(entity, _converterFactory.ConverterContext);
                        await _request.OnEvent(domainEvent).ConfigureAwait(false);
                        lastProcessedVersion = entity.StreamVersion;
                    }

                    // Wait before polling again using the configured polling interval
                    if (events.Count == 0)
                    {
                        await Task.Delay(_request.PollingInterval, _cts.Token).ConfigureAwait(false);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when disposed
            }
        }

        public async ValueTask DisposeAsync()
        {
            await _cts.CancelAsync().ConfigureAwait(false);
            try
            {
                await _subscriptionTask.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Expected
            }
            _cts.Dispose();
        }
    }

    // Subscription implementation for all streams
    private sealed class AllStreamsSubscription : IAsyncDisposable
    {
        private readonly EventDataContext _context;
        private readonly SubscribeToAllStreamsRequest _request;
        private readonly IEventConverter<TEntityEventDomain, IDomainEvent> _domainConverter;
        private readonly IEventConverterFactory _converterFactory;
        private readonly CancellationTokenSource _cts;
        private readonly Task _subscriptionTask;

        public AllStreamsSubscription(
            EventDataContext context,
            SubscribeToAllStreamsRequest request,
            IEventConverter<TEntityEventDomain, IDomainEvent> domainConverter,
            IEventConverterFactory converterFactory,
            CancellationToken cancellationToken)
        {
            _context = context;
            _request = request;
            _domainConverter = domainConverter;
            _converterFactory = converterFactory;
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _subscriptionTask = RunSubscriptionAsync();
        }

        private async Task RunSubscriptionAsync()
        {
            long lastProcessedSequence = 0;

            try
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    var events = await _context.Set<TEntityEventDomain>()
                        .AsNoTracking()
                        .Where(e => e.Sequence > lastProcessedSequence)
                        .OrderBy(e => e.Sequence)
                        .Take(100) // Process in batches
                        .ToListAsync(_cts.Token)
                        .ConfigureAwait(false);

                    foreach (var entity in events)
                    {
                        var domainEvent = _domainConverter.ConvertEntityToEvent(entity, _converterFactory.ConverterContext);
                        await _request.OnEvent(domainEvent).ConfigureAwait(false);
                        lastProcessedSequence = entity.Sequence;
                    }

                    // Wait before polling again using the configured polling interval
                    if (events.Count == 0)
                    {
                        await Task.Delay(_request.PollingInterval, _cts.Token).ConfigureAwait(false);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when disposed
            }
        }

        public async ValueTask DisposeAsync()
        {
            await _cts.CancelAsync().ConfigureAwait(false);
            try
            {
                await _subscriptionTask.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Expected
            }
            _cts.Dispose();
        }
    }
}
