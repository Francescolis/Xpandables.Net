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
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace System.Net.Optionals;

/// <summary>
/// JSON converter factory for Optional&lt;T&gt; types, providing AOT-compatible serialization for .NET 10.
/// </summary>
public sealed class OptionalJsonConverterFactory : JsonConverterFactory
{
    /// <inheritdoc/>
    public override bool CanConvert(Type typeToConvert)
    {
        ArgumentNullException.ThrowIfNull(typeToConvert);

        if (!typeToConvert.IsGenericType)
            return false;

        var genericTypeDef = typeToConvert.GetGenericTypeDefinition();
        return genericTypeDef == typeof(Optional<>);
    }

    /// <inheritdoc/>
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL3050", Justification = "The converter factory is designed for known Optional<T> types only")]
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(typeToConvert);
        ArgumentNullException.ThrowIfNull(options);

        options.TypeInfoResolver ??= OptionalJsonContext.Default;

        var valueType = typeToConvert.GetGenericArguments()[0];
        var converterType = typeof(OptionalJsonConverter<>).MakeGenericType(valueType);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}

/// <summary>
/// JSON converter for Optional&lt;T&gt; that provides clean serialization - empty optionals become null, 
/// non-empty optionals serialize as their value directly.
/// </summary>
/// <typeparam name="T">The type of the optional value.</typeparam>
public sealed class OptionalJsonConverter<T> : JsonConverter<Optional<T>>
{
    /// <inheritdoc/>
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026", Justification = "Optional JSON converter is used with source-generated contexts")]
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL3050", Justification = "Optional JSON converter is used with source-generated contexts")]
    public override Optional<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(typeToConvert);
        ArgumentNullException.ThrowIfNull(options);

        if (reader.TokenType == JsonTokenType.Null)
        {
            return Optional.Empty<T>();
        }

        // Deserialize the value directly - suppress warnings as this is used with source generation
        var deserializedValue = JsonSerializer.Deserialize<T>(ref reader, options);
        return deserializedValue is not null ? Optional.Some(deserializedValue) : Optional.Empty<T>();
    }

    /// <inheritdoc/>
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026", Justification = "Optional JSON converter is used with source-generated contexts")]
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL3050", Justification = "Optional JSON converter is used with source-generated contexts")]
    public override void Write(Utf8JsonWriter writer, Optional<T> value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(options);

        if (value.IsEmpty)
        {
            writer.WriteNullValue();
            return;
        }

        // Serialize the value directly for clean JSON - suppress warnings as this is used with source generation
        JsonSerializer.Serialize(writer, value.Value, options);
    }
}

/// <summary>
/// Source generation context for Optional types, optimized for .NET 10 AOT scenarios.
/// Use this with JsonSerializerOptions.TypeInfoResolver for optimal performance.
/// </summary>
[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    PropertyNameCaseInsensitive = true,
    UseStringEnumConverter = true)]
[JsonSerializable(typeof(Optional<string>))]
[JsonSerializable(typeof(Optional<int>))]
[JsonSerializable(typeof(Optional<long>))]
[JsonSerializable(typeof(Optional<double>))]
[JsonSerializable(typeof(Optional<decimal>))]
[JsonSerializable(typeof(Optional<bool>))]
[JsonSerializable(typeof(Optional<DateTime>))]
[JsonSerializable(typeof(Optional<DateTimeOffset>))]
[JsonSerializable(typeof(Optional<Guid>))]
[JsonSerializable(typeof(Optional<object>))]
[JsonSerializable(typeof(Optional<string>[]))]
[JsonSerializable(typeof(List<Optional<string>>))]
[JsonSerializable(typeof(Dictionary<string, Optional<string>>))]
[JsonSerializable(typeof(Dictionary<string, Optional<int>>))]
public partial class OptionalJsonContext : JsonSerializerContext { }