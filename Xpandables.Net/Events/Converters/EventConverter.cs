
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

namespace Xpandables.Net.Events.Converters;

/// <summary>
/// Provides an abstract base class for converting events to and 
/// from event entities.
/// </summary>
public abstract class EventConverter : IEventConverter
{
    /// <inheritdoc/>
    public abstract Type EventType { get; }

    /// <inheritdoc/>
    public abstract bool CanConvert(Type typeToConvert);

    /// <inheritdoc/>
    public abstract IEventEntity ConvertTo(
        IEvent @event,
        JsonSerializerOptions? options = default);

    /// <inheritdoc/>
    public abstract IEvent ConvertFrom(
        IEventEntity entity,
        JsonSerializerOptions? options = default);

    /// <summary>
    /// Serializes the given event to a JSON document.
    /// </summary>
    /// <param name="event">The event to serialize.</param>
    /// <param name="options">Optional JSON serializer options.</param>
    /// <returns>A JSON document representing the serialized event.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the event cannot be serialized.
    /// </exception>
    protected static JsonDocument SerializeEvent(
        IEvent @event,
        JsonSerializerOptions? options = default)
    {
        try
        {
            byte[] json = JsonSerializer.SerializeToUtf8Bytes(@event, options);
            return JsonDocument.Parse(json);
        }
        catch (Exception exception)
            when (exception is not InvalidOperationException)
        {
            throw new InvalidOperationException(
                $"Failed to serialize the event {@event.GetType().FullName}. " +
                $"See inner exception for details.", exception);
        }
    }
}
