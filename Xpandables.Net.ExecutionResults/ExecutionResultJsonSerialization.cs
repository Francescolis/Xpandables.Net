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
using System.Globalization;
using System.Net;
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
    /// <summary>
    /// Gets or sets a value indicating whether to use ASP.NET Core compatibility.
    /// </summary>
    /// <remarks>The default value is <see langword="false"/>.
    /// The ASP.NET Core compatibility is used to serialize only the result of 
    /// the operation.</remarks>
    public bool UseAspNetCoreCompatibility { get; init; }

    /// <inheritdoc/>
    public override bool CanConvert(Type typeToConvert)
    {
        ArgumentNullException.ThrowIfNull(typeToConvert);

        return typeToConvert == typeof(ExecutionResult)
        || (typeToConvert.IsGenericType
            && typeToConvert.GetGenericTypeDefinition() == typeof(ExecutionResult<>));
    }

    /// <inheritdoc/>
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL3050", Justification = "The converter factory is designed for known Optional<T> types only")]
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(typeToConvert);
        ArgumentNullException.ThrowIfNull(options);

        options.TypeInfoResolver ??= ExecutionResultJsonContext.Default;

        if (typeToConvert == typeof(ExecutionResult))
        {
            return new ExecutionResultJsonConverter() { UseAspNetCoreCompatibility = UseAspNetCoreCompatibility };
        }

        Type resultType = typeToConvert.GetGenericArguments()[0];
        Type converterType = typeof(ExecutionResultJsonConverter<>).MakeGenericType(resultType);

        JsonConverter converter = (JsonConverter)Activator.CreateInstance(
            converterType,
            [UseAspNetCoreCompatibility])!;

        return converter;
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
            throw new NotSupportedException();
        }

        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected StartObject token for ExecutionResult.");

        // Known properties
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
                    switch (reader.TokenType)
                    {
                        case JsonTokenType.String:
                            {
                                string? s = reader.GetString();
                                if (!string.IsNullOrWhiteSpace(s))
                                {
                                    if (Enum.TryParse<HttpStatusCode>(s, true, out var sc))
                                    {
                                        statusCode = sc;
                                    }
                                    else if (int.TryParse(s, out int codeInt) && Enum.IsDefined(typeof(HttpStatusCode), codeInt))
                                    {
                                        statusCode = (HttpStatusCode)codeInt;
                                    }
                                }
                                break;
                            }
                        case JsonTokenType.Number:
                            {
                                if (reader.TryGetInt32(out int codeInt) && Enum.IsDefined(typeof(HttpStatusCode), codeInt))
                                    statusCode = (HttpStatusCode)codeInt;
                                break;
                            }
                        default:
                            reader.Skip();
                            break;
                    }
                    break;

                case "Title" or "title":
                    title = reader.TokenType == JsonTokenType.Null ? null : reader.GetString();
                    break;

                case "Detail" or "detail":
                    detail = reader.TokenType == JsonTokenType.Null ? null : reader.GetString();
                    break;

                case "Location" or "location":
                    if (reader.TokenType == JsonTokenType.Null)
                    {
                        location = null;
                    }
                    else
                    {
                        string? s = reader.GetString();
                        location = !string.IsNullOrEmpty(s) && Uri.TryCreate(s, UriKind.RelativeOrAbsolute, out var uri)
                            ? uri
                            : null;
                    }
                    break;

                case "Value" or "value":
                    valueObj = reader.TokenType == JsonTokenType.Null
                        ? null
                        : JsonSerializer.Deserialize(ref reader, ObjectContext.Default.Object);
                    break;

                case "Errors" or "errors":
                    errors = reader.TokenType == JsonTokenType.Null
                        ? default
                        : (ElementCollection)JsonSerializer.Deserialize(ref reader, typeof(ElementCollection), ElementCollectionContext.Default)!;
                    break;

                case "Headers" or "headers":
                    headers = reader.TokenType == JsonTokenType.Null
                        ? default
                        : (ElementCollection)JsonSerializer.Deserialize(ref reader, typeof(ElementCollection), ElementCollectionContext.Default)!;
                    break;

                case "Extensions" or "extensions":
                    extensions = reader.TokenType == JsonTokenType.Null
                        ? default
                        : (ElementCollection)JsonSerializer.Deserialize(ref reader, typeof(ElementCollection), ElementCollectionContext.Default)!;
                    break;

                default:
                    // Skip unknown/ignored properties (IsSuccess, IsGeneric, Exception, etc.)
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
                Type resultType = value.Value.GetType();
                if (resultType is { IsPrimitive: true } || resultType == typeof(string))
                {
                    switch (value.Value)
                    {
                        case string strValue:
                            writer.WriteStringValue(strValue);
                            break;
                        case bool boolValue:
                            writer.WriteBooleanValue(boolValue);
                            break;
                        case int intValue:
                            writer.WriteNumberValue(intValue);
                            break;
                        case long longValue:
                            writer.WriteNumberValue(longValue);
                            break;
                        case double doubleValue:
                            writer.WriteNumberValue(doubleValue);
                            break;
                        case decimal decimalValue:
                            writer.WriteNumberValue(decimalValue);
                            break;
                        case float floatValue:
                            writer.WriteNumberValue(floatValue);
                            break;
                        case DateTime dateTimeValue:
                            writer.WriteStringValue(dateTimeValue);
                            break;
                        case DateTimeOffset dateTimeOffsetValue:
                            writer.WriteStringValue(dateTimeOffsetValue);
                            break;
                        case Guid guidValue:
                            writer.WriteStringValue(guidValue);
                            break;
                        default:
                            throw new NotSupportedException($"Type {resultType.Name} is not supported for direct serialization.");
                    }
                }
                else
                {
                    JsonTypeInfo jsonTypeInfo = options.TypeInfoResolver?.GetTypeInfo(value.GetType(), options)
                        ?? throw new JsonException($"No JSON type info found for type {value.GetType().Name}.");

                    JsonSerializer.Serialize(writer, value.Value, jsonTypeInfo);
                }
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
                Type resultType = value.Value.GetType();
                if (resultType is { IsPrimitive: true } || resultType == typeof(string))
                {
                    switch (value.Value)
                    {
                        case string strValue:
                            writer.WriteStringValue(strValue);
                            break;
                        case bool boolValue:
                            writer.WriteBooleanValue(boolValue);
                            break;
                        case int intValue:
                            writer.WriteNumberValue(intValue);
                            break;
                        case long longValue:
                            writer.WriteNumberValue(longValue);
                            break;
                        case double doubleValue:
                            writer.WriteNumberValue(doubleValue);
                            break;
                        case decimal decimalValue:
                            writer.WriteNumberValue(decimalValue);
                            break;
                        case float floatValue:
                            writer.WriteNumberValue(floatValue);
                            break;
                        case DateTime dateTimeValue:
                            writer.WriteStringValue(dateTimeValue);
                            break;
                        case DateTimeOffset dateTimeOffsetValue:
                            writer.WriteStringValue(dateTimeOffsetValue);
                            break;
                        case Guid guidValue:
                            writer.WriteStringValue(guidValue);
                            break;
                        default:
                            throw new NotSupportedException($"Type {resultType.Name} is not supported for direct serialization.");
                    }
                }
                else
                {
                    JsonTypeInfo jsonTypeInfo = options.TypeInfoResolver?.GetTypeInfo(value.GetType(), options)
                        ?? throw new JsonException($"No JSON type info found for type {value.GetType().Name}.");

                    JsonSerializer.Serialize(writer, value.Value, jsonTypeInfo);
                }
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
            throw new NotSupportedException();
        }

        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected StartObject token for ExecutionResult.");

        // Known properties
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
                    switch (reader.TokenType)
                    {
                        case JsonTokenType.String:
                            {
                                string? s = reader.GetString();
                                if (!string.IsNullOrWhiteSpace(s))
                                {
                                    if (Enum.TryParse<HttpStatusCode>(s, true, out var sc))
                                    {
                                        statusCode = sc;
                                    }
                                    else if (int.TryParse(s, out int codeInt) && Enum.IsDefined(typeof(HttpStatusCode), codeInt))
                                    {
                                        statusCode = (HttpStatusCode)codeInt;
                                    }
                                }
                                break;
                            }
                        case JsonTokenType.Number:
                            {
                                if (reader.TryGetInt32(out int codeInt) && Enum.IsDefined(typeof(HttpStatusCode), codeInt))
                                    statusCode = (HttpStatusCode)codeInt;
                                break;
                            }
                        default:
                            reader.Skip();
                            break;
                    }
                    break;

                case "Title" or "title":
                    title = reader.TokenType == JsonTokenType.Null ? null : reader.GetString();
                    break;

                case "Detail" or "detail":
                    detail = reader.TokenType == JsonTokenType.Null ? null : reader.GetString();
                    break;

                case "Location" or "location":
                    if (reader.TokenType == JsonTokenType.Null)
                    {
                        location = null;
                    }
                    else
                    {
                        string? s = reader.GetString();
                        location = !string.IsNullOrEmpty(s) && Uri.TryCreate(s, UriKind.RelativeOrAbsolute, out var uri)
                            ? uri
                            : null;
                    }
                    break;

                case "Value" or "value":
                    if (reader.TokenType == JsonTokenType.Null)
                    {
                        valueT = default;
                    }
                    else
                    {
                        if (typeof(TResult) is { IsPrimitive: true } || typeof(TResult) == typeof(string))
                        {
                            string? json = reader.GetString();
                            if (!string.IsNullOrWhiteSpace(json))
                            {
                                valueT = (TResult?)Convert.ChangeType(json, typeof(TResult?), CultureInfo.InvariantCulture);
                            }
                        }
                        else
                        {
                            JsonTypeInfo jsonTypeInfo = options.TypeInfoResolver?.GetTypeInfo(typeof(TResult), options)
                                ?? throw new JsonException($"No JSON type info found for type {typeof(TResult).Name}.");

                            valueT = (TResult?)JsonSerializer.Deserialize(ref reader, jsonTypeInfo);
                        }
                    }
                    break;

                case "Errors" or "errors":
                    errors = reader.TokenType == JsonTokenType.Null
                        ? default
                        : (ElementCollection)JsonSerializer.Deserialize(ref reader, typeof(ElementCollection), ElementCollectionContext.Default)!;
                    break;

                case "Headers" or "headers":
                    headers = reader.TokenType == JsonTokenType.Null
                        ? default
                        : (ElementCollection)JsonSerializer.Deserialize(ref reader, typeof(ElementCollection), ElementCollectionContext.Default)!;
                    break;

                case "Extensions" or "extensions":
                    extensions = reader.TokenType == JsonTokenType.Null
                        ? default
                        : (ElementCollection)JsonSerializer.Deserialize(ref reader, typeof(ElementCollection), ElementCollectionContext.Default)!;
                    break;

                default:
                    // Skip unknown/ignored properties (IsSuccess, IsGeneric, Exception, etc.)
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
                if (typeof(TResult) is { IsPrimitive: true } || typeof(TResult) == typeof(string))
                {
                    switch (value.Value)
                    {
                        case string strValue:
                            writer.WriteStringValue(strValue);
                            break;
                        case bool boolValue:
                            writer.WriteBooleanValue(boolValue);
                            break;
                        case int intValue:
                            writer.WriteNumberValue(intValue);
                            break;
                        case long longValue:
                            writer.WriteNumberValue(longValue);
                            break;
                        case double doubleValue:
                            writer.WriteNumberValue(doubleValue);
                            break;
                        case decimal decimalValue:
                            writer.WriteNumberValue(decimalValue);
                            break;
                        case float floatValue:
                            writer.WriteNumberValue(floatValue);
                            break;
                        case DateTime dateTimeValue:
                            writer.WriteStringValue(dateTimeValue);
                            break;
                        case DateTimeOffset dateTimeOffsetValue:
                            writer.WriteStringValue(dateTimeOffsetValue);
                            break;
                        case Guid guidValue:
                            writer.WriteStringValue(guidValue);
                            break;
                        default:
                            throw new NotSupportedException($"Type {typeof(TResult).Name} is not supported for direct serialization.");
                    }
                }
                else
                {
                    JsonTypeInfo jsonTypeInfo = options.TypeInfoResolver?.GetTypeInfo(typeof(TResult), options)
                        ?? throw new JsonException($"No JSON type info found for type {typeof(TResult).Name}.");

                    JsonSerializer.Serialize(writer, value.Value, jsonTypeInfo);
                }
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

                if (typeof(TResult) is { IsPrimitive: true } || typeof(TResult) == typeof(string))
                {
                    switch (value.Value)
                    {
                        case string strValue:
                            writer.WriteStringValue(strValue);
                            break;
                        case bool boolValue:
                            writer.WriteBooleanValue(boolValue);
                            break;
                        case int intValue:
                            writer.WriteNumberValue(intValue);
                            break;
                        case long longValue:
                            writer.WriteNumberValue(longValue);
                            break;
                        case double doubleValue:
                            writer.WriteNumberValue(doubleValue);
                            break;
                        case decimal decimalValue:
                            writer.WriteNumberValue(decimalValue);
                            break;
                        case float floatValue:
                            writer.WriteNumberValue(floatValue);
                            break;
                        case DateTime dateTimeValue:
                            writer.WriteStringValue(dateTimeValue);
                            break;
                        case DateTimeOffset dateTimeOffsetValue:
                            writer.WriteStringValue(dateTimeOffsetValue);
                            break;
                        case Guid guidValue:
                            writer.WriteStringValue(guidValue);
                            break;
                        default:
                            throw new NotSupportedException($"Type {typeof(TResult).Name} is not supported for direct serialization.");
                    }
                }
                else
                {
                    JsonTypeInfo jsonTypeInfo = options.TypeInfoResolver?.GetTypeInfo(typeof(TResult), options)
                        ?? throw new JsonException($"No JSON type info found for type {typeof(TResult).Name}.");

                    JsonSerializer.Serialize(writer, value.Value, jsonTypeInfo);
                }
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
[JsonSerializable(typeof(ExecutionResult<string>[]))]
[JsonSerializable(typeof(ExecutionResult<IAsyncEnumerable<string>>))]
[JsonSerializable(typeof(ExecutionResult<IAsyncEnumerable<int>>))]
[JsonSerializable(typeof(ExecutionResult<IAsyncEnumerable<long>>))]
[JsonSerializable(typeof(ExecutionResult<IAsyncEnumerable<double>>))]
[JsonSerializable(typeof(ExecutionResult<IAsyncEnumerable<decimal>>))]
[JsonSerializable(typeof(ExecutionResult<IAsyncEnumerable<bool>>))]
[JsonSerializable(typeof(ExecutionResult<IAsyncEnumerable<DateTime>>))]
[JsonSerializable(typeof(ExecutionResult<IAsyncEnumerable<DateTimeOffset>>))]
[JsonSerializable(typeof(ExecutionResult<IAsyncEnumerable<Guid>>))]
[JsonSerializable(typeof(ExecutionResult<IAsyncEnumerable<object>>))]
public partial class ExecutionResultJsonContext : JsonSerializerContext { }