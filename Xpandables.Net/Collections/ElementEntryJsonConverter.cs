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

using Microsoft.Extensions.Primitives;

namespace Xpandables.Net.Collections;

/// <summary>
/// A custom JSON converter for <see cref="ElementEntry"/> that handles <see cref="StringValues"/>.
/// </summary>
public class ElementEntryJsonConverter : JsonConverter<ElementEntry>
{
    /// <inheritdoc/>
    public override ElementEntry Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected start of object");
        }

        string key = string.Empty;
        StringValues values = default;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException("Expected property name");
            }

            string propertyName = reader.GetString()!;
            reader.Read();

            switch (propertyName)
            {
                case nameof(ElementEntry.Key):
                    key = reader.GetString()!;
                    break;
                case nameof(ElementEntry.Values):
                    switch (reader.TokenType)
                    {
                        case JsonTokenType.StartArray:
                            {
                                List<string> valuesList = [];
                                while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                                {
                                    if (reader.TokenType == JsonTokenType.String)
                                    {
                                        valuesList.Add(reader.GetString()!);
                                    }
                                }

                                values = new StringValues([.. valuesList]);
                                break;
                            }
                        case JsonTokenType.String:
                            values = new StringValues(reader.GetString());
                            break;
                        default:
                            throw new JsonException("Expected string or array of strings for Values");
                    }

                    break;
                default:
                    throw new JsonException($"Unexpected property name: {propertyName}");
            }
        }

        return new ElementEntry { Key = key, Values = values };
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, ElementEntry value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WritePropertyName(nameof(ElementEntry.Key));
        writer.WriteStringValue(value.Key);

        writer.WritePropertyName(nameof(ElementEntry.Values));
        writer.WriteStartArray();
        foreach (string? item in value.Values)
        {
            writer.WriteStringValue(item);
        }

        writer.WriteEndArray();

        writer.WriteEndObject();
    }
}