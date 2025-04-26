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

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using Xpandables.Net.Executions.Tasks;
using Xpandables.Net.Repositories;
using Xpandables.Net.Repositories.Converters;
using Xpandables.Net.Repositories.Filters;
using Xpandables.Net.Text;

namespace Xpandables.Net.Executions.Domains;

/// <summary>
/// Represents a store for events, providing methods to append, fetch,
/// and mark events as published.
/// </summary>
/// <param name="context">The data context for the event store.</param>
/// <param name="options">The event options.</param>
// ReSharper disable once ClassNeverInstantiated.Global
public sealed class EventStore(IOptions<EventOptions> options, DataContextEvent context) : Disposable, IEventStore
{
    private readonly List<IEntityEvent> _disposableEntities = [];
    private readonly EventOptions _options = options.Value;

    /// <inheritdoc />
    public async Task AppendAsync(IEvent @event, CancellationToken cancellationToken = default)
    {
        IEventConverter eventConverter = _options.GetEventConverterFor(@event);

        IEntityEvent entityEvent = eventConverter.ConvertTo(@event, DefaultSerializerOptions.Defaults);

        _disposableEntities.Add(entityEvent);

        await context
            .AddAsync(entityEvent, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task AppendAsync(IEnumerable<IEvent> events, CancellationToken cancellationToken = default)
    {
        List<IEvent> eventsList = [.. events];
        if (eventsList.Count == 0)
        {
            return;
        }

        foreach (IEvent @event in eventsList)
        {
            await AppendAsync(@event, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public IAsyncEnumerable<IEvent> FetchAsync(IEventFilter filter, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        IQueryable<IEntityEvent> queryable = filter.EventType switch
        {
            { } type when type == typeof(IDomainEvent) =>
                context.Domains.AsNoTracking(),
            { } type when type == typeof(IIntegrationEvent) =>
                context.Integrations.AsNoTracking(),
            { } type when type == typeof(ISnapshotEvent) =>
                context.Snapshots.AsNoTracking(),
            _ => throw new InvalidOperationException("The event type is not supported.")
        };

        IEventConverter eventConverter = _options.GetEventConverterFor(filter.EventType);

        IAsyncEnumerable<IEntityEvent> entities = filter.FetchAsync(queryable, cancellationToken);

        return entities.Select(entity =>
        {
            try
            {
                return eventConverter.ConvertFrom(entity, DefaultSerializerOptions.Defaults);
            }
            finally
            {
                entity.Dispose();
            }
        });
    }

    /// <inheritdoc />
    public async Task MarkAsProcessedAsync(EventProcessed eventProcessed, CancellationToken cancellationToken = default)
    {
        string status = eventProcessed.ErrorMessage is null
            ? EntityStatus.PUBLISHED
            : EntityStatus.ONERROR;

        await context.Integrations
            .Where(e => e.KeyId == eventProcessed.EventId)
            .ExecuteUpdateAsync(entity =>
                    entity
                        .SetProperty(e => e.Status, status)
                        .SetProperty(e => e.ErrorMessage, eventProcessed.ErrorMessage)
                        .SetProperty(e => e.UpdatedOn, DateTime.UtcNow),
                cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _disposableEntities.ForEach(entity => entity.Dispose());
            _disposableEntities.Clear();
        }

        base.Dispose(disposing);
    }
}