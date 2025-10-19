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
namespace Xpandables.Net.Events;

/// <summary>
/// Defines a contract for handling events of a specified type asynchronously.
/// </summary>
/// <remarks>Implementations should perform event-specific processing within the <see cref="HandleAsync"/> method.
/// This interface is typically used in event-driven architectures to decouple event publishing from handling
/// logic.</remarks>
/// <typeparam name="TEvent">The type of event to handle. Must implement <see cref="IEvent"/>.</typeparam>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "<Pending>")]
public interface IEventHandler<in TEvent>
    where TEvent : class, IEvent
{
    /// <summary>
    /// Handles the specified event asynchronously, allowing for cancellation via a token.
    /// </summary>
    /// <param name="eventInstance">The event instance to be processed. Cannot be null.</param>
    /// <param name="cancellationToken">A token that can be used to request cancellation of the operation. The default value is <see
    /// cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous handling operation.</returns>
    Task HandleAsync(TEvent eventInstance, CancellationToken cancellationToken = default);
}
