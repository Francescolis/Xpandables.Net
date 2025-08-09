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
/// Converts event entities to domain events and vice versa.
/// </summary>
public sealed class EventConverterDomain : EventConverter
{
    /// <inheritdoc />
    public override Type EventType => typeof(IDomainEvent);

    /// <inheritdoc />
    public override bool CanConvert(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        return EventType.IsAssignableFrom(type);
    }

    /// <inheritdoc />
    public override IEvent ConvertFrom(
        IEntityEvent entity,
        JsonSerializerOptions? options = null)
    {
        try
        {
            Type eventType = Type.GetType(entity.FullName, true)!;

            IEvent @event = DeserializeEvent(entity.Data, eventType, options);

            return (IDomainEvent)@event;
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
            IDomainEvent domainEvent = (IDomainEvent)@event;

            return new EntityDomainEvent
            {
                KeyId = domainEvent.Id,
                AggregateId = Guid.Parse(domainEvent.AggregateId.ToString()!),
                AggregateName = domainEvent.AggregateName,
                Name = domainEvent.GetType().Name,
                FullName = domainEvent.GetType().AssemblyQualifiedName!,
                StreamVersion = domainEvent.StreamVersion,
                Data = SerializeEvent(domainEvent, options)
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