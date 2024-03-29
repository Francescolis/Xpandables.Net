﻿
/*******************************************************************************
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
********************************************************************************/
using System.Net;
using System.Text.Json.Serialization;

using Xpandables.Net.Optionals;
using Xpandables.Net.Primitives;

namespace Xpandables.Net.Operations;

/// <summary>
/// Represents an abstract implementation of 
/// <see cref="IOperationResult"/> that contains the status of an operation.
/// </summary>
public abstract record OperationResultBase : IOperationResult
{
    /// <summary>
    /// Constructs a new instance of 
    /// <see cref="OperationResultBase"/> with the default values.
    /// </summary>
#pragma warning disable S3442 // "abstract" classes should not have "public" constructors
    protected internal OperationResultBase() { }
#pragma warning restore S3442 // "abstract" classes should not have "public" constructors

    ///<inheritdoc/>
    [JsonInclude]
    public Optional<object> Result { get; internal init; }

    /// <inheritdoc/>
    [JsonInclude]
    public Optional<string> LocationUrl { get; internal init; }

    /// <inheritdoc/>
    [JsonInclude]
    public ElementCollection Headers { get; internal init; } = [];

    /// <inheritdoc/>
    [JsonInclude]
    public ElementCollection Errors { get; internal init; } = [];

    /// <inheritdoc/>
    [JsonInclude]
    public ElementCollection Extensions { get; internal init; } = [];

    /// <inheritdoc/>
    [JsonInclude]
    public HttpStatusCode StatusCode { get; internal init; }

    /// <inheritdoc/>
    [JsonInclude]
    public Optional<string> Title { get; internal init; }

    /// <inheritdoc/>
    [JsonInclude]
    public Optional<string> Detail { get; internal init; }

    ///<inheritdoc/>
    [JsonIgnore]
    public bool IsSuccess => StatusCode.IsSuccessStatusCode();

    ///<inheritdoc/>
    [JsonIgnore]
    public bool IsFailure => StatusCode.IsFailureStatusCode();

    /// <summary>
    /// Creates a new instance of <see cref="OperationResultBase"/> 
    /// class with the specified values.
    /// </summary>
    /// <param name="statusCode">The HTTP operation status code.</param>
    /// <param name="result">The result of the operation if available.</param>
    /// <param name="errors">The errors collection if available.</param>
    /// <param name="locationUrl">The URL location header if available</param>
    /// <param name="headers">The collection of header values.</param>
    /// <param name="extensions">The collection of extensions.</param>
    /// <param name="title">The title of the execution operation.</param>
    /// <param name="detail">The explanation of the execution 
    /// operation problem.</param>
    protected OperationResultBase(
        HttpStatusCode statusCode,
        Optional<object>? result = default,
        Optional<string>? locationUrl = default,
        ElementCollection? errors = default,
        ElementCollection? headers = default,
        ElementCollection? extensions = default,
        Optional<string>? title = default,
        Optional<string>? detail = default)
    {
        StatusCode = statusCode;
        Result = result ?? Optional.Empty<object>();
        Errors = errors ?? ([]);
        Extensions = extensions ?? ([]);
        LocationUrl = locationUrl ?? Optional.Empty<string>();
        Headers = headers ?? ([]);
        Title = title ?? Optional.Empty<string>();
        Detail = detail ?? Optional.Empty<string>();
    }
}

/// <summary>
/// Represents an abstract implementation of 
/// <see cref="IOperationResult{TResult}"/> that contains the status 
/// of an operation with generic type result.
/// </summary>
/// <typeparam name="TResult">the type of the result.</typeparam>
public abstract record OperationResultBase<TResult>
    : OperationResultBase, IOperationResult<TResult>
{
    /// <summary>
    /// Constructs a new instance of 
    /// <see cref="OperationResultBase{TValue}"/> with the default values.
    /// </summary>
#pragma warning disable S3442 // "abstract" classes should not have "public" constructors
    protected internal OperationResultBase() { }
#pragma warning restore S3442 // "abstract" classes should not have "public" constructors

    /// <inheritdoc/>
    [JsonIgnore]
    public new Optional<TResult> Result { get; internal init; }

    /// <summary>
    /// Creates a new instance of <see cref="OperationResult{TValue}"/> 
    /// with the specified values.
    /// </summary>
    /// <param name="statusCode">The HTTP operation status code.</param>
    /// <param name="result">The value of the specific type.</param>
    /// <param name="errors">The errors collection.</param>
    /// <param name="locationUrl">The URL location header</param>
    /// <param name="headers">The collection of header values.</param>
    /// <param name="extensions">The collection of extensions.</param>
    /// <param name="title">The title of the execution operation.</param>
    /// <param name="detail">The explanation of the 
    /// execution operation problem.</param>
    protected OperationResultBase(
        HttpStatusCode statusCode,
        Optional<TResult>? result = default,
        Optional<string>? locationUrl = default,
        ElementCollection? errors = default,
        ElementCollection? headers = default,
        ElementCollection? extensions = default,
        Optional<string>? title = default,
        Optional<string>? detail = default)
        : base(
            statusCode,
            result.HasValue && result.Value.IsNotEmpty
                ? Optional.Some<object>(result.Value.Value)
                : Optional.Empty<object>(),
            locationUrl,
            errors,
            headers,
            extensions,
            title,
            detail)
    => Result = result.HasValue && result.Value.IsNotEmpty
            ? Optional.Some(result.Value.Value)
            : Optional.Empty<TResult>();
}
