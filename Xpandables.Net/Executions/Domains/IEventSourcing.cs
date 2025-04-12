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
namespace Xpandables.Net.Executions.Domains;

/// <summary>
/// Interface for event sourcing, providing methods to handle events.
/// </summary>
public interface IEventSourcing
{
    /// <summary>
    /// Marks all events as committed.
    /// </summary>
    void MarkEventsAsCommitted();

    /// <summary>
    /// Gets the collection of uncommitted events.
    /// </summary>
    /// <returns>A read-only collection of uncommitted events.</returns>
    IReadOnlyCollection<IEventDomain> GetUncommittedEvents();

    /// <summary>
    /// Loads events from history.
    /// </summary>
    /// <param name="events">The collection of events to load.</param>
    void LoadFromHistory(IEnumerable<IEventDomain> events);

    /// <summary>
    /// Loads a single event from history.
    /// </summary>
    /// <param name="event">The event to load.</param>
    void LoadFromHistory(IEventDomain @event);

    /// <summary>
    /// Adds an event to the system for processing or handling. 
    /// This allows the system to respond to specific occurrences.
    /// </summary>
    /// <param name="event">The input represents an occurrence that needs to be processed by the system.</param>
    void PushEvent(IEventDomain @event);
}
