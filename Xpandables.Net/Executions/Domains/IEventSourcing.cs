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
/// Represents an interface that defines operations for event sourcing in an aggregate domain model.
/// Event sourcing allows capturing domain changes (events) as a sequence, enabling replay and persistence of those events.
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
    IReadOnlyCollection<IDomainEvent> GetUncommittedEvents();

    /// <summary>
    /// Loads a collection of events from history into the aggregate root.
    /// </summary>
    /// <param name="events">The collection of events to load.</param>
    void LoadFromHistory(IEnumerable<IDomainEvent> events);

    /// <summary>
    /// Loads a single event from history.
    /// </summary>
    /// <param name="domainEvent">The event to load.</param>
    void LoadFromHistory(IDomainEvent domainEvent);

    /// <summary>
    /// Pushes an event into the aggregate root for processing.
    /// </summary>
    /// <param name="domainEvent"> The event to push.</param>
    void PushEvent(IDomainEvent domainEvent);
}