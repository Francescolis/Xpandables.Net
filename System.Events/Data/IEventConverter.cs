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
namespace System.Events.Data;

/// <summary>
/// Defines methods for converting between domain events and their corresponding entity event representations.
/// </summary>
/// <remarks>Implementations of this interface enable translation between domain-level events and
/// persistence-layer entity events, facilitating event sourcing and serialization scenarios. The interface supports
/// both strongly-typed and generic conversion methods, allowing for flexible integration with various event storage and
/// processing systems.</remarks>
/// <typeparam name="TEntityEvent">The type of the entity event that implements IEntityEvent. Must be a reference type.</typeparam>
/// <typeparam name="TEvent">The type of the domain event that implements IEvent.</typeparam>
public interface IEventConverter<TEntityEvent, TEvent>
    where TEntityEvent : class, IEntityEvent
    where TEvent : IEvent
{
    /// <summary>
    /// Converts the specified event to its corresponding entity representation.
    /// </summary>
    /// <param name="event">The event to convert. Cannot be null.</param>
    /// <param name="context">The context that provides additional information required for the conversion process. Cannot be null.</param>
    /// <returns>An entity representation of the specified event.</returns>
    TEntityEvent ConvertEventToEntity(TEvent @event, IEventConverterContext context);

    /// <summary>
    /// Converts the specified entity event to an event of type TEvent.
    /// </summary>
    /// <param name="event">The entity event to convert. Cannot be null.</param>
    /// <param name="context">The context that provides additional information required for the conversion. Cannot be null.</param>
    /// <returns>An event of type TEvent that represents the converted entity event.</returns>
    TEvent ConvertEntityToEvent(TEntityEvent @event, IEventConverterContext context);
}
