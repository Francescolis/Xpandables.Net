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
using System.Text.Json;

using Xpandables.Net.Events.Entities;

namespace Xpandables.Net.Events.Converters;

/// <summary>
/// Defines methods for converting events to and from event entities.
/// </summary>
public interface IEventConverter
{
    /// <summary>
    /// Gets the type of the event.
    /// </summary>
    Type EventType { get; }

    /// <summary>
    /// Determines whether this instance can convert the specified type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns><see langword="true"/> if this instance can convert the specified 
    /// type; otherwise, <see langword="false"/>.</returns>
    bool CanConvert(Type type);

    /// <summary>
    /// Converts the specified event to an event entity.
    /// </summary>
    /// <param name="event">The event to convert.</param>
    /// <param name="options">The JSON serializer options.</param>
    /// <returns>The converted event entity.</returns>
    /// <exception cref="InvalidOperationException">Thrown when conversion 
    /// failed. See inner exception.</exception>
    IEventEntity ConvertTo(
        IEvent @event,
        JsonSerializerOptions? options = default);

    /// <summary>
    /// Converts the specified event entity to an event.
    /// </summary>
    /// <param name="entity">The event entity to convert.</param>
    /// <param name="options">The JSON serializer options.</param>
    /// <returns>The converted event.</returns>
    /// <exception cref="InvalidOperationException">Thrown when conversion
    /// failed. See inner exception.</exception>
    IEvent ConvertFrom(
        IEventEntity entity,
        JsonSerializerOptions? options = default);
}
