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


using System.ComponentModel;

namespace Xpandables.Net.Events;

/// <summary>
/// Defines a contract for handling events asynchronously.
/// </summary>
/// <remarks>Implementations of this interface are responsible for processing specific types of events. The <see
/// cref="HandleAsync"/> method is invoked to handle an event instance, allowing for custom logic to be executed in
/// response to the event.</remarks>
public interface IEventHandler
{
    /// <summary>
    /// Handles the specified event asynchronously.
    /// </summary>
    /// <param name="event">The event instance to handle.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation if necessary.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the event handler encounters an issue while handling the event.</exception>
    Task HandleAsync(IEvent @event, CancellationToken cancellationToken = default);
}

/// <summary>
/// Defines a contract for handling events of a specific type.
/// </summary>
/// <typeparam name="TEvent">The type of event to be handled. The event type must implement the <see cref="IEvent"/> interface.</typeparam>
#pragma warning disable CA1711
public interface IEventHandler<in TEvent> : IEventHandler
#pragma warning restore CA1711
    where TEvent : class, IEvent
{
    /// <summary>
    /// Handles the specified event asynchronously.
    /// </summary>
    /// <param name="event">The event instance to handle.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation if necessary.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the event handler encounters an issue while handling the event.</exception>
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);

    [EditorBrowsable(EditorBrowsableState.Never)]
    Task IEventHandler.HandleAsync(IEvent @event, CancellationToken cancellationToken)
    {
        if (@event is not TEvent typedEvent)
        {
            throw new InvalidOperationException($"Cannot handle event of type {nameof(@event)} as {typeof(TEvent).Name}.");
        }

        return HandleAsync(typedEvent, cancellationToken);
    }
}