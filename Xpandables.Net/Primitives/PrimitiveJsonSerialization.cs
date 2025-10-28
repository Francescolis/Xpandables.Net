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
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

using Xpandables.Net.Primitives;

namespace Xpandables.Net.Text;

/// <summary>
/// Attribute to specify a custom JSON converter for a primitive type, optimized for .NET 10 AOT scenarios.
/// </summary>
/// <remarks>This attribute automatically applies the primitive JSON converter factory to enable 
/// seamless JSON serialization/deserialization for types implementing <see cref="IPrimitive{TPrimitive, TValue}"/>.
/// The converter handles the primitive as its underlying value type for clean JSON output.</remarks>
[AttributeUsage(AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
public sealed class PrimitiveJsonConverterAttribute : JsonConverterAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PrimitiveJsonConverterAttribute"/> class.
    /// </summary>
    public PrimitiveJsonConverterAttribute()
        : base(typeof(PrimitiveJsonConverterFactory)) { }
}

/// <summary>
/// A high-performance factory for creating JSON converters for primitive types with caching and AOT optimization.
/// </summary>
/// <remarks>This factory creates converters for types implementing <see cref="IPrimitive{TPrimitive, TValue}"/> 
/// and caches them for optimal performance. It's designed to work efficiently with .NET 10 AOT compilation.</remarks>
public sealed class PrimitiveJsonConverterFactory : JsonConverterFactory
{
    // Cache for expensive reflection operations
    private static readonly ConcurrentDictionary<Type, (bool CanConvert, Type? ValueType)> _typeCache = new();
    private static readonly ConcurrentDictionary<Type, JsonConverter?> _converterCache = new();

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool CanConvert(Type typeToConvert)
    {
        ArgumentNullException.ThrowIfNull(typeToConvert);

        return _typeCache.GetOrAdd(typeToConvert, static type =>
        {
            if (type.IsAbstract || type.IsGenericType || type.IsInterface || !type.IsValueType)
                return (false, null);

            // More efficient interface lookup using spans
            var interfaces = type.GetInterfaces();
            for (int i = 0; i < interfaces.Length; i++)
            {
                var interfaceType = interfaces[i];
                if (interfaceType.IsGenericType &&
                    interfaceType.GetGenericTypeDefinition() == typeof(IPrimitive<,>))
                {
                    var valueType = interfaceType.GetGenericArguments()[1];
                    return (true, valueType);
                }
            }

            return (false, null);
        }).CanConvert;
    }

    /// <inheritdoc/>
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2070",
        Justification = "The generic arguments are validated and cached during CanConvert")]
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2071",
        Justification = "Primitive types are constrained to struct types with known constructors")]
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL3050",
        Justification = "Primitive JSON converter factory is designed for known primitive types")]
    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(typeToConvert);
        ArgumentNullException.ThrowIfNull(options);

        options.TypeInfoResolver ??= PrimitiveJsonContext.Default;

        return _converterCache.GetOrAdd(typeToConvert, static type =>
        {
            var cacheResult = _typeCache.GetOrAdd(type, static t =>
            {
                if (t.IsAbstract || t.IsGenericType || t.IsInterface || !t.IsValueType)
                    return (false, null);

                var interfaces = t.GetInterfaces();
                for (int i = 0; i < interfaces.Length; i++)
                {
                    var interfaceType = interfaces[i];
                    if (interfaceType.IsGenericType &&
                        interfaceType.GetGenericTypeDefinition() == typeof(IPrimitive<,>))
                    {
                        return (true, interfaceType.GetGenericArguments()[1]);
                    }
                }
                return (false, null);
            });

            if (!cacheResult.CanConvert || cacheResult.ValueType is null)
                return null;

            var converterType = typeof(PrimitiveJsonConverter<,>)
                .MakeGenericType(type, cacheResult.ValueType);

            return (JsonConverter)Activator.CreateInstance(converterType)!;
        });
    }

    /// <summary>
    /// Clears the internal caches. This method is primarily intended for testing scenarios.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void ClearCaches()
    {
        _typeCache.Clear();
        _converterCache.Clear();
    }
}

/// <summary>
/// A high-performance JSON converter for primitive types that serializes them as their underlying value.
/// </summary>
/// <typeparam name="TPrimitive">The primitive type implementing <see cref="IPrimitive{TPrimitive, TValue}"/>.</typeparam>
/// <typeparam name="TValue">The underlying value type of the primitive.</typeparam>
/// <remarks>This converter serializes primitives directly as their underlying value for clean JSON output,
/// and deserializes JSON values back into primitive instances. It's optimized for performance and AOT compatibility.</remarks>
public sealed class PrimitiveJsonConverter<TPrimitive, TValue> : JsonConverter<TPrimitive>
    where TPrimitive : struct, IPrimitive<TPrimitive, TValue>
    where TValue : notnull
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override TPrimitive Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return TPrimitive.GetDefault();

        if (typeof(TValue) is not { IsPrimitive: true } && typeof(TValue) != typeof(string))
        {
            throw new JsonException($"Cannot convert JSON to {typeof(TValue).Name}. Expected a primitive or string type.");
        }

        TValue? valueT = (TValue?)JsonSerializer.Deserialize(ref reader, typeof(TValue), PrimitiveJsonContext.Default);

        return valueT is not null
            ? TPrimitive.Create(valueT)
            : TPrimitive.GetDefault();
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Write(Utf8JsonWriter writer, TPrimitive value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);

        if (typeof(TValue) is not { IsPrimitive: true } && typeof(TValue) != typeof(string))
        {
            throw new JsonException($"Cannot serialize to JSON from {typeof(TValue).Name}. Expected a primitive or string type.");
        }

        if (value.Value is not null)
        {
            JsonSerializer.Serialize(writer, value.Value, typeof(TValue), PrimitiveJsonContext.Default);
        }
    }

    /// <inheritdoc/>
    public override bool HandleNull => false; // Let the framework handle null values
}

/// <summary>
/// Source generation context for primitive JSON serialization, optimized for .NET 10 AOT scenarios.
/// </summary>
/// <remarks>Use this context with JsonSerializerOptions.TypeInfoResolver for optimal performance 
/// when working with primitive types in AOT-compiled applications.</remarks>
[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
    PropertyNameCaseInsensitive = true,
    UseStringEnumConverter = true,
    AllowTrailingCommas = true)]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(long))]
[JsonSerializable(typeof(float))]
[JsonSerializable(typeof(double))]
[JsonSerializable(typeof(decimal))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(DateTime))]
[JsonSerializable(typeof(DateTimeOffset))]
[JsonSerializable(typeof(Guid))]
[JsonSerializable(typeof(TimeSpan))]
public partial class PrimitiveJsonContext : JsonSerializerContext { }
