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
using System.Events.Integration;

namespace System.Events.Data;

/// <summary>
/// Converts between <see cref="IIntegrationEvent"/> and <see cref="EntityEventInbox"/>.
/// </summary>
public sealed class EventConverterInbox : IEventConverter<EntityEventInbox, IIntegrationEvent>
{
    /// <inheritdoc/>
    public EntityEventInbox ConvertEventToEntity(IIntegrationEvent @event, IEventConverterContext context)
    {
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentNullException.ThrowIfNull(context);

        try
        {
            return new EntityEventInbox
            {
                KeyId = @event.EventId,
                EventName = @event.GetEventName(),
                CorrelationId = @event.CorrelationId,
                CausationId = @event.CausationId,
                Consumer = string.Empty
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
    public IIntegrationEvent ConvertEntityToEvent(EntityEventInbox entity, IEventConverterContext context) =>
        throw new NotSupportedException("Conversion to event is not supported.");
}