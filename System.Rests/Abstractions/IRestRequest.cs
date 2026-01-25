/*******************************************************************************
 * Copyright (C) 2025 Kamersoft
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
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace System.Rests.Abstractions;

/// <summary>
/// Defines a contract for RESTful requests. 
/// It serves as a blueprint for implementing REST request functionalities.
/// </summary>
public interface IRestRequest
{
    /// <summary>
    /// Represents the date and time when the object was created, 
    /// set to the current UTC time at initialization.
    /// </summary>
    public DateTime CreatedAt => DateTime.UtcNow;

    /// <summary>
    /// Returns the name of the type of the current instance as a string.
    /// This is typically the class name.
    /// </summary>
    public string Name => GetType().Name;

    /// <summary>
    /// Returns the default value of the ResultType, which can be null. 
    /// It indicates the type of the result.
    /// </summary>
    public Type? ResultType => default;
}

/// <summary>
/// Represents the result of a REST request operation.
/// </summary>
/// <remarks>This interface extends <see cref="IRestRequest"/>, providing additional context or result information for REST request processing.</remarks>
[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "<Pending>")]
public interface IRestRequestResult : IRestRequest
{
    /// <summary>
    /// Gets the type of the result produced by the REST request.
    /// </summary>
    new Type ResultType { get; }

    [NotNull]
    [EditorBrowsable(EditorBrowsableState.Never)]
    Type? IRestRequest.ResultType => ResultType;
}

/// <summary>
/// Defines the result of a REST request that returns a strongly typed value.
/// </summary>
/// <typeparam name="TResult">The type of the result returned by the REST request. Must be a non-nullable type.</typeparam>
public interface IRestRequestResult<TResult> : IRestRequestResult
    where TResult : notnull
{
    /// <summary>
    /// Returns the default value of the ResultType, which can be null. 
    /// It indicates the type of the result.
    /// </summary>
    public new Type ResultType => typeof(TResult);

    [EditorBrowsable(EditorBrowsableState.Never)]
    Type IRestRequestResult.ResultType => ResultType;
}

/// <summary>
/// Represents a REST request that provides access to the request body as a stream.
/// </summary>
/// <remarks>This interface is intended for advanced scenarios where direct manipulation of the request body
/// stream is required, such as uploading large files or streaming data.</remarks>
[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "<Pending>")]
public interface IRestRequestStream : IRestRequest
{
    /// <summary>
    /// Gets the type of the result produced by the REST request.
    /// </summary>
    new Type ResultType { get; }

    [NotNull]
    [EditorBrowsable(EditorBrowsableState.Never)]
    Type? IRestRequest.ResultType => ResultType;
}

/// <summary>
/// Represents a strongly typed REST request stream that produces results of a specified type.
/// </summary>
/// <typeparam name="TResult">The type of the result produced by the request stream. Must be a non-nullable type.</typeparam>
[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "<Pending>")]
public interface IRestRequestStream<TResult> : IRestRequestStream
    where TResult : notnull
{
    /// <summary>
    /// Returns the default value of the ResultType, which can be null. 
    /// It indicates the type of the result.
    /// </summary>
    public new Type ResultType => typeof(TResult);

    [EditorBrowsable(EditorBrowsableState.Never)]
    Type IRestRequestStream.ResultType => ResultType;
}


/// <summary>
/// Represents a REST request that supports retrieving paged results as a stream.
/// </summary>
/// <remarks>This interface extends <see cref="IRestRequest"/> to provide support for paged streaming scenarios, such as iterating over large result sets
/// that are returned in multiple pages from a REST API.</remarks>
[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "<Pending>")]
public interface IRestRequestStreamPaged : IRestRequest
{
    /// <summary>
    /// Gets the type of the result produced by the REST request.
    /// </summary>
    new Type ResultType { get; }

    [NotNull]
    [EditorBrowsable(EditorBrowsableState.Never)]
    Type? IRestRequest.ResultType => ResultType;
}

/// <summary>
/// Defines a strongly typed, paged streaming REST request that returns results of a specified type.
/// </summary>
/// <typeparam name="TResult">The type of the result returned by the request. Must not be null.</typeparam>
public interface IRestRequestStreamPaged<TResult> : IRestRequestStreamPaged
    where TResult : notnull
{
    /// <summary>
    /// Returns the default value of the ResultType, which can be null. 
    /// It indicates the type of the result.
    /// </summary>
    public new Type? ResultType => typeof(TResult);

    [EditorBrowsable(EditorBrowsableState.Never)]
    Type? IRestRequest.ResultType => ResultType;
}

/// <summary>
/// Provides AOT-compatible deserialization for streaming REST responses.
/// </summary>
/// <remarks>
/// Implement this interface on your request class to enable AOT-compatible streaming deserialization.
/// This avoids reflection-based generic method invocation at runtime.
/// </remarks>
public interface IRestStreamDeserializer
{
    /// <summary>
    /// Deserializes the HTTP content to an async enumerable stream.
    /// </summary>
    /// <param name="content">The HTTP content to deserialize.</param>
    /// <param name="options">The JSON serializer options to use.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>An async enumerable of the deserialized items.</returns>
    object DeserializeAsAsyncEnumerable(
        HttpContent content,
        Text.Json.JsonSerializerOptions options,
        CancellationToken cancellationToken);
}

/// <summary>
/// Provides AOT-compatible deserialization for paged streaming REST responses.
/// </summary>
/// <remarks>
/// Implement this interface on your request class to enable AOT-compatible paged streaming deserialization.
/// This avoids reflection-based generic method invocation at runtime.
/// </remarks>
public interface IRestStreamPagedDeserializer
{
    /// <summary>
    /// Deserializes the HTTP content to an async paged enumerable stream.
    /// </summary>
    /// <param name="content">The HTTP content to deserialize.</param>
    /// <param name="options">The JSON serializer options to use.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>An async paged enumerable of the deserialized items.</returns>
    object DeserializeAsAsyncPagedEnumerable(
        HttpContent content,
        Text.Json.JsonSerializerOptions options,
        CancellationToken cancellationToken);
}
