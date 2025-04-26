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

namespace Xpandables.Net.Executions.Tasks;

/// <summary>
/// Defines a contract for handling events.
/// </summary>
/// <typeparam name="TEvent">The type of event to handle.</typeparam>
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
public interface IEventHandler<in TEvent>
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
    where TEvent : class, IEvent
{
    /// <summary>
    /// Handles the specified event asynchronously.
    /// </summary>
    /// <param name="event">The event to handle.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when then event handler fails.</exception>
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}