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
using System.Events.Domain;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace System.Events.Data;

/// <summary>
/// Converts event snapshots to and from their entity representations.
/// </summary>
public sealed class EventConverterSnapshot(ICacheTypeResolver cacheTypeResolver) : EventConverter(cacheTypeResolver)
{
    /// <inheritdoc />
    public override Type EventType => typeof(ISnapshotEvent);

    /// <inheritdoc />
    public override bool CanConvert(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        return EventType.IsAssignableFrom(type);
    }

    /// <inheritdoc/>
    public sealed override IEntityEvent ConvertEventToEntity(IEvent eventInstance, JsonTypeInfo typeInfo)
    {
        ArgumentNullException.ThrowIfNull(eventInstance);
        ArgumentNullException.ThrowIfNull(typeInfo);

        return ConvertEventToEntityCore(eventInstance, () => SerializeEventToJsonDocument(eventInstance, typeInfo));
    }


    /// <inheritdoc/>
    [RequiresUnreferencedCode("Serialization may require types that are trimmed.")]
    [RequiresDynamicCode("Serialization may require types that are generated dynamically.")]
    public sealed override IEntityEvent ConvertEventToEntity(IEvent eventInstance, JsonSerializerOptions? serializerOptions = default)
    {
        ArgumentNullException.ThrowIfNull(eventInstance);
        return ConvertEventToEntityCore(eventInstance, () => SerializeEventToJsonDocument(eventInstance, serializerOptions));
    }

    /// <inheritdoc/>
    public sealed override IEvent ConvertEntityToEvent(IEntityEvent entityInstance, JsonTypeInfo typeInfo)
    {
        ArgumentNullException.ThrowIfNull(entityInstance);
        ArgumentNullException.ThrowIfNull(typeInfo);

        return ConvertEntityToEventCore(() => DeserializeEntityToEvent(entityInstance, typeInfo));
    }

    /// <inheritdoc/>
    [RequiresUnreferencedCode("Serialization may require types that are trimmed.")]
    [RequiresDynamicCode("Serialization may require types that are generated dynamically.")]
    public sealed override IEvent ConvertEntityToEvent(IEntityEvent entityInstance, JsonSerializerOptions? serializerOptions = default)
    {
        ArgumentNullException.ThrowIfNull(entityInstance);

        return ConvertEntityToEventCore(() => DeserializeEntityToEvent(entityInstance, serializerOptions));
    }

    private static EntitySnapshotEvent ConvertEventToEntityCore(IEvent @event, Func<JsonDocument> documentFactory)
    {
        try
        {
            ISnapshotEvent snapshot = (ISnapshotEvent)@event;

            return new EntitySnapshotEvent
            {
                KeyId = snapshot.EventId,
                OwnerId = snapshot.OwnerId,
                EventName = snapshot.GetEventName(),
                EventData = documentFactory()
            };
        }
        catch (Exception exception)
            when (exception is not InvalidOperationException)
        {
            throw new InvalidOperationException(
                $"Failed to convert the event {@event.GetType().Name} to entity. " +
                $"See inner exception for details.", exception);
        }
    }

    private static IEvent ConvertEntityToEventCore(Func<IEvent> eventFactory)
    {
        try
        {
            IEvent @event = eventFactory();
            return (ISnapshotEvent)@event;
        }
        catch (Exception exception)
            when (exception is not InvalidOperationException)
        {
            throw new InvalidOperationException(
                $"Failed to convert the event entity. " +
                $"See inner exception for details.", exception);
        }
    }
}