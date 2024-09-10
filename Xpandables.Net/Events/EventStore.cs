
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
using System.Runtime.CompilerServices;

using Microsoft.Extensions.Options;

namespace Xpandables.Net.Events;

/// <summary>
/// Represents the base event store implementation.
/// </summary>
public abstract class EventStore(
    IOptions<EventOptions> options) : Disposable, IEventStore
{
    private IDisposable[] _disposables = [];

    ///<inheritdoc/>
    public async Task AppendEventAsync(
        IEvent @event,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(@event);

        IEventConverter converter = options
            .Value
            .GetEventConverterFor(@event);

        IEntityEvent entity = converter
            .ConvertTo(@event, options.Value.SerializerOptions);

        try
        {
            if (options
                .Value
                .DisposeEventEntityAfterPersistence)
            {
                Array.Resize(ref _disposables, _disposables.Length + 1);
                _disposables[^1] = entity;
            }

            await AppendEventCoreAsync(entity, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception)
            when (exception is not OperationCanceledException
                or InvalidOperationException)
        {
            throw new InvalidOperationException(
                $"An error occurred while appending the event " +
                $"{@event.GetType().Name}.",
                exception);
        }
    }

    /// <summary>
    /// When overridden in a derived class, appends the specified entity to 
    /// the store.
    /// </summary>
    /// <param name="entity">The entity to act on.</param>
    /// <param name="cancellationToken">A CancellationToken to observe while 
    /// waiting for the task to complete.</param>
    /// <returns>A value that represents an asynchronous operation.</returns>
    protected abstract Task AppendEventCoreAsync(
        IEntityEvent entity,
        CancellationToken cancellationToken = default);

    ///<inheritdoc/>
    public async IAsyncEnumerable<IEvent> FetchEventsAsync(
        IEventFilter eventFilter,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eventFilter);
        await Task.Yield();

        IEventConverter converter = options
            .Value
            .GetEventConverterFor(eventFilter.Type);

        IQueryable queryable;
        IAsyncEnumerable<IEntityEvent> entities;

        try
        {
            queryable = GetQueryableCore(eventFilter);
            entities = eventFilter.FetchAsync(queryable);
        }
        catch (Exception exception)
            when (exception is not OperationCanceledException
                or InvalidOperationException)
        {
            throw new InvalidOperationException(
                $"An error occurred while fetching events of type " +
                $"{eventFilter.Type.Name}.",
                exception);
        }

        await foreach (IEntityEvent entity in entities
            .WithCancellation(cancellationToken))
        {
            yield return converter
                .ConvertFrom(entity, options.Value.SerializerOptions);
        }
    }

    /// <summary>
    /// When overridden in a derived class, returns the data source to be 
    /// filtered from the store.
    /// </summary>
    /// <param name="eventFilter">The event filter to act on.</param>
    /// <returns>An <see cref="IQueryable"/> that represents the data 
    /// source.</returns>
    protected abstract IQueryable GetQueryableCore(IEventFilter eventFilter);

    /// <summary>
    /// When overridden in a derived class, marks the event as published.
    /// </summary>
    /// <param name="eventId">The event id to act on.</param>
    /// <param name="exception">The optional handled exception during publishing
    /// the event.</param>
    /// <param name="cancellationToken">A CancellationToken to observe while
    /// waiting for the task to complete.</param>
    /// <returns>A value that represents an asynchronous operation.</returns>
    /// <inheritdoc/>
    public abstract Task MarkEventAsPublishedAsync(
        Guid eventId,
        Exception? exception = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// When overridden in a derived class, persists pending entities 
    /// to the store.
    /// </summary>
    /// <param name="cancellationToken">A CancellationToken 
    /// to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the number of persisted objects
    /// .</returns>
    /// <inheritdoc/>
    public abstract Task<int> PersistEventsAsync(
        CancellationToken cancellationToken = default);

    ///<inheritdoc/>
    protected sealed override async ValueTask DisposeAsync(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        if (options
            .Value
            .DisposeEventEntityAfterPersistence)
        {
            foreach (IDisposable disposable in _disposables)
            {
                disposable?.Dispose();
            }
        }

        await base.DisposeAsync(disposing)
            .ConfigureAwait(false);
    }
}
