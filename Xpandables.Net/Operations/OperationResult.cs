
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

using Xpandables.Net.Primitives;

namespace Xpandables.Net.Operations;

/// <summary>
/// Represents an implementation of 
/// <see cref="IOperationResult"/> that contains the status of an operation.
/// </summary>
public sealed record OperationResult : OperationResultBase
{
    /// <summary>
    /// Used by the deserialization process.
    /// </summary>
    [JsonConstructor]
    internal OperationResult() { }

    /// <summary>
    /// Creates a new instance of <see cref="OperationResult"/> 
    /// class with the specified values.
    /// </summary>
    /// <param name="statusCode">The HTTP operation status code.</param>
    /// <param name="result">The result of the operation if available.</param>
    /// <param name="errors">The errors collection if available.</param>
    /// <param name="locationUrl">The URL location header if available</param>
    /// <param name="headers">The collection of header values.</param>
    /// <param name="extensions">The collection of extension values.</param>
    /// <param name="title">The title of the execution operation.</param>
    /// <param name="detail">The explanation of the execution 
    /// operation problem.</param>
    internal OperationResult(
        HttpStatusCode statusCode,
        object? result = null,
        Uri? locationUrl = null,
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
            detail)
    { }
}

/// <summary>
/// Represents an implementation of 
/// <see cref="IOperationResult{TResult}"/> that contains the status 
/// of an operation with generic type result.
/// </summary>
/// <typeparam name="TResult">the type of the result.</typeparam>
public sealed record OperationResult<TResult> : OperationResultBase<TResult>
{
    /// <summary>
    /// Used by the deserialization process.
    /// </summary>
    [JsonConstructor]
    internal OperationResult() { }

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
    /// <param name="detail">The explanation of the execution 
    /// operation problem.</param>
    internal OperationResult(
        HttpStatusCode statusCode,
        object? result = null,
        Uri? locationUrl = null,
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
            detail)
    { }

    ///<inheritdoc/>
    public static implicit operator OperationResult(
        OperationResult<TResult> operation)
    {
        ArgumentNullException.ThrowIfNull(operation);
        return (OperationResult)((IOperationResult<TResult>)operation)
            .ToOperationResult();
    }
}
