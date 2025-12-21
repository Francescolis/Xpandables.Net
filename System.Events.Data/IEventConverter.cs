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
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace System.Events.Data;

/// <summary>
/// Defines methods for converting between domain event objects and their corresponding entity representations for
/// persistence or transport purposes.
/// </summary>
/// <remarks>Implementations of this interface enable serialization and deserialization of events to and from a
/// format suitable for storage or transmission, such as a database entity or a message payload. This abstraction allows
/// event handling infrastructure to work with different event types and storage mechanisms in a consistent
/// manner.</remarks>
public interface IEventConverter
{
    /// <summary>
    /// Gets the type of the event associated with this instance.
    /// </summary>
    Type EventType { get; }

    /// <summary>
    /// Determines whether the specified type can be converted by this converter.
    /// </summary>
    /// <param name="type">The type to evaluate for conversion support. Cannot be null.</param>
    /// <returns>true if the specified type can be converted; otherwise, false.</returns>
    bool CanConvert(Type type);

    /// <summary>
    /// Converts the specified event instance to an entity event using the provided JSON type information.
    /// </summary>
    /// <param name="eventInstance">The event instance to convert. Cannot be null.</param>
    /// <param name="typeInfo">The JSON type metadata used to guide the conversion process. Cannot be null.</param>
    /// <returns>An <see cref="IEntityEvent"/> representing the converted event.</returns>
    IEntityEvent ConvertEventToEntity(IEvent eventInstance, JsonTypeInfo typeInfo);

    /// <summary>
    /// Converts the specified event instance to an entity event using the provided serializer options.
    /// </summary>
    /// <param name="eventInstance">The event to convert. Cannot be null.</param>
    /// <param name="serializerOptions">The serializer options to use when converting the event.</param>
    /// <returns>An <see cref="IEntityEvent"/> that represents the converted event.</returns>
    [RequiresUnreferencedCode("Serialization may require types that are trimmed.")]
    [RequiresDynamicCode("Serialization may require types that are generated dynamically.")]
    IEntityEvent ConvertEventToEntity(IEvent eventInstance, JsonSerializerOptions? serializerOptions = default);

    /// <summary>
    /// Converts the specified entity event instance to an event representation using the provided JSON type
    /// information.
    /// </summary>
    /// <param name="entityInstance">The entity event instance to convert. Cannot be null.</param>
    /// <param name="typeInfo">The JSON type metadata used to guide the conversion process. Cannot be null.</param>
    /// <returns>An event object representing the converted entity event. The returned object implements the IEvent interface.</returns>
    IEvent ConvertEntityToEvent(IEntityEvent entityInstance, JsonTypeInfo typeInfo);

    /// <summary>
    /// Converts the specified entity event to an event representation using the provided serializer options.
    /// </summary>
    /// <param name="entityInstance">The entity event to convert. Cannot be null.</param>
    /// <param name="serializerOptions">The serializer options to use when serializing the entity event.</param>
    /// <returns>An event representation of the specified entity event.</returns>
    [RequiresUnreferencedCode("Deserialization may require types that are trimmed.")]
    [RequiresDynamicCode("Deserialization may require types that are generated dynamically.")]
    IEvent ConvertEntityToEvent(IEntityEvent entityInstance, JsonSerializerOptions? serializerOptions = default);
}
