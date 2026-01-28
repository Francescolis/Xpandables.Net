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

namespace System.Events.Domain;

/// <summary>
/// Defines the contract for event sourcing capabilities, including managing and retrieving domain events for an
/// aggregate or entity.
/// </summary>
/// <remarks>Implementations of this interface enable tracking, loading, and committing domain events to support
/// event sourcing patterns. This interface is typically used in domain-driven design to reconstruct aggregate state
/// from a sequence of events and to manage uncommitted changes prior to persistence.</remarks>
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
    /// Returns and clears the uncommitted events.
    /// </summary>
    IReadOnlyCollection<IDomainEvent> DequeueUncommittedEvents();

    /// <summary>
    /// Reconstructs the current state by applying a sequence of domain events.
    /// </summary>
    /// <remarks>This method is typically used to restore an aggregate's state from its event history, such as
    /// when rehydrating from an event store. The method does not clear existing state before applying the events;
    /// ensure the target object is in a valid initial state before calling this method.</remarks>
    /// <param name="events">The collection of domain events to apply in order. The events should be provided in the order they occurred.</param>
    void LoadFromHistory(IEnumerable<IDomainEvent> events);

    /// <summary>
    /// Replays the specified domain event to restore the object's state from its event history.
    /// </summary>
    /// <param name="event">The domain event to apply. Cannot be null.</param>
    void LoadFromHistory(IDomainEvent @event);

    /// <summary>
    /// Appends a domain event to the event stream.
    /// </summary>
    /// <param name="event">The domain event to be appended. Cannot be null.</param>
    void AppendEvent(IDomainEvent @event);

    /// <summary>
    /// Appends a versioning domain event to the event stream.
    /// </summary>
    /// <param name="event">The domain event to be appended. Cannot be null.</param>
    void AppendVersioningEvent(IDomainEvent @event);

    /// <summary>
    /// Appends a versioning event to the event stream using the specified event factory.
    /// </summary>
    /// <remarks>The event factory is invoked with the next available version number for the event stream. Use
    /// this method to ensure that versioning is handled consistently when creating and pushing domain events.</remarks>
    /// <typeparam name="TEvent">The type of domain event to be created and pushed. Must implement IDomainEvent and cannot be null.</typeparam>
    /// <param name="eventFactory">A function that takes the next version number as a parameter and returns an instance of the event to be appended.
    /// Cannot be null.</param>
    void AppendVersioningEvent<TEvent>(Func<long, TEvent> eventFactory)
        where TEvent : notnull, IDomainEvent;
}
