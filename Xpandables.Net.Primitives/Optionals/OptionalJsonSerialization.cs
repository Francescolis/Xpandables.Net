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
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Xpandables.Net.Optionals;

/// <summary>
/// JSON converter for Optional&lt;T&gt; that provides clean serialization - empty optionals become null, 
/// non-empty optionals serialize as their value directly.
/// </summary>
/// <typeparam name="T">The type of the optional value.</typeparam>
public sealed class OptionalJsonConverter<T> : JsonConverter<Optional<T>>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override Optional<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(typeToConvert);
        ArgumentNullException.ThrowIfNull(options);

        if (reader.TokenType == JsonTokenType.Null)
        {
            return Optional.Empty<T>();
        }

        JsonTypeInfo<T>? typeInfo = (JsonTypeInfo<T>?)options.GetTypeInfo(typeof(T))
            ?? throw new JsonException($"No type info found for type {typeof(T)}.");

        var valueT = JsonSerializer.Deserialize(ref reader, typeInfo);

        return valueT is not null ? Optional.Some(valueT) : Optional.Empty<T>();
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Write(Utf8JsonWriter writer, Optional<T> value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(options);

        if (value.IsEmpty)
        {
            writer.WriteNullValue();
            return;
        }

        JsonTypeInfo<T>? typeInfo = (JsonTypeInfo<T>?)options.GetTypeInfo(typeof(T))
            ?? throw new JsonException($"No type info found for type {typeof(T)}.");

        JsonSerializer.Serialize(writer, value.Value, typeInfo);
    }
}

/// <summary>
/// Source generation context for Optional types, optimized for .NET 10 AOT scenarios.
/// Use this with JsonSerializerOptions.TypeInfoResolver for optimal performance.
/// </summary>
/// <remarks>
/// The OptionalJsonConverterFactory will automatically discover all Optional&lt;T&gt; types
/// declared here as well as those used throughout your codebase.
/// </remarks>
[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    PropertyNameCaseInsensitive = true,
    UseStringEnumConverter = true)]
// Optional types
[JsonSerializable(typeof(Optional<string>))]
[JsonSerializable(typeof(Optional<int>))]
[JsonSerializable(typeof(Optional<long>))]
[JsonSerializable(typeof(Optional<float>))]
[JsonSerializable(typeof(Optional<short>))]
[JsonSerializable(typeof(Optional<double>))]
[JsonSerializable(typeof(Optional<decimal>))]
[JsonSerializable(typeof(Optional<ushort>))]
[JsonSerializable(typeof(Optional<byte>))]
[JsonSerializable(typeof(Optional<bool>))]
[JsonSerializable(typeof(Optional<DateTime>))]
[JsonSerializable(typeof(Optional<DateTimeOffset>))]
[JsonSerializable(typeof(Optional<Guid>))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(long))]
[JsonSerializable(typeof(float))]
[JsonSerializable(typeof(short))]
[JsonSerializable(typeof(double))]
[JsonSerializable(typeof(decimal))]
[JsonSerializable(typeof(ushort))]
[JsonSerializable(typeof(byte))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(DateTime))]
[JsonSerializable(typeof(DateTimeOffset))]
[JsonSerializable(typeof(Guid))]
public partial class OptionalJsonContext : JsonSerializerContext { }