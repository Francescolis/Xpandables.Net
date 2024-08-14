
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
using System.Text.Json;

using Xpandables.Net.Distribution;

namespace Xpandables.Net.Aggregates.Events;

/// <summary>
/// Represents a converter to convert events to entities and vice versa.
/// </summary>
public interface IEventConverter
{
    /// <summary>
    /// Gets the event type being converted by the current converter instance.
    /// </summary>
    Type Type { get; }

    /// <summary>
    /// When overridden in a derived class, determines whether the converter 
    /// instance can convert the specified object type.
    /// </summary>
    /// <param name="typeToConvert">The type of the object to convert.</param>
    /// <returns><see langword="true"/> if the instance can convert the 
    /// specified object type; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="typeToConvert"/> is null.</exception>"
    bool CanConvert(Type typeToConvert);

    ///<summary>
    /// Converts the specified event to an entity.
    /// </summary>
    /// <param name="event">The event to convert.</param>
    /// <param name="options">The serialization options to use.</param>
    /// <returns>The entity converted from the event.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    IEntityEvent ConvertTo(
        IEvent @event,
        JsonSerializerOptions? options = default);

    /// <summary>
    /// Converts the specified entity to an event.
    /// </summary>
    /// <param name="entity">The entity to convert.</param>
    /// <param name="options">The serialization options to use.</param>
    /// <returns>The event converted from the entity.</returns>
    /// <exception cref="InvalidOperationException">Unable to convert to
    /// event type.</exception>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="entity"/> is null.</exception>
    IEvent ConvertFrom(
        IEntityEvent entity,
        JsonSerializerOptions? options = default);
}