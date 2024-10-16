
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

using Xpandables.Net.Events.Converters;

namespace Xpandables.Net.Events.Providers.PostgreSql;
/// <summary>
/// Converts event entities to domain events and vice versa for PostgreSQL.
/// </summary>
public sealed class EventEntityDomainConverter : EventConverter
{
    /// <inheritdoc/>
    public override Type EventType => typeof(IEventDomain);

    /// <inheritdoc/>
    public override bool CanConvert(Type type) => EventType.IsAssignableFrom(type);

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
                    $"Failed to deserialize the event data to {eventType.FullName}.");

            return (IEventDomain)@event;
        }
        catch (Exception exception)
            when (exception is not InvalidOperationException)
        {
            throw new InvalidOperationException(
                $"Failed to convert the event entity to {EventType.FullName}. " +
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
            IEventDomain eventDomain = (IEventDomain)@event;

            return new EventEntityDomain(
                Guid.Parse(eventDomain.AggregateId.ToString()!),
                eventDomain.EventId,
                eventDomain.GetType().Name,
                eventDomain.GetType().FullName!,
                eventDomain.EventVersion,
                SerializeEvent(eventDomain, options));
        }
        catch (Exception exception)
            when (exception is not InvalidOperationException)
        {
            throw new InvalidOperationException(
                $"Failed to convert the event {@event.GetType().FullName} to {EventType.FullName}. " +
                $"See inner exception for details.", exception);
        }
    }
}
