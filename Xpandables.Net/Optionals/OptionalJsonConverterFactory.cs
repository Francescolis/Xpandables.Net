
/************************************************************************************************************
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
************************************************************************************************************/
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Xpandables.Net.Optionals;

/// <summary>
/// Supports converting an <see cref="Optional{T}"/> using a factory pattern.
/// </summary>
public sealed class OptionalJsonConverterFactory : JsonConverterFactory
{
    ///<inheritdoc/>
    public override bool CanConvert(Type typeToConvert)
    {
        ArgumentNullException.ThrowIfNull(typeToConvert);
        return typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(Optional<>);
    }

    ///<inheritdoc/>
    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(typeToConvert);
        Type elementType = typeToConvert.GetGenericArguments()[0];

        Type jsonConverterType = typeof(OptionalJsonConverter<>).MakeGenericType(elementType);

        return Activator.CreateInstance(jsonConverterType) as JsonConverter;
    }
}
