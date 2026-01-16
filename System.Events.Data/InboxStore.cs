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
    IEventConverterFactory converterFactory) : IInboxStore
    where TEntityEventInbox : class, IEntityEventInbox
{
    private readonly EventDataContext _db = inboxStoreDataContextFactory.Create();
    private readonly IEventConverterFactory _converterFactory = converterFactory;
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

        TEntityEventInbox entity = _converter.ConvertEventToEntity(@event, _converterFactory.ConverterContext);
        entity.SetStatus(EntityStatus.PROCESSING);
        entity.Consumer = consumer;
        entity.ClaimId = Guid.NewGuid();
        entity.NextAttemptOn = DateTime.UtcNow.Add(visibilityTimeout ?? TimeSpan.FromMinutes(5));

        await _db.AddAsync(entity, cancellationToken).ConfigureAwait(false);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new InboxReceiveResult(@event.EventId, EntityStatus.ACCEPTED);
    }

    /// <inheritdoc />
    public async Task CompleteAsync(
        CancellationToken cancellationToken,
        params CompletedInboxEvent[] events)
    {
        ArgumentNullException.ThrowIfNull(events);
        ArgumentOutOfRangeException.ThrowIfEqual(events.Length, 0);

        var now = DateTime.UtcNow;
        var set = _db.Set<TEntityEventInbox>();

        foreach (var evt in events)
        {
            await set
                .Where(e => e.KeyId == evt.EventId && e.Consumer == evt.Consumer)
                .ExecuteUpdateAsync(updater => updater
                    .SetProperty(e => e.Status, EntityStatus.PUBLISHED.Value)
                    .SetProperty(e => e.ErrorMessage, (string?)null)
                    .SetProperty(e => e.AttemptCount, e => e.AttemptCount)
                    .SetProperty(e => e.NextAttemptOn, (DateTime?)null)
                    .SetProperty(e => e.ClaimId, (Guid?)null)
                    .SetProperty(e => e.UpdatedOn, now), cancellationToken)
                .ConfigureAwait(false);
        }
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
                            .SetProperty(e => e.NextAttemptOn, e => now.AddSeconds(
                                e.AttemptCount < 1 ? 10 :
                                e.AttemptCount < 2 ? 20 :
                                e.AttemptCount < 3 ? 40 :
                                e.AttemptCount < 4 ? 80 :
                                e.AttemptCount < 5 ? 160 :
                                e.AttemptCount < 6 ? 320 :
                                600))
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