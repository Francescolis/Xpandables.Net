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
using System.Events.Integration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace System.Events.Data;

/// <summary>
/// Provides an implementation of an outbox pattern for managing integration events.
/// </summary>
/// <remarks>The <see cref="OutboxStore{TEntityIntegrationEvent}"/> class is designed to facilitate reliable event processing
/// by implementing the outbox pattern. It ensures that integration events are stored, claimed, and processed in a
/// consistent and fault-tolerant manner. This class supports operations such as enqueuing events, claiming pending
/// events for processing, marking events as completed, and handling event failures.</remarks>
/// <param name="outboxStoreDataContextFactory">The data context used for accessing the outbox store.</param>
/// <param name="converterFactory">The factory used to obtain event converters.</param>
public sealed class OutboxStore<[DynamicallyAccessedMembers(EntityEvent.DynamicallyAccessedMemberTypes)] TEntityIntegrationEvent>(
    IOutboxStoreDataContextFactory outboxStoreDataContextFactory,
    IEventConverterFactory converterFactory) : IOutboxStore
    where TEntityIntegrationEvent : class, IEntityEventIntegration
{
    private readonly EventDataContext _db = outboxStoreDataContextFactory.Create();
    private readonly IEventConverterFactory _converterFactory = converterFactory;
    private readonly IEventConverter<TEntityIntegrationEvent, IIntegrationEvent> _converter = converterFactory
        .GetIntegrationEventConverter<TEntityIntegrationEvent>();

    /// <inheritdoc />
    public async Task EnqueueAsync(
        CancellationToken cancellationToken,
        params IIntegrationEvent[] events)
    {
        ArgumentNullException.ThrowIfNull(events);
        ArgumentOutOfRangeException.ThrowIfEqual(events.Length, 0);

        var list = new List<TEntityIntegrationEvent>(events.Length);

        foreach (var @event in events)
        {
            var entity = _converter.ConvertEventToEntity(@event, _converterFactory.ConverterContext);
            entity.SetStatus(EventStatus.PENDING);
            list.Add(entity);
        }

        await _db.AddRangeAsync(list, cancellationToken).ConfigureAwait(false);
        // defer SaveChanges to Event Store or Unit of Work
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<IIntegrationEvent>> DequeueAsync(
        CancellationToken cancellationToken,
        int maxEvents = 10,
        TimeSpan? visibilityTimeout = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxEvents);

        var now = DateTime.UtcNow;
        var lease = visibilityTimeout ?? TimeSpan.FromMinutes(5);
        var claimId = Guid.NewGuid();
        var set = _db.Set<TEntityIntegrationEvent>();

        var candidateIds = await set
            .Where(e =>
                (e.Status == EventStatus.PENDING) ||
                (e.Status == EventStatus.ONERROR && (e.NextAttemptOn == null || e.NextAttemptOn <= now)))
            .Where(e => e.ClaimId == null)
            .OrderBy(e => e.Sequence)
            .Select(e => e.KeyId)
            .Take(Math.Max(1, maxEvents))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (candidateIds.Count == 0) return [];

        var updated = await set
            .Where(e => candidateIds.Contains(e.KeyId) && e.ClaimId == null)
            .ExecuteUpdateAsync(updater => updater
                .SetProperty(e => e.Status, EventStatus.PROCESSING)
                .SetProperty(e => e.ClaimId, claimId)
                .SetProperty(e => e.ErrorMessage, (string?)null)
                .SetProperty(e => e.NextAttemptOn, now.Add(lease))
                .SetProperty(e => e.UpdatedOn, now), cancellationToken)
            .ConfigureAwait(false);

        if (updated == 0) return [];

        var claimed = await set
            .AsNoTracking()
            .Where(e => e.ClaimId == claimId)
            .OrderBy(e => e.Sequence)
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
    public async Task CompleteAsync(
        CancellationToken cancellationToken,
        params Guid[] eventIds)
    {
        ArgumentNullException.ThrowIfNull(eventIds);
        ArgumentOutOfRangeException.ThrowIfEqual(eventIds.Length, 0);

        var now = DateTime.UtcNow;
        await _db.Set<TEntityIntegrationEvent>()
            .Where(e => eventIds.Contains(e.KeyId))
            .ExecuteUpdateAsync(updater => updater
                .SetProperty(e => e.Status, EventStatus.PUBLISHED)
                .SetProperty(e => e.ErrorMessage, (string?)null)
                .SetProperty(e => e.AttemptCount, e => e.AttemptCount)
                .SetProperty(e => e.NextAttemptOn, (DateTime?)null)
                .SetProperty(e => e.ClaimId, (Guid?)null)
                .SetProperty(e => e.UpdatedOn, now), cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task FailAsync(
        CancellationToken cancellationToken,
        params FailedEvent[] failures)
    {
        ArgumentNullException.ThrowIfNull(failures);
        ArgumentOutOfRangeException.ThrowIfEqual(failures.Length, 0);

        var now = DateTime.UtcNow;
        IExecutionStrategy strategy = _db.Database.CreateExecutionStrategy();

        _ = await strategy.ExecuteAsync(async () =>
         {
             using var transaction = await _db.Database
                 .BeginTransactionAsync(cancellationToken)
                 .ConfigureAwait(false);
             try
             {
                 foreach (var failure in failures)
                 {
                     await _db.Set<TEntityIntegrationEvent>()
                         .Where(e => e.KeyId == failure.EventId)
                         .ExecuteUpdateAsync(updater => updater
                             .SetProperty(e => e.Status, EventStatus.ONERROR)
                             .SetProperty(e => e.ErrorMessage, failure.Error)
                             .SetProperty(e => e.AttemptCount, e => e.AttemptCount + 1)
                             .SetProperty(e => e.NextAttemptOn, e => now.AddSeconds(Math.Min(600, 10 * Math.Pow(2, Math.Min(10, e.AttemptCount)))))
                             .SetProperty(e => e.ClaimId, (Guid?)null)
                             .SetProperty(e => e.UpdatedOn, now), cancellationToken)
                         .ConfigureAwait(false);
                 }

                 if (transaction is not null)
                 {
                     await transaction
                         .CommitAsync(cancellationToken)
                         .ConfigureAwait(false);
                 }

                 return failures.Length;
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
}