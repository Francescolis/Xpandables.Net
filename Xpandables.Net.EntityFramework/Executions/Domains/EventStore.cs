
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

using System.ComponentModel.DataAnnotations;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using Xpandables.Net.Executions.Tasks;
using Xpandables.Net.Repositories;
using Xpandables.Net.Repositories.Converters;
using Xpandables.Net.Repositories.Filters;

namespace Xpandables.Net.Executions.Domains;

/// <summary>
/// Represents a store for events, providing methods to append, fetch, 
/// and mark events as published.
/// </summary>
/// <param name="context">The data context for the event store.</param>
/// <param name="options">The event options.</param>
public sealed class EventStore(IOptions<EventOptions> options, DataContextEvent context) : Disposable, IEventStore
{
    private readonly EventOptions _options = options.Value;
#pragma warning disable CA2213 // Disposable fields should be disposed
    private readonly DataContextEvent _context = context;
#pragma warning restore CA2213 // Disposable fields should be disposed
    private readonly List<IEventEntity> _disposableEntities = [];

    /// <inheritdoc/>
    public async Task AppendAsync(
        IEvent @event,
        CancellationToken cancellationToken = default)
    {
        IEventConverter eventConverter = _options.GetEventConverterFor(@event);

        IEventEntity eventEntity = eventConverter.ConvertTo(@event, _options.SerializerOptions);

        _disposableEntities.Add(eventEntity);

        await _context
            .AddAsync(eventEntity, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task AppendAsync(
        IEnumerable<IEvent> events,
        CancellationToken cancellationToken = default)
    {
        List<IEvent> eventsList = [.. events];
        if (eventsList.Count == 0)
        {
            return;
        }

        foreach (IEvent @event in eventsList)
        {
            await AppendAsync(@event, cancellationToken)
                .ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<IEvent> FetchAsync(
        IEventFilter filter,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        IQueryable<IEventEntity> queryable = filter.EventType switch
        {
            Type type when type == typeof(IEventDomain) =>
                _context.Domains.AsNoTracking(),
            Type type when type == typeof(IEventIntegration) =>
                _context.Integrations.AsNoTracking(),
            Type type when type == typeof(IEventSnapshot) =>
            _context.Snapshots.AsNoTracking(),
            _ => throw new InvalidOperationException("The event type is not supported.")
        };

        IEventConverter eventConverter =
            _options.GetEventConverterFor(filter.EventType);

        IAsyncEnumerable<IEventEntity> entities =
            filter.FetchAsync(queryable, cancellationToken);

        return entities.Select(entity =>
        {
            try
            {
                return eventConverter.ConvertFrom(entity, _options.SerializerOptions);
            }
            finally
            {
                entity.Dispose();
            }
        });
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(
        IEventFilter filter,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        IQueryable<IEventEntity> queryable = filter.EventType switch
        {
            Type type when type == typeof(IEventDomain) =>
                _context.Domains,
            Type type when type == typeof(IEventIntegration) =>
                _context.Integrations,
            Type type when type == typeof(IEventSnapshot) =>
            _context.Snapshots,
            _ => throw new InvalidOperationException("The event type is not supported.")
        };

        try
        {
            await filter
                .Apply(queryable)
                .OfType<IEventEntity>()
                .ExecuteDeleteAsync(cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception)
            when (exception is not ValidationException and not InvalidOperationException)
        {
            throw new InvalidOperationException(
                "An error occurred while deleting the events.",
                exception);
        }
    }

    /// <inheritdoc/>
    public async Task MarkAsPublishedAsync(
        EventPublished eventPublished,
        CancellationToken cancellationToken = default)
    {
        try
        {
            string status = eventPublished.ErrorMessage is null
                ? EntityStatus.PUBLISHED : EntityStatus.ONERROR;

            await _context.Integrations
                .Where(e => e.KeyId == eventPublished.EventId)
                .ExecuteUpdateAsync(entity =>
                    entity
                    .SetProperty(e => e.Status, status)
                    .SetProperty(e => e.UpdatedOn, DateTime.UtcNow),
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception)
            when (exception is not ValidationException
            and not InvalidOperationException)
        {
            throw new InvalidOperationException(
                "An error occurred while marking the event as published.",
                exception);
        }
    }

    /// <inheritdoc/>
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
