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
using System.Cache;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace System.Events.Repositories;

/// <summary>
/// Provides static methods for registering, retrieving, serializing, and deserializing event converters used in event
/// serialization and deserialization processes.
/// </summary>
/// <param name="cacheTypeResolver">The type resolver used to resolve event types by name. Cannot be null.</param>
/// <remarks>The EventConverter class manages a global collection of event converters, allowing applications to
/// register custom converters for specific event types. It supports serialization of event instances to JSON documents
/// and deserialization from JSON documents back to event objects, using configured JsonSerializerOptions for type
/// metadata. All methods are thread-safe and intended for use in scenarios where event data must be converted to and
/// from JSON representations. Registering duplicate converters of the same type is not allowed and will result in an
/// exception.</remarks>
public abstract class EventConverter(ICacheTypeResolver cacheTypeResolver) : IEventConverter
{
    private static readonly MemoryAwareCache<(Type Type, JsonSerializerOptions Options), JsonTypeInfo> _typeInfoCache = new();
    private static readonly MemoryAwareCache<(JsonTypeInfo TypeInfo, JsonSerializerOptions Options), Func<JsonDocument, IEvent>> _deserializerCache = new();

    private static JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = null,
        WriteIndented = true
    };

    /// <summary>
    /// Gets or sets the default options used for JSON serialization and deserialization operations.
    /// </summary>
    /// <remarks>Changing this property affects how all subsequent JSON serialization and deserialization is
    /// performed using these options. The property must be set to a non-null value.</remarks>
    public static JsonSerializerOptions SerializerOptions
    {
        get => _serializerOptions;
        set => _serializerOptions = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Serializes the specified event instance to a JSON document using the provided serializer and document options.
    /// </summary>
    /// <remarks>The serializerOptions parameter must be configured to provide metadata for the runtime type
    /// of the event instance, typically via source generation or manual registration. If the required metadata is
    /// missing, an InvalidOperationException is thrown.</remarks>
    /// <param name="eventInstance">The event instance to serialize. Cannot be null.</param>
    /// <param name="serializerOptions">The options to use when serializing the event instance. Cannot be null. Must be configured to provide metadata
    /// for the event type.</param>
    /// <param name="documentOptions">Options to control the behavior of the created JSON document. Optional.</param>
    /// <returns>A JsonDocument representing the serialized event instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no JsonTypeInfo is found for the event type, or if serialization fails. See the inner exception for
    /// details.</exception>
    public static JsonDocument SerializeEventToJsonDocument(
         IEvent eventInstance,
         JsonSerializerOptions? serializerOptions = default,
         JsonDocumentOptions documentOptions = default)
    {
        ArgumentNullException.ThrowIfNull(eventInstance);
        serializerOptions ??= SerializerOptions;

        try
        {
            Type type = eventInstance.GetType();
            JsonTypeInfo typeInfo = ResolveEventJsonTypeInfo(type, serializerOptions);
            return JsonSerializer.SerializeToDocument(eventInstance, typeInfo);
        }
        catch (Exception exception)
            when (exception is not InvalidOperationException)
        {
            throw new InvalidOperationException(
                $"Failed to serialize the event {eventInstance.GetType().FullName} to JsonDocument. " +
                $"See inner exception for details.", exception);
        }
    }

    /// <summary>
    /// Deserializes the specified JSON document into an event object of the given type using the provided serializer
    /// options.
    /// </summary>
    /// <remarks>The event type must be registered with the provided JsonSerializerOptions for correct
    /// deserialization. This method throws an exception if the event data cannot be converted to the specified
    /// type.</remarks>
    /// <param name="eventData">The JSON document containing the event data to deserialize. Cannot be null.</param>
    /// <param name="targetType">The type of event to deserialize the JSON document into. Must implement IEvent. Cannot be null.</param>
    /// <param name="serializerOptions">The serializer options to use for deserialization, including type metadata. Cannot be null.</param>
    /// <returns>An instance of IEvent representing the deserialized event data.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the type information for the specified event type is not found in the serializer options, or if
    /// deserialization fails.</exception>
    public static IEvent DeserializeJsonDocumentToEvent(
        JsonDocument eventData,
        Type targetType,
        JsonSerializerOptions? serializerOptions = default)
    {
        ArgumentNullException.ThrowIfNull(eventData);
        ArgumentNullException.ThrowIfNull(targetType);
        serializerOptions ??= SerializerOptions;

        try
        {
            Func<JsonDocument, IEvent> deserializer = ResolveEventDeserializer(targetType, serializerOptions);
            return deserializer(eventData);
        }
        catch (Exception exception)
            when (exception is not InvalidOperationException)
        {
            throw new InvalidOperationException(
                $"Failed to convert the event entity to {targetType.Name}. " +
                $"See inner exception for details.", exception);
        }
    }

    /// <summary>
    /// Deserializes the event data from an entity event into an instance of the corresponding event type.
    /// </summary>
    /// <param name="entityEvent">The entity event containing the event type name and serialized event data to be deserialized. Cannot be null.</param>
    /// <param name="serializerOptions">The options to use when deserializing the event data. Cannot be null.</param>
    /// <returns>An instance of the event type represented by the entity event, deserialized from the provided event data.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the event type specified by the entity event cannot be found.</exception>
    public IEvent DeserializeEntityToEvent(
        IEntityEvent entityEvent,
        JsonSerializerOptions? serializerOptions = default)
    {
        ArgumentNullException.ThrowIfNull(entityEvent);

        serializerOptions ??= SerializerOptions;
        Type targetType = cacheTypeResolver.Resolve(entityEvent.EventName);

        return DeserializeJsonDocumentToEvent(entityEvent.EventData, targetType, serializerOptions);
    }

    /// <summary>
    /// Deserializes the specified JSON document into an event object of the given type.
    /// </summary>
    /// <typeparam name="TEvent">The type of event to deserialize to. Must implement the IEvent interface.</typeparam>
    /// <param name="eventData">The JSON document containing the event data to deserialize. Cannot be null.</param>
    /// <param name="serializerOptions">The options to use when deserializing the JSON document. If null, default serialization options are used.</param>
    /// <returns>An instance of type TEvent representing the deserialized event data.</returns>
    public static TEvent DeserializeJsonDocumentToEvent<TEvent>(
        JsonDocument eventData,
        JsonSerializerOptions serializerOptions)
        where TEvent : IEvent =>
        (TEvent)DeserializeJsonDocumentToEvent(eventData, typeof(TEvent), serializerOptions);

    /// <summary>
    /// Resolves the <see cref="JsonTypeInfo"/> for the specified event type using the provided <see
    /// cref="JsonSerializerOptions"/>.
    /// </summary>
    /// <param name="type">The event type for which to resolve the JSON type metadata. Cannot be null.</param>
    /// <param name="options">The <see cref="JsonSerializerOptions"/> used to obtain the type metadata. Cannot be null.</param>
    /// <returns>The <see cref="JsonTypeInfo"/> associated with the specified event type and serializer options.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no <see cref="JsonTypeInfo"/> can be found for the specified event type.</exception>
    public static JsonTypeInfo ResolveEventJsonTypeInfo(Type type, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(options);

        var key = (type, options);

        return _typeInfoCache.GetOrAdd(key, static k =>
        {
            var (eventType, options) = k;
            JsonTypeInfo? info = options.TypeInfoResolver?.GetTypeInfo(eventType, options)
                ?? throw new InvalidOperationException(
                    $"No JsonTypeInfo found for the event type {eventType.Name} in the provided serializer options.");

            return info;
        })
            ?? options.GetTypeInfo(type)
            ?? throw new InvalidOperationException(
                $"No JsonTypeInfo found for the event type {type.Name} in the provided serializer options.");
    }

    /// <summary>
    /// Resolves and returns a delegate that deserializes a JSON event payload into an instance of the specified event
    /// type.
    /// </summary>
    /// <remarks>The returned delegate is cached for each unique combination of event type and serializer
    /// options to improve performance on repeated calls.</remarks>
    /// <param name="type">The event type for which to resolve the deserializer. Cannot be null.</param>
    /// <param name="options">The serializer options to use for deserialization. Cannot be null.</param>
    /// <returns>A delegate that takes a <see cref="JsonDocument"/> and returns an <see cref="IEvent"/> instance representing the
    /// deserialized event data.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no deserializer can be found for the specified event type.</exception>
    public static Func<JsonDocument, IEvent> ResolveEventDeserializer(Type type, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(options);

        JsonTypeInfo typeInfo = ResolveEventJsonTypeInfo(type, options);

        var key = (typeInfo, options);

        return _deserializerCache.GetOrAdd(key, static k =>
        {
            var (info, options) = k;

            return new Func<JsonDocument, IEvent>(json =>
            {
                object? @event = json.Deserialize(info)
                    ?? throw new InvalidOperationException(
                        $"Failed to deserialize the event data to {info.Type.Name}.");

                return (IEvent)@event;
            });
        })
            ?? throw new InvalidOperationException(
                $"No deserializer found for the event {typeInfo.Type.Name}");
    }

    /// <summary>
    /// Resolves the JSON type information for the specified event type using the provided serializer options.
    /// </summary>
    /// <typeparam name="TEvent">The event type for which to resolve JSON type information. Must implement <see cref="IEvent"/>.</typeparam>
    /// <param name="options">The serializer options to use when resolving the JSON type information. Cannot be null.</param>
    /// <returns>A <see cref="JsonTypeInfo"/> instance representing the JSON type information for the specified event type.</returns>
    public static JsonTypeInfo ResolveEventJsonTypeInfo<TEvent>(JsonSerializerOptions options)
        where TEvent : IEvent =>
        ResolveEventJsonTypeInfo(typeof(TEvent), options);

    /// <inheritdoc/>
    public abstract Type EventType { get; }

    /// <inheritdoc />
    public abstract bool CanConvert(Type type);

    /// <summary>
    /// When implemented in a derived class, converts the specified event instance to an entity event representation.
    /// </summary>
    /// <param name="eventInstance">The event instance to convert. Cannot be null.</param>
    /// <param name="serializerOptions">Optional JSON serializer options to use during conversion.</param>
    /// <returns>An <see cref="IEntityEvent"/> that represents the converted event.</returns>
    public abstract IEntityEvent ConvertEventToEntity(IEvent eventInstance, JsonSerializerOptions? serializerOptions = default);

    /// <summary>
    /// When implemented in a derived class, converts the specified entity event instance back to an event representation.
    /// </summary>
    /// <param name="entityInstance">The entity event instance to convert. Cannot be null.</param>
    /// <param name="serializerOptions">The serializer options to use when converting the entity.</param>
    /// <returns>An event representation of the specified entity event instance.</returns>
    public abstract IEvent ConvertEntityToEvent(IEntityEvent entityInstance, JsonSerializerOptions? serializerOptions = default);
}
