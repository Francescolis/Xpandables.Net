
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

using Xpandables.Net.Executions.Tasks;

namespace Xpandables.Net.Repositories.Converters;

/// <summary>
/// Provides an abstract base class for converting events to and 
/// from event entities.
/// </summary>
public abstract class EventConverter : IEventConverter
{
    /// <inheritdoc/>
    public abstract Type EventType { get; }

    /// <inheritdoc/>
    public abstract bool CanConvert(Type type);

    /// <inheritdoc/>
    public abstract IEventEntity ConvertTo(
        IEvent @event,
        JsonSerializerOptions? options = default);

    /// <inheritdoc/>
    public abstract IEvent ConvertFrom(
        IEventEntity entity,
        JsonSerializerOptions? options = default);

    /// <summary>
    /// Serializes the given event to <see cref="JsonDocument"/>.
    /// </summary>
    /// <param name="event">The event to serialize.</param>
    /// <param name="jsonOptions">Optional JSON serializer options.</param>
    /// <param name="documentOptions">Optional JSON document options.</param>
    /// <returns>A JSON document representing the serialized event.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the event cannot be serialized.
    /// </exception>
    protected static JsonDocument SerializeEvent(
        IEvent @event,
        JsonSerializerOptions? jsonOptions = default,
        JsonDocumentOptions documentOptions = default)
    {
        try
        {
            byte[] json = JsonSerializer.SerializeToUtf8Bytes(@event, @event.GetType(), jsonOptions);
            return JsonDocument.Parse(json, documentOptions);
        }
        catch (Exception exception)
            when (exception is not InvalidOperationException)
        {
            throw new InvalidOperationException(
                $"Failed to serialize the event {@event.GetType().FullName}. " +
                $"See inner exception for details.", exception);
        }
    }

    /// <summary>
    /// Deserializes the given <see cref="JsonDocument"/>> to an event of the specified type.
    /// </summary>
    /// <param name="eventData">The JSON document representing the event data.</param>
    /// <param name="eventType">The type of the event to deserialize to.</param>
    /// <param name="options">Optional JSON serializer options.</param>
    /// <returns>An instance of the deserialized event.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the event data cannot be deserialized.
    /// </exception>
    protected static IEvent DeserializeEvent(
        JsonDocument eventData,
        Type eventType,
        JsonSerializerOptions? options = default)
    {
        try
        {
            object? @event = JsonSerializer.Deserialize(eventData, eventType, options)
                ?? throw new InvalidOperationException(
                    $"Failed to deserialize the event data to {eventType.Name}.");

            return (IEvent)@event;
        }
        catch (Exception exception)
            when (exception is not InvalidOperationException)
        {
            throw new InvalidOperationException(
                $"Failed to convert the event entity to {eventType.Name}. " +
                $"See inner exception for details.", exception);
        }
    }
}
