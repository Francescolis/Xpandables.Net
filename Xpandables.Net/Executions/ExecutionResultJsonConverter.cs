
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

namespace Xpandables.Net.Executions;

/// <summary>
/// A JSON converter for <see cref="ExecutionResult"/>.
/// </summary>
public sealed class ExecutionResultJsonConverter : JsonConverter<ExecutionResult>
{
    /// <summary>
    /// Gets or sets a value indicating whether to use ASP.NET Core compatibility.
    /// </summary>
    /// <remarks>The default value is <see langword="false"/>.
    /// The ASP.NET Core compatibility is used to serialize only the result of 
    /// the operation.</remarks>
    public bool UseAspNetCoreCompatibility { get; set; }

    /// <inheritdoc/>
    public override ExecutionResult? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) =>
        UseAspNetCoreCompatibility
        ? throw new NotSupportedException()
        : JsonSerializer.Deserialize<ExecutionResult>(ref reader, options);

    /// <inheritdoc/>
    public override void Write(
        Utf8JsonWriter writer,
        ExecutionResult value,
        JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (UseAspNetCoreCompatibility)
        {
            if (value.Result is not null)
            {
                JsonSerializer.Serialize(
                    writer,
                    value.Result,
                    value.Result.GetType(),
                    options);
            }
        }
        else
        {
            JsonSerializer.Serialize(writer, value, options);
        }
    }
}

/// <summary>
/// A JSON converter for <see cref="ExecutionResult{TResult}"/>.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
public sealed class ExecutionResultJsonConverter<TResult> :
    JsonConverter<ExecutionResult<TResult>>
{
    /// <summary>
    /// Gets or sets a value indicating whether to use ASP.NET Core compatibility.
    /// </summary>
    /// <remarks>The default value is <see langword="false"/>.
    /// The ASP.NET Core compatibility is used to serialize only the result of 
    /// the operation.</remarks>
    public bool UseAspNetCoreCompatibility { get; set; }

    /// <inheritdoc/>
    public override ExecutionResult<TResult>? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) =>
        UseAspNetCoreCompatibility
        ? throw new NotSupportedException()
        : JsonSerializer.Deserialize<ExecutionResult<TResult>>(
            ref reader, options);

    /// <inheritdoc/>
    public override void Write(
        Utf8JsonWriter writer,
        ExecutionResult<TResult> value,
        JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (UseAspNetCoreCompatibility)
        {
            if (value.Result is not null)
            {
                JsonSerializer.Serialize(
                    writer,
                    value.Result,
                    value.Result.GetType(),
                    options);
            }
        }
        else
        {
            JsonSerializer.Serialize(writer, value, options);
        }
    }
}

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
    public bool UseAspNetCoreCompatibility { get; set; }

    /// <inheritdoc/>
    public override bool CanConvert(Type typeToConvert) =>
        typeToConvert == typeof(ExecutionResult)
        || (typeToConvert.IsGenericType
            && typeToConvert.GetGenericTypeDefinition() == typeof(ExecutionResult<>));

    /// <inheritdoc/>
    public override JsonConverter CreateConverter(
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (typeToConvert == typeof(ExecutionResult))
        {
            return new ExecutionResultJsonConverter()
            {
                UseAspNetCoreCompatibility = UseAspNetCoreCompatibility
            };
        }

        Type resultType = typeToConvert.GetGenericArguments()[0];
        Type converterType = typeof(ExecutionResultJsonConverter<>).MakeGenericType(resultType);

        dynamic converter = Activator.CreateInstance(converterType)!;
        converter.UseAspNetCoreCompatibility = UseAspNetCoreCompatibility;

        return converter;
    }
}