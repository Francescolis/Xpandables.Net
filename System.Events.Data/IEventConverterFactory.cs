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
using System.Events.Domain;
using System.Events.Integration;

namespace System.Events.Data;

/// <summary>
/// Defines a factory for retrieving event converters based on event type.
/// </summary>
public interface IEventConverterFactory
{
    /// <summary>
    /// Gets the context used for event conversion operations.
    /// </summary>
    /// <remarks>The event converter context provides access to services and information required during event
    /// conversion. This property is typically used by components that need to customize or extend event conversion
    /// behavior.</remarks>
    IEventConverterContext ConverterContext { get; }

    /// <summary>
    /// Gets an event converter that transforms domain-specific entity events of the specified type to generic domain
    /// events.
    /// </summary>
    /// <typeparam name="TEntityEventDomain">The type of the domain-specific entity event. Must implement <see cref="IDataEventDomain"/>.</typeparam>
    /// <returns>An <see cref="IEventConverter{TEntityEventDomain, IDomainEvent}"/> instance that converts events of type
    /// <typeparamref name="TEntityEventDomain"/> to <see cref="IDomainEvent"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown if a suitable event converter cannot be found for the specified domain event entity type.</exception>
    IEventConverter<TEntityEventDomain, IDomainEvent> GetDomainEventConverter<TEntityEventDomain>()
        where TEntityEventDomain : class, IDataEventDomain;

    /// <summary>
    /// Gets an event converter that transforms outbox entity events of the specified type into integration events.
    /// </summary>
    /// <typeparam name="TEntityEventOutbox">The type of the outbox entity event to convert. Must be a class that implements IEntityEventOutbox.</typeparam>
    /// <returns>An event converter that converts instances of the specified outbox entity event type to integration events.</returns>
    /// <exception cref="InvalidOperationException">Thrown if a suitable event converter cannot be found for the specified event entity type.</exception>
    IEventConverter<TEntityEventOutbox, IIntegrationEvent> GetOutboxEventConverter<TEntityEventOutbox>()
        where TEntityEventOutbox : class, IDataEventOutbox;

    /// <summary>
    /// Gets an event converter that transforms inbox entity events of the specified type to integration events.
    /// </summary>
    /// <typeparam name="TEntityEventInbox">The type of the inbox entity event. Must be a class that implements <see cref="IDataEventInbox"/>.</typeparam>
    /// <returns>An <see cref="IEventConverter{TEntityEventInbox, IIntegrationEvent}"/> instance for converting inbox entity
    /// events to integration events.</returns>
    /// <exception cref="InvalidOperationException">Thrown if a suitable event converter cannot be found for the specified event entity type.</exception>
    IEventConverter<TEntityEventInbox, IIntegrationEvent> GetInboxEventConverter<TEntityEventInbox>()
        where TEntityEventInbox : class, IDataEventInbox;

    /// <summary>
    /// Gets an event converter that transforms snapshot event entities of the specified type to the standard snapshot
    /// event interface.
    /// </summary>
    /// <typeparam name="TEntityEventSnapshot">The type of the snapshot event entity to convert. Must implement IEntityEventSnapshot and be a reference type.</typeparam>
    /// <returns>An event converter that converts instances of the specified snapshot event entity type to ISnapshotEvent.</returns>
    /// <exception cref="InvalidOperationException">Thrown if a suitable event converter cannot be found for the specified snapshot event entity type.</exception>
    IEventConverter<TEntityEventSnapshot, ISnapshotEvent> GetSnapshotEventConverter<TEntityEventSnapshot>()
        where TEntityEventSnapshot : class, IDataEventSnapshot;
}