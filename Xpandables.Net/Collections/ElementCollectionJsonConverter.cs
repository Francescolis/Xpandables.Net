/*******************************************************************************
 * Copyright (C) 2024 Francis-Black EWANE
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

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Xpandables.Net.Collections;

/// <summary>
/// Converts between JSON and the ElementCollection type. 
/// It reads JSON data to create an ElementCollection and writes an ElementCollection back to JSON.
/// </summary>
public sealed class ElementCollectionJsonConverter : JsonConverter<ElementCollection>
{
    /// <summary>
    /// Reads and converts the JSON to <see cref="ElementCollection"/>.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="typeToConvert">The type to convert.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>The converted <see cref="ElementCollection"/>.</returns>
    public override ElementCollection Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        ElementEntry[]? entries = JsonSerializer.Deserialize<ElementEntry[]>(ref reader, options);

        return entries is not null
            ? ElementCollection.With(entries)
            : ElementCollection.Empty;
    }

    /// <summary>
    /// Writes the <see cref="ElementCollection"/> to JSON.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <param name="value">The value to write.</param>
    /// <param name="options">The serializer options.</param>
    public override void Write(
        Utf8JsonWriter writer,
        ElementCollection value,
        JsonSerializerOptions options) =>
        JsonSerializer.Serialize(writer, value.ToArray(), options);
}