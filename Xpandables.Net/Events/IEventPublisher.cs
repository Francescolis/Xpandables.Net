
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
using Xpandables.Net.Operations;

namespace Xpandables.Net.Events;
/// <summary>
/// Defines a contract for publishing events.
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Publishes the specified event asynchronously.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    /// <param name="event">The event to publish.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation 
    /// requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task 
    /// result contains the operation result.</returns>
    Task<IOperationResult> PublishAsync<TEvent>(
        TEvent @event,
        CancellationToken cancellationToken = default)
        where TEvent : notnull, IEvent;

    /// <summary>
    /// Publishes the specified events asynchronously.
    /// </summary>
    /// <typeparam name="TEvent">The type of the events.</typeparam>
    /// <param name="events">The events to publish.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation 
    /// requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task 
    /// result contains the operation result.</returns>
    Task<IOperationResult> PublishAsync<TEvent>(
        IEnumerable<TEvent> events,
        CancellationToken cancellationToken = default)
        where TEvent : notnull, IEvent;
}
