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
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace System.Events.Data;

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
    private static readonly MemoryAwareCache<Type, JsonTypeInfo> _typeInfoCache = new();

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
    /// Serializes the specified event instance to a JSON document using the provided type metadata.
    /// </summary>
    /// <param name="eventInstance">The event instance to serialize. Cannot be null.</param>
    /// <param name="typeInfo">The type metadata to use for serialization. Cannot be null.</param>
    /// <returns>A JsonDocument representing the serialized event instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the event instance cannot be serialized to a JSON document. See the inner exception for details.</exception>
    public static JsonDocument SerializeEventToJsonDocument(
         IEvent eventInstance,
         JsonTypeInfo typeInfo)
    {
        ArgumentNullException.ThrowIfNull(eventInstance);
        ArgumentNullException.ThrowIfNull(typeInfo);

        try
        {
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
    /// Serializes the specified event instance to a JSON document using the provided serializer options.
    /// </summary>
    /// <remarks>The returned JsonDocument must be disposed by the caller to release resources.</remarks>
    /// <param name="eventInstance">The event instance to serialize. Cannot be null.</param>
    /// <param name="serializerOptions">The options to use for JSON serialization. If null, default serializer options are used.</param>
    /// <returns>A JsonDocument representing the serialized event instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the event instance cannot be serialized to a JsonDocument. See the inner exception for details.</exception>
    [RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializerOptions.Web")]
    [RequiresDynamicCode("Calls System.Text.Json.JsonSerializerOptions.Web")]
    public static JsonDocument SerializeEventToJsonDocument(
         IEvent eventInstance,
         JsonSerializerOptions? serializerOptions = default)
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
    /// Deserializes the specified JSON event data into an instance of the target event type.
    /// </summary>
    /// <remarks>The caller is responsible for providing a JsonTypeInfo that matches the targetType. If the
    /// JSON structure does not match the expected event type, deserialization will fail.</remarks>
    /// <param name="eventData">The JSON document containing the event data to deserialize. Cannot be null.</param>
    /// <param name="targetType">The type of event to deserialize the JSON data into. Must implement IEvent. Cannot be null.</param>
    /// <param name="typeInfo">The metadata used to guide the deserialization process for the target type. Cannot be null.</param>
    /// <returns>An instance of IEvent representing the deserialized event data.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the event data cannot be deserialized to the specified target type, or if the deserialized object
    /// cannot be cast to IEvent.</exception>
    public static IEvent DeserializeJsonDocumentToEvent(
        JsonDocument eventData,
        Type targetType,
        JsonTypeInfo typeInfo)
    {
        ArgumentNullException.ThrowIfNull(eventData);
        ArgumentNullException.ThrowIfNull(targetType);
        ArgumentNullException.ThrowIfNull(typeInfo);

        try
        {
            object? @event = eventData.Deserialize(typeInfo)
                ?? throw new InvalidOperationException(
                    $"Failed to deserialize the event data to {typeInfo.Type.Name}.");

            return (IEvent)@event;
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
    /// Deserializes the specified JSON event data into an instance of the given event type.
    /// </summary>
    /// <remarks>This method requires unreferenced and dynamic code due to its use of System.Text.Json
    /// serialization features. Ensure that the target type is compatible with the provided serializer
    /// options.</remarks>
    /// <param name="eventData">The JSON document containing the event data to deserialize. Cannot be null.</param>
    /// <param name="targetType">The type of event to deserialize the JSON data into. Must implement IEvent. Cannot be null.</param>
    /// <param name="serializerOptions">The options to use for JSON deserialization. If null, default serializer options are used.</param>
    /// <returns>An instance of the specified event type that implements IEvent, populated with data from the JSON document.</returns>
    /// <exception cref="InvalidOperationException">Thrown if deserialization fails or if the resulting object cannot be cast to IEvent.</exception>
    [RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializerOptions.Web")]
    [RequiresDynamicCode("Calls System.Text.Json.JsonSerializerOptions.Web")]
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
            JsonTypeInfo typeInfo = ResolveEventJsonTypeInfo(targetType, serializerOptions);

            object? @event = eventData.Deserialize(typeInfo)
                ?? throw new InvalidOperationException(
                    $"Failed to deserialize the event data to {typeInfo.Type.Name}.");

            return (IEvent)@event;
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
    /// Deserializes the specified JSON document into an event object of type <typeparamref name="TEvent"/>.
    /// </summary>
    /// <remarks>This method requires unreferenced and dynamic code due to its use of <see
    /// cref="System.Text.Json.JsonSerializerOptions.Web"/>. Ensure that the provided <typeparamref name="TEvent"/> type
    /// is compatible with the event data structure in <paramref name="eventData"/>.</remarks>
    /// <typeparam name="TEvent">The type of event to deserialize to. Must implement <see cref="IEvent"/>.</typeparam>
    /// <param name="eventData">The <see cref="JsonDocument"/> containing the event data to deserialize. Cannot be null.</param>
    /// <param name="serializerOptions">The <see cref="JsonSerializerOptions"/> to use during deserialization. Provides custom serialization settings;
    /// can be null to use default options.</param>
    /// <returns>An instance of <typeparamref name="TEvent"/> representing the deserialized event data.</returns>
    [RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializerOptions.Web")]
    [RequiresDynamicCode("Calls System.Text.Json.JsonSerializerOptions.Web")]
    public static TEvent DeserializeJsonDocumentToEvent<TEvent>(
        JsonDocument eventData,
        JsonSerializerOptions serializerOptions)
        where TEvent : IEvent =>
        (TEvent)DeserializeJsonDocumentToEvent(eventData, typeof(TEvent), serializerOptions);

    /// <summary>
    /// Deserializes the specified JSON document into an event object of type <typeparamref name="TEvent"/> using the
    /// provided type information.
    /// </summary>
    /// <typeparam name="TEvent">The type of event to deserialize. Must implement <see cref="IEvent"/>.</typeparam>
    /// <param name="eventData">The <see cref="JsonDocument"/> containing the event data to deserialize. Cannot be null.</param>
    /// <param name="typeInfo">The <see cref="JsonTypeInfo{TEvent}"/> that provides serialization metadata for the target event type. Cannot be
    /// null.</param>
    /// <returns>An instance of <typeparamref name="TEvent"/> representing the deserialized event data.</returns>
    public static TEvent DeserializeJsonDocumentToEvent<TEvent>(
    JsonDocument eventData,
    JsonTypeInfo<TEvent> typeInfo)
        where TEvent : IEvent =>
        (TEvent)DeserializeJsonDocumentToEvent(eventData, typeof(TEvent), typeInfo);

    /// <summary>
    /// Deserializes the specified entity event data into an event object using the provided type information.
    /// </summary>
    /// <param name="entityEvent">The entity event containing the event name and serialized event data to be deserialized. Cannot be null.</param>
    /// <param name="typeInfo">The type metadata used to guide the deserialization process. Cannot be null.</param>
    /// <returns>An event object representing the deserialized data from the entity event.</returns>
    public IEvent DeserializeEntityToEvent(
        IEntityEvent entityEvent,
        JsonTypeInfo typeInfo)
    {
        ArgumentNullException.ThrowIfNull(entityEvent);
        ArgumentNullException.ThrowIfNull(typeInfo);

        Type type = cacheTypeResolver.Resolve(entityEvent.EventName);

        return DeserializeJsonDocumentToEvent(entityEvent.EventData, type, typeInfo);
    }

    /// <summary>
    /// Deserializes the specified entity event data into an event object of the appropriate type.
    /// </summary>
    /// <remarks>The target event type is determined by resolving the event name from <paramref
    /// name="entityEvent"/>. This method requires dynamic code and may not be compatible with trimming or ahead-of-time
    /// compilation scenarios.</remarks>
    /// <param name="entityEvent">The entity event containing the event name and serialized event data to be deserialized. Cannot be null.</param>
    /// <param name="serializerOptions">The options to use for JSON deserialization. If null, default serializer options are used.</param>
    /// <returns>An event object implementing <see cref="IEvent"/> that represents the deserialized entity event data.</returns>
    [RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializerOptions.Web")]
    [RequiresDynamicCode("Calls System.Text.Json.JsonSerializerOptions.Web")]
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
    /// Resolves the JSON type metadata for the specified event type using the provided serializer options.
    /// </summary>
    /// <param name="type">The event type for which to resolve the JSON type metadata. Cannot be null.</param>
    /// <param name="options">The <see cref="JsonSerializerOptions"/> used to obtain the type metadata. Cannot be null.</param>
    /// <returns>The <see cref="JsonTypeInfo"/> associated with the specified event type and serializer options.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no <see cref="JsonTypeInfo"/> can be found for the specified event type.</exception>
    [RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializerOptions.Web")]
    [RequiresDynamicCode("Calls System.Text.Json.JsonSerializerOptions.Web")]
    public static JsonTypeInfo ResolveEventJsonTypeInfo(Type type, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(options);

        return _typeInfoCache.GetOrAdd(type, t =>
        {
            JsonTypeInfo typeInfo = options.TryGetTypeInfo(t, out var info) ? info : JsonSerializer.GetJsonTypeInfo(t, options);
            return typeInfo;
        });
    }

    /// <inheritdoc/>
    public abstract Type EventType { get; }

    /// <inheritdoc />
    public abstract bool CanConvert(Type type);

    /// <summary>
    /// Converts the specified event instance to an entity event representation
    /// </summary>
    /// <param name="eventInstance">The event instance to convert. Cannot be null.</param>
    /// <param name="serializerOptions">Optional JSON serializer options to use during conversion.</param>
    /// <returns>An <see cref="IEntityEvent"/> that represents the converted event.</returns>
    [RequiresUnreferencedCode("Serialization may require types that are trimmed.")]
    [RequiresDynamicCode("Serialization may require types that are generated dynamically.")]
    public abstract IEntityEvent ConvertEventToEntity(IEvent eventInstance, JsonSerializerOptions? serializerOptions = default);

    /// <summary>
    /// Converts the specified event instance to an entity event representation
    /// </summary>
    /// <param name="eventInstance">The event instance to convert. Cannot be null.</param>
    /// <param name="typeInfo">The type metadata used to guide the conversion process. Cannot be null.</param>
    /// <returns>An <see cref="IEntityEvent"/> representing the converted event.</returns>
    public abstract IEntityEvent ConvertEventToEntity(IEvent eventInstance, JsonTypeInfo typeInfo);

    /// <summary>
    /// Converts the specified entity event instance back to an event representation.
    /// </summary>
    /// <param name="entityInstance">The entity event instance to convert. Cannot be null.</param>
    /// <param name="serializerOptions">The serializer options to use when converting the entity.</param>
    /// <returns>An event representation of the specified entity event instance.</returns>
    [RequiresUnreferencedCode("Deserialization may require types that are trimmed.")]
    [RequiresDynamicCode("Deserialization may require types that are generated dynamically.")]
    public abstract IEvent ConvertEntityToEvent(IEntityEvent entityInstance, JsonSerializerOptions? serializerOptions = default);

    /// <summary>
    /// Converts the specified entity event instance back to an event representation.
    /// </summary>
    /// <param name="entityInstance">The entity event instance to convert. Cannot be null.</param>
    /// <param name="typeInfo">The JSON type metadata used to guide the conversion process. Cannot be null.</param>
    /// <returns>An event representation of the specified entity event instance.</returns>
    public abstract IEvent ConvertEntityToEvent(IEntityEvent entityInstance, JsonTypeInfo typeInfo);
}
