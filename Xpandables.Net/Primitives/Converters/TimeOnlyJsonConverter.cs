
/*******************************************************************************
 * Copyright (C) 2023 Francis-Black EWANE
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

namespace Xpandables.Net.Primitives.Converters;

/// <summary>
/// Converts an <see cref="TimeOnly"/> to JSON and vis-versa.
/// </summary>
public sealed class TimeOnlyJsonConverter : TypeOnlyJsonConverter<TimeOnly>
{
    ///<inheritdoc/>
    protected override TimeOnly DoRead(ref Utf8JsonReader reader)
        => TimeOnly.Parse(reader.GetString()!, CultureInfo.CurrentCulture);

    ///<inheritdoc/>
    public override TimeOnly ReadAsPropertyName(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
        => TimeOnly.Parse(reader.GetString()!, CultureInfo.CurrentCulture);

    ///<inheritdoc/>
    public override void Write(
        Utf8JsonWriter writer,
        TimeOnly value,
        JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);

        writer.WriteStringValue(
            value.ToString("O", CultureInfo.CurrentCulture));
    }

    ///<inheritdoc/>
    public override void WriteAsPropertyName
        (Utf8JsonWriter writer,
        TimeOnly value,
        JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WritePropertyName(
            value.ToString("O", CultureInfo.CurrentCulture));
    }
}

/// <summary>
/// Converts a null-able <see cref="TimeOnly"/> to JSON and vis-versa.
/// </summary>
public sealed class JsonNullableTimeOnlyConverter
    : TypeOnlyJsonConverter<TimeOnly?>
{
    ///<inheritdoc/>
    protected override TimeOnly? DoRead(ref Utf8JsonReader reader)
        => reader.GetString() is { } value
        ? TimeOnly.Parse(value, CultureInfo.CurrentCulture)
        : default;

    ///<inheritdoc/>
    public override TimeOnly? ReadAsPropertyName(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
        => reader.GetString() is { } value
        ? TimeOnly.Parse(value, CultureInfo.CurrentCulture)
        : default;

    ///<inheritdoc/>
    public override void Write(
        Utf8JsonWriter writer,
        TimeOnly? value,
        JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);

        if (value is null)
        {
            writer.WriteNullValue();
        }
        else
        {
            writer.WriteStringValue(
                value.Value.ToString("O", CultureInfo.CurrentCulture));
        }
    }

    ///<inheritdoc/>
    public override void WriteAsPropertyName(
        Utf8JsonWriter writer,
        TimeOnly? value,
        JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        if (value is null)
        {
            writer.WriteNullValue();
        }
        else
        {
            writer.WritePropertyName(
                value.Value.ToString("O", CultureInfo.CurrentCulture));
        }
    }
}