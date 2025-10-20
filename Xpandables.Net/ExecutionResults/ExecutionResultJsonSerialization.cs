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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

using Xpandables.Net.Collections;
using Xpandables.Net.Text;

namespace Xpandables.Net.ExecutionResults;

/// <summary>
/// A factory for creating JSON converters for <see cref="ExecutionResult{TResult}"/>.
/// </summary>
/// <remarks>
/// The <see cref="UseAspNetCoreCompatibility"/> indicates whether to use
/// ASP.NET Core compatibility. The default value is <see langword="false"/>.
/// The ASP.NET Core compatibility is used to serialize only the result of 
/// the operation.
/// </remarks>
public sealed class ExecutionResultJsonConverterFactory : JsonConverterFactory
{
    // Cache converters for better performance
    private static readonly ConcurrentDictionary<(Type Type, bool UseAspNetCore), JsonConverter> _converterCache = new();

    /// <summary>
    /// Gets or sets a value indicating whether to use ASP.NET Core compatibility.
    /// </summary>
    /// <remarks>The default value is <see langword="false"/>.
    /// The ASP.NET Core compatibility is used to serialize only the result of 
    /// the operation.</remarks>
    public bool UseAspNetCoreCompatibility { get; init; }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool CanConvert(Type typeToConvert)
    {
        ArgumentNullException.ThrowIfNull(typeToConvert);

        return typeToConvert == typeof(ExecutionResult)
        || (typeToConvert.IsGenericType
            && typeToConvert.GetGenericTypeDefinition() == typeof(ExecutionResult<>));
    }

    /// <inheritdoc/>
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2070",
        Justification = "The generic arguments are validated during CanConvert")]
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2071",
        Justification = "ExecutionResult types are constrained to known types")]
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL3050",
        Justification = "ExecutionResult JSON converter factory is designed for known result types")]
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(typeToConvert);
        ArgumentNullException.ThrowIfNull(options);

        options.TypeInfoResolver ??= ExecutionResultJsonContext.Default;

        return _converterCache.GetOrAdd((typeToConvert, UseAspNetCoreCompatibility), key =>
        {
            if (key.Type == typeof(ExecutionResult))
            {
                return new ExecutionResultJsonConverter { UseAspNetCoreCompatibility = key.UseAspNetCore };
            }

            Type resultType = key.Type.GetGenericArguments()[0];
            Type converterType = typeof(ExecutionResultJsonConverter<>).MakeGenericType(resultType);

            return (JsonConverter)Activator.CreateInstance(converterType, [key.UseAspNetCore])!;
        });
    }
}

/// <summary>
/// Converts <see cref="ExecutionResult"/> objects to and from JSON, with optional ASP.NET Core compatibility for
/// serialization.
/// </summary>
/// <remarks>Use the <see cref="UseAspNetCoreCompatibility"/> property to control whether the converter serializes
/// only the operation result for ASP.NET Core compatibility. This converter can be used with source-generated JSON
/// serialization contexts.</remarks>
public sealed class ExecutionResultJsonConverter : JsonConverter<ExecutionResult>
{
    /// <summary>
    /// Gets or sets a value indicating whether to use ASP.NET Core compatibility.
    /// </summary>
    /// <remarks>The default value is <see langword="false"/>.
    /// The ASP.NET Core compatibility is used to serialize only the result of 
    /// the operation.</remarks>
    public bool UseAspNetCoreCompatibility { get; init; }

    /// <inheritdoc/>
    public override ExecutionResult? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(typeToConvert);
        ArgumentNullException.ThrowIfNull(options);

        if (UseAspNetCoreCompatibility)
        {
            // In ASP.NET Core compatibility mode, only the "Value" is written.
            // Deserialization is ambiguous without surrounding metadata.
            throw new NotSupportedException("Deserialization is not supported in ASP.NET Core compatibility mode.");
        }

        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected StartObject token for ExecutionResult.");

        // Initialize with default values
        HttpStatusCode statusCode = HttpStatusCode.OK;
        string? title = null;
        string? detail = null;
        Uri? location = null;
        object? valueObj = null;
        ElementCollection errors = [];
        ElementCollection headers = [];
        ElementCollection extensions = [];

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;

            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException("Expected a property name while reading ExecutionResult.");

            string? prop = reader.GetString();
            if (!reader.Read())
                throw new JsonException("Unexpected end while reading ExecutionResult property value.");

            switch (prop)
            {
                case "StatusCode" or "statusCode":
                    statusCode = ReadStatusCode(ref reader);
                    break;

                case "Title" or "title":
                    title = reader.TokenType == JsonTokenType.Null ? null : reader.GetString();
                    break;

                case "Detail" or "detail":
                    detail = reader.TokenType == JsonTokenType.Null ? null : reader.GetString();
                    break;

                case "Location" or "location":
                    location = ReadLocation(ref reader);
                    break;

                case "Value" or "value":
                    valueObj = reader.TokenType == JsonTokenType.Null
                        ? null
                        : JsonSerializer.Deserialize(ref reader, ObjectContext.Default.Object);
                    break;

                case "Errors" or "errors":
                    errors = ReadElementCollection(ref reader);
                    break;

                case "Headers" or "headers":
                    headers = ReadElementCollection(ref reader);
                    break;

                case "Extensions" or "extensions":
                    extensions = ReadElementCollection(ref reader);
                    break;

                default:
                    // Skip unknown/ignored properties
                    reader.Skip();
                    break;
            }
        }

        return new ExecutionResult
        {
            StatusCode = statusCode,
            Title = title,
            Detail = detail,
            Location = location,
            Value = valueObj,
            Errors = errors,
            Headers = headers,
            Extensions = extensions
        };
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, ExecutionResult value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(options);

        if (UseAspNetCoreCompatibility)
        {
            if (value.Value is not null)
            {
                SerializationHelper.WriteValue(writer, value.Value, options);
            }
        }
        else
        {
            writer.WriteStartObject();

            writer.WriteNumber("statusCode", (int)value.StatusCode);
            if (value.Title is not null)
                writer.WriteString("title", value.Title);
            if (value.Detail is not null)
                writer.WriteString("detail", value.Detail);
            if (value.Location is not null)
                writer.WriteString("location", value.Location.ToString());

            if (value.Value is not null)
            {
                writer.WritePropertyName("value");
                SerializationHelper.WriteValue(writer, value.Value, options);
            }

            writer.WritePropertyName("errors");
            JsonSerializer.Serialize(writer, value.Errors, typeof(ElementCollection), ElementCollectionContext.Default);

            writer.WritePropertyName("headers");
            JsonSerializer.Serialize(writer, value.Headers, typeof(ElementCollection), ElementCollectionContext.Default);

            writer.WritePropertyName("extensions");
            JsonSerializer.Serialize(writer, value.Extensions, typeof(ElementCollection), ElementCollectionContext.Default);

            writer.WriteEndObject();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static HttpStatusCode ReadStatusCode(ref Utf8JsonReader reader)
    {
        return reader.TokenType switch
        {
            JsonTokenType.String => ParseStatusCodeFromString(reader.GetString()),
            JsonTokenType.Number => ParseStatusCodeFromNumber(ref reader),
            _ => HttpStatusCode.OK
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static HttpStatusCode ParseStatusCodeFromString(string? s)
    {
        if (string.IsNullOrWhiteSpace(s))
            return HttpStatusCode.OK;

        if (Enum.TryParse<HttpStatusCode>(s, true, out var sc))
            return sc;

        if (int.TryParse(s, out int codeInt) && Enum.IsDefined(typeof(HttpStatusCode), codeInt))
            return (HttpStatusCode)codeInt;

        return HttpStatusCode.OK;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static HttpStatusCode ParseStatusCodeFromNumber(ref Utf8JsonReader reader)
    {
        if (reader.TryGetInt32(out int codeInt) && Enum.IsDefined(typeof(HttpStatusCode), codeInt))
            return (HttpStatusCode)codeInt;

        return HttpStatusCode.OK;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Uri? ReadLocation(ref Utf8JsonReader reader)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        string? s = reader.GetString();
        return !string.IsNullOrEmpty(s) && Uri.TryCreate(s, UriKind.RelativeOrAbsolute, out var uri)
            ? uri
            : null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ElementCollection ReadElementCollection(ref Utf8JsonReader reader)
    {
        return reader.TokenType == JsonTokenType.Null
            ? default
            : (ElementCollection)JsonSerializer.Deserialize(ref reader, typeof(ElementCollection), ElementCollectionContext.Default)!;
    }
}

/// <summary>
/// Provides a custom JSON converter for serializing and deserializing instances of <see cref="ExecutionResult{TResult}"/>, with
/// optional ASP.NET Core compatibility for result formatting.
/// </summary>
/// <remarks>When ASP.NET Core compatibility is enabled, the converter serializes only the result value, omitting
/// additional metadata from ExecutionResult. This is useful for scenarios where minimal output is required, such as
/// certain web API responses. The converter is intended for use with source-generated JSON serialization
/// contexts.</remarks>
/// <typeparam name="TResult">The type of the result value contained within the ExecutionResult to be serialized or deserialized.</typeparam>
/// <param name="useAspNetCoreCompatibility">A value indicating whether ASP.NET Core compatibility should be used when serializing the result. If set to true,
/// only the result value is serialized; otherwise, the full ExecutionResult object is serialized.</param>
public sealed class ExecutionResultJsonConverter<TResult>(bool useAspNetCoreCompatibility)
    : JsonConverter<ExecutionResult<TResult>>
{
    /// <summary>
    /// Gets or sets a value indicating whether to use ASP.NET Core compatibility.
    /// </summary>
    /// <remarks>The default value is <see langword="false"/>.
    /// The ASP.NET Core compatibility is used to serialize only the result of 
    /// the operation.</remarks>
    // ReSharper disable once MemberCanBePrivate.Global
    public bool UseAspNetCoreCompatibility { get; } = useAspNetCoreCompatibility;

    /// <inheritdoc/>
    public override ExecutionResult<TResult>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(typeToConvert);
        ArgumentNullException.ThrowIfNull(options);

        if (UseAspNetCoreCompatibility)
        {
            // In ASP.NET Core compatibility mode, only the "Value" is written.
            // Deserialization is ambiguous without surrounding metadata.
            throw new NotSupportedException("Deserialization is not supported in ASP.NET Core compatibility mode.");
        }

        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected StartObject token for ExecutionResult.");

        // Initialize with default values
        HttpStatusCode statusCode = HttpStatusCode.OK;
        string? title = null;
        string? detail = null;
        Uri? location = null;
        TResult? valueT = default;
        ElementCollection errors = [];
        ElementCollection headers = [];
        ElementCollection extensions = [];

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;

            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException("Expected a property name while reading ExecutionResult.");

            string? prop = reader.GetString();
            if (!reader.Read())
                throw new JsonException("Unexpected end while reading ExecutionResult property value.");

            switch (prop)
            {
                case "StatusCode" or "statusCode":
                    statusCode = ExecutionResultJsonConverter.ReadStatusCode(ref reader);
                    break;

                case "Title" or "title":
                    title = reader.TokenType == JsonTokenType.Null ? null : reader.GetString();
                    break;

                case "Detail" or "detail":
                    detail = reader.TokenType == JsonTokenType.Null ? null : reader.GetString();
                    break;

                case "Location" or "location":
                    location = ExecutionResultJsonConverter.ReadLocation(ref reader);
                    break;

                case "Value" or "value":
                    valueT = ReadTypedValue(ref reader, options);
                    break;

                case "Errors" or "errors":
                    errors = ExecutionResultJsonConverter.ReadElementCollection(ref reader);
                    break;

                case "Headers" or "headers":
                    headers = ExecutionResultJsonConverter.ReadElementCollection(ref reader);
                    break;

                case "Extensions" or "extensions":
                    extensions = ExecutionResultJsonConverter.ReadElementCollection(ref reader);
                    break;

                default:
                    // Skip unknown/ignored properties
                    reader.Skip();
                    break;
            }
        }

        return new ExecutionResult<TResult>
        {
            StatusCode = statusCode,
            Title = title,
            Detail = detail,
            Location = location,
            Value = valueT,
            Errors = errors,
            Headers = headers,
            Extensions = extensions
        };
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, ExecutionResult<TResult> value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(options);

        if (UseAspNetCoreCompatibility)
        {
            if (value.Value is not null)
            {
                SerializationHelper.WriteTypedValue(writer, value.Value, options);
            }
        }
        else
        {
            writer.WriteStartObject();

            writer.WriteNumber("statusCode", (int)value.StatusCode);
            if (value.Title is not null)
                writer.WriteString("title", value.Title);
            if (value.Detail is not null)
                writer.WriteString("detail", value.Detail);
            if (value.Location is not null)
                writer.WriteString("location", value.Location.ToString());

            if (value.Value is not null)
            {
                writer.WritePropertyName("value");
                SerializationHelper.WriteTypedValue(writer, value.Value, options);
            }

            writer.WritePropertyName("errors");
            JsonSerializer.Serialize(writer, value.Errors, typeof(ElementCollection), ElementCollectionContext.Default);

            writer.WritePropertyName("headers");
            JsonSerializer.Serialize(writer, value.Headers, typeof(ElementCollection), ElementCollectionContext.Default);

            writer.WritePropertyName("extensions");
            JsonSerializer.Serialize(writer, value.Extensions, typeof(ElementCollection), ElementCollectionContext.Default);

            writer.WriteEndObject();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026",
        Justification = "Uses TypeInfoResolver when available, falls back to dynamic deserialization for non-AOT scenarios")]
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL3050",
        Justification = "Dynamic code generation is only used as fallback for non-AOT scenarios")]
    private static TResult? ReadTypedValue(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return default;

        if (typeof(TResult).IsPrimitive || typeof(TResult) == typeof(string))
        {
            string? json = reader.GetString();
            if (!string.IsNullOrWhiteSpace(json))
            {
                return (TResult?)Convert.ChangeType(json, typeof(TResult), CultureInfo.InvariantCulture);
            }
            return default;
        }

        JsonTypeInfo? jsonTypeInfo = options.GetTypeInfo(typeof(TResult));
        return jsonTypeInfo switch
        {
            not null => (TResult?)JsonSerializer.Deserialize(ref reader, jsonTypeInfo),
            _ => JsonSerializer.Deserialize<TResult?>(ref reader, options)
        };
    }
}

/// <summary>
/// Internal helper class for common serialization operations.
/// </summary>
internal static class SerializationHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026",
        Justification = "Uses TypeInfoResolver when available, falls back to dynamic serialization for non-AOT scenarios")]
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL3050",
        Justification = "Dynamic code generation is only used as fallback for non-AOT scenarios")]
    internal static void WriteValue(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        Type resultType = value.GetType();

        if (TryWritePrimitiveValue(writer, value, resultType))
            return;

        // For complex types, use TypeInfoResolver
        JsonTypeInfo? jsonTypeInfo = options.GetTypeInfo(resultType);
        if (jsonTypeInfo is null)
            JsonSerializer.Serialize(writer, value, resultType, options);
        else
            JsonSerializer.Serialize(writer, value, jsonTypeInfo);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026",
        Justification = "Uses TypeInfoResolver when available, falls back to dynamic serialization for non-AOT scenarios")]
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL3050",
        Justification = "Dynamic code generation is only used as fallback for non-AOT scenarios")]
    internal static void WriteTypedValue<TResult>(Utf8JsonWriter writer, TResult value, JsonSerializerOptions options)
    {
        if (TryWritePrimitiveValue(writer, value!, typeof(TResult)))
            return;

        // For complex types, use TypeInfoResolver
        JsonTypeInfo? jsonTypeInfo = options.GetTypeInfo(typeof(TResult));
        if (jsonTypeInfo is null)
            JsonSerializer.Serialize(writer, value, options);
        else
            JsonSerializer.Serialize(writer, value, jsonTypeInfo);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryWritePrimitiveValue(Utf8JsonWriter writer, object value, Type type)
    {
        if (!type.IsPrimitive && type != typeof(string) && type != typeof(DateTime) &&
            type != typeof(DateTimeOffset) && type != typeof(Guid))
            return false;

        switch (value)
        {
            case string strValue:
                writer.WriteStringValue(strValue);
                return true;
            case bool boolValue:
                writer.WriteBooleanValue(boolValue);
                return true;
            case int intValue:
                writer.WriteNumberValue(intValue);
                return true;
            case long longValue:
                writer.WriteNumberValue(longValue);
                return true;
            case double doubleValue:
                writer.WriteNumberValue(doubleValue);
                return true;
            case decimal decimalValue:
                writer.WriteNumberValue(decimalValue);
                return true;
            case float floatValue:
                writer.WriteNumberValue(floatValue);
                return true;
            case DateTime dateTimeValue:
                writer.WriteStringValue(dateTimeValue);
                return true;
            case DateTimeOffset dateTimeOffsetValue:
                writer.WriteStringValue(dateTimeOffsetValue);
                return true;
            case Guid guidValue:
                writer.WriteStringValue(guidValue);
                return true;
            default:
                return false;
        }
    }
}

/// <summary>
/// Provides source generation context for serializing and deserializing execution result objects using System.Text.Json
/// with predefined options.
/// </summary>
/// <remarks>This context configures JSON serialization to use camel case property names, indented formatting,
/// case-insensitive property matching, and string representation for enums. Properties with null values are ignored
/// during serialization. Use this context when working with execution result types to benefit from optimized
/// serialization performance and consistent JSON output.</remarks>
[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    PropertyNameCaseInsensitive = true,
    UseStringEnumConverter = true)]
[JsonSerializable(typeof(ExecutionResult))]
[JsonSerializable(typeof(ExecutionResult<string>))]
[JsonSerializable(typeof(ExecutionResult<int>))]
[JsonSerializable(typeof(ExecutionResult<long>))]
[JsonSerializable(typeof(ExecutionResult<double>))]
[JsonSerializable(typeof(ExecutionResult<decimal>))]
[JsonSerializable(typeof(ExecutionResult<bool>))]
[JsonSerializable(typeof(ExecutionResult<DateTime>))]
[JsonSerializable(typeof(ExecutionResult<DateTimeOffset>))]
[JsonSerializable(typeof(ExecutionResult<Guid>))]
[JsonSerializable(typeof(ExecutionResult<object>))]
[JsonSerializable(typeof(ExecutionResult<List<string>>))]
[JsonSerializable(typeof(ExecutionResult<List<int>>))]
[JsonSerializable(typeof(ExecutionResult<List<long>>))]
[JsonSerializable(typeof(ExecutionResult<List<double>>))]
[JsonSerializable(typeof(ExecutionResult<List<decimal>>))]
[JsonSerializable(typeof(ExecutionResult<List<bool>>))]
[JsonSerializable(typeof(ExecutionResult<List<DateTime>>))]
[JsonSerializable(typeof(ExecutionResult<List<DateTimeOffset>>))]
[JsonSerializable(typeof(ExecutionResult<List<Guid>>))]
[JsonSerializable(typeof(ExecutionResult<object[]>))]
[JsonSerializable(typeof(ExecutionResult<string[]>))]
[JsonSerializable(typeof(ExecutionResult<int[]>))]
[JsonSerializable(typeof(ExecutionResult<long[]>))]
[JsonSerializable(typeof(ExecutionResult<double[]>))]
[JsonSerializable(typeof(ExecutionResult<decimal[]>))]
[JsonSerializable(typeof(ExecutionResult<bool[]>))]
[JsonSerializable(typeof(ExecutionResult<DateTime[]>))]
[JsonSerializable(typeof(ExecutionResult<DateTimeOffset[]>))]
[JsonSerializable(typeof(ExecutionResult<Guid[]>))]
[JsonSerializable(typeof(ExecutionResult<object[]>))]
public partial class ExecutionResultJsonContext : JsonSerializerContext { }