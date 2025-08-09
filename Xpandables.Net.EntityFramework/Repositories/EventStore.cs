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
/// Represents a store for events, providing methods to append, fetch,
/// and mark events as published.
/// </summary>
public sealed class EventStore : Repository<DataContextEvent>, IEventStore
{
    private readonly ConcurrentBag<IEntityEvent> _disposableEntities = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="EventStore"/> class with the specified data context.
    /// </summary>
    /// <param name="context">The data context used to interact with the event store. 
    /// Cannot be <see langword="null"/>.</param>
    public EventStore(DataContextEvent context)
    {
        ArgumentNullException.ThrowIfNull(context);
        Context = context;
    }

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
        where TEntity : class, IEntity
    {
        ArgumentNullException.ThrowIfNull(filter);

        IQueryable<TResult> filteredQuery = filter(Context.Set<TEntity>().AsNoTracking());

        return DoFetchAsync(filteredQuery, cancellationToken);
    }

    /// <inheritdoc />
    public async Task MarkAsProcessedAsync(EventProcessedInfo info, CancellationToken cancellationToken = default)
    {
        string status = info.ErrorMessage is null
            ? EntityStatus.PUBLISHED
            : EntityStatus.ONERROR;

        await Context.Integrations
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

        // Group by success/error for more efficient updates
        var successfulEvents = infoArray.Where(i => i.ErrorMessage is null).ToArray();
        var failedEvents = infoArray.Where(i => i.ErrorMessage is not null).ToArray();

        // Update successful events
        if (successfulEvents.Length > 0)
        {
            var successIds = successfulEvents.Select(i => i.EventId).ToArray();
            await Context.Integrations
                .Where(e => successIds.Contains(e.KeyId))
                .ExecuteUpdateAsync(entity =>
                    entity
                        .SetProperty(e => e.Status, EntityStatus.PUBLISHED.Value)
                        .SetProperty(e => e.ErrorMessage, (string?)null)
                        .SetProperty(e => e.UpdatedOn, DateTime.UtcNow),
                    cancellationToken)
                .ConfigureAwait(false);
        }

        // Update failed events (needs individual processing due to different error messages)
        foreach (var failedEvent in failedEvents)
        {
            await Context.Integrations
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
            .GroupBy(e => e.GetType())
            .ToArray();

        foreach (var eventGroup in eventGroups)
        {
            await AppendEventGroupAsync(eventGroup, cancellationToken)
                .ConfigureAwait(false);
        }
    }

    private async Task AppendEventGroupAsync(IGrouping<Type, IEvent> eventGroup, CancellationToken cancellationToken)
    {
        // Get converter once per event type for efficiency
        IEventConverter eventConverter = EventConverter.GetConverterFor(eventGroup.Key);
        var entityEvents = new List<IEntityEvent>();

        // Convert all events of this type
        foreach (IEvent @event in eventGroup)
        {
            IEntityEvent entityEvent = eventConverter.ConvertTo(@event, DefaultSerializerOptions.Defaults);
            entityEvents.Add(entityEvent);
            _disposableEntities.Add(entityEvent);
        }

        // Add all converted events in a single batch operation
        await Context
            .AddRangeAsync(entityEvents, cancellationToken)
            .ConfigureAwait(false);
    }
}