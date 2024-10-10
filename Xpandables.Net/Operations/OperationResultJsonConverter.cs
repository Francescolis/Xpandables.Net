
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

namespace Xpandables.Net.Operations;

/// <summary>
/// A JSON converter for <see cref="IOperationResult"/>.
/// </summary>
public sealed class OperationResultJsonConverter : JsonConverter<IOperationResult>
{
    /// <summary>
    /// Gets or sets a value indicating whether to use ASP.NET Core compatibility.
    /// </summary>
    /// <remarks>The default value is <see langword="false"/>.
    /// The ASP.NET Core compatibility is used to serialize only the result of 
    /// the operation.</remarks>
    public bool UseAspNetCoreCompatibility { get; set; }

    /// <inheritdoc/>
    public override IOperationResult? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) =>
        UseAspNetCoreCompatibility
        ? throw new NotSupportedException()
        : JsonSerializer.Deserialize<OperationResult>(ref reader, options);

    /// <inheritdoc/>
    public override void Write(
        Utf8JsonWriter writer,
        IOperationResult value,
        JsonSerializerOptions options)
    {
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
            JsonSerializer.Serialize(
                writer,
                value,
                typeof(OperationResult),
                options);
        }
    }
}

/// <summary>
/// A JSON converter for <see cref="OperationResult{TResult}"/>.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
public sealed class OperationResultJsonConverter<TResult> :
    JsonConverter<IOperationResult<TResult>>
{
    /// <summary>
    /// Gets or sets a value indicating whether to use ASP.NET Core compatibility.
    /// </summary>
    /// <remarks>The default value is <see langword="false"/>.
    /// The ASP.NET Core compatibility is used to serialize only the result of 
    /// the operation.</remarks>
    public bool UseAspNetCoreCompatibility { get; set; }

    /// <inheritdoc/>
    public override IOperationResult<TResult>? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) =>
        UseAspNetCoreCompatibility
        ? throw new NotSupportedException()
        : JsonSerializer.Deserialize<OperationResult<TResult>>(
            ref reader, options);

    /// <inheritdoc/>
    public override void Write(
        Utf8JsonWriter writer,
        IOperationResult<TResult> value,
        JsonSerializerOptions options)
    {
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
            JsonSerializer.Serialize(
                writer,
                value,
                typeof(OperationResult<TResult>),
                options);
        }
    }
}
