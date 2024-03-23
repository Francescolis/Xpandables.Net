
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
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Xpandables.Net.Primitives;

/// <summary>
/// When placed on an implementation of 
/// <see cref="IPrimitive{TSelf, TValue}"/> type, 
/// specifies the json converter factory type to be used.
/// </summary>
/// <remarks>You can use <see cref="IPrimitiveOnDeserialized{TPrimitive, TValue}"/> 
/// and <see cref="IPrimitiveOnSerializing{TPrimitive, TValue}"/>
/// on the target type to manage serialization process.</remarks>
[AttributeUsage(AttributeTargets.Struct, AllowMultiple = false)]
public sealed class PrimitiveJsonConverterAttribute : JsonConverterAttribute
{
    /// <summary>
    /// Initializes a new instance of 
    /// <see cref="PrimitiveJsonConverterAttribute"/> with the
    /// <see cref="PrimitiveJsonConverterFactory"/> type.
    /// </summary>
    public PrimitiveJsonConverterAttribute()
        : base(typeof(PrimitiveJsonConverterFactory)) { }
}

/// <summary>
/// Converts a primitive type to/from JSON.
/// </summary>
/// <typeparam name="TPrimitive">The type of target primitive.</typeparam>
/// <typeparam name="TValue">The type of the primitive value.</typeparam>
public sealed class PrimitiveJsonConverter<TPrimitive, TValue>
    : JsonConverter<TPrimitive>
    where TValue : notnull
    where TPrimitive : struct, IPrimitive<TPrimitive, TValue>
{
    ///<inheritdoc/>
    public override TPrimitive Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        TPrimitive primitive = JsonSerializer
            .Deserialize<TValue?>(ref reader, options) switch
        {
            TValue value => TPrimitive.CreateInstance(value),
            _ => TPrimitive.DefaultInstance()
        };

        if (primitive
            is IPrimitiveOnDeserialized<TPrimitive, TValue> { } deserialized)
            primitive = deserialized.OnDeserialized(primitive);

        return primitive;
    }

    ///<inheritdoc/>
    public override void Write(
        Utf8JsonWriter writer,
        TPrimitive value,
        JsonSerializerOptions options)
    {
        if (value is IPrimitiveOnSerializing<TPrimitive, TValue> { } serializing)
            value = serializing.OnSerializing(value);

        JsonSerializer.Serialize(writer, value.Value, typeof(TValue), options);
    }
}

/// <summary>
/// Supports converting an implementation 
/// of <see cref="IPrimitive{TValue}"/> using a factory pattern.
/// </summary>
public sealed class PrimitiveJsonConverterFactory : JsonConverterFactory
{
    ///<inheritdoc/>
    public override bool CanConvert(Type typeToConvert)
    {
        ArgumentNullException.ThrowIfNull(typeToConvert);

        return !typeToConvert.IsAbstract
            && !typeToConvert.IsInterface
            && !typeToConvert.IsGenericType
            && Array.Exists(
                typeToConvert.GetInterfaces(),
                t => t.IsGenericType
                && t.GetGenericTypeDefinition() == typeof(IPrimitive<,>));
    }

    ///<inheritdoc/>
    public override JsonConverter? CreateConverter(
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(typeToConvert);

        Type[] valueTypes = typeToConvert
            .GetInterfaces()
            .First(f => f.IsGenericType
            && f.GetGenericTypeDefinition() == typeof(IPrimitive<,>))
            .GetGenericArguments();

        Type jsonPrimitiveConverterType = typeof(PrimitiveJsonConverter<,>)
            .MakeGenericType(valueTypes);

        return Activator
            .CreateInstance(jsonPrimitiveConverterType) as JsonConverter;
    }
}