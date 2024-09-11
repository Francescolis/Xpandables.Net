
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

    /// <summary>
    /// When overridden in a derived class, appends the specified event to
    /// the store.
    /// </summary>
    /// <remarks>It is recommended to use the <see cref="CreateEntityEvent(IEvent)"/>
    /// to create the entity event while implementing this method.</remarks>
    ///<inheritdoc/>
    public abstract Task AppendEventAsync(
        IEvent @event,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates an entity event from the specified event.
    /// </summary>
    /// <param name="event">The event to act on.</param>
    /// <returns>The entity event created.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the
    /// <paramref name="event"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the operation
    /// fails. See inner exception for details.</exception>
    protected IEntityEvent CreateEntityEvent(IEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        IEventConverter converter = options
            .Value
            .GetEventConverterFor(@event);

        IEntityEvent entity = converter
            .ConvertTo(@event, options.Value.SerializerOptions);

        if (options
                 .Value
                 .DisposeEventEntityAfterPersistence)
        {
            Array.Resize(ref _disposables, _disposables.Length + 1);
            _disposables[^1] = entity;
        }

        return entity;
    }

    /// <summary>
    /// When overridden in a derived class, asynchronously fetches a collection
    /// of events matching the filter. Returns an empty collection if no events
    /// is found.
    /// </summary>
    /// <remarks>It is recommended to use the 
    /// <see cref="CreateEventsAsync(IEventFilter, IAsyncEnumerable{IEntityEvent}, CancellationToken)"/>
    /// to returns the events while implementing this method.</remarks>
    ///<inheritdoc/>
    public abstract IAsyncEnumerable<IEvent> FetchEventsAsync(
        IEventFilter eventFilter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates the events asynchronously.
    /// </summary>
    /// <param name="eventFilter">The event filter to act on.</param>
    /// <param name="entities">The entities to act on.</param>
    /// <param name="cancellationToken">A CancellationToken to observe while
    /// operation is in progress.</param>
    /// <exception cref="ArgumentNullException">Thrown when the
    /// <paramref name="eventFilter"/> or <paramref name="entities"/> is null.
    /// </exception>
    /// <exception cref="InvalidOperationException">Thrown when the operation
    /// fails. See inner exception for details.</exception>
    /// <returns>An <see cref="IAsyncEnumerable{T}"/> that allows asynchronous
    /// enumeration of the entity events.</returns>
    protected async IAsyncEnumerable<IEvent> CreateEventsAsync(
        IEventFilter eventFilter,
        IAsyncEnumerable<IEntityEvent> entities,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        IEventConverter converter = options
            .Value
            .GetEventConverterFor(eventFilter.Type);

        await foreach (IEntityEvent entity in entities
               .WithCancellation(cancellationToken))
        {
            yield return converter
                .ConvertFrom(entity, options.Value.SerializerOptions);
        }
    }

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
