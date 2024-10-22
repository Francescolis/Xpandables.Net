
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

using Xpandables.Net.Events.Converters;
using Xpandables.Net.Repositories;

namespace Xpandables.Net.Events;

/// <summary>
/// Represents a store for events, providing methods to append, fetch, 
/// and mark events as published.
/// </summary>
public sealed class EventStore(
    IOptions<EventOptions> options, DataContextEvent context) :
    Disposable, IEventStore
{
    private readonly EventOptions _options = options.Value;
    private readonly DataContextEvent _context = context;
    private readonly List<IDisposable> _disposables = [];

    /// <inheritdoc/>
    public Task AppendAsync(
        IEnumerable<IEvent> events,
        CancellationToken cancellationToken = default)
    {
        try
        {
            IEventConverter eventConverter =
                _options.GetEventConverterFor(events.First());

            List<IEventEntity> eventEntities = new(events.Count());

            foreach (IEvent @event in events)
            {
                IEventEntity eventEntity = eventConverter
                    .ConvertTo(@event, _options.SerializerOptions);

                eventEntities.Add(eventEntity);

                if (_options.DisposeEventEntityAfterPersistence)
                {
                    _disposables.Add(eventEntity);
                }
            }

            return _context.AddRangeAsync(eventEntities, cancellationToken);
        }
        catch (Exception exception)
            when (exception is not InvalidOperationException)
        {
            throw new InvalidOperationException(
                "An error occurred while appending the events.",
                exception);
        }
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<IEvent> FetchAsync(
        IEventFilter filter,
        CancellationToken cancellationToken = default)
    {
        IQueryable<IEventEntity> queryable = filter.EventType switch
        {
            Type type when type == typeof(IEventDomain) => _context.Domains,
            Type type when type == typeof(IEventIntegration) => _context.Integrations,
            Type type when type == typeof(IEventSnapshot) => _context.Snapshots,
            _ => throw new InvalidOperationException("The event type is not supported.")
        };

        try
        {
            IEventConverter eventConverter =
                _options.GetEventConverterFor(filter.EventType);

            IAsyncEnumerable<IEventEntity> entities =
                filter.FetchAsync(queryable, cancellationToken)
                .OfType<IEventEntity>();

            return entities.Select(entity =>
                eventConverter.ConvertFrom(entity, _options.SerializerOptions));
        }
        catch (Exception exception)
            when (exception is not InvalidOperationException)
        {
            throw new InvalidOperationException(
                "An error occurred while fetching the events.",
                exception);
        }
    }

    /// <inheritdoc/>
    public Task MarkAsPublishedAsync(
    IEnumerable<EventPublished> events,
    CancellationToken cancellationToken = default)
    {
        try
        {
            Dictionary<Guid, EventPublished> publishedEvents =
                events.ToDictionary(e => e.EventId, e => e);

            return _context.Integrations
                .Where(e => publishedEvents.Keys.Contains(e.Id))
                .ExecuteUpdateAsync(setters =>
                    setters
                        .SetProperty(p => p.Status, EntityStatus.PUBLISHED)
                        .SetProperty(p => p.UpdatedOn, DateTime.UtcNow)
                        .SetProperty(p => p.ErrorMessage, p => publishedEvents[p.Id].ErrorMessage),
                        cancellationToken);
        }
        catch (Exception exception)
            when (exception is not InvalidOperationException)
        {
            throw new InvalidOperationException(
                "An error occurred while marking the events as published.",
                exception);
        }
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (_options.DisposeEventEntityAfterPersistence)
            {
                foreach (IDisposable disposable in _disposables)
                {
                    disposable?.Dispose();
                }
            }
        }

        base.Dispose(disposing);
    }
}
