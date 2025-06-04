
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
/// A factory for creating JSON converters for the <see cref="Optional{T}"/> type.  
/// </summary> 
public sealed class OptionalJsonConverterFactory : JsonConverterFactory
{
    /// <inheritdoc />
    public override bool CanConvert(Type typeToConvert) =>
        typeToConvert.IsGenericType
        && typeToConvert.GetGenericTypeDefinition() == typeof(Optional<>);

    /// <inheritdoc />
    public override JsonConverter? CreateConverter(
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(typeToConvert);
        ArgumentNullException.ThrowIfNull(options);

        Type valueType = typeToConvert.GetGenericArguments()[0];

        // Get JsonTypeInfo for the inner type T
        var jsonTypeInfoForValueType = options.GetTypeInfo(valueType);
        if (jsonTypeInfoForValueType is null)
        {
            throw new InvalidOperationException(
                $"Could not get JsonTypeInfo for type {valueType.FullName} from JsonSerializerOptions. " +
                $"Ensure {valueType.FullName} (or a more generic version like 'object' if used with Optional<object>) " +
                $"is included in a JsonSerializableAttribute on your JsonSerializerContext.");
        }

        // Type converterType = typeof(OptionalJsonConverter<>)
        //     .MakeGenericType(valueType);

        // // Pass the JsonTypeInfo to the constructor of OptionalJsonConverter<T>
        // return (JsonConverter)Activator.CreateInstance(converterType, jsonTypeInfoForValueType)!;

        // Attempt to get the converter from the options.
        // If Optional<T> for the specific T is included in the JsonSerializerContext associated with options,
        // this should return the source-generated converter.
        if (options.GetConverter(typeToConvert) is JsonConverter converter)
        {
            return converter;
        }

        // If the specific Optional<T> is not in the context, and we want to strictly enforce AOT-safety,
        // we should not fall back to reflection-based instantiation.
        // Returning null tells the JsonSerializer to use its next available converter or default behavior,
        // which might involve reflection if the type isn't fully covered by source generation elsewhere.
        // For full AOT safety, all necessary Optional<T> instantiations should be in a context.
        return null;
    }
}
