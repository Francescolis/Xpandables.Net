﻿
/************************************************************************************************************
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
************************************************************************************************************/
using Xpandables.Net.Aggregates.DomainEvents;

namespace Xpandables.Net.Aggregates;

/// <summary>
/// Event-sourcing pattern interface using domain event.
/// </summary>
/// <typeparam name="TAggregateId">The type of aggregate Id</typeparam>
public interface IDomainEventSourcing<TAggregateId>
    where TAggregateId : struct, IAggregateId<TAggregateId>
{
    /// <summary>
    /// Marks all the domain events as committed.
    /// </summary>
    void MarkEventsAsCommitted();

    /// <summary>
    /// Returns a collection of uncommitted domain events.
    /// </summary>
    /// <returns>A list of uncommitted domain events.</returns>
    IReadOnlyCollection<IDomainEvent<TAggregateId>> GetUncommittedEvents();

    /// <summary>
    /// Initializes the underlying object with the specified history collection of domain events.
    /// </summary>
    /// <remarks>The domain events are not added to the collection of domain events.</remarks>
    /// <param name="events">The collection of domain events to act with.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="events"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The operation failed. See inner exception.</exception>
    void LoadFromHistory(IOrderedEnumerable<IDomainEvent<TAggregateId>> events);

    /// <summary>
    /// Applies the history specified domain event to the underlying object.
    /// </summary>
    /// <param name="event">The domain event to be applied.</param>
    /// <remarks>The domain event is not added to the collection of domain events.</remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="event"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The operation failed. See inner exception.</exception>
    void LoadFromHistory(IDomainEvent<TAggregateId> @event);
}
