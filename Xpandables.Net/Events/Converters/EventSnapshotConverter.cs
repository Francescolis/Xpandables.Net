
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

using Xpandables.Net.Events.Entities;

namespace Xpandables.Net.Events.Converters;

/// <summary>
/// Converts event snapshots to and from their entity representations.
/// </summary>
public sealed class EventSnapshotConverter : EventConverter
{
    /// <inheritdoc/>
    public override Type EventType => typeof(IEventSnapshot);

    /// <inheritdoc/>
    public override bool CanConvert(Type typeToConvert) =>
        EventType.IsAssignableFrom(typeToConvert);

    /// <inheritdoc/>
    public override IEvent ConvertFrom(
        IEventEntity entity,
        JsonSerializerOptions? options = null)
    {
        try
        {
            Type eventType = Type.GetType(entity.EventFullName, true)!;

            object? @event = entity.EventData.Deserialize(eventType, options)
                ?? throw new InvalidOperationException(
                    $"Failed to deserialize the event data to {eventType.Name}.");

            return (IEventSnapshot)@event;
        }
        catch (Exception exception)
            when (exception is not InvalidOperationException)
        {
            throw new InvalidOperationException(
                $"Failed to convert the event entity to {EventType.Name}. " +
                $"See inner exception for details.", exception);
        }
    }

    /// <inheritdoc/>
    public override IEventEntity ConvertTo(
        IEvent @event,
        JsonSerializerOptions? options = null)
    {
        try
        {
            IEventSnapshot snapshot = (IEventSnapshot)@event;

            return new EventEntitySnapshot
            {
                Id = snapshot.EventId,
                Owner = snapshot.Owner,
                EventVersion = snapshot.EventVersion,
                EventName = snapshot.GetType().Name,
                EventFullName = snapshot.GetType().AssemblyQualifiedName!,
                EventData = SerializeEvent(snapshot, options)
            };
        }
        catch (Exception exception)
            when (exception is not InvalidOperationException)
        {

            throw;
        }
    }
}
