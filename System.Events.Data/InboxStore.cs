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

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace System.Events.Data;

/// <summary>
/// EF Core implementation of <see cref="IInboxStore"/> enabling idempotent (exactly-once) consumption.
/// </summary>
/// <typeparam name="TEntityEventInbox">Inbox entity type.</typeparam>
public sealed class InboxStore<[DynamicallyAccessedMembers(EntityEvent.DynamicallyAccessedMemberTypes)] TEntityEventInbox>(
    IInboxStoreDataContextFactory inboxStoreDataContextFactory,
    IEventConverterFactory converterFactory,
    IIntegrationEventEnricher eventEnricher) : IInboxStore
    where TEntityEventInbox : class, IEntityEventInbox
{
    private readonly EventDataContext _db = inboxStoreDataContextFactory.Create();
    private readonly IEventConverterFactory _converterFactory = converterFactory;
    private readonly IIntegrationEventEnricher _eventEnricher = eventEnricher;
    private readonly IEventConverter<TEntityEventInbox, IIntegrationEvent> _converter =
        converterFactory.GetInboxEventConverter<TEntityEventInbox>();

    /// <inheritdoc />
    public async Task<InboxReceiveResult> ReceiveAsync(
        IIntegrationEvent @event,
        string consumer,
        TimeSpan? visibilityTimeout = default,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentException.ThrowIfNullOrWhiteSpace(consumer);

        var set = _db.Set<TEntityEventInbox>();
        var existing = await set
            .AsNoTracking()
            .Where(e => e.KeyId == @event.EventId && e.Consumer == consumer)
            .Select(e => new { e.Status, e.NextAttemptOn })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (existing is not null && existing.Status == EntityStatus.PUBLISHED)
        {
            return new InboxReceiveResult(@event.EventId, EntityStatus.DUPLICATE);
        }

        if (existing is not null && existing.Status == EntityStatus.PROCESSING)
        {
            return new InboxReceiveResult(@event.EventId, EntityStatus.PROCESSING);
        }

        if (existing is not null && existing.Status == EntityStatus.ONERROR)
        {
            // allow retry only if lease expired
            if (existing.NextAttemptOn is not null && existing.NextAttemptOn > DateTime.UtcNow)
            {
                return new InboxReceiveResult(@event.EventId, EntityStatus.PROCESSING);
            }
        }

        var enriched = _eventEnricher.Enrich(@event);
        var entity = _converter.ConvertEventToEntity(enriched, _converterFactory.ConverterContext);
        entity.SetStatus(EntityStatus.PROCESSING);
        entity = entity switch
        {
            TEntityEventInbox typed => typed,
            _ => throw new InvalidOperationException($"Expected entity convertible to {typeof(TEntityEventInbox).Name}.")
        };

        entity.Consumer = consumer;
        entity.ClaimId = Guid.NewGuid();
        entity.NextAttemptOn = DateTime.UtcNow.Add(visibilityTimeout ?? TimeSpan.FromMinutes(5));

        await _db.AddAsync(entity, cancellationToken).ConfigureAwait(false);
        // SaveChanges is intentionally deferred to the caller's unit of work.

        return new InboxReceiveResult(@event.EventId, EntityStatus.ACCEPTED);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<IIntegrationEvent>> DequeueAsync(
       string consumer,
        int maxEvents = 10,
        TimeSpan? visibilityTimeout = default,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxEvents);
        ArgumentException.ThrowIfNullOrWhiteSpace(consumer);

        var now = DateTime.UtcNow;
        var lease = visibilityTimeout ?? TimeSpan.FromMinutes(5);
        var claimId = Guid.NewGuid();
        var set = _db.Set<TEntityEventInbox>();

        var candidateIds = await set
            .Where(e => e.Consumer == consumer)
            .Where(e =>
                e.Status == EntityStatus.PENDING.Value ||
                (e.Status == EntityStatus.ONERROR.Value && (e.NextAttemptOn == null || e.NextAttemptOn <= now)))
            .Where(e => e.ClaimId == null)
            .OrderBy(e => e.Sequence)
            .Select(e => e.KeyId)
            .Take(Math.Max(1, maxEvents))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (candidateIds.Count == 0) return [];

        var updated = await set
            .Where(e => e.Consumer == consumer && candidateIds.Contains(e.KeyId) && e.ClaimId == null)
            .ExecuteUpdateAsync(updater => updater
                .SetProperty(e => e.Status, EntityStatus.PROCESSING.Value)
                .SetProperty(e => e.ClaimId, claimId)
                .SetProperty(e => e.ErrorMessage, (string?)null)
                .SetProperty(e => e.NextAttemptOn, now.Add(lease))
                .SetProperty(e => e.UpdatedOn, now), cancellationToken)
            .ConfigureAwait(false);

        if (updated == 0) return [];

        var claimed = await set
            .AsNoTracking()
            .Where(e => e.ClaimId == claimId && e.Consumer == consumer)
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
        params CompletedInboxEvent[] events)
    {
        ArgumentNullException.ThrowIfNull(events);
        ArgumentOutOfRangeException.ThrowIfEqual(events.Length, 0);

        var now = DateTime.UtcNow;
        await _db.Set<TEntityEventInbox>()
            .Where(e => events.Select(evt => new { evt.EventId, evt.Consumer })
                .Contains(new { e.KeyId, e.Consumer }))
            .ExecuteUpdateAsync(updater => updater
                .SetProperty(e => e.Status, EntityStatus.PUBLISHED.Value)
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
        params FailedInboxEvent[] failures)
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
                    await _db.Set<TEntityEventInbox>()
                        .Where(e => e.KeyId == failure.EventId && e.Consumer == failure.Consumer)
                        .ExecuteUpdateAsync(updater => updater
                            .SetProperty(e => e.Status, EntityStatus.ONERROR.Value)
                            .SetProperty(e => e.ErrorMessage, failure.Error)
                            .SetProperty(e => e.AttemptCount, e => e.AttemptCount + 1)
                            .SetProperty(e => e.NextAttemptOn, e => now.AddSeconds(Math.Min(600, 10 * Math.Pow(2, Math.Min(10, e.AttemptCount)))))
                            .SetProperty(e => e.ClaimId, (Guid?)null)
                            .SetProperty(e => e.UpdatedOn, now), cancellationToken)
                        .ConfigureAwait(false);
                }

                await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                return failures.Length;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                throw;
            }
        }).ConfigureAwait(false);
    }
}