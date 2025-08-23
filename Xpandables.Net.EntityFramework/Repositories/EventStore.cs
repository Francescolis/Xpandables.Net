
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
using System.Collections.Concurrent;

using Microsoft.EntityFrameworkCore;

using Xpandables.Net.Collections;
using Xpandables.Net.Events;
using Xpandables.Net.Repositories;
using Xpandables.Net.Repositories.Converters;
using Xpandables.Net.Text;

namespace Xpandables.Net.Repositories;

/// <summary>
/// Provides functionality for storing and managing events in a database context.
/// </summary>
/// <typeparam name="TDataContext">The type of the database context used by the event store. Must derive from <see cref="DataContext"/>.</typeparam>
/// <remarks>The <see cref="EventStore{TDataContext}"/> class is a specialized repository for handling event-sourced data. It
/// supports appending single or multiple events, marking events as processed, and fetching paginated results. This
/// class is designed to work with Entity Framework Core and ensures proper disposal of resources.</remarks>
/// <param name="context"></param>
public sealed class EventStore<TDataContext>(TDataContext context) : Repository<TDataContext>(context), IEventStore, IIntegrationOutboxStore
    where TDataContext : DataContext
{
    private readonly ConcurrentBag<IEntityEvent> _disposableEntities = [];

    /// <inheritdoc />
    public async Task AppendAsync(IEvent @event, CancellationToken cancellationToken = default)
    {
        IEventConverter eventConverter = EventConverter.GetConverterFor(@event);
        IEntityEvent entityEvent = eventConverter.ConvertTo(@event, DefaultSerializerOptions.Defaults);

        _disposableEntities.Add(entityEvent);
        await Context.AddAsync(entityEvent, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task AppendAsync(IEnumerable<IEvent> events, CancellationToken cancellationToken = default)
    {
        IEvent[] eventsArray = events as IEvent[] ?? [.. events];
        if (eventsArray.Length == 0) return;

        await AppendEventBatchAsync(eventsArray, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public new IAsyncPagedEnumerable<TResult> FetchAsync<TEntity, TResult>(
        Func<IQueryable<TEntity>, IQueryable<TResult>> filter,
        CancellationToken cancellationToken = default)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(filter);

        IQueryable<TResult> filteredQuery = filter(Context.Set<TEntity>().AsNoTracking());
        return filteredQuery.WithPagination();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<IIntegrationEvent>> ClaimPendingAsync(
        int batchSize, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var claimId = Guid.NewGuid();
        var eventsSet = Context.Set<EntityIntegrationEvent>();

        // 1) Select candidate ids
        var candidateIds = await eventsSet
            .Where(e =>
                (e.Status == EntityStatus.PENDING.Value) ||
                (e.Status == EntityStatus.ONERROR.Value && (e.NextAttemptOn == null || e.NextAttemptOn <= now)))
            .Where(e => e.ClaimId == null)
            .OrderBy(e => e.Sequence)
            .Select(e => e.KeyId)
            .Take(Math.Max(1, batchSize))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (candidateIds.Count == 0) return [];

        // 2) Claim them (best-effort, another instance may have raced; ClaimId==null prevents double-claim)
        var updated = await eventsSet
            .Where(e => candidateIds.Contains(e.KeyId) && e.ClaimId == null)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(e => e.Status, EntityStatus.PROCESSING.Value)
                .SetProperty(e => e.ClaimId, claimId)
                .SetProperty(e => e.ErrorMessage, (string?)null)
                .SetProperty(e => e.UpdatedOn, now),
                cancellationToken)
            .ConfigureAwait(false);

        if (updated == 0) return [];

        // 3) Load the claimed rows for this claimId and convert to IIntegrationEvent
        var claimed = await eventsSet
            .AsNoTracking()
            .Where(e => e.ClaimId == claimId)
            .OrderBy(e => e.Sequence)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        IEventConverter converter = EventConverter.GetConverterFor<IIntegrationEvent>();
        var integrationEvents = new List<IIntegrationEvent>(claimed.Count);
        foreach (var entity in claimed)
        {
            if (converter.ConvertFrom(entity, DefaultSerializerOptions.Defaults) is IIntegrationEvent ie)
            {
                integrationEvents.Add(ie);
            }
        }

        return integrationEvents;
    }

    /// <inheritdoc />
    public async Task MarkAsProcessedAsync(EventProcessedInfo info, CancellationToken cancellationToken = default)
    {
        string status = info.ErrorMessage is null
            ? EntityStatus.PUBLISHED
            : EntityStatus.ONERROR;
        DateTime now = DateTime.UtcNow;
        bool success = info.ErrorMessage is null;

        await Context.Set<EntityIntegrationEvent>()
            .Where(e => e.KeyId == info.EventId)
            .ExecuteUpdateAsync(entity =>
                    entity
                        .SetProperty(e => e.Status, status)
                        .SetProperty(e => e.ErrorMessage, info.ErrorMessage)
                        .SetProperty(e => e.AttemptCount, success ? e => e.AttemptCount : e => e.AttemptCount + 1)
                        .SetProperty(e => e.NextAttemptOn, success ? e => null : e => GetNextAttempt(now, e.AttemptCount + 1))
                        .SetProperty(e => e.ClaimId, (Guid?)null)
                        .SetProperty(e => e.UpdatedOn, now),
                cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task MarkAsProcessedAsync(
        IEnumerable<EventProcessedInfo> infos,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(infos);

        var items = infos as EventProcessedInfo[] ?? [.. infos];
        if (items.Length == 0) return;

        var successIds = items.Where(i => i.ErrorMessage is null).Select(i => i.EventId).ToArray();
        var failed = items.Where(i => i.ErrorMessage is not null).ToArray();
        var now = DateTime.UtcNow;

        if (successIds.Length > 0)
        {
            await Context.Set<EntityIntegrationEvent>()
                .Where(e => successIds.Contains(e.KeyId))
                .ExecuteUpdateAsync(entity =>
                    entity
                        .SetProperty(e => e.Status, EntityStatus.PUBLISHED.Value)
                        .SetProperty(e => e.ErrorMessage, (string?)null)
                        .SetProperty(e => e.NextAttemptOn, (DateTime?)null)
                        .SetProperty(e => e.ClaimId, (Guid?)null)
                        .SetProperty(e => e.UpdatedOn, now),
                    cancellationToken)
                .ConfigureAwait(false);
        }

        foreach (var f in failed)
        {
            await Context.Set<EntityIntegrationEvent>()
                .Where(e => e.KeyId == f.EventId)
                .ExecuteUpdateAsync(entity =>
                    entity
                        .SetProperty(e => e.Status, EntityStatus.ONERROR.Value)
                        .SetProperty(e => e.ErrorMessage, f.ErrorMessage)
                        .SetProperty(e => e.AttemptCount, e => e.AttemptCount + 1)
                        .SetProperty(e => e.NextAttemptOn, e => GetNextAttempt(now, e.AttemptCount + 1))
                        .SetProperty(e => e.ClaimId, (Guid?)null)
                        .SetProperty(e => e.UpdatedOn, now),
                    cancellationToken)
                .ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    protected override ValueTask DisposeAsync(bool disposing)
    {
        if (disposing)
        {
            _disposableEntities.ForEach(entity => entity.Dispose());
            _disposableEntities.Clear();
        }

        return base.DisposeAsync(disposing);
    }

    private async Task AppendEventBatchAsync(IEvent[] events, CancellationToken cancellationToken)
    {
        var eventGroups = events
            .GroupBy(e =>
            {
                if (e is IDomainEvent) return typeof(IDomainEvent);
                if (e is IIntegrationEvent) return typeof(IIntegrationEvent);
                if (e is ISnapshotEvent) return typeof(ISnapshotEvent);
                throw new InvalidOperationException($"Unsupported event type: {e.GetType().FullName}.");
            })
            .ToArray();

        foreach (var eventGroup in eventGroups)
        {
            await AppendEventGroupAsync(eventGroup, cancellationToken)
                .ConfigureAwait(false);
        }
    }

    private async Task AppendEventGroupAsync(IGrouping<Type, IEvent> eventGroup, CancellationToken cancellationToken)
    {
        IEventConverter eventConverter = EventConverter.GetConverterFor(eventGroup.Key);
        var entityEvents = new List<IEntityEvent>();

        foreach (IEvent @event in eventGroup)
        {
            IEntityEvent entityEvent = eventConverter.ConvertTo(@event, DefaultSerializerOptions.Defaults);
            entityEvents.Add(entityEvent);
            _disposableEntities.Add(entityEvent);
        }

        await Context
            .AddRangeAsync(entityEvents, cancellationToken)
            .ConfigureAwait(false);
    }

    // Simple exponential backoff with cap (customize as needed)
    private static DateTime GetNextAttempt(DateTime now, int attemptCount)
    {
        // base 10s, double each time, max 10 minutes
        var delay = TimeSpan.FromSeconds(Math.Min(600, 10 * Math.Pow(2, Math.Min(10, attemptCount - 1))));
        return now.Add(delay);
    }
}