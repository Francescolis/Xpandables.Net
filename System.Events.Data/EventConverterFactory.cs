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
using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Events.Domain;
using System.Events.Integration;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace System.Events.Data;

/// <summary>
/// Provides a factory for retrieving event converters based on event type.
/// </summary>
/// <remarks>This factory enables dynamic selection of an appropriate event converter for a given event type. All
/// converters supplied must implement the IEventConverter interface. The factory is immutable after construction and is
/// thread-safe for concurrent use.</remarks>
/// <param name="converters">A collection of event converters to be used by the factory. Cannot be null.</param>
/// <param name="options">The JSON serializer options to use when converting events. Cannot be null.</param>
public sealed class EventConverterFactory(IEnumerable<IEventConverter> converters, JsonSerializerOptions options) : IEventConverterFactory
{
    private readonly FrozenDictionary<Type, IEventConverter> _convertersSet = converters.ToFrozenDictionary(conv => conv.EventType, conv => conv);

    /// <inheritdoc />
    public IEventConverter GetEventConverter(Type eventType)
    {
        ArgumentNullException.ThrowIfNull(eventType);
        Type eventSourceType = GetEventSourceType(eventType);

        EventConverter.SerializerOptions = options;

        return _convertersSet[eventSourceType];
    }

    /// <inheritdoc />
    public IEventConverter GetEventConverter<TEvent>()
        where TEvent : IEvent
    {
        Type eventType = typeof(TEvent);
        return GetEventConverter(eventType);
    }

    /// <inheritdoc/>
    public IEntityEvent ConvertEventToEntity(IEvent eventInstance, JsonTypeInfo typeInfo)
    {
        ArgumentNullException.ThrowIfNull(eventInstance);
        IEventConverter converter = GetEventConverter(eventInstance.GetType());
        return converter.ConvertEventToEntity(eventInstance, typeInfo);
    }

    /// <inheritdoc />
    [RequiresUnreferencedCode("Serialization may require types that are trimmed.")]
    [RequiresDynamicCode("Serialization may require types that are generated dynamically.")]
    public IEntityEvent ConvertEventToEntity(IEvent eventInstance, JsonSerializerOptions? serializerOptions = default)
    {
        ArgumentNullException.ThrowIfNull(eventInstance);
        IEventConverter converter = GetEventConverter(eventInstance.GetType());
        return converter.ConvertEventToEntity(eventInstance, serializerOptions ?? options);
    }

    /// <inheritdoc/>
    public IEvent ConvertEntityToEvent(IEntityEvent entityInstance, JsonTypeInfo typeInfo)
    {
        ArgumentNullException.ThrowIfNull(entityInstance);
        IEventConverter converter = GetEventConverter(entityInstance.GetType());
        return converter.ConvertEntityToEvent(entityInstance, typeInfo);
    }

    /// <inheritdoc />
    [RequiresUnreferencedCode("Serialization may require types that are trimmed.")]
    [RequiresDynamicCode("Serialization may require types that are generated dynamically.")]
    public IEvent ConvertEntityToEvent(IEntityEvent entityInstance, JsonSerializerOptions? serializerOptions = default)
    {
        ArgumentNullException.ThrowIfNull(entityInstance);
        IEventConverter converter = GetEventConverter(entityInstance.GetType());
        return converter.ConvertEntityToEvent(entityInstance, serializerOptions ?? options);
    }

    private static Type GetEventSourceType(Type type)
    {
        if (typeof(IDomainEvent).IsAssignableFrom(type))
            return typeof(IDomainEvent);
        if (typeof(ISnapshotEvent).IsAssignableFrom(type))
            return typeof(ISnapshotEvent);
        if (typeof(IIntegrationEvent).IsAssignableFrom(type))
            return typeof(IIntegrationEvent);

        throw new InvalidOperationException(
            $"The event type '{type.Name}' is not supported by any converter.");
    }
}
