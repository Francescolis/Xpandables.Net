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
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Events.Integration;

namespace System.Events.Data;

/// <summary>
/// Provides an ADO.NET implementation of an outbox pattern for managing integration events.
/// </summary>
/// <typeparam name="TEntityEventOutbox">The type of the entity outbox event.</typeparam>
/// <remarks>
/// <para>
/// The <see cref="OutboxStore{TEntityEventOutbox}"/> class implements the outbox pattern using
/// raw ADO.NET (not Entity Framework Core). It ensures that integration events are stored, claimed, 
/// and processed in a consistent and fault-tolerant manner.
/// </para>
/// <para>
/// This class supports operations such as enqueuing events, claiming pending events for processing, 
/// marking events as completed, and handling event failures.
/// </para>
/// </remarks>
[RequiresDynamicCode("Expression compilation requires dynamic code generation.")]
public sealed class OutboxStore<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TEntityEventOutbox> : IOutboxStore
    where TEntityEventOutbox : class, IDataEventOutbox
{
    private readonly IDataRepository<TEntityEventOutbox> _outboxRepository;
    private readonly IDataUnitOfWork _unitOfWork;
    private readonly IEventConverterFactory _converterFactory;
    private readonly IIntegrationEventEnricher _eventEnricher;
    private readonly IEventConverter<TEntityEventOutbox, IIntegrationEvent> _converter;

    /// <summary>
    /// Initializes a new instance of the DataOutboxStore class.
    /// </summary>
    /// <param name="unitOfWork">The ADO.NET unit of work.</param>
    /// <param name="converterFactory">The factory used to obtain event converters.</param>
    /// <param name="eventEnricher">The enricher used to enrich integration events before processing.</param>
    public OutboxStore(
        IDataUnitOfWork unitOfWork,
        IEventConverterFactory converterFactory,
        IIntegrationEventEnricher eventEnricher)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(converterFactory);
        ArgumentNullException.ThrowIfNull(eventEnricher);

        _unitOfWork = unitOfWork;
        _outboxRepository = unitOfWork.GetRepository<TEntityEventOutbox>();
        _converterFactory = converterFactory;
        _eventEnricher = eventEnricher;
        _converter = converterFactory.GetOutboxEventConverter<TEntityEventOutbox>();
    }

    /// <inheritdoc />
    public async Task EnqueueAsync(
        IEnumerable<IIntegrationEvent> events,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(events);
        var countEvents = events.ToList();
        ArgumentOutOfRangeException.ThrowIfEqual(countEvents.Count, 0);

        var list = new List<TEntityEventOutbox>(countEvents.Count);
        var enriched = countEvents.Select(_eventEnricher.Enrich).ToArray();

        foreach (var @event in enriched)
        {
            var entity = _converter.ConvertEventToEntity(@event, _converterFactory.ConverterContext);
            entity.SetStatus(EventStatus.PENDING.Value);
            list.Add(entity);
        }

        await _outboxRepository
            .InsertAsync(list, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<IIntegrationEvent>> DequeueAsync(
        int maxEvents = 10,
        TimeSpan? visibilityTimeout = default,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxEvents);

        var now = DateTime.UtcNow;
        var lease = visibilityTimeout ?? TimeSpan.FromMinutes(5);
        var claimId = Guid.NewGuid();

        // Step 1: Find candidate events
        var specification = DataSpecification
            .For<TEntityEventOutbox>()
            .Where(e =>
                (e.Status == EventStatus.PENDING.Value) ||
                (e.Status == EventStatus.ONERROR.Value && (e.NextAttemptOn == null || e.NextAttemptOn <= now)))
            .Where(e => e.ClaimId == null)
            .OrderBy(e => e.Sequence)
            .Take(Math.Max(1, maxEvents))
            .Build();

        var candidates = new List<Guid>();
        await foreach (var entity in _outboxRepository.QueryAsync(specification, cancellationToken).ConfigureAwait(false))
        {
            candidates.Add(entity.KeyId);
        }

        if (candidates.Count == 0)
            return [];

        // Step 2: Claim the events atomically
        var updater = DataUpdater
            .For<TEntityEventOutbox>()
            .SetProperty(e => e.Status, EventStatus.PROCESSING.Value)
            .SetProperty(e => e.ClaimId, claimId)
            .SetProperty(e => e.ErrorMessage, (string?)null)
            .SetProperty(e => e.NextAttemptOn, now.Add(lease))
            .SetProperty(e => e.UpdatedOn, now);

        var updateSpec = DataSpecification
            .For<TEntityEventOutbox>()
            .Where(e => candidates.Contains(e.KeyId) && e.ClaimId == null)
            .Build();

        var updated = await _outboxRepository
            .UpdateAsync(updateSpec, updater, cancellationToken)
            .ConfigureAwait(false);

        if (updated == 0)
            return [];

        // Step 3: Fetch the claimed events
        var selectSpec = DataSpecification
            .For<TEntityEventOutbox>()
            .Where(e => e.ClaimId == claimId)
            .OrderBy(e => e.Sequence)
            .Build();

        var list = new List<IIntegrationEvent>();
        await foreach (var entity in _outboxRepository.QueryAsync(selectSpec, cancellationToken).ConfigureAwait(false))
        {
            if (_converter.ConvertEntityToEvent(entity, _converterFactory.ConverterContext) is IIntegrationEvent ie)
            {
                list.Add(ie);
            }
        }

        return list;
    }

    /// <inheritdoc />
    public async Task CompleteAsync(
        IEnumerable<CompletedOutboxEvent> successes,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(successes);
        var countSuccesses = successes.ToList();
        ArgumentOutOfRangeException.ThrowIfEqual(countSuccesses.Count, 0);

        var eventIds = countSuccesses.Select(e => e.EventId).ToList();
        var now = DateTime.UtcNow;
        var specification = DataSpecification
            .For<TEntityEventOutbox>()
            .Where(e => eventIds.Contains(e.KeyId))
            .Build();

        var updater = DataUpdater
            .For<TEntityEventOutbox>()
            .SetProperty(e => e.Status, EventStatus.PUBLISHED.Value)
            .SetProperty(e => e.ErrorMessage, (string?)null)
            .SetProperty(e => e.NextAttemptOn, (DateTime?)null)
            .SetProperty(e => e.ClaimId, (Guid?)null)
            .SetProperty(e => e.UpdatedOn, now);

        await _outboxRepository
            .UpdateAsync(specification, updater, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task FailAsync(
        IEnumerable<FailedOutboxEvent> failures,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(failures);
        var failuresList = failures.ToList();
        ArgumentOutOfRangeException.ThrowIfEqual(failuresList.Count, 0);

        var now = DateTime.UtcNow;

        foreach (var failure in failuresList)
        {
            // Get current attempt count
            var getSpec = DataSpecification
                .For<TEntityEventOutbox>()
                .Where(e => e.KeyId == failure.EventId)
                .Select(e => e.AttemptCount);

            var currentAttempt = await _outboxRepository
                .QueryFirstOrDefaultAsync(getSpec, cancellationToken)
                .ConfigureAwait(false);

            var nextAttempt = currentAttempt + 1;
            var backoffSeconds = nextAttempt < 1 ? 10 :
                                 nextAttempt < 2 ? 20 :
                                 nextAttempt < 3 ? 40 :
                                 nextAttempt < 4 ? 80 :
                                 nextAttempt < 5 ? 160 : 320;

            var specification = DataSpecification
                .For<TEntityEventOutbox>()
                .Where(e => e.KeyId == failure.EventId)
                .Build();

            var updater = DataUpdater
                .For<TEntityEventOutbox>()
                .SetProperty(e => e.Status, EventStatus.ONERROR.Value)
                .SetProperty(e => e.ErrorMessage, failure.Error)
                .SetProperty(e => e.AttemptCount, nextAttempt)
                .SetProperty(e => e.NextAttemptOn, now.AddSeconds(backoffSeconds))
                .SetProperty(e => e.ClaimId, (Guid?)null)
                .SetProperty(e => e.UpdatedOn, now);

            await _outboxRepository
                .UpdateAsync(specification, updater, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
