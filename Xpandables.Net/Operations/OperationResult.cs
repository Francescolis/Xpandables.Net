
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
using System.Net;
using System.Text.Json.Serialization;

using Xpandables.Net.Optionals;

namespace Xpandables.Net.Operations;

/// <summary>
/// Represents an implementation of 
/// <see cref="IOperationResult"/> that contains the status of an operation.
/// </summary>
public readonly record struct OperationResult : IOperationResult
{
    /// <summary>
    /// You may use the <see cref="OperationResults"/> builder instead.
    /// </summary>
    public OperationResult() { }

    ///<inheritdoc/>
    [JsonInclude]
    public readonly Optional<object> Result { get; internal init; }

    /// <inheritdoc/>
    [JsonInclude]
    public readonly Optional<string> LocationUrl { get; internal init; }

    /// <inheritdoc/>
    [JsonInclude]
    public readonly ElementCollection Headers { get; internal init; } = [];

    /// <inheritdoc/>
    [JsonInclude]
    public readonly ElementCollection Errors { get; internal init; } = [];

    /// <inheritdoc/>
    [JsonInclude]
    public readonly HttpStatusCode StatusCode { get; internal init; }

    /// <inheritdoc/>
    [JsonInclude]
    public readonly Optional<string> Title { get; internal init; }

    /// <inheritdoc/>
    [JsonInclude]
    public readonly Optional<string> Detail { get; internal init; }

    ///<inheritdoc/>
    public readonly bool IsSuccess => IOperationResult.IsSuccessStatusCode(StatusCode);

    ///<inheritdoc/>
    public bool IsFailure => IOperationResult.IsFailureStatusCode(StatusCode);

    /// <summary>
    /// Creates a new instance of <see cref="OperationResult"/> class with the specified values.
    /// </summary>
    /// <param name="statusCode">The HTTP operation status code.</param>
    /// <param name="result">The result of the operation if available.</param>
    /// <param name="errors">The errors collection if available.</param>
    /// <param name="locationUrl">The URL location header if available</param>
    /// <param name="headers">The collection of header values.</param>
    /// <param name="title">The title of the execution operation.</param>
    /// <param name="detail">The explanation of the execution operation problem.</param>
    internal OperationResult(
        HttpStatusCode statusCode,
        Optional<object>? result = default,
        Optional<string>? locationUrl = default,
        ElementCollection? errors = default,
        ElementCollection? headers = default,
        Optional<string>? title = default,
        Optional<string>? detail = default)
    {
        StatusCode = statusCode;
        Result = result ?? Optional.Empty<object>();
        Errors = errors ?? [];
        LocationUrl = locationUrl ?? Optional.Empty<string>();
        Headers = headers ?? [];
        Title = title ?? Optional.Empty<string>();
        Detail = detail ?? Optional.Empty<string>();
    }

    ///<inheritdoc/>
    public OperationResult<TResult> ToOperationResult<TResult>()
        => new(
            StatusCode,
            Result.Bind(o => ((TResult?)o).AsOptional()),
            LocationUrl,
            Errors,
            Headers,
            Title,
            Detail);
}

/// <summary>
/// Represents an implementation of 
/// <see cref="IOperationResult{TResult}"/> that contains the status of an operation with generic type result.
/// </summary>
/// <typeparam name="TResult">the type of the result.</typeparam>
public readonly record struct OperationResult<TResult> : IOperationResult<TResult>
{
    /// <inheritdoc/>
    [JsonInclude]
    public readonly Optional<TResult> Result { get; internal init; }

    /// <inheritdoc/>
    [JsonInclude]
    public readonly Optional<string> LocationUrl { get; internal init; }

    /// <inheritdoc/>
    [JsonInclude]
    public readonly ElementCollection Headers { get; internal init; } = [];

    /// <inheritdoc/>
    [JsonInclude]
    public readonly ElementCollection Errors { get; internal init; } = [];

    /// <inheritdoc/>
    [JsonInclude]
    public readonly HttpStatusCode StatusCode { get; internal init; }

    /// <inheritdoc/>
    [JsonInclude]
    public readonly Optional<string> Title { get; internal init; }

    /// <inheritdoc/>
    [JsonInclude]
    public readonly Optional<string> Detail { get; internal init; }

    ///<inheritdoc/>
    public readonly bool IsSuccess => IOperationResult.IsSuccessStatusCode(StatusCode);

    ///<inheritdoc/>
    public readonly bool IsFailure => IOperationResult.IsFailureStatusCode(StatusCode);

    /// <summary>
    /// Creates a new instance of <see cref="OperationResult{TValue}"/> with the specified values.
    /// </summary>
    /// <param name="statusCode">The HTTP operation status code.</param>
    /// <param name="result">The value of the specific type.</param>
    /// <param name="errors">The errors collection.</param>
    /// <param name="locationUrl">The URL location header</param>
    /// <param name="headers">The collection of header values.</param>
    /// <param name="title">The title of the execution operation.</param>
    /// <param name="detail">The explanation of the execution operation problem.</param>
    internal OperationResult(
        HttpStatusCode statusCode,
        Optional<TResult>? result = default,
        Optional<string>? locationUrl = default,
        ElementCollection? errors = default,
        ElementCollection? headers = default,
        Optional<string>? title = default,
        Optional<string>? detail = default)
    {
        StatusCode = statusCode;
        Result = result ?? Optional.Empty<TResult>();
        Errors = errors ?? [];
        LocationUrl = locationUrl ?? Optional.Empty<string>();
        Headers = headers ?? [];
        Title = title ?? Optional.Empty<string>();
        Detail = detail ?? Optional.Empty<string>();
    }

    ///<inheritdoc/>
    public static implicit operator OperationResult(OperationResult<TResult> operation)
        => operation.ToOperationResult();

    ///<inheritdoc/>
    public OperationResult ToOperationResult()
        => new(
            StatusCode,
            Result,
            LocationUrl,
            Errors,
            Headers,
            Title,
            Detail);
}
