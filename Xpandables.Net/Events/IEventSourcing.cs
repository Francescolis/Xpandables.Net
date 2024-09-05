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

namespace Xpandables.Net.Events;

/// <summary>
/// Event-sourcing pattern interface using domain event.
/// </summary>
public interface IEventSourcing
{
    /// <summary>
    /// Marks all the domain events as committed.
    /// </summary>
    void MarkEventsAsCommitted();

    /// <summary>
    /// Returns a collection of uncommitted domain events.
    /// </summary>
    /// <returns>A list of uncommitted domain events.</returns>
    IReadOnlyCollection<IEventDomain> GetUncommittedEvents();

    /// <summary>
    /// Initializes the underlying object with the 
    /// specified history collection of domain events.
    /// </summary>
    /// <remarks>The domain events are not added to the 
    /// collection of domain events.</remarks>
    /// <param name="events">The collection of domain events to act with.</param>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="events"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The operation failed. 
    /// See inner exception.</exception>
    void LoadFromHistory(IEnumerable<IEventDomain> events);

    /// <summary>
    /// Applies the history specified domain event to the underlying object.
    /// </summary>
    /// <param name="event">The domain event to be applied.</param>
    /// <remarks>The domain event is not added to
    /// the collection of domain events.</remarks>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="event"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The operation failed. 
    /// See inner exception.</exception>
    void LoadFromHistory(IEventDomain @event);

    /// <summary>
    /// Pushes the specified domain event to the aggregate instance.
    /// </summary>
    /// <typeparam name="TEventDomain">The type of the event.</typeparam>
    /// <param name="event">The domain event instance to act on.</param>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="event"/> is null.</exception>
    void PushEvent<TEventDomain>(TEventDomain @event)
        where TEventDomain : notnull, IEventDomain;
}