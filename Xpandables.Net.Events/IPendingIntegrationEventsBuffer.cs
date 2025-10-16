
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
/// Per-request buffer for integration events to be enqueued to the outbox.
/// </summary>
public interface IPendingIntegrationEventsBuffer
{
    /// <summary>
    /// Adds the specified integration event to the system for processing.
    /// </summary>
    /// <param name="eventInstance">The integration event to be added. This parameter cannot be null.</param>
    void Add(IIntegrationEvent eventInstance);

    /// <summary>
    /// Adds a collection of integration events to the current instance.
    /// </summary>
    /// <param name="events">The collection of <see cref="IIntegrationEvent"/> instances to add. Cannot be null.</param>
    void AddRange(IEnumerable<IIntegrationEvent> events);

    /// <summary>
    /// Captures a snapshot of the current state of integration events.
    /// </summary>
    /// <returns>A read-only collection of <see cref="IIntegrationEvent"/> instances representing the current state.  The
    /// collection will be empty if no integration events are available.</returns>
    IReadOnlyCollection<IIntegrationEvent> Snapshot();

    /// <summary>
    /// Retrieves and removes all pending integration events from the queue.
    /// </summary>
    /// <returns>An enumerable collection of <see cref="IIntegrationEvent"/> representing the integration events  that were in
    /// the queue. The collection will be empty if no events are available.</returns>
    IEnumerable<IIntegrationEvent> Drain();
}