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

using System.Collections.Concurrent;
using System.Text.Json;

using Xpandables.Net.Executions.Tasks;

namespace Xpandables.Net.Repositories.Converters;

/// <summary>
/// Provides an abstract base class for converting events to and
/// from event entities.
/// </summary>
public abstract class EventConverter : IEventConverter
{
    private readonly static ConcurrentBag<IEventConverter> _converters =
    [
        new EventConverterDomain(),
        new EventConverterIntegration(),
        new EventConverterSnapshot()
    ];

    /// <summary>
    /// Gets the collection of event converters used to transform events into different formats.
    /// </summary>
    public static IReadOnlyCollection<IEventConverter> Converters => _converters;

    /// <summary>
    /// Registers a new event converter to the system.
    /// </summary>
    /// <param name="converter">The event converter to register. Cannot be <see langword="null"/>.</param>
    /// <exception cref="InvalidOperationException">Thrown if the converter is already registered.</exception>
    public static void RegisterConverter(IEventConverter converter)
    {
        ArgumentNullException.ThrowIfNull(converter);
        if (_converters.Contains(converter))
        {
            throw new InvalidOperationException(
                $"The converter {converter.GetType().Name} is already registered.");
        }

        _converters.Add(converter);
    }

    /// <summary>
    /// Clears all registered converters from the internal collection.
    /// </summary>
    /// <remarks>This method removes all converters that have been added, resetting the collection to an empty
    /// state. It is useful when you need to reinitialize the converters or ensure no converters are present.</remarks>
    public static void ClearConverters() => _converters.Clear();

    /// <summary>
    /// Retrieves an event converter capable of converting the specified type.
    /// </summary>
    /// <param name="type">The type for which to find a suitable event converter. Cannot be <see langword="null"/>.</param>
    /// <returns>An <see cref="IEventConverter"/> that can convert the specified type.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no suitable converter is found for the specified type.</exception>
    public static IEventConverter GetConverterFor(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        return _converters.FirstOrDefault(converter => converter.CanConvert(type))
            ?? throw new InvalidOperationException(
                $"No converter found for the type {type.FullName}. " +
                $"Available converters: {string.Join(", ", _converters.Select(c => c.EventType.Name))}.");
    }

    /// <summary>
    /// Retrieves an event converter for the specified event.
    /// </summary>
    /// <param name="event">The event for which to obtain a converter. Cannot be <see langword="null"/>.</param>
    /// <returns>An <see cref="IEventConverter"/> instance that can convert the specified event.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no suitable converter is found for the specified type.</exception>
    public static IEventConverter GetConverterFor(IEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);
        return GetConverterFor(@event.GetType());
    }

    /// <summary>
    /// Retrieves an event converter for the specified event type.
    /// </summary>
    /// <typeparam name="TEvent">The type of event for which to get the converter. Must implement <see cref="IEvent"/>.</typeparam>
    /// <returns>An instance of <see cref="IEventConverter"/> that can convert the specified event type.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no suitable converter is found for the specified type.</exception>
    public static IEventConverter GetConverterFor<TEvent>()
        where TEvent : IEvent =>
        GetConverterFor(typeof(TEvent));

    /// <inheritdoc />
    public abstract Type EventType { get; }

    /// <inheritdoc />
    public abstract bool CanConvert(Type type);

    /// <inheritdoc />
    public abstract IEntityEvent ConvertTo(
        IEvent @event,
        JsonSerializerOptions? options = null);

    /// <inheritdoc />
    public abstract IEvent ConvertFrom(
        IEntityEvent entity,
        JsonSerializerOptions? options = null);

    /// <summary>
    /// Serializes the given event to <see cref="JsonDocument" />.
    /// </summary>
    /// <param name="event">The event to serialize.</param>
    /// <param name="jsonOptions">Optional JSON serializer options.</param>
    /// <param name="documentOptions">Optional JSON document options.</param>
    /// <returns>A JSON document representing the serialized event.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the event cannot be serialized.
    /// </exception>
    public static JsonDocument SerializeEvent(
        IEvent @event,
        JsonSerializerOptions? jsonOptions = null,
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
    /// Deserializes the given <see cref="JsonDocument" />> to an event of the specified type.
    /// </summary>
    /// <param name="eventData">The JSON document representing the event data.</param>
    /// <param name="eventType">The type of the event to deserialize to.</param>
    /// <param name="options">Optional JSON serializer options.</param>
    /// <returns>An instance of the deserialized event.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the event data cannot be deserialized.
    /// </exception>
    public static IEvent DeserializeEvent(
        JsonDocument eventData,
        Type eventType,
        JsonSerializerOptions? options = null)
    {
        try
        {
            object? @event = eventData.Deserialize(eventType, options)
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

    /// <summary>
    /// Deserializes the specified JSON document into an event of the specified type.
    /// </summary>
    /// <typeparam name="TEvent">The type of event to deserialize to, which must implement <see cref="IEvent"/>.</typeparam>
    /// <param name="eventData">The JSON document containing the event data to deserialize.</param>
    /// <param name="options">Optional. The serializer options to use during deserialization. If not provided, default options are used.</param>
    /// <returns>An instance of <typeparamref name="TEvent"/> representing the deserialized event.</returns>
    public static TEvent DeserializeEvent<TEvent>(
        JsonDocument eventData,
        JsonSerializerOptions? options = null)
        where TEvent : IEvent =>
        (TEvent)DeserializeEvent(eventData, typeof(TEvent), options);
}