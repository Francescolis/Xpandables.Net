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
using System.Events.Domain;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace System.Events.Data;

/// <summary>
/// Converts between <see cref="ISnapshotEvent"/> and <see cref="EntityEventSnapshot"/>.
/// </summary>
/// <param name="typeResolver">The type resolver to use for resolving event types. Cannot be null.</param>  
public sealed class EventConverterSnapshot(ICacheTypeResolver typeResolver) : IEventConverter<EntityEventSnapshot, ISnapshotEvent>
{
    private readonly ICacheTypeResolver _typeResolver = typeResolver ?? throw new ArgumentNullException(nameof(typeResolver));

    /// <inheritdoc/>
    public EntityEventSnapshot ConvertEventToEntity(ISnapshotEvent @event, IEventConverterContext context)
    {
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentNullException.ThrowIfNull(context);

        try
        {
            JsonTypeInfo typeInfo = context.ResolveJsonTypeInfo(@event.GetType());
            JsonDocument data = JsonSerializer.SerializeToDocument(@event, typeInfo);

            return new EntityEventSnapshot
            {
                KeyId = @event.EventId,
                OwnerId = @event.OwnerId,
                EventName = @event.GetEventName(),
                EventData = data,
                CausationId = @event.CausationId,
                CorrelationId = @event.CorrelationId
            };
        }
        catch (Exception exception) when (exception is not InvalidOperationException)
        {
            throw new InvalidOperationException(
                $"Failed to convert the event {@event.GetType().Name} to entity. " +
                "See inner exception for details.",
                exception);
        }
    }

    /// <inheritdoc/>
    public ISnapshotEvent ConvertEntityToEvent(EntityEventSnapshot entity, IEventConverterContext context)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(context);

        try
        {
            Type targetType = _typeResolver.Resolve(entity.EventName);
            JsonTypeInfo typeInfo = context.ResolveJsonTypeInfo(targetType);

            object? @event = entity.EventData.Deserialize(typeInfo)
                ?? throw new InvalidOperationException(
                    $"Failed to deserialize the event data to {typeInfo.Type.Name}.");

            return (ISnapshotEvent)@event;
        }
        catch (Exception exception) when (exception is not InvalidOperationException)
        {
            throw new InvalidOperationException(
                "Failed to convert the event entity. See inner exception for details.",
                exception);
        }
    }
}