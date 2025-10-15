using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using Xpandables.Net.Text;

namespace Xpandables.Net.Events;

/// <summary>
/// Provides static methods for registering, retrieving, serializing, and deserializing event converters used in event
/// serialization and deserialization processes.
/// </summary>
/// <remarks>The EventConverter class manages a global collection of event converters, allowing applications to
/// register custom converters for specific event types. It supports serialization of event instances to JSON documents
/// and deserialization from JSON documents back to event objects, using configured JsonSerializerOptions for type
/// metadata. All methods are thread-safe and intended for use in scenarios where event data must be converted to and
/// from JSON representations. Registering duplicate converters of the same type is not allowed and will result in an
/// exception.</remarks>
public abstract class EventConverter : IEventConverter
{
    private readonly static HashSet<IEventConverter> _converters = [];
    private static readonly MemoryAwareCache<string, Type> _eventTypeCache = new();
    private static readonly MemoryAwareCache<(Type Type, JsonSerializerOptions Options), JsonTypeInfo> _typeInfoCache = new();
    private static readonly MemoryAwareCache<(JsonTypeInfo TypeInfo, JsonSerializerOptions Options), Func<JsonDocument, IEvent>> _deserializerCache = new();

    private static JsonSerializerOptions _serializerOptions = SerializationDefaultOptions.Value;

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
    /// Registers the specified event converter for use in event serialization and deserialization.
    /// </summary>
    /// <param name="converter">The event converter to register. Cannot be null.</param>
    /// <exception cref="InvalidOperationException">Thrown if a converter of the same type has already been registered.</exception>
    public static void RegisterConverter(IEventConverter converter)
    {
        ArgumentNullException.ThrowIfNull(converter);

        if (!_converters.Add(converter))
        {
            throw new InvalidOperationException(
                $"The converter {converter.GetType().Name} is already registered.");
        }
    }

    /// <summary>
    /// Clears all registered converters from the internal collection.
    /// </summary>
    /// <remarks>This method removes all converters that have been added, resetting the collection to an empty
    /// state. It is useful when you need to reinitialize the converters or ensure no converters are present.</remarks>
    public static void ClearConverters() => _converters.Clear();

    /// <summary>
    /// Retrieves an event converter capable of handling the specified type.
    /// </summary>
    /// <param name="type">The type of event for which to obtain a corresponding converter. Cannot be null.</param>
    /// <returns>An <see cref="IEventConverter"/> instance that can convert events of the specified type.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no suitable converter is found for the specified <paramref name="type"/>.</exception>
    public static IEventConverter GetConverterFor(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        return _converters
            .FirstOrDefault(converter => converter.CanConvert(type))
            ?? throw new InvalidOperationException(
                $"No converter found for the type {type.Name}");
    }

    /// <summary>
    /// Retrieves an event converter suitable for the specified event instance.
    /// </summary>
    /// <param name="event">The event instance for which to obtain a corresponding event converter. Cannot be null.</param>
    /// <returns>An <see cref="IEventConverter"/> that can convert the specified event.</returns>
    public static IEventConverter GetConverterFor(IEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);
        return GetConverterFor(@event.GetType());
    }

    /// <summary>
    /// Retrieves an event converter for the specified event type parameter.
    /// </summary>
    /// <typeparam name="TEvent">The type of event for which to obtain a converter. Must implement <see cref="IEvent"/>.</typeparam>
    /// <returns>An <see cref="IEventConverter"/> instance capable of converting events of type <typeparamref name="TEvent"/>.</returns>
    public static IEventConverter GetConverterFor<TEvent>()
        where TEvent : IEvent =>
        GetConverterFor(typeof(TEvent));

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
        ArgumentNullException.ThrowIfNull(serializerOptions);

        try
        {
            serializerOptions ??= SerializerOptions;
            Type type = eventInstance.GetType();
            JsonTypeInfo jsonTypeInfo = ResolveEventJsonTypeInfo(type, serializerOptions);

            byte[] json = JsonSerializer.SerializeToUtf8Bytes(eventInstance, jsonTypeInfo);
            return JsonDocument.Parse(json, documentOptions);
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

        try
        {
            serializerOptions ??= SerializerOptions;
            JsonTypeInfo jsonTypeInfo = ResolveEventJsonTypeInfo(targetType, serializerOptions);
            Func<JsonDocument, IEvent> deserializer = ResolveEventDeserializer(jsonTypeInfo, serializerOptions);
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
    [RequiresUnreferencedCode("May use unreferenced code to convert IEntityEvent to IEvent.")]
    [RequiresDynamicCode("May use dynamic code to convert IEntityEvent to IEvent.")]
    public static IEvent DeserializeEntityToEvent(
        IEntityEvent entityEvent,
        JsonSerializerOptions? serializerOptions = default)
    {
        ArgumentNullException.ThrowIfNull(entityEvent);

        serializerOptions ??= SerializerOptions;
        Type targetType = ResolveEventType(entityEvent.EventFullName);

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
        JsonSerializerOptions? serializerOptions = default)
        where TEvent : IEvent =>
        (TEvent)DeserializeJsonDocumentToEvent(eventData, typeof(TEvent), serializerOptions);

    /// <summary>
    /// Resolves the runtime type of an event given its fully qualified type name.
    /// </summary>
    /// <remarks>The resolved type must be accessible and referenced by the application at runtime. The method
    /// caches resolved types for improved performance on subsequent calls.</remarks>
    /// <param name="fullName">The fully qualified name of the event type to resolve. Cannot be null.</param>
    /// <returns>A Type object representing the resolved event type.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the event type specified by fullName cannot be found or is not available at runtime.</exception>
    //[SuppressMessage("Trimming", "IL2057:Unrecognized value passed to the parameter of method. It's not possible to guarantee the availability of the target type.", Justification = "<Pending>")]
    [RequiresUnreferencedCode("May use unreferenced code to resolve type.")]
    [RequiresDynamicCode("May use dynamic code to resolve type.")]
    public static Type ResolveEventType(string fullName)
    {
        ArgumentNullException.ThrowIfNull(fullName);

        return _eventTypeCache.GetOrAdd(fullName, static name =>
        {
            Type? eventType = Type.GetType(name, true);
            return eventType ?? throw new InvalidOperationException(
                $"The event type '{name}' could not be found. Ensure it is referenced and available at runtime.");
        })
            ?? Type.GetType(fullName) ?? throw new InvalidOperationException(
            $"The event type '{fullName}' could not be found. Ensure it is referenced and available at runtime.");
    }

    /// <summary>
    /// Resolves the <see cref="JsonTypeInfo"/> for the specified event type using the provided <see
    /// cref="JsonSerializerOptions"/>.
    /// </summary>
    /// <param name="type">The event type for which to resolve the JSON type metadata. Cannot be null.</param>
    /// <param name="options">The <see cref="JsonSerializerOptions"/> used to obtain the type metadata. Cannot be null.</param>
    /// <returns>The <see cref="JsonTypeInfo"/> associated with the specified event type and serializer options.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no <see cref="JsonTypeInfo"/> can be found for the specified event type.</exception>
    public static JsonTypeInfo ResolveEventJsonTypeInfo(Type type, JsonSerializerOptions? options = default)
    {
        ArgumentNullException.ThrowIfNull(type);

        options ??= SerializerOptions;
        var key = (type, options);

        return _typeInfoCache.GetOrAdd(key, static k =>
        {
            var (eventType, options) = k;
            JsonTypeInfo? info = options.GetTypeInfo(eventType);
            return info ?? throw new InvalidOperationException(
                $"No JsonTypeInfo found for the event {eventType.Name}");
        })
            ?? options.GetTypeInfo(type) ?? throw new InvalidOperationException(
            $"No JsonTypeInfo found for the event {type.Name}");
    }

    /// <summary>
    /// Resolves and returns a delegate that deserializes a JSON event payload into an instance of the specified event
    /// type.
    /// </summary>
    /// <remarks>The returned delegate is cached for each unique combination of event type and serializer
    /// options to improve performance on repeated calls.</remarks>
    /// <param name="typeInfo">The metadata describing the event type to deserialize to. Cannot be null.</param>
    /// <param name="options">The serializer options to use for deserialization. Cannot be null.</param>
    /// <returns>A delegate that takes a <see cref="JsonDocument"/> and returns an <see cref="IEvent"/> instance representing the
    /// deserialized event data.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no deserializer can be found for the specified event type.</exception>
    public static Func<JsonDocument, IEvent> ResolveEventDeserializer(
        JsonTypeInfo typeInfo, JsonSerializerOptions? options = default)
    {
        ArgumentNullException.ThrowIfNull(typeInfo);

        options ??= SerializerOptions;
        var key = (typeInfo, options);

        return _deserializerCache.GetOrAdd(key, static k =>
        {
            var (type, options) = k;

            JsonTypeInfo info = ResolveEventJsonTypeInfo(type.Type, options);
            return new Func<JsonDocument, IEvent>(json =>
            {
                object? @event = json.Deserialize(info)
                    ?? throw new InvalidOperationException(
                        $"Failed to deserialize the event data to {type.Type.Name}.");

                return (IEvent)@event;
            });
        })
            ?? throw new InvalidOperationException(
                $"No deserializer found for the event {typeInfo.Type.Name}");
    }

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
    [RequiresUnreferencedCode("May use unreferenced code to convert IEntityEvent to IEvent.")]
    [RequiresDynamicCode("May use dynamic code to convert IENtityEvent to IEvent.")]
    public abstract IEntityEvent ConvertEventToEntity(IEvent eventInstance, JsonSerializerOptions? serializerOptions = default);

    /// <summary>
    /// When implemented in a derived class, converts the specified entity event instance back to an event representation.
    /// </summary>
    /// <param name="entityInstance">The entity event instance to convert. Cannot be null.</param>
    /// <param name="serializerOptions">The serializer options to use when converting the entity.</param>
    /// <returns>An event representation of the specified entity event instance.</returns>
    [RequiresUnreferencedCode("May use unreferenced code to convert IEntityEvent to IEvent.")]
    [RequiresDynamicCode("May use dynamic code to convert IENtityEvent to IEvent.")]
    public abstract IEvent ConvertEntityToEvent(IEntityEvent entityInstance, JsonSerializerOptions? serializerOptions = default);
}
