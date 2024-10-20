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
using System.Net;

using Xpandables.Net.Collections;

namespace Xpandables.Net.Operations;

/// <summary>
/// Represents the result of an operation, including status code, title, detail,
/// location, result, errors, headers, extensions, and status.
/// </summary>
#pragma warning disable IDE0250 // Make struct 'readonly'
public struct OperationResult : IOperationResult
#pragma warning restore IDE0250 // Make struct 'readonly'
{
    /// <inheritdoc/>
    public required HttpStatusCode StatusCode { get; init; }

    /// <inheritdoc/>
    [MaybeNull, AllowNull]
    public string Title { get; init; }

    /// <inheritdoc/>
    [MaybeNull, AllowNull]
    public required string Detail { get; init; }

    /// <inheritdoc/>
    [MaybeNull, AllowNull]
    public required Uri Location { get; init; }

    /// <inheritdoc/>
    [MaybeNull, AllowNull]
    public required object Result { get; init; }

    /// <inheritdoc/>
    public required ElementCollection Errors { get; init; }

    /// <inheritdoc/>
    public required ElementCollection Headers { get; init; }

    /// <inheritdoc/>
    public required ElementCollection Extensions { get; init; }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public static implicit operator OperationResult<dynamic>(OperationResult operationResult) =>
        new()
        {
            StatusCode = operationResult.StatusCode,
            Title = operationResult.Title,
            Detail = operationResult.Detail,
            Location = operationResult.Location,
            Result = operationResult.Result,
            Errors = operationResult.Errors,
            Headers = operationResult.Headers,
            Extensions = operationResult.Extensions
        };
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

/// <summary>  
/// Represents the result of an operation with a specific result type, including 
/// status code, title, detail,  location, result, errors, headers, extensions, 
/// and status.  
/// </summary>  
/// <typeparam name="TResult">The type of the result object.</typeparam>  
public readonly record struct OperationResult<TResult> : IOperationResult<TResult>
{
    /// <inheritdoc/>
    public required HttpStatusCode StatusCode { get; init; }

    /// <inheritdoc/>
    [MaybeNull, AllowNull]
    public string Title { get; init; }

    /// <inheritdoc/>
    [MaybeNull, AllowNull]
    public required string Detail { get; init; }

    /// <inheritdoc/>
    [MaybeNull, AllowNull]
    public required Uri Location { get; init; }

    /// <inheritdoc/>
    [MaybeNull, AllowNull]
    public required TResult Result { get; init; }

    /// <inheritdoc/>
    public required ElementCollection Errors { get; init; }

    /// <inheritdoc/>
    public required ElementCollection Headers { get; init; }

    /// <inheritdoc/>
    public required ElementCollection Extensions { get; init; }

    /// <summary>
    /// Implicitly converts an <see cref="OperationResult{TResult}"/> to 
    /// an <see cref="OperationResult"/>.
    /// </summary>
    /// <param name="operationResult">The operation result to convert.</param>
    /// <returns>An <see cref="OperationResult"/> instance.</returns>
    public static implicit operator OperationResult(
        OperationResult<TResult> operationResult) =>
        new()
        {
            StatusCode = operationResult.StatusCode,
            Title = operationResult.Title,
            Detail = operationResult.Detail,
            Location = operationResult.Location,
            Result = operationResult.Result,
            Errors = operationResult.Errors,
            Headers = operationResult.Headers,
            Extensions = operationResult.Extensions
        };

    /// <summary>  
    /// Implicitly converts an <see cref="OperationResult"/> to  
    /// an <see cref="OperationResult{TResult}"/>.  
    /// </summary>  
    /// <param name="operationResult">The operation result to convert.</param>  
    /// <returns>An <see cref="OperationResult{TResult}"/> instance.</returns>  
    public static implicit operator OperationResult<TResult>(
        OperationResult operationResult) =>
        new()
        {
            StatusCode = operationResult.StatusCode,
            Title = operationResult.Title,
            Detail = operationResult.Detail,
            Location = operationResult.Location,
            Result = (TResult?)operationResult.Result,
            Errors = operationResult.Errors,
            Headers = operationResult.Headers,
            Extensions = operationResult.Extensions
        };
}