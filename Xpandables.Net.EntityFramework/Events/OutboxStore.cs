
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
using System.Diagnostics.CodeAnalysis;

using Microsoft.EntityFrameworkCore;

using Xpandables.Net;
using Xpandables.Net.Events;
using Xpandables.Net.Events.Repositories;
using Xpandables.Net.Repositories;

namespace Xpandables.Net.Events;

/// <summary>
/// Provides an implementation of an outbox pattern for managing integration events.
/// </summary>
/// <remarks>The <see cref="OutboxStore"/> class is designed to facilitate reliable event processing
/// by implementing the outbox pattern. It ensures that integration events are stored, claimed, and processed in a
/// consistent and fault-tolerant manner. This class supports operations such as enqueuing events, claiming pending
/// events for processing, marking events as completed, and handling event failures.</remarks>
/// <param name="context">The data context used for accessing the outbox store.</param>
public sealed class OutboxStore(OutboxStoreDataContext context) : IOutboxStore
{
    private readonly OutboxStoreDataContext _db = context
        ?? throw new ArgumentNullException(nameof(context));

    /// <inheritdoc />
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
    public async Task EnqueueAsync(
        CancellationToken cancellationToken,
        params IIntegrationEvent[] events)
    {
        ArgumentNullException.ThrowIfNull(events);
        ArgumentOutOfRangeException.ThrowIfEqual(events.Length, 0);

        var converter = EventConverter.GetConverterFor<IIntegrationEvent>();
        var list = new List<IEntityEventIntegration>(events.Length);

        foreach (var @event in events)
        {
            var entity = (EntityIntegrationEvent)converter.ConvertEventToEntity(@event);
            entity.SetStatus(EntityStatus.PENDING);
            list.Add(entity);
        }

        await _db.AddRangeAsync(list, cancellationToken).ConfigureAwait(false);
        // defer SaveChanges to Unit of Work
    }

    /// <inheritdoc />
    [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public async Task<IReadOnlyList<IIntegrationEvent>> DequeueAsync(
        CancellationToken cancellationToken,
        int maxEvents = 10,
        TimeSpan? visibilityTimeout = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxEvents);

        var now = DateTime.UtcNow;
        var lease = visibilityTimeout ?? TimeSpan.FromMinutes(5);
        var claimId = Guid.NewGuid();
        var set = _db.Set<EntityIntegrationEvent>();

        var candidateIds = await set
            .Where(e =>
                (e.Status == EntityStatus.PENDING.Value) ||
                (e.Status == EntityStatus.ONERROR.Value && (e.NextAttemptOn == null || e.NextAttemptOn <= now)))
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
                .SetProperty(e => e.Status, EntityStatus.PROCESSING.Value)
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

        var converter = EventConverter.GetConverterFor<IIntegrationEvent>();
        var list = new List<IIntegrationEvent>(claimed.Count);
        foreach (var entity in claimed)
        {
            if (converter.ConvertEntityToEvent(entity) is IIntegrationEvent ie)
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
        await _db.Set<EntityIntegrationEvent>()
            .Where(e => eventIds.Contains(e.KeyId))
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
        params FailedEvent[] failures)
    {
        ArgumentNullException.ThrowIfNull(failures);
        ArgumentOutOfRangeException.ThrowIfEqual(failures.Length, 0);

        var now = DateTime.UtcNow;
        foreach (var failure in failures)
        {
            await _db.Set<EntityIntegrationEvent>()
                .Where(e => e.KeyId == failure.EventId)
                .ExecuteUpdateAsync(updater => updater
                    .SetProperty(e => e.Status, EntityStatus.ONERROR.Value)
                    .SetProperty(e => e.ErrorMessage, failure.Error)
                    .SetProperty(e => e.AttemptCount, e => e.AttemptCount + 1)
                    .SetProperty(e => e.NextAttemptOn, e => GetNextAttempt(now, e.AttemptCount + 1))
                    .SetProperty(e => e.ClaimId, (Guid?)null)
                    .SetProperty(e => e.UpdatedOn, now), cancellationToken)
                .ConfigureAwait(false);
        }
    }

    private static DateTime GetNextAttempt(DateTime now, int attemptCount)
    {
        var delay = TimeSpan.FromSeconds(Math.Min(600, 10 * Math.Pow(2, Math.Min(10, attemptCount - 1))));
        return now.Add(delay);
    }
}