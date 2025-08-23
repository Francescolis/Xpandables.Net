
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
public sealed class EventStore<TDataContext>(TDataContext context) : Repository<TDataContext>(context), IEventStore
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
    public async Task MarkAsProcessedAsync(EventProcessedInfo info, CancellationToken cancellationToken = default)
    {
        string status = info.ErrorMessage is null
            ? EntityStatus.PUBLISHED
            : EntityStatus.ONERROR;

        await Context.Set<EntityIntegrationEvent>()
            .Where(e => e.KeyId == info.EventId)
            .ExecuteUpdateAsync(entity =>
                    entity
                        .SetProperty(e => e.Status, status)
                        .SetProperty(e => e.ErrorMessage, info.ErrorMessage)
                        .SetProperty(e => e.UpdatedOn, DateTime.UtcNow),
                cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task MarkAsProcessedAsync(
        IEnumerable<EventProcessedInfo> infos,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(infos);

        var infoArray = infos as EventProcessedInfo[] ?? [.. infos];
        if (infoArray.Length == 0) return;

        var successfulEvents = infoArray.Where(i => i.ErrorMessage is null).ToArray();
        var failedEvents = infoArray.Where(i => i.ErrorMessage is not null).ToArray();

        if (successfulEvents.Length > 0)
        {
            var successIds = successfulEvents.Select(i => i.EventId).ToArray();
            await Context.Set<EntityIntegrationEvent>()
                .Where(e => successIds.Contains(e.KeyId))
                .ExecuteUpdateAsync(entity =>
                    entity
                        .SetProperty(e => e.Status, EntityStatus.PUBLISHED.Value)
                        .SetProperty(e => e.ErrorMessage, (string?)null)
                        .SetProperty(e => e.UpdatedOn, DateTime.UtcNow),
                    cancellationToken)
                .ConfigureAwait(false);
        }

        foreach (var failedEvent in failedEvents)
        {
            await Context.Set<EntityIntegrationEvent>()
                .Where(e => e.KeyId == failedEvent.EventId)
                .ExecuteUpdateAsync(entity =>
                    entity
                        .SetProperty(e => e.Status, EntityStatus.ONERROR.Value)
                        .SetProperty(e => e.ErrorMessage, failedEvent.ErrorMessage)
                        .SetProperty(e => e.UpdatedOn, DateTime.UtcNow),
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
}