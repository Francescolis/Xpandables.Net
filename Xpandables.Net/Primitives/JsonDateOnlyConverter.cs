
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

// Ignore Spelling: Nullable Json

using System.Globalization;
using System.Text.Json;

namespace Xpandables.Net.Primitives;

/// <summary>
/// Converts an <see cref="DateOnly"/> to JSON and vis-versa.
/// </summary>
public sealed class JsonDateOnlyConverter : JsonTypeOnlyConverter<DateOnly>
{
    ///<inheritdoc/>
    protected override DateOnly DoRead(ref Utf8JsonReader reader)
        => DateOnly.FromDateTime(reader.GetDateTime());

    ///<inheritdoc/>
    public override void Write(
        Utf8JsonWriter writer,
        DateOnly value,
        JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);

        writer
            .WriteStringValue(value.ToString("O", CultureInfo.CurrentCulture));
    }
}

/// <summary>
/// Converts a null-able <see cref="DateOnly"/> to JSON and vis-versa.
/// </summary>
public sealed class JsonNullableDateOnlyConverter
    : JsonTypeOnlyConverter<DateOnly?>
{
    ///<inheritdoc/>
    protected override DateOnly? DoRead(ref Utf8JsonReader reader)
        => reader.TryGetDateTime(out DateTime dateTime)
        ? DateOnly.FromDateTime(dateTime)
        : default;

    ///<inheritdoc/>
    public override void Write(
        Utf8JsonWriter writer,
        DateOnly? value,
        JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);

        if (value is null)
            writer.WriteNullValue();
        else
            writer
                .WriteStringValue(value.Value
                    .ToString("O", CultureInfo.CurrentCulture));
    }
}
