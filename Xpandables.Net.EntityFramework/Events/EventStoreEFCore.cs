
/*******************************************************************************
 * Copyright (C) 2023 Francis-Black EWANE
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

using Xpandables.Net.Aggregates;
using Xpandables.Net.Repositories;

namespace Xpandables.Net.Events;

/// <summary>
/// Represents the <see cref="IEventStore"/> using EFCore.
/// </summary>
/// <param name="context">The target event data context.</param>
/// <param name="options">The event options.</param>
public sealed class EventStoreEFCore(
    IOptions<EventOptions> options,
    DataContextEvent context) : EventStore(options)
{
    ///<inheritdoc/>
    public override async Task AppendEventAsync(
        IEvent @event,
        CancellationToken cancellationToken = default)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(@event);

            IEntityEvent entity = CreateEntityEvent(@event);

            _ = await context
                .AddAsync(entity, cancellationToken)
                .ConfigureAwait(false);

        }
        catch (Exception exception)
            when (exception is not OperationCanceledException
                or InvalidOperationException
                or ArgumentNullException)
        {
            throw new InvalidOperationException(
                $"An error occurred while appending the event " +
                $"{@event.GetType().Name}.",
                exception);
        }
    }

    /// <inheritdoc/>
    public override IAsyncEnumerable<IEvent> FetchEventsAsync(
        IEventFilter eventFilter,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eventFilter);

        IQueryable queryable = GetQueryable(eventFilter);

        try
        {
            IAsyncEnumerable<IEntityEvent> entities =
                eventFilter.FetchAsync(queryable);

            return CreateEventsAsync(eventFilter, entities, cancellationToken);
        }
        catch (Exception exception)
            when (exception is not OperationCanceledException
                or InvalidOperationException
                or ArgumentNullException)
        {
            throw new InvalidOperationException(
                $"An error occurred while fetching events of type " +
                $"{eventFilter.Type.Name}.",
                exception);
        }
    }

    private IQueryable GetQueryable(IEventFilter eventFilter)
        => eventFilter.Type switch
        {
            Type type when type == typeof(IEventDomain)
                => context.Domains.AsNoTracking(),
            Type type when type == typeof(IEventIntegration)
                => context.Integrations.AsNoTracking(),
            Type type when type == typeof(IEventSnapshot)
                => context.Snapshots.AsNoTracking(),
            _ => throw new InvalidOperationException(
                $"The type {eventFilter.Type} is not supported.")
        };

    /// <inheritdoc/>
    public override async Task MarkEventAsPublishedAsync(
        Guid eventId,
        Exception? exception = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _ = await context
                .Integrations
                .Where(x => x.Id == eventId)
                .ExecuteUpdateAsync(setters =>
                    setters
                        .SetProperty(p => p.Status, EntityStatus.DELETED)
                        .SetProperty(p => p.UpdatedOn, DateTime.UtcNow)
                        .SetProperty(
                            p => p.ErrorMessage,
                            exception != null
                                ? exception.ToString()
                                : null),
                                cancellationToken)
                .ConfigureAwait(false);

        }
        catch (Exception ex)
            when (exception is not OperationCanceledException
                or InvalidOperationException)
        {
            throw new InvalidOperationException(
                "An error occurred while marking the event as published.",
                ex);
        }
    }

    /// <inheritdoc/>
    public override async Task<int> PersistEventsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await context
                    .SaveChangesAsync(cancellationToken)
                    .ConfigureAwait(false);
        }
        catch (Exception exception)
            when (exception is not OperationCanceledException
                or InvalidOperationException)
        {
            throw new InvalidOperationException(
                "An error occurred while persisting events.",
                exception);
        }
    }
}
