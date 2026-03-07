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
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace System.Events.Data;

/// <summary>
/// Factory that returns the appropriate converter for a given entity event type.
/// </summary>
public sealed class EventConverterProvider : IEventConverterProvider
{
	private readonly ConcurrentDictionary<string, IEventConverter> _converters;

	/// <summary>
	/// Initializes a new instance of the <see cref="EventConverterProvider"/> class with the specified event converters.
	/// </summary>
	/// <param name="converters">A collection of event converters. Cannot be null.</param>
	public EventConverterProvider(IEnumerable<IEventConverter> converters)
	{
		ArgumentNullException.ThrowIfNull(converters);

		_converters = new ConcurrentDictionary<string, IEventConverter>(
			converters.ToDictionary(c => c.EventDataType, c => c));
	}

	/// <inheritdoc/>
	public IEventConverter GetEventConverter(string dataEventType)
	{
		ArgumentNullException.ThrowIfNull(dataEventType);

		if (_converters.TryGetValue(dataEventType, out IEventConverter? converter))
		{
			return converter;
		}

		throw new InvalidOperationException(
			$"No event converter found for data event type '{dataEventType}'. " +
			"Ensure that a converter for this data event type is registered in the service collection.");
	}

	/// <inheritdoc/>
	public IEventConverter<TDataEvent, TEvent> GetEventConverter<TDataEvent, TEvent>()
		where TDataEvent : class, IDataEvent
		where TEvent : class, IEvent
	{
		string dataEventType = typeof(TDataEvent).Name;

		if (_converters.TryGetValue(dataEventType, out IEventConverter? converter) &&
			converter is IEventConverter<TDataEvent, TEvent> typedConverter)
		{
			return typedConverter;
		}

		throw new InvalidOperationException(
			$"No event converter found for data event type '{dataEventType}' and event type '{typeof(TEvent).Name}'. " +
			"Ensure that a converter for this data event type and event type is registered in the service collection.");
	}

	/// <inheritdoc/>
	public bool TryGetEventConverter(string dataEventType, [NotNullWhen(true)] out IEventConverter? converter)
	{
		ArgumentNullException.ThrowIfNull(dataEventType);

		return _converters.TryGetValue(dataEventType, out converter);
	}
}
