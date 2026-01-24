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
using System.Events.Integration;

namespace System.Events.Data;

/// <summary>
/// Provides an implementation of an outbox pattern for managing integration events.
/// </summary>
/// <typeparam name="TEntityEventOutbox">The type of the entity integration event.</typeparam>
/// <remarks>The <see cref="OutboxStore{TEntityIntegrationEvent}"/> class is designed to facilitate reliable event processing
/// by implementing the outbox pattern. It ensures that integration events are stored, claimed, and processed in a
/// consistent and fault-tolerant manner. This class supports operations such as enqueuing events, claiming pending
/// events for processing, marking events as completed, and handling event failures.</remarks>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="converterFactory">The factory used to obtain event converters.</param>
/// <param name="eventEnricher">The enricher used to enrich integration events before processing.</param>
public sealed class OutboxStore<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TEntityEventOutbox>(
    IUnitOfWork unitOfWork,
    IEventConverterFactory converterFactory,
    IIntegrationEventEnricher eventEnricher) : IOutboxStore
    where TEntityEventOutbox : class, IEntityEventOutbox
{
    private readonly IRepository<TEntityEventOutbox> _integrationRepository = unitOfWork.GetRepository<IRepository<TEntityEventOutbox>>();
    private readonly IEventConverterFactory _converterFactory = converterFactory;
    private readonly IIntegrationEventEnricher _eventEnricher = eventEnricher;
    private readonly IEventConverter<TEntityEventOutbox, IIntegrationEvent> _converter = converterFactory
        .GetOutboxEventConverter<TEntityEventOutbox>();

    /// <inheritdoc />
    public async Task EnqueueAsync(
        CancellationToken cancellationToken,
        params IIntegrationEvent[] events)
    {
        ArgumentNullException.ThrowIfNull(events);
        ArgumentOutOfRangeException.ThrowIfEqual(events.Length, 0);

        var list = new List<TEntityEventOutbox>(events.Length);
        var enriched = events.Select(_eventEnricher.Enrich).ToArray();

        foreach (var @event in enriched)
        {
            var entity = _converter.ConvertEventToEntity(@event, _converterFactory.ConverterContext);
            entity.SetStatus(EntityStatus.PENDING.Value);
            list.Add(entity);
        }

        await _integrationRepository.AddAsync(list, cancellationToken).ConfigureAwait(false);
        // defer SaveChanges to Event Store flush
    }

    /// <inheritdoc />
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
    public async Task<IReadOnlyList<IIntegrationEvent>> DequeueAsync(
        CancellationToken cancellationToken,
        int maxEvents = 10,
        TimeSpan? visibilityTimeout = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxEvents);

        var now = DateTime.UtcNow;
        var lease = visibilityTimeout ?? TimeSpan.FromMinutes(5);
        var claimId = Guid.NewGuid();

        var specification = QuerySpecification
            .For<TEntityEventOutbox>()
            .Where(e =>
                (e.Status == EntityStatus.PENDING.Value) ||
                (e.Status == EntityStatus.ONERROR.Value && (e.NextAttemptOn == null || e.NextAttemptOn <= now)))
            .Where(e => e.ClaimId == null)
            .OrderBy(e => e.Sequence)
            .Take(Math.Max(1, maxEvents))
            .Select(e => e.KeyId);

        var candidateIds = await _integrationRepository
            .FetchAsync(specification, cancellationToken)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (candidateIds.Count == 0) return [];

        var updater = EntityUpdater
            .For<TEntityEventOutbox>()
            .SetProperty(e => e.Status, EntityStatus.PROCESSING.Value)
            .SetProperty(e => e.ClaimId, claimId)
            .SetProperty(e => e.ErrorMessage, (string?)null)
            .SetProperty(e => e.NextAttemptOn, now.Add(lease))
            .SetProperty(e => e.UpdatedOn, now);

        var specificationUpdater = QuerySpecification
            .For<TEntityEventOutbox>()
            .Where(e => candidateIds.Contains(e.KeyId) && e.ClaimId == null)
            .Build();

        var updated = await _integrationRepository
            .UpdateAsync(specificationUpdater, updater, cancellationToken)
            .ConfigureAwait(false);

        if (updated == 0) return [];

        var specificationSelect = QuerySpecification
            .For<TEntityEventOutbox>()
            .Where(e => e.ClaimId == claimId)
            .OrderBy(e => e.Sequence)
            .Build();

        var claimed = await _integrationRepository
            .FetchAsync(specificationSelect, cancellationToken)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var list = new List<IIntegrationEvent>(claimed.Count);
        foreach (var entity in claimed)
        {
            if (_converter.ConvertEntityToEvent(entity, _converterFactory.ConverterContext) is IIntegrationEvent ie)
            {
                list.Add(ie);
            }
        }

        return list;
    }

    /// <inheritdoc />
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
    public async Task CompleteAsync(
        CancellationToken cancellationToken,
        params Guid[] eventIds)
    {
        ArgumentNullException.ThrowIfNull(eventIds);
        ArgumentOutOfRangeException.ThrowIfEqual(eventIds.Length, 0);

        var now = DateTime.UtcNow;
        var specification = QuerySpecification
            .For<TEntityEventOutbox>()
            .Where(e => eventIds.Contains(e.KeyId))
            .Build();

        var updater = EntityUpdater
            .For<TEntityEventOutbox>()
            .SetProperty(e => e.Status, EntityStatus.PUBLISHED.Value)
            .SetProperty(e => e.ErrorMessage, (string?)null)
            .SetProperty(e => e.AttemptCount, e => e.AttemptCount)
            .SetProperty(e => e.NextAttemptOn, (DateTime?)null)
            .SetProperty(e => e.ClaimId, (Guid?)null)
            .SetProperty(e => e.UpdatedOn, now);

        await _integrationRepository
            .UpdateAsync(specification, updater, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
    public async Task FailAsync(
        CancellationToken cancellationToken,
        params FailedOutboxEvent[] failures)
    {
        ArgumentNullException.ThrowIfNull(failures);
        ArgumentOutOfRangeException.ThrowIfEqual(failures.Length, 0);

        var now = DateTime.UtcNow;
        try
        {
            foreach (var failure in failures)
            {
                var specification = QuerySpecification
                    .For<TEntityEventOutbox>()
                    .Where(e => e.KeyId == failure.EventId)
                    .Build();

                var updater = EntityUpdater
                    .For<TEntityEventOutbox>()
                    .SetProperty(e => e.Status, EntityStatus.ONERROR.Value)
                    .SetProperty(e => e.ErrorMessage, failure.Error)
                    .SetProperty(e => e.AttemptCount, e => e.AttemptCount + 1)
                    .SetProperty(e => e.NextAttemptOn, e => now.AddSeconds(Math.Min(600, 10 * Math.Pow(2, Math.Min(10, e.AttemptCount)))))
                    .SetProperty(e => e.ClaimId, (Guid?)null)
                    .SetProperty(e => e.UpdatedOn, now);

                await _integrationRepository
                    .UpdateAsync(specification, updater, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException(
                "An error occurred while processing failed outbox events.",
                exception);
        }
    }
}