
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
        Type valueType = typeToConvert.GetInterfaces()
            .First(i => i.IsGenericType
                && i.GetGenericTypeDefinition() == typeof(IPrimitive<,>))
            .GetGenericArguments()[1];

        Type converterType = typeof(PrimitiveJsonConverter<,>)
            .MakeGenericType(typeToConvert, valueType);

        return (JsonConverter)Activator.CreateInstance(converterType)!;
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
    /// <inheritdoc/>
    public override TPrimitive Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        TPrimitive primitive = JsonSerializer
            .Deserialize<TValue>(ref reader, options) switch
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
        JsonSerializer.Serialize(writer, value.Value, options);
}
