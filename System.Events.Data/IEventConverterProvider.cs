/*******************************************************************************
 * Copyright (C) 2025-2026 Kamersoft
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
using System.Events.Domain;
using System.Events.Integration;

namespace System.Events.Data;

/// <summary>
/// Defines a factory for retrieving event converters based on event type.
/// </summary>
public interface IEventConverterProvider
{
	/// <summary>
	/// Gets an event converter for the specified data event type. The returned converter can be used to convert between
	/// the specified data event type and its corresponding event type.
	/// </summary>
	/// <param name="dataEventType">The type of the data event for which to retrieve a converter.</param>
	/// <returns>An <see cref="IEventConverter"/> instance capable of converting the specified data event type.</returns>
	/// <exception cref="InvalidOperationException">Thrown if a suitable event converter cannot be found for the specified data event type.</exception>
	/// <exception cref="ArgumentNullException">Thrown if the dataEventType parameter is null.</exception>
	IEventConverter GetEventConverter(string dataEventType);

	/// <summary>
	/// Gets an event converter that transforms between the specified data event type and event type.
	/// </summary>
	/// <typeparam name="TDataEvent">The type of the data event. Must implement <see cref="IDataEvent"/>.</typeparam>
	/// <typeparam name="TEvent">The type of the event. Must implement <see cref="IEvent"/>.</typeparam>
	/// <returns>An <see cref="IEventConverter{TDataEvent, TEvent}"/> instance capable of converting between the specified data event type and event type.</returns>
	/// <exception cref="InvalidOperationException">Thrown if a suitable event converter cannot be found for the specified data event type and event type.</exception>
	IEventConverter<TDataEvent, TEvent> GetEventConverter<TDataEvent, TEvent>()
		where TDataEvent : class, IDataEvent
		where TEvent : class, IEvent;

	/// <summary>
	/// Tries to get an event converter for the specified data event type. Returns true if a suitable converter is found; otherwise, false.
	/// </summary>
	/// <param name="dataEventType">The type of the data event for which to retrieve a converter.</param>
	/// <param name="converter">When this method returns, contains the event converter if found; otherwise, null.</param>
	/// <returns>True if a suitable event converter is found; otherwise, false.</returns>
	/// <exception cref="ArgumentNullException">Thrown if the dataEventType parameter is null.</exception>
	bool TryGetEventConverter(string dataEventType, [NotNullWhen(true)] out IEventConverter? converter);
}

/// <summary>
/// Defines a factory for retrieving event converters based on event type.
/// </summary>
public static class EventConverterFactoryExtensions
{
	extension(IEventConverterProvider factory)
	{
		/// <summary>
		/// Gets an event converter that transforms domain data events of the specified type into domain events.
		/// </summary>
		/// <typeparam name="TDataEventDomain">The type of the domain data event. Must implement <see cref="IDataEventDomain"/>.</typeparam>
		/// <returns>An <see cref="IEventConverter{TDataEventDomain, IDomainEvent}"/> instance for converting domain data events to domain events.</returns>
		/// <exception cref="InvalidOperationException">Thrown if a suitable event converter cannot be found for the specified domain data event type.</exception>
		public IEventConverter<TDataEventDomain, IDomainEvent> GetDomainEventConverter<TDataEventDomain>()
			where TDataEventDomain : class, IDataEventDomain
		{
			ArgumentNullException.ThrowIfNull(factory);

			if (factory.TryGetEventConverter(typeof(TDataEventDomain).Name, out IEventConverter? converter) &&
				converter is IEventConverter<TDataEventDomain, IDomainEvent> typed)
			{
				return typed;
			}

			throw new InvalidOperationException(
				$"No converter registered for domain data event type '{typeof(TDataEventDomain).Name}'.");
		}


		/// <summary>
		/// Gets an event converter that transforms between the specified data outbox event type and integration events.
		/// </summary>
		/// <typeparam name="TDataEventOutbox">The type of the data outbox event. Must implement <see cref="IDataEventOutbox"/>.</typeparam>
		/// <returns>An <see cref="IEventConverter{TDataEventOutbox, IIntegrationEvent}"/> instance for converting data outbox events to integration events.</returns>
		/// <exception cref="InvalidOperationException">Thrown if a suitable event converter cannot be found for the specified data outbox event type.</exception>
		public IEventConverter<TDataEventOutbox, IIntegrationEvent> GetOutboxEventConverter<TDataEventOutbox>()
			where TDataEventOutbox : class, IDataEventOutbox
		{
			ArgumentNullException.ThrowIfNull(factory);

			if (factory.TryGetEventConverter(typeof(TDataEventOutbox).Name, out IEventConverter? converter) &&
				converter is IEventConverter<TDataEventOutbox, IIntegrationEvent> typed)
			{
				return typed;
			}

			throw new InvalidOperationException(
				$"No converter registered for data outbox event type '{typeof(TDataEventOutbox).Name}'.");
		}

		/// <summary>
		/// Gets an event converter that transforms inbox data events of the specified type into integration events.
		/// </summary>
		/// <typeparam name="TDataEventInbox">The type of the inbox data event. Must implement <see cref="IDataEventInbox"/>.</typeparam>
		/// <returns>An <see cref="IEventConverter{TDataEventInbox, IIntegrationEvent}"/> instance for converting inbox data events to integration events.</returns>
		/// <exception cref="InvalidOperationException">Thrown if a suitable event converter cannot be found for the specified inbox data event type.</exception>
		public IEventConverter<TDataEventInbox, IIntegrationEvent> GetInboxEventConverter<TDataEventInbox>()
			where TDataEventInbox : class, IDataEventInbox
		{
			ArgumentNullException.ThrowIfNull(factory);

			if (factory.TryGetEventConverter(typeof(TDataEventInbox).Name, out IEventConverter? converter) &&
				converter is IEventConverter<TDataEventInbox, IIntegrationEvent> typed)
			{
				return typed;
			}

			throw new InvalidOperationException(
				$"No converter registered for inbox data event type '{typeof(TDataEventInbox).Name}'.");
		}

		/// <summary>
		/// Gets an event converter that transforms snapshot data events of the specified type into snapshot events.
		/// </summary>
		/// <typeparam name="TDataEventSnapshot">The type of the snapshot data event. Must implement <see cref="IDataEventSnapshot"/>.</typeparam>
		/// <returns>An <see cref="IEventConverter{TDataEventSnapshot, ISnapshotEvent}"/> instance for converting snapshot data events to snapshot events.</returns>
		/// <exception cref="InvalidOperationException">Thrown if a suitable event converter cannot be found for the specified snapshot data event type.</exception>
		public IEventConverter<TDataEventSnapshot, ISnapshotEvent> GetSnapshotEventConverter<TDataEventSnapshot>()
			where TDataEventSnapshot : class, IDataEventSnapshot
		{
			ArgumentNullException.ThrowIfNull(factory);

			if (factory.TryGetEventConverter(typeof(TDataEventSnapshot).Name, out IEventConverter? converter) &&
				converter is IEventConverter<TDataEventSnapshot, ISnapshotEvent> typed)
			{
				return typed;
			}

			throw new InvalidOperationException(
				$"No converter registered for snapshot data event type '{typeof(TDataEventSnapshot).Name}'.");
		}
	}
}
