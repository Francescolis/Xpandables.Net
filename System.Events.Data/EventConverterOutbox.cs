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
using System.Cache;
using System.ComponentModel.DataAnnotations;
using System.Events.Integration;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace System.Events.Data;

/// <summary>
/// Converts between <see cref="IIntegrationEvent"/> and <see cref="DataEventOutbox"/>.
/// </summary>
/// <param name="typeResolver">The type resolver to use for resolving event types.</param>
/// <param name="converterContext">The context for event conversion. Cannot be null.</param>
public sealed class EventConverterOutbox(ICacheTypeResolver typeResolver, IEventConverterContext converterContext) : IEventConverter<DataEventOutbox, IIntegrationEvent>
{
	private readonly ICacheTypeResolver _typeResolver = typeResolver ?? throw new ArgumentNullException(nameof(typeResolver));
	private readonly IEventConverterContext _converterContext = converterContext ?? throw new ArgumentNullException(nameof(converterContext));

	/// <inheritdoc/>
	public DataEventOutbox ConvertEventToData(IIntegrationEvent @event)
	{
		ArgumentNullException.ThrowIfNull(@event);

		try
		{
			JsonTypeInfo typeInfo = _converterContext.ResolveJsonTypeInfo(@event.GetType());
			string data = JsonSerializer.Serialize(@event, typeInfo);

			return new DataEventOutbox
			{
				KeyId = @event.EventId,
				EventName = @event.GetEventName(),
				CorrelationId = @event.CorrelationId,
				CausationId = @event.CausationId,
				EventData = data
			};
		}
		catch (Exception exception)
			when (exception is not InvalidOperationException and not ValidationException)
		{
			throw new InvalidOperationException(
				$"Failed to convert the event {@event.GetType().Name} to data event. " +
				"See inner exception for details.",
				exception);
		}
	}

	/// <inheritdoc/>
	public IIntegrationEvent ConvertDataToEvent(DataEventOutbox entity)
	{
		ArgumentNullException.ThrowIfNull(entity);

		try
		{
			Type targetType = _typeResolver.Resolve(entity.EventName);
			JsonTypeInfo typeInfo = _converterContext.ResolveJsonTypeInfo(targetType);

			object? @event = JsonSerializer.Deserialize(entity.EventData, typeInfo)
				?? throw new InvalidOperationException(
					$"Failed to deserialize the event data to {typeInfo.Type.Name}.");

			return (IIntegrationEvent)@event;
		}
		catch (Exception exception)
			when (exception is not InvalidOperationException and not ValidationException)
		{
			throw new InvalidOperationException(
				"Failed to convert the data event to event. See inner exception for details.",
				exception);
		}
	}
}
