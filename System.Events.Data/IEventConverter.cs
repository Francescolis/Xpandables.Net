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
using System.ComponentModel;

namespace System.Events.Data;

/// <summary>
/// Defines methods for converting between events and their corresponding data event representations.
/// </summary>
public interface IEventConverter
{
	/// <summary>
	/// Gets the type name of the event data that this converter handles.
	/// This property is used to identify the type of data event during conversion processes.
	/// Implementations should return a unique string that represents the specific data event type they are designed to convert.
	///  This allows for proper routing and handling of events within the system, ensuring that the correct converter is used for each event type.
	/// </summary>
	string EventDataType { get; }

	/// <summary>
	/// Converts the specified event to its corresponding data representation.
	/// This method takes an event and a conversion context as parameters and returns a data event that represents the event in a format suitable for storage or transmission.
	/// Implementations of this method should handle the serialization and mapping of event properties to the data event structure, ensuring that all relevant information from the event is accurately captured in the resulting data event.
	/// The conversion context provides additional information and services that may be necessary during the conversion process, such as type resolution, serialization settings, or other contextual data that can influence how the event is converted.
	/// The method should also include error handling to manage any issues that may arise during the conversion process, such as serialization errors or missing required information in the event. In such cases, it should throw appropriate exceptions to indicate the nature of the failure, allowing calling code to handle these scenarios gracefully.
	/// </summary>
	/// <param name="event">The event to convert. This parameter cannot be null and should contain all necessary information for the conversion process.</param>
	/// <returns>A data representation of the specified event. The returned data event should accurately reflect the properties and information contained in the original event, formatted according to the requirements of the data event structure.</returns>
	/// <exception cref="ArgumentNullException">Thrown when the event parameter is null.</exception>
	/// <exception cref="InvalidOperationException">Thrown when the conversion process fails due to issues such as serialization errors or missing required information in the event.</exception>
	[Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "<Pending>")]
	IDataEvent ConvertEventToData(IEvent @event);

	/// <summary>
	/// Converts the specified data event to an event.
	/// This method takes a data event and a conversion context as parameters and returns an event that represents the data event in a format suitable for processing within the system.
	/// Implementations of this method should handle the deserialization and mapping of data event properties to the event structure, ensuring that all relevant information from the data event is accurately captured in the resulting event.
	/// The conversion context provides additional information and services that may be necessary during the conversion process, such as type resolution, deserialization settings, or other contextual data that can influence how the data event is converted back into an event.
	/// The method should also include error handling to manage any issues that may arise during the conversion process, such as deserialization errors or missing required information in the data event. In such cases, it should throw appropriate exceptions to indicate the nature of the failure, allowing calling code to handle these scenarios gracefully.
	/// </summary>
	/// <param name="event">The data event to convert. This parameter cannot be null and should contain all necessary information for the conversion process.</param>
	/// <returns>An event representation of the specified data event. The returned event should accurately reflect the properties and information contained in the original data event, formatted according to the requirements of the event structure.</returns>
	/// <exception cref="ArgumentNullException">Thrown when the data event parameter is null.</exception>
	/// <exception cref="InvalidOperationException">Thrown when the conversion process fails due to issues such as deserialization errors or missing required information in the data event.</exception>
	[Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "<Pending>")]
	IEvent ConvertDataToEvent(IDataEvent @event);
}

/// <summary>
/// Defines methods for converting between domain events and their corresponding data event representations.
/// </summary>
/// <remarks>Implementations of this interface enable translation between domain-level events and
/// persistence-layer data events, facilitating event sourcing and serialization scenarios. The interface supports
/// both strongly-typed and generic conversion methods, allowing for flexible integration with various event storage and
/// processing systems.</remarks>
/// <typeparam name="TDataEvent">The type of the data event that implements IDataEvent. Must be a reference type.</typeparam>
/// <typeparam name="TEvent">The type of the domain event that implements IEvent.</typeparam>
public interface IEventConverter<TDataEvent, TEvent> : IEventConverter
	where TDataEvent : class, IDataEvent
	where TEvent : class, IEvent
{

	[Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1033:Interface methods should be callable by child types", Justification = "<Pending>")]
	string IEventConverter.EventDataType => typeof(TDataEvent).Name;

	/// <summary>
	/// Converts the specified event to its corresponding data representation.
	/// </summary>
	/// <param name="event">The event to convert. Cannot be null.</param>
	/// <returns>A data representation of the specified event.</returns>
	[Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "<Pending>")]
	TDataEvent ConvertEventToData(TEvent @event);

	[EditorBrowsable(EditorBrowsableState.Never)]
	IDataEvent IEventConverter.ConvertEventToData(IEvent @event)
	{
		if (@event is not TEvent typedEvent)
		{
			throw new InvalidOperationException($"Invalid event type. Expected {typeof(TEvent).FullName}, but received {@event.GetType().FullName}.");
		}

		return ConvertEventToData(typedEvent);
	}

	/// <summary>
	/// Converts the specified data event to an event of type TEvent.
	/// </summary>
	/// <param name="event">The data event to convert. Cannot be null.</param>
	/// <returns>An event of type TEvent that represents the converted data event.</returns>
	[Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "<Pending>")]
	TEvent ConvertDataToEvent(TDataEvent @event);

	[EditorBrowsable(EditorBrowsableState.Never)]
	IEvent IEventConverter.ConvertDataToEvent(IDataEvent @event)
	{
		if (@event is not TDataEvent typedDataEvent)
		{
			throw new InvalidOperationException($"Invalid data event type. Expected {typeof(TDataEvent).FullName}, but received {@event.GetType().FullName}.");
		}

		return ConvertDataToEvent(typedDataEvent);
	}
}
