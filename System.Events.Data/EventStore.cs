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
using System.Entities;
using System.Entities.Data;
using System.Events.Domain;
using System.Runtime.CompilerServices;

namespace System.Events.Data;

/// <summary>
/// Provides an ADO.NET implementation of an event store that persists domain events and snapshots.
/// </summary>
/// <remarks>
/// <para>
/// This class enables appending, reading, and managing event streams and snapshots in an event-sourced
/// system using raw ADO.NET (not Entity Framework Core).
/// </para>
/// <para>
/// All operations are asynchronous and support cancellation via CancellationToken.
/// This class is not thread-safe; concurrent usage should be managed externally if required.
/// </para>
/// </remarks>
[RequiresDynamicCode("Expression compilation requires dynamic code generation.")]
public sealed class EventStore<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TEntityEventDomain,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TEntityEventSnapshot> : IEventStore
    where TEntityEventDomain : class, IEntityEventDomain
    where TEntityEventSnapshot : class, IEntityEventSnapshot
{
    private readonly IDataRepository<TEntityEventDomain> _domainRepository;
    private readonly IDataRepository<TEntityEventSnapshot> _snapshotRepository;
    private readonly IDataUnitOfWork _unitOfWork;
    private readonly IEventConverterFactory _converterFactory;
    private readonly IEventConverter<TEntityEventDomain, IDomainEvent> _domainConverter;
    private readonly IEventConverter<TEntityEventSnapshot, ISnapshotEvent> _snapshotConverter;

    /// <summary>
    /// Initializes a new instance of the DataEventStore class.
    /// </summary>
    /// <param name="unitOfWork">The ADO.NET unit of work used to manage repositories and transactions.</param>
    /// <param name="converterFactory">The factory used to obtain event converters for domain and snapshot events.</param>
    public EventStore(IDataUnitOfWork unitOfWork, IEventConverterFactory converterFactory)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(converterFactory);

        _unitOfWork = unitOfWork;
        _domainRepository = unitOfWork.GetRepository<TEntityEventDomain>();
        _snapshotRepository = unitOfWork.GetRepository<TEntityEventSnapshot>();

        _converterFactory = converterFactory;
        _domainConverter = converterFactory.GetDomainEventConverter<TEntityEventDomain>();
        _snapshotConverter = converterFactory.GetSnapshotEventConverter<TEntityEventSnapshot>();
    }

    /// <inheritdoc/>
    public async Task<AppendResult> AppendToStreamAsync(
        AppendRequest request,
        CancellationToken cancellationToken = default)
    {
        var batch = request.Events.OfType<IDomainEvent>().ToArray();
        if (batch.Length == 0)
        {
            return AppendResult.Create(
                request.ExpectedVersion.GetValueOrDefault() + 1,
                request.ExpectedVersion.GetValueOrDefault());
        }

        long current = await GetStreamVersionCoreAsync(request.StreamId, cancellationToken).ConfigureAwait(false);
        if (request.ExpectedVersion.HasValue && current != request.ExpectedVersion)
        {
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

            var entity = _domainConverter.ConvertEventToEntity(nextEvent, _converterFactory.ConverterContext);
            entities.Add(entity);
        }

        await _domainRepository.InsertAsync(entities, cancellationToken).ConfigureAwait(false);

        Guid[] guids = [.. entities.ConvertAll(e => e.StreamId)];
        return AppendResult.Create(guids, next, request.ExpectedVersion.GetValueOrDefault());
    }

    /// <inheritdoc/>
    public async Task AppendSnapshotAsync(
        ISnapshotEvent @event,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(@event);

        var entity = _snapshotConverter.ConvertEventToEntity(@event, _converterFactory.ConverterContext);
        await _snapshotRepository.InsertAsync(entity, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task DeleteStreamAsync(
        DeleteStreamRequest request,
        CancellationToken cancellationToken = default)
    {
        var specification = QuerySpecification
            .For<TEntityEventDomain>()
            .Where(e => e.StreamId == request.StreamId)
            .Build();

        if (request.HardDelete)
        {
            await _domainRepository
                .DeleteAsync(specification, cancellationToken)
                .ConfigureAwait(false);
        }
        else
        {
            var updater = EntityUpdater
                .For<TEntityEventDomain>()
                .SetProperty(e => e.Status, EventStatus.DELETED.Value);

            await _domainRepository
                .UpdateAsync(specification, updater, cancellationToken)
                .ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    public async Task<EnvelopeResult?> GetLatestSnapshotAsync(
        Guid ownerId,
        CancellationToken cancellationToken = default)
    {
        var specification = QuerySpecification
            .For<TEntityEventSnapshot>()
            .Where(e => e.OwnerId == ownerId)
            .OrderByDescending(e => e.Sequence)
            .Take(1)
            .Build();

        var last = await _snapshotRepository
            .QueryFirstOrDefaultAsync(specification, cancellationToken)
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

    /// <inheritdoc/>
    public async Task<long> GetStreamVersionAsync(Guid streamId, CancellationToken cancellationToken = default) =>
        await GetStreamVersionCoreAsync(streamId, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc/>
    public async IAsyncEnumerable<EnvelopeResult> ReadAllStreamsAsync(
        ReadAllStreamsRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var specification = QuerySpecification
            .For<TEntityEventDomain>()
            .Where(e => e.Sequence > request.FromPosition)
            .OrderBy(e => e.Sequence)
            .Take(request.MaxCount)
            .Build();

        await foreach (var entity in _domainRepository.QueryAsync(specification, cancellationToken).ConfigureAwait(false))
        {
            yield return new EnvelopeResult
            {
                Event = _domainConverter.ConvertEntityToEvent(entity, _converterFactory.ConverterContext),
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

    /// <inheritdoc/>
    public async IAsyncEnumerable<EnvelopeResult> ReadStreamAsync(
        ReadStreamRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var specification = QuerySpecification
            .For<TEntityEventDomain>()
            .Where(e => e.StreamId == request.StreamId && e.StreamVersion > request.FromVersion)
            .OrderBy(e => e.StreamVersion)
            .Take(request.MaxCount)
            .Build();

        await foreach (var entity in _domainRepository.QueryAsync(specification, cancellationToken).ConfigureAwait(false))
        {
            yield return new EnvelopeResult
            {
                Event = _domainConverter.ConvertEntityToEvent(entity, _converterFactory.ConverterContext),
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

    /// <inheritdoc/>
    public async Task<bool> StreamExistsAsync(Guid streamId, CancellationToken cancellationToken = default)
    {
        var specification = QuerySpecification
            .For<TEntityEventDomain>()
            .Where(e => e.StreamId == streamId)
            .Build();

        return await _domainRepository
            .ExistsAsync(specification, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task TruncateStreamAsync(
        TruncateStreamRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentVersion = await GetStreamVersionCoreAsync(request.StreamId, cancellationToken)
            .ConfigureAwait(false);

        if (currentVersion == -1)
            return;

        var specification = QuerySpecification
            .For<TEntityEventDomain>()
            .Where(e => e.StreamId == request.StreamId && e.StreamVersion < request.TruncateBeforeVersion)
            .Build();

        await _domainRepository
            .DeleteAsync(specification, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public IAsyncDisposable SubscribeToStream(
        SubscribeToStreamRequest request,
        CancellationToken cancellationToken = default)
    {
        // ADO.NET doesn't support change notifications natively
        // Return a polling-based subscription
        return new StreamSubscription<TEntityEventDomain>(
            _domainRepository,
            request,
            _converterFactory,
            _domainConverter,
            cancellationToken);
    }

    /// <inheritdoc/>
    public IAsyncDisposable SubscribeToAllStreams(
        SubscribeToAllStreamsRequest request,
        CancellationToken cancellationToken = default)
    {
        return new AllStreamsSubscription<TEntityEventDomain>(
            _domainRepository,
            request,
            _converterFactory,
            _domainConverter,
            cancellationToken);
    }

    private async Task<long> GetStreamVersionCoreAsync(Guid streamId, CancellationToken cancellationToken)
    {
        var specification = QuerySpecification
            .For<TEntityEventDomain>()
            .Where(e => e.StreamId == streamId)
            .OrderByDescending(e => e.StreamVersion)
            .Build();

        var @event = await _domainRepository
            .QueryFirstOrDefaultAsync(specification, cancellationToken)
            .ConfigureAwait(false);

        return @event?.StreamVersion ?? -1;
    }
}

/// <summary>
/// Polling-based stream subscription for ADO.NET.
/// </summary>
[RequiresDynamicCode("Expression compilation requires dynamic code generation.")]
#pragma warning disable CA1812 // Avoid uninstantiated internal classes
internal sealed class StreamSubscription<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TEntityEventDomain>
    : IAsyncDisposable
    where TEntityEventDomain : class, IEntityEventDomain
#pragma warning restore CA1812
{
    private readonly IDataRepository<TEntityEventDomain> _domainRepository;
    private readonly SubscribeToStreamRequest _request;
    private readonly IEventConverterFactory _converterFactory;
    private readonly IEventConverter<TEntityEventDomain, IDomainEvent> _domainConverter;
    private readonly CancellationTokenSource _cts;
    private readonly Task _subscriptionTask;

    public StreamSubscription(
        IDataRepository<TEntityEventDomain> repository,
        SubscribeToStreamRequest request,
        IEventConverterFactory converterFactory,
        IEventConverter<TEntityEventDomain, IDomainEvent> converter,
        CancellationToken cancellationToken)
    {
        _domainRepository = repository ?? throw new ArgumentNullException(nameof(repository));
        _request = request;
        _converterFactory = converterFactory ?? throw new ArgumentNullException(nameof(converterFactory));
        _domainConverter = converter ?? throw new ArgumentNullException(nameof(converter));
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _subscriptionTask = RunSubscriptionAsync();
    }

    private async Task RunSubscriptionAsync()
    {
        long lastProcessedVersion = _request.FromVersion - 1;

        try
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                var specification = QuerySpecification
                    .For<TEntityEventDomain>()
                    .Where(e => e.StreamId == _request.StreamId && e.StreamVersion > lastProcessedVersion)
                    .OrderBy(e => e.StreamVersion)
                    .Take(100)
                    .Build();

                var events = new List<TEntityEventDomain>();
                await foreach (var entity in _domainRepository.QueryAsync(specification, _cts.Token).ConfigureAwait(false))
                {
                    events.Add(entity);
                }

                foreach (var entity in events)
                {
                    var domainEvent = _domainConverter.ConvertEntityToEvent(entity, _converterFactory.ConverterContext);
                    await _request.OnEvent(domainEvent).ConfigureAwait(false);
                    lastProcessedVersion = entity.StreamVersion;
                }

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

/// <summary>
/// Polling-based all-streams subscription for ADO.NET.
/// </summary>
[RequiresDynamicCode("Expression compilation requires dynamic code generation.")]
#pragma warning disable CA1812 // Avoid uninstantiated internal classes
internal sealed class AllStreamsSubscription<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TEntityEventDomain>
    : IAsyncDisposable
    where TEntityEventDomain : class, IEntityEventDomain
#pragma warning restore CA1812
{
    private readonly IDataRepository<TEntityEventDomain> _domainRepository;
    private readonly SubscribeToAllStreamsRequest _request;
    private readonly IEventConverter<TEntityEventDomain, IDomainEvent> _domainConverter;
    private readonly IEventConverterFactory _converterFactory;
    private readonly CancellationTokenSource _cts;
    private readonly Task _subscriptionTask;

    public AllStreamsSubscription(
        IDataRepository<TEntityEventDomain> repository,
        SubscribeToAllStreamsRequest request,
        IEventConverterFactory converterFactory,
        IEventConverter<TEntityEventDomain, IDomainEvent> converter,
        CancellationToken cancellationToken)
    {
        _domainRepository = repository ?? throw new ArgumentNullException(nameof(repository));
        _request = request;
        _domainConverter = converter ?? throw new ArgumentNullException(nameof(converter));
        _converterFactory = converterFactory ?? throw new ArgumentNullException(nameof(converterFactory));
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _subscriptionTask = RunSubscriptionAsync();
    }

    private async Task RunSubscriptionAsync()
    {
        long lastProcessedSequence = _request.FromPosition - 1;

        try
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                var specification = QuerySpecification
                    .For<TEntityEventDomain>()
                    .Where(e => e.Sequence > lastProcessedSequence)
                    .OrderBy(e => e.Sequence)
                    .Take(100)
                    .Build();

                var events = new List<TEntityEventDomain>();
                await foreach (var entity in _domainRepository.QueryAsync(specification, _cts.Token).ConfigureAwait(false))
                {
                    events.Add(entity);
                }

                foreach (var entity in events)
                {
                    var domainEvent = _domainConverter.ConvertEntityToEvent(entity, _converterFactory.ConverterContext);
                    await _request.OnEvent(domainEvent).ConfigureAwait(false);
                    lastProcessedSequence = entity.Sequence;
                }

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
