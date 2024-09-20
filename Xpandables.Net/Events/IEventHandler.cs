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
using Xpandables.Net.Operations;

namespace Xpandables.Net.Events;

/// <summary>
/// Allows an application author to define a generic handler 
/// for domain events in a Distributed Event Mapper.
/// The domain event must implement 
/// <see cref="IEventDomain"/> interface.
/// The implementation must be thread-safe when
/// working in a multi-threaded environment.
/// </summary>
/// <typeparam name="TEvent">The event type.</typeparam>
public interface IEventHandler<TEvent>
    where TEvent : notnull, IEvent
{
    /// <summary>
    /// Handles the event.
    /// </summary>
    /// <param name="event">The event to handle.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation 
    /// requests.</param>
    Task<IOperationResult> HandleAsync(
        TEvent @event,
        CancellationToken cancellationToken = default);
}
