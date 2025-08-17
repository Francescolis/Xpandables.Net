
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

using Xpandables.Net.Events;

namespace Xpandables.Net.Repositories.Converters;

/// <summary>
/// Converts event snapshots to and from their entity representations.
/// </summary>
public sealed class EventConverterSnapshot : EventConverter
{
    /// <inheritdoc />
    public override Type EventType => typeof(ISnapshotEvent);

    /// <inheritdoc />
    public override bool CanConvert(Type type) =>
        EventType.IsAssignableFrom(type);

    /// <inheritdoc />
    public override IEvent ConvertFrom(
        IEntityEvent entity,
        JsonSerializerOptions? options = null)
    {
        try
        {
            Type eventType = Type.GetType(entity.FullName, true)!;

            IEvent @event = DeserializeEvent(entity.Data, eventType, options);

            return (ISnapshotEvent)@event;
        }
        catch (Exception exception)
            when (exception is not InvalidOperationException)
        {
            throw new InvalidOperationException(
                $"Failed to convert the event entity to {EventType.Name}. " +
                $"See inner exception for details.", exception);
        }
    }

    /// <inheritdoc />
    public override IEntityEvent ConvertTo(
        IEvent @event,
        JsonSerializerOptions? options = null)
    {
        try
        {
            ISnapshotEvent snapshot = (ISnapshotEvent)@event;

            return new EntitySnapshotEvent
            {
                KeyId = snapshot.Id,
                OwnerId = snapshot.OwnerId,
                Name = snapshot.GetType().Name,
                FullName = snapshot.GetType().AssemblyQualifiedName!,
                Data = SerializeEvent(snapshot, options)
            };
        }
        catch (Exception exception)
            when (exception is not InvalidOperationException)
        {
            throw new InvalidOperationException(
                $"Failed to convert the event {@event?.GetType().Name} to entity. " +
                $"See inner exception for details.", exception);
        }
    }
}