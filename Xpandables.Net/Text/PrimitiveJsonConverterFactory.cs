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
using System.Text.Json.Serialization.Metadata; // Moved to top

namespace Xpandables.Net.Text;

/// <summary>
/// A factory for creating JSON converters for primitive types.
/// </summary>
public sealed class PrimitiveJsonConverterFactory : JsonConverterFactory
{
    /// <inheritdoc/>
    public override bool CanConvert(Type typeToConvert) =>
        typeToConvert is { IsAbstract: false, IsGenericType: false, IsInterface: false }
        && Array.Exists(
            typeToConvert.GetInterfaces(), i =>
            i.IsGenericType && i.GetGenericTypeDefinition() ==
                typeof(IPrimitive<,>));

    /// <inheritdoc/>
    public override JsonConverter? CreateConverter(
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(typeToConvert);
        ArgumentNullException.ThrowIfNull(options);

        Type valueType = typeToConvert.GetInterfaces()
            .First(i => i.IsGenericType
                && i.GetGenericTypeDefinition() == typeof(IPrimitive<,>))
            .GetGenericArguments()[1];

        var jsonTypeInfoForValue = options.GetTypeInfo(valueType);
        if (jsonTypeInfoForValue is null)
        {
            // Fallback or error handling if type info is not found for TValue
            // This might happen if TValue itself is not part of the context,
            // which is expected as Primitive<T> can wrap any TValue.
            // The factory itself is hard to make fully AOT-safe without restricting TValue.
            // For now, we proceed, but this highlights a deeper issue for full AOT.
            throw new InvalidOperationException(
                $"Could not get JsonTypeInfo for value type {valueType.FullName} used in PrimitiveJsonConverter for {typeToConvert.FullName}. " +
                $"Ensure {valueType.FullName} is included in a JsonSerializableAttribute on your JsonSerializerContext if it's a common type, " +
                $"or this converter might not be fully AOT compatible for all TValue types.");
        }

        // Type converterType = typeof(PrimitiveJsonConverter<,>)
        //     .MakeGenericType(typeToConvert, valueType);

        // return (JsonConverter)Activator.CreateInstance(converterType, jsonTypeInfoForValue)!;

        // Attempt to get the converter from the options.
        // If the specific IPrimitive implementing type is included in the JsonSerializerContext
        // associated with options, this should return the source-generated converter.
        if (options.GetConverter(typeToConvert) is JsonConverter converter)
        {
            return converter;
        }

        // For full AOT safety, all necessary IPrimitive implementing types should be in a context.
        return null;
    }
}

/// <summary>
/// A JSON converter for primitive types.
/// </summary>
/// <typeparam name="TPrimitive">The primitive type.</typeparam>
/// <typeparam name="TValue">The value type.</typeparam>
public sealed class PrimitiveJsonConverter<TPrimitive, TValue> :
    JsonConverter<TPrimitive>
    where TPrimitive : struct, IPrimitive<TPrimitive, TValue>
    where TValue : notnull
{
    private readonly JsonTypeInfo<TValue> _valueTypeInfo;

    public PrimitiveJsonConverter(JsonTypeInfo<TValue> valueTypeInfo)
    {
        ArgumentNullException.ThrowIfNull(valueTypeInfo);
        _valueTypeInfo = valueTypeInfo;
    }

    /// <inheritdoc/>
    public override TPrimitive Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        TPrimitive primitive = JsonSerializer
            .Deserialize(ref reader, _valueTypeInfo) switch
        {
            TValue value => TPrimitive.Create(value),
            _ => TPrimitive.Default()
        };

        return primitive;
    }

    /// <inheritdoc/>
    public override void Write(
        Utf8JsonWriter writer,
        TPrimitive value,
        JsonSerializerOptions options) =>
        JsonSerializer.Serialize(writer, value.Value, _valueTypeInfo);
}
