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
namespace Xpandables.Net.Aggregates;

/// <summary>
/// Allows application author to map domain event to notification 
/// in a Centralized Event Mapper.
/// </summary>
/// <typeparam name="TAggregateId">The type of aggregate Id.</typeparam>
/// <remarks>The implementation should be a singleton class or 
/// a class with singleton lifetime.</remarks>
public interface IEventDomainMapper<TAggregateId>
    where TAggregateId : struct, IAggregateId<TAggregateId>
{
    /// <summary>
    /// Maps the specified domain event to a notification.
    /// </summary>
    /// <param name="event">The domain event to be mapped.</param>
    /// <returns>A  notification from the domain event mapped.</returns>
    IEventNotification Map(IEventDomain<TAggregateId> @event);

    /// <summary>
    /// Maps the specified domain events to notifications.
    /// </summary>
    /// <param name="events">The collection of domain events to be  
    /// mapped.</param>
    /// <returns>A collection of notifications from the domain events mapped
    /// .</returns>
    public IEnumerable<IEventNotification> MapAll(
        IEnumerable<IEventDomain<TAggregateId>> events)
        => events.Select(Map);
}
