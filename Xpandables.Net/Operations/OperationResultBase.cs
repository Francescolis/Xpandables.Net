
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
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json.Serialization;

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
    protected internal OperationResultBase() { }

    ///<inheritdoc/>
    [JsonInclude]
    [AllowNull, MaybeNull]
    public object Result { get; internal init; }

    /// <inheritdoc/>
    [JsonInclude]
    [AllowNull, MaybeNull]
    public Uri LocationUrl { get; internal init; }

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
    [AllowNull, MaybeNull]
    public string Title { get; internal init; }

    /// <inheritdoc/>
    [JsonInclude]
    [AllowNull, MaybeNull]
    public string Detail { get; internal init; }

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
        object? result = default,
        Uri? locationUrl = default,
        ElementCollection? errors = default,
        ElementCollection? headers = default,
        ElementCollection? extensions = default,
        string? title = default,
        string? detail = default)
    {
        StatusCode = statusCode;
        Result = result;
        Errors = errors ?? [];
        Extensions = extensions ?? [];
        LocationUrl = locationUrl;
        Headers = headers ?? [];
        Title = title;
        Detail = detail;
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
    protected internal OperationResultBase() { }

    /// <inheritdoc/>
    [JsonIgnore]
    [AllowNull, MaybeNull]
    public new TResult Result { get; internal init; }

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
        TResult? result = default,
        Uri? locationUrl = default,
        ElementCollection? errors = default,
        ElementCollection? headers = default,
        ElementCollection? extensions = default,
        string? title = default,
        string? detail = default)
        : base(
            statusCode,
            result,
            locationUrl,
            errors,
            headers,
            extensions,
            title,
            detail) => Result = result;
}
