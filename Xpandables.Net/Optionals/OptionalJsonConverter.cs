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

namespace Xpandables.Net.Optionals;

/// <summary>
/// A JSON converter for the <see cref="Optional{T}"/> type.
/// </summary>
using System.Text.Json.Serialization.Metadata;

/// <typeparam name="T">The type of the value.</typeparam>
public sealed class OptionalJsonConverter<T> : JsonConverter<Optional<T>>
{
    private readonly JsonTypeInfo<T> _jsonTypeInfo;

    public OptionalJsonConverter(JsonTypeInfo<T> jsonTypeInfo)
    {
        ArgumentNullException.ThrowIfNull(jsonTypeInfo);
        _jsonTypeInfo = jsonTypeInfo;
    }

    /// <inheritdoc />
    public override Optional<T> Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType is JsonTokenType.Null or JsonTokenType.None)
        {
            return Optional.Empty<T>();
        }

        T? value = JsonSerializer.Deserialize(ref reader, _jsonTypeInfo);

        return value.ToOptional();
    }

    /// <inheritdoc />
    public override void Write(
        Utf8JsonWriter writer,
        Optional<T> value,
        JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        // options are not used directly if _jsonTypeInfo is used,
        // but the parameter is part of the overridden method signature.

        if (value.IsNotEmpty)
        {
            JsonSerializer.Serialize(writer, value.Value, _jsonTypeInfo);
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}