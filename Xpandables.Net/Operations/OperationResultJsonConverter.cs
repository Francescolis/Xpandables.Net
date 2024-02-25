
/************************************************************************************************************
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
************************************************************************************************************/
using System.Text.Json;
using System.Text.Json.Serialization;

using Xpandables.Net.Primitives.Collections;

namespace Xpandables.Net.Operations;

/// <summary>
/// Converts an <see cref="IOperationResult"/> to JSON.
/// </summary>
/// <typeparam name="TOperationResult">Type of the operation result.</typeparam>
public sealed class OperationResultJsonConverter<TOperationResult> : JsonConverter<TOperationResult>
    where TOperationResult : class, IOperationResult
{
    /// <summary>
    /// Reads and converts the JSON to type <see cref="IOperationResult.Result"/>.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="typeToConvert">The type to convert.</param>
    /// <param name="options"> An object that specifies serialization options to use.</param>
    /// <returns>The converted value.</returns>
    public override TOperationResult Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        object? result = JsonSerializer.Deserialize(ref reader, typeof(OperationResult), options);
        return result is TOperationResult { } operation
            ? operation
            : throw new InvalidCastException($"Invalid Json reader to be deserialized to {typeof(TOperationResult).Name}.");
    }
    /// <summary>
    /// Writes a <see cref="IOperationResult"/> value as JSON.
    /// </summary>
    /// <param name="writer">The writer to write to.</param>
    /// <param name="value">The value to convert to JSON.</param>
    /// <param name="options">An object that specifies serialization options to use.</param>
    public override void Write(Utf8JsonWriter writer, TOperationResult value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(value);

        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}

/// <summary>
/// Converts an <see cref="IOperationResult{TValue}"/> to JSON using only 
/// the <see cref="IOperationResult{TValue}.Result"/>.
/// </summary>
/// <typeparam name="TOperationResult">The type of operation result.</typeparam>
/// <typeparam name="TValue">The type of the value.</typeparam>
public sealed class OperationResultJsonConverter<TOperationResult, TValue> : JsonConverter<TOperationResult>
    where TOperationResult : class, IOperationResult<TValue>
{
    /// <summary>
    /// determines whether to handle null value.
    /// </summary>
    public override bool HandleNull => false;

    /// <summary>
    /// Reads and converts the JSON to type <see cref="IOperationResult{TResult}"/>.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="typeToConvert">The type to convert.</param>
    /// <param name="options"> An object that specifies serialization options to use.</param>
    /// <returns>The converted value.</returns>
    public override TOperationResult Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        object? result = JsonSerializer.Deserialize(ref reader, typeof(OperationResult<TValue>), options);
        return result is TOperationResult { } operation
            ? operation
            : throw new InvalidCastException($"Invalid Json reader to be deserialized to {typeof(TOperationResult).Name}."); ;
    }

    /// <summary>
    /// Writes a <see cref="IOperationResult{TValue}"/> instance as JSON.
    /// </summary>
    /// <param name="writer">The writer to write to.</param>
    /// <param name="value">The value to convert to JSON.</param>
    /// <param name="options">An object that specifies serialization options to use.</param>
    public override void Write(Utf8JsonWriter writer, TOperationResult value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(value);

        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}

/// <summary>
/// Supports converting <see cref="IOperationResult"/> or <see cref="IOperationResult{TResult}"/> using the appropriate converter.
/// </summary>
public sealed class OperationResultJsonConverterFactory : JsonConverterFactory
{
    /// <summary>
    /// Determines whether the converter instance can convert the specified object type.
    /// </summary>
    /// <param name="typeToConvert">The type of the object to check whether it can be converted by this converter instance.</param>
    /// <returns>true if the instance can convert the specified object type; otherwise, false.</returns>
    public override bool CanConvert(Type typeToConvert) => typeof(IOperationResult).IsAssignableFrom(typeToConvert);

    /// <summary>
    /// Creates a converter for a specified type.
    /// </summary>
    /// <param name="typeToConvert">The type handled by the converter.</param>
    /// <param name="options">The serialization options to use.</param>
    /// <returns> A converter for which <see cref="IOperationResult"/> or <see cref="IOperationResult{TValue}"/> 
    /// is compatible with typeToConvert.</returns>
    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(typeToConvert);

        if (typeToConvert.IsGenericType
            && typeToConvert.GetInterfaces()
                .Exists(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IOperationResult<>)))
        {
            Type elementType = typeToConvert.GetGenericArguments()[0];
            return Activator
                .CreateInstance(typeof(OperationResultJsonConverter<,>)
                    .MakeGenericType([typeToConvert, elementType])) as JsonConverter;
        }
        else if (!typeToConvert.IsGenericType
            && typeToConvert.GetInterfaces()
                .Exists(i => !i.IsGenericType && i == typeof(IOperationResult)))
        {
            return Activator
                .CreateInstance(typeof(OperationResultJsonConverter<>)
                    .MakeGenericType(typeToConvert)) as JsonConverter;
        }
        else
        {
            throw new NotSupportedException($"The expected type must implement : {nameof(IOperationResult)}.");
        }
    }
}