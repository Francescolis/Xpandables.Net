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
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Xpandables.Net.Events.Repositories;

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
    /// Converts the specified event to an entity event representation.
    /// </summary>
    /// <param name="eventInstance">The event to convert. Cannot be null.</param>
    /// <param name="serializerOptions">The serializer options to use when converting the event.</param>
    /// <returns>An <see cref="IEntityEvent"/> that represents the converted event.</returns>
    [RequiresUnreferencedCode("May use unreferenced code to convert IEntityEvent to IEvent.")]
    [RequiresDynamicCode("May use dynamic code to convert IEntityEvent to IEvent.")]
    IEntityEvent ConvertEventToEntity(IEvent eventInstance, JsonSerializerOptions serializerOptions);

    /// <summary>
    /// Converts the specified entity event to an event representation.
    /// </summary>
    /// <param name="entityInstance">The entity event to convert. Cannot be null.</param>
    /// <param name="serializerOptions">The serializer options to use when serializing the entity event.</param>
    /// <returns>An event representation of the specified entity event.</returns>
    [RequiresUnreferencedCode("May use unreferenced code to convert IEntityEvent to IEvent.")]
    [RequiresDynamicCode("May use dynamic code to convert IEntityEvent to IEvent.")]
    IEvent ConvertEntityToEvent(IEntityEvent entityInstance, JsonSerializerOptions serializerOptions);
}
