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
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Xpandables.Net.Events.Repositories;

/// <summary>
/// Converts event entities to and from <see cref="IIntegrationEvent" />.
/// </summary>
public sealed class EventConverterIntegration : EventConverter
{
    /// <inheritdoc />
    public override Type EventType => typeof(IIntegrationEvent);

    /// <inheritdoc />
    public override bool CanConvert(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        return EventType.IsAssignableFrom(type);
    }

    /// <summary>
    /// Converts the specified entity event instance to an event representation.
    /// </summary>
    /// <param name="entityInstance">The entity event instance to convert. Cannot be null.</param>
    /// <param name="serializerOptions">The serializer options to use when converting the entity.</param>
    /// <returns>An event representation of the specified entity event instance.</returns>
    [RequiresUnreferencedCode("May use unreferenced code to convert IEntityEvent to IEvent.")]
    [RequiresDynamicCode("May use dynamic code to convert IEntityEvent to IEvent.")]
    public sealed override IEvent ConvertEntityToEvent(IEntityEvent entityInstance, JsonSerializerOptions serializerOptions)
    {
        ArgumentNullException.ThrowIfNull(entityInstance);

        try
        {
            Type eventType = Type.GetType(entityInstance.EventFullName, true)
                ?? throw new InvalidOperationException(
                    $"The event type '{entityInstance.EventFullName}' could not be found.");

            IEvent @event = DeserializeJsonDocumentToEvent(entityInstance.EventData, eventType, serializerOptions);

            return (IIntegrationEvent)@event;
        }
        catch (Exception exception)
            when (exception is not InvalidOperationException)
        {
            throw new InvalidOperationException(
                $"Failed to convert the event entity to {EventType.Name}. " +
                $"See inner exception for details.", exception);
        }
    }

    /// <summary>
    /// Converts the specified event instance to an entity event representation.
    /// </summary>
    /// <param name="eventInstance">The event instance to convert. Cannot be null.</param>
    /// <param name="serializerOptions">Optional JSON serializer options to use during conversion.</param>
    /// <returns>An <see cref="IEntityEvent"/> that represents the converted event.</returns>
    [RequiresUnreferencedCode("May use unreferenced code to convert IEntityEvent to IEvent.")]
    [RequiresDynamicCode("May use dynamic code to convert IEntityEvent to IEvent.")]
    public sealed override IEntityEvent ConvertEventToEntity(IEvent eventInstance, JsonSerializerOptions serializerOptions)
    {
        ArgumentNullException.ThrowIfNull(eventInstance);

        try
        {
            IIntegrationEvent integrationEvent = (IIntegrationEvent)eventInstance;

            return new EntityIntegrationEvent
            {
                KeyId = integrationEvent.EventId,
                EventType = integrationEvent.GetType().Name,
                EventFullName = integrationEvent.GetType().AssemblyQualifiedName!,
                EventData = SerializeEventToJsonDocument(integrationEvent, serializerOptions)
            };
        }
        catch (Exception exception)
            when (exception is not InvalidOperationException)
        {
            throw new InvalidOperationException(
                $"Failed to convert the event {eventInstance.GetType().Name} to entity. " +
                $"See inner exception for details.", exception);
        }
    }
}