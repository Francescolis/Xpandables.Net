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
using System.Diagnostics.CodeAnalysis;

namespace Xpandables.Net.Events;

/// <summary>
/// Defines a contract for asynchronously publishing events to all registered event handlers.
/// </summary>
/// <remarks>Implementations of this interface are responsible for delivering events to all appropriate
/// subscribers. Thread safety and delivery guarantees may vary depending on the implementation. Events are dispatched
/// based on their type, and only handlers registered for the specific event type will be invoked.</remarks>
public interface IPublisher
{
    /// <summary>
    /// Publishes an event asynchronously to all subscribed handlers.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event being published.</typeparam>
    /// <param name="eventInstance">The event to be published.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous publishing operation.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when publishing the event fails. See inner exception for additional details.
    /// </exception>
    [RequiresDynamicCode("The event type might require dynamic code generation for certain operations.")]
    [RequiresUnreferencedCode("The event type might require reflection which could be trimmed.")]
    Task PublishAsync<TEvent>(TEvent eventInstance, CancellationToken cancellationToken = default)
        where TEvent : class, IEvent;
}