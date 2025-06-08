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
using System.Text.Json.Serialization.Metadata; // Added for JsonTypeInfo

using Xpandables.Net.Executions.Domains;
using Xpandables.Net.Executions.Tasks;

namespace Xpandables.Net.Repositories.Converters;

/// <summary>
/// Converts event entities to domain events and vice versa.
/// </summary>
public sealed class EventConverterDomain : EventConverter
{
    private readonly IEventTypeResolver _eventTypeResolver;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventConverterDomain"/> class.
    /// </summary>
    /// <param name="eventTypeResolver">The resolver for event type names to JsonTypeInfo.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="eventTypeResolver"/> is null.</exception>
    public EventConverterDomain(IEventTypeResolver eventTypeResolver)
    {
        ArgumentNullException.ThrowIfNull(eventTypeResolver);
        _eventTypeResolver = eventTypeResolver;
    }

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
            JsonTypeInfo? resolvedJsonTypeInfo = _eventTypeResolver.GetJsonTypeInfo(entity.EventFullName);

            if (resolvedJsonTypeInfo is null)
            {
                // Fallback: This path is still an AOT concern due to Type.GetType().
                // Ideally, the resolver should handle all known event types.
                Type eventType = Type.GetType(entity.EventFullName, true)!; // AOT concern
                resolvedJsonTypeInfo = options?.GetTypeInfo(eventType)
                    ?? throw new InvalidOperationException(
                        $"Could not resolve JsonTypeInfo for type name '{entity.EventFullName}' via resolver or options. " +
                        $"Ensure the IEventTypeResolver is correctly populated and/or JsonSerializerOptions includes a resolver for this type.");
            }

            // The base DeserializeEvent now expects a non-nullable JsonTypeInfo.
            // The check above ensures resolvedJsonTypeInfo is not null if we reach here.
            IEvent @event = DeserializeEvent(entity.EventData, resolvedJsonTypeInfo!);

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
                KeyId = domainEvent.EventId,
                AggregateId = Guid.Parse(domainEvent.AggregateId.ToString()!),
                EventName = domainEvent.GetType().Name,
                EventFullName = domainEvent.GetType().AssemblyQualifiedName!,
                EventVersion = domainEvent.EventVersion,
                EventData = SerializeEvent(domainEvent, options)
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