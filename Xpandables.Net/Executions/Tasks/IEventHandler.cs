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

namespace Xpandables.Net.Executions.Tasks;

/// <summary>
/// Defines a handler for events of type <see cref="IEvent"/>.
/// </summary>
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
public interface IEventHandler
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
{
    /// <summary>
    /// Handles the specified event asynchronously.
    /// </summary>
    /// <param name="event">The event to handle.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation 
    /// requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Handling the event failed.
    /// See inner exception for details.</exception>
    Task Handle(
        object @event,
        CancellationToken cancellationToken = default);
}


/// <summary>
/// Defines a handler for events of type <typeparamref name="TEvent"/>.
/// </summary>
/// <typeparam name="TEvent">The type of event to handle.</typeparam>
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
public interface IEventHandler<TEvent> : IEventHandler
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
    where TEvent : notnull, IEvent
{
    /// <summary>
    /// Handles the specified event asynchronously.
    /// </summary>
    /// <param name="event">The event to handle.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation 
    /// requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Handling the event failed.
    /// See inner exception for details.</exception>
    Task HandleAsync(
        TEvent @event,
        CancellationToken cancellationToken = default);

    [EditorBrowsable(EditorBrowsableState.Never)]
#pragma warning disable CA1033 // Interface methods should be callable by child types
    Task IEventHandler.Handle(
#pragma warning restore CA1033 // Interface methods should be callable by child types
        object @event,
        CancellationToken cancellationToken) =>
        HandleAsync((TEvent)@event, cancellationToken);
}
