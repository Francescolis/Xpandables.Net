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
/// Provides functionality to convert between DateOnly instances and their JSON representation.
/// </summary>
/// <remarks>This converter handles reading and writing of DateOnly values in JSON format, utilizing the current
/// culture for formatting. If the input string is null or whitespace, the default DateOnly value is returned. The
/// converter attempts to parse the date using the current culture first, and falls back to invariant culture if parsing
/// fails.</remarks>
public sealed class DateOnlyJsonConverter : JsonConverter<DateOnly>
{
	/// <inheritdoc/>
	public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		string? value = reader.GetString();

		if (string.IsNullOrWhiteSpace(value))
		{
			return default;
		}

		// Try current culture first
		if (DateOnly.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.None, out var result))
		{
			return result;
		}

		// Fallback to invariant culture
		return DateOnly.Parse(value, CultureInfo.InvariantCulture);
	}

	/// <inheritdoc/>
	public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
	{
		ArgumentNullException.ThrowIfNull(writer);

		// Format using current culture
		writer.WriteStringValue(value.ToString(CultureInfo.CurrentCulture));
	}
}

/// <summary>
/// Provides a custom JSON converter for handling nullable DateOnly values during serialization and deserialization.
/// </summary>
/// <remarks>This converter enables the conversion of nullable DateOnly values to and from their string
/// representation in JSON. During serialization, null values are written as JSON null, while non-null values are
/// formatted using the current culture. During deserialization, empty or whitespace strings are interpreted as null. If
/// the string representation is not in a valid format, a FormatException may be thrown. Use this converter to support
/// DateOnly? properties in JSON payloads where null values and culture-specific formatting are required.</remarks>
public sealed class NullableDateOnlyJsonConverter : JsonConverter<DateOnly?>
{
	/// <inheritdoc/>
	public override DateOnly? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		string? value = reader.GetString();
		if (string.IsNullOrWhiteSpace(value))
		{
			return null;
		}

		if (DateOnly.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.None, out var result))
		{
			return result;
		}

		return DateOnly.Parse(value, CultureInfo.InvariantCulture);
	}

	/// <inheritdoc/>
	public override void Write(Utf8JsonWriter writer, DateOnly? value, JsonSerializerOptions options)
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
