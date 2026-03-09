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

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace System;

/// <summary>
/// Provides functionality to convert between <see langword="TimeOnly"/> instances and their JSON representation.
/// </summary>
/// <remarks>This converter handles the serialization and deserialization of <see langword="TimeOnly"/> values,
/// ensuring that they are correctly formatted as strings in JSON. It supports parsing and writing using the current
/// culture for string representation.</remarks>
public sealed class TimeOnlyJsonConverter : JsonConverter<TimeOnly>
{
	/// <inheritdoc/>
	public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		string? value = reader.GetString();
		if (string.IsNullOrWhiteSpace(value))
		{
			return default;
		}

		if (TimeOnly.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.None, out var result))
		{
			return result;
		}

		return TimeOnly.Parse(value, CultureInfo.InvariantCulture);
	}

	/// <inheritdoc/>
	public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
	{
		ArgumentNullException.ThrowIfNull(writer);

		writer.WriteStringValue(value.ToString(CultureInfo.CurrentCulture));
	}
}

/// <summary>
/// Provides a JSON converter for nullable TimeOnly values, enabling serialization and deserialization of TimeOnly?
/// types.
/// </summary>
/// <remarks>This converter handles null values by writing a null JSON value and reading an empty string as null.
/// It supports parsing TimeOnly values from strings using the current culture and can throw exceptions if the string
/// format is invalid.</remarks>
public sealed class NullableTimeOnlyJsonConverter : JsonConverter<TimeOnly?>
{
	/// <inheritdoc/>
	public override TimeOnly? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		string? value = reader.GetString();
		if (string.IsNullOrWhiteSpace(value))
		{
			return null;
		}

		if (TimeOnly.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.None, out var result))
		{
			return result;
		}

		return TimeOnly.Parse(value, CultureInfo.InvariantCulture);
	}

	/// <inheritdoc/>
	public override void Write(Utf8JsonWriter writer, TimeOnly? value, JsonSerializerOptions options)
	{
		ArgumentNullException.ThrowIfNull(writer);

		if (value is null)
		{
			writer.WriteNullValue();
			return;
		}

		writer.WriteStringValue(value.Value.ToString(CultureInfo.CurrentCulture));
	}
}
