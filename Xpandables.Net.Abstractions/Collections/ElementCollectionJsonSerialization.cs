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
/// JSON converter factory for ElementCollection types, providing AOT-compatible serialization for .NET 10.
/// </summary>
public sealed class ElementCollectionJsonConverterFactory : JsonConverterFactory
{
    /// <inheritdoc/>
    public override bool CanConvert(Type typeToConvert)
    {
        ArgumentNullException.ThrowIfNull(typeToConvert);

        return typeToConvert == typeof(ElementCollection) || typeToConvert == typeof(ElementCollection?);
    }

    /// <inheritdoc/>
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(typeToConvert);
        ArgumentNullException.ThrowIfNull(options);

        options.TypeInfoResolver ??= ElementEntryContext.Default;

        return new ElementCollectionJsonConverter();
    }
}

/// <summary>
/// JSON converter for ElementCollection that provides clean serialization as an array of ElementEntry objects.
/// </summary>
public sealed class ElementCollectionJsonConverter : JsonConverter<ElementCollection>
{
    /// <inheritdoc/>
    public override ElementCollection Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return ElementCollection.Empty;
        }

        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("ElementCollection must be serialized as a JSON array.");
        }

        var entries = new List<ElementEntry>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
                break;

            if (reader.TokenType == JsonTokenType.StartObject)
            {
                // Use the ElementEntry converter to deserialize individual entries
                var entry = (ElementEntry)JsonSerializer.Deserialize(ref reader, typeof(ElementEntry), ElementEntryContext.Default)!;
                entries.Add(entry);
            }
        }

        return ElementCollection.With(entries);
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, ElementCollection value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);

        writer.WriteStartArray();

        foreach (var entry in value)
        {
            JsonSerializer.Serialize(writer, entry, typeof(ElementEntry), ElementEntryContext.Default);
        }

        writer.WriteEndArray();
    }
}