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
using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.Extensions.Primitives;

namespace Xpandables.Net.Collections;

/// <summary>
/// JSON converter factory for ElementEntry types, providing AOT-compatible serialization for .NET 10.
/// </summary>
public sealed class ElementEntryJsonConverterFactory : JsonConverterFactory
{
    /// <inheritdoc/>
    public override bool CanConvert(Type typeToConvert)
    {
        ArgumentNullException.ThrowIfNull(typeToConvert);
        return typeToConvert == typeof(ElementEntry) || typeToConvert == typeof(ElementEntry?);
    }

    /// <inheritdoc/>
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(typeToConvert);
        ArgumentNullException.ThrowIfNull(options);

        if (options.TypeInfoResolverChain.FirstOrDefault(resolver => resolver is ElementEntryContext) is null)
        {
            options.TypeInfoResolverChain.Add(ElementEntryContext.Default);
        }

        return new ElementEntryJsonConverter();
    }
}

/// <summary>
/// JSON converter for ElementEntry that provides clean serialization optimized for AOT scenarios.
/// </summary>
public sealed class ElementEntryJsonConverter : JsonConverter<ElementEntry>
{
    /// <inheritdoc/>
    public override ElementEntry Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("ElementEntry must be a JSON object.");
        }

        string? key = null;
        StringValues values = default;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();
                reader.Read();

                switch (propertyName?.ToUpperInvariant())
                {
                    case "KEY":
                        key = reader.GetString();
                        break;
                    case "VALUES":
                        if (reader.TokenType == JsonTokenType.StartArray)
                        {
                            var valuesList = new List<string>();
                            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                            {
                                if (reader.TokenType == JsonTokenType.String)
                                {
                                    var value = reader.GetString();
                                    if (value is not null)
                                        valuesList.Add(value);
                                }
                            }
                            values = new StringValues([.. valuesList]);
                        }
                        else if (reader.TokenType == JsonTokenType.String)
                        {
                            var singleValue = reader.GetString();
                            values = singleValue is not null ? new StringValues(singleValue) : StringValues.Empty;
                        }
                        break;
                }
            }
        }

        if (string.IsNullOrEmpty(key))
        {
            throw new JsonException("ElementEntry requires a non-null key.");
        }

        return new ElementEntry { Key = key, Values = values };
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, ElementEntry value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);

        writer.WriteStartObject();
        writer.WriteString("key", value.Key);

        writer.WritePropertyName("values");
        if (value.Values.Count == 0)
        {
            writer.WriteStartArray();
            writer.WriteEndArray();
        }
        else if (value.Values.Count == 1)
        {
            writer.WriteStringValue(value.Values[0]);
        }
        else
        {
            writer.WriteStartArray();
            foreach (var val in value.Values)
            {
                writer.WriteStringValue(val);
            }
            writer.WriteEndArray();
        }

        writer.WriteEndObject();
    }
}