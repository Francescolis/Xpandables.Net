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

using System.Text.Json;

namespace System.Events.Repositories;

/// <summary>
/// Defines a factory for retrieving event converters based on event type.
/// </summary>
public interface IEventConverterFactory
{
    /// <summary>
    /// Gets the event converter for the specified event type.
    /// </summary>
    /// <param name="eventType">The type of the event to get the converter for. Cannot be null.</param>
    /// <returns>An <see cref="IEventConverter"/> that can convert the specified event type.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when no converter is found for the specified event type.</exception>
    IEventConverter GetEventConverter(Type eventType);

    /// <summary>
    /// Retrieves an event converter instance for the specified event type.
    /// </summary>
    /// <typeparam name="TEvent">The type of event for which to obtain a converter. Must implement <see cref="IEvent"/>.</typeparam>
    /// <returns>An <see cref="IEventConverter"/> instance capable of converting events of type <typeparamref name="TEvent"/>.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when no converter is found for the specified event type.</exception>
    IEventConverter GetEventConverter<TEvent>()
        where TEvent : IEvent;

    /// <summary>
    /// Converts the specified event to an entity event representation.
    /// </summary>
    /// <param name="eventInstance">The event to convert. Cannot be null.</param>
    /// <param name="serializerOptions">The serializer options to use when converting the event.</param>
    /// <returns>An <see cref="IEntityEvent"/> that represents the converted event.</returns>
    IEntityEvent ConvertEventToEntity(IEvent eventInstance, JsonSerializerOptions? serializerOptions = default);

    /// <summary>
    /// Converts the specified entity event to an event representation.
    /// </summary>
    /// <param name="entityInstance">The entity event to convert. Cannot be null.</param>
    /// <param name="serializerOptions">The serializer options to use when serializing the entity event.</param>
    /// <returns>An event representation of the specified entity event.</returns>
    IEvent ConvertEntityToEvent(IEntityEvent entityInstance, JsonSerializerOptions? serializerOptions = default);
}
