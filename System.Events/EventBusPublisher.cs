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

namespace System.Events;

/// <summary>
/// Publishes integration events to an external bus using <see cref="IEventBus"/>.
/// </summary>
public sealed class EventBusPublisher(IEventBus eventBus) : IEventPublisher
{
    private readonly IEventBus _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

    /// <inheritdoc/>
    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, IEvent
    {
        ArgumentNullException.ThrowIfNull(@event);

        if (@event is not IIntegrationEvent integrationEvent)
        {
            throw new InvalidOperationException(
                $"Cannot publish event type '{@event.GetType().FullName}'. " +
                $"'{nameof(EventBusPublisher)}' only supports '{nameof(IIntegrationEvent)}'.");
        }

        return _eventBus.PublishAsync(integrationEvent, cancellationToken);
    }
}
