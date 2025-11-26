/*******************************************************************************
 * Copyright (C) 2025 Kamersoft
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

namespace System.Events;

/// <summary>
/// Defines a wrapper for handling events of a specific type asynchronously.
/// </summary>
/// <remarks>Implementations of this interface are responsible for processing events represented by the type
/// specified in <see cref="EventType"/>. This abstraction allows event handling logic to be decoupled from the event
/// dispatching mechanism. Implementations should ensure thread safety if they are intended to be used
/// concurrently.</remarks>
public interface IEventHandlerWrapper
{
    /// <summary>
    /// Gets the runtime type of the event associated with this instance.
    /// </summary>
    Type EventType { get; }

    /// <summary>
    /// Processes the specified instance asynchronously, allowing cancellation via a token.
    /// </summary>
    /// <param name="instance">The object instance to be handled. Cannot be null.</param>
    /// <param name="cancellationToken">A token that can be used to request cancellation of the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous handling operation.</returns>
    Task HandleAsync(object instance, CancellationToken cancellationToken = default);
}

/// <summary>
/// Provides a wrapper for a collection of event handlers that process events of a specified type.
/// </summary>
/// <typeparam name="TEvent">The type of event to be handled. Must implement <see cref="IEvent"/> and be a reference type.</typeparam>
/// <param name="handlers">The collection of event handlers that will be invoked to process events of type <typeparamref name="TEvent"/>.</param>
public sealed class EventHandlerWrapper<TEvent>(IEnumerable<IEventHandler<TEvent>> handlers) : IEventHandlerWrapper
    where TEvent : class, IEvent
{
    /// <inheritdoc />
    public Type EventType => typeof(TEvent);

    /// <inheritdoc />
    public async Task HandleAsync(object instance, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(instance);

        if (instance is not TEvent @event)
        {
            throw new ArgumentException($"Invalid event type. Expected {typeof(TEvent).Name}, but got {instance.GetType().Name}.", nameof(instance));
        }

        if (!handlers.Any())
        {
            return;
        }

        foreach (var handler in handlers)
        {
            await handler.HandleAsync(@event, cancellationToken).ConfigureAwait(false);
        }
    }
}
