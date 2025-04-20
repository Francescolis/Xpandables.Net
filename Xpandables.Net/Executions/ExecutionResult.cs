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
using System.Text.Json.Serialization;

using Xpandables.Net.Collections;

namespace Xpandables.Net.Executions;

/// <summary>
/// Abstract record representing the result of an execution, including status code, title, detail, location, result,
/// errors, headers, and extensions. It provides methods to convert to an HTTP status code.
/// </summary>
#pragma warning disable CA1707 // Identifiers should not contain underscores
#pragma warning disable IDE1006 // Naming Styles
public abstract record _ExecutionResult
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore CA1707 // Identifiers should not contain underscores
{
    /// <summary>
    /// Contains the key for the exception in the <see cref="ElementCollection"/>.
    /// </summary>
    public const string ExceptionKey = "Exception";

    [JsonConstructor]
    internal _ExecutionResult() { }

    /// <summary>
    /// Gets the status code of the execution.
    /// </summary>
    public required HttpStatusCode StatusCode { get; init; } = HttpStatusCode.OK;

    /// <summary>
    /// Gets the title of the execution result.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Gets the detail of the execution result.
    /// </summary>
    public string? Detail { get; init; }

    /// <summary>
    /// Gets the location URI of the execution result.
    /// </summary>
    public Uri? Location { get; init; }

    /// <summary>
    /// Gets the result object of the execution.
    /// </summary>
    /// <remarks>May be null if the execution has no result.</remarks>
    public object? Result { get; init; }

    /// <summary>
    /// Gets the collection of errors associated with the execution.
    /// </summary>
    public ElementCollection Errors { get; init; } = [];

    /// <summary>
    /// Gets the collection of headers associated with the execution.
    /// </summary>
    public ElementCollection Headers { get; init; } = [];

    /// <summary>
    /// Gets the collection of extensions associated with the execution.
    /// </summary>
    public ElementCollection Extensions { get; init; } = [];

    /// <summary>
    /// Gets a value indicating whether the execution result is generic.
    /// </summary>
    public abstract bool IsGeneric { get; }

    /// <summary>
    /// Gets a value indicating whether the execution's status code 
    /// is a success status code.
    /// </summary>
    public abstract bool IsSuccessStatusCode { get; }

    /// <summary>
    /// Ensures that the execution result has a success status code.
    /// </summary>
    /// <exception cref="ExecutionResultException">Thrown if the status code 
    /// is not a success status code.</exception>
    public abstract void EnsureSuccessStatusCode();

    /// <summary>
    /// Gets the exception associated with the execution result, if any.
    /// </summary>
    /// <returns>The exception entry if available; otherwise, null.</returns>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ElementEntry? Exception => Errors[ExceptionKey];

    /// <summary>
    /// Converts an ExecutionResultAbstract instance to an HttpStatusCode.
    /// </summary>
    /// <param name="result">Represents the status code associated with the execution result.</param>
    public static implicit operator HttpStatusCode(_ExecutionResult result) => result.ToHttpStatusCode();

    /// <summary>
    /// Converts the current status code to an HTTP status code.
    /// </summary>
    /// <returns>Returns the corresponding HttpStatusCode value.</returns>
    public HttpStatusCode ToHttpStatusCode() => StatusCode;
}

/// <summary>
/// Represents the result of an execution, including status code, title, detail,
/// location, result, errors, headers, extensions, and status.
/// </summary>
[Serializable]
public sealed record ExecutionResult : _ExecutionResult
{
    [JsonConstructor]
    internal ExecutionResult() { }

    /// <inheritdoc/>
    public override bool IsGeneric => false;

    /// <inheritdoc/>
    public override bool IsSuccessStatusCode => StatusCode.IsSuccessStatusCode();

    /// <inheritdoc/>
    public override void EnsureSuccessStatusCode()
    {
        if (!IsSuccessStatusCode)
        {
            throw new ExecutionResultException(this);
        }
    }

    /// <summary>
    /// Converts an ExecutionResult to an ExecutionResult of a generic object type.
    /// </summary>
    /// <param name="result">The conversion provides a more specific type for the execution result.</param>
    public static implicit operator ExecutionResult<object>(ExecutionResult result) =>
        result.ToExecutionResult();

    /// <summary>
    /// Creates an ExecutionResult object containing various properties such as Detail, Errors, and StatusCode.
    /// </summary>
    /// <returns>Returns an ExecutionResult object encapsulating the current state and data.</returns>
    public ExecutionResult<object> ToExecutionResult() =>
        new()
        {
            Detail = Detail,
            Errors = Errors,
            Extensions = Extensions,
            Headers = Headers,
            Location = Location,
            Result = Result,
            StatusCode = StatusCode,
            Title = Title
        };
}

/// <summary>  
/// Represents the result of an execution with a specific result type, including 
/// status code, title, detail,  location, result, errors, headers, extensions, 
/// and status.  
/// </summary>  
/// <typeparam name="TResult">The type of the result object.</typeparam>  
public sealed record ExecutionResult<TResult> : _ExecutionResult
{
    [JsonConstructor]
    internal ExecutionResult() { }

    /// <summary>
    /// Gets the result object of the execution.
    /// </summary>
    /// <remarks>May be null if the execution has no result.</remarks>
    [MaybeNull, AllowNull]
    public new TResult Result
    {
        get => (TResult?)base.Result;
        init => base.Result = value;
    }

    /// <inheritdoc/>>
    [MemberNotNullWhen(true, nameof(Result))]
    public override bool IsSuccessStatusCode => StatusCode.IsSuccessStatusCode();

    /// <inheritdoc/>
    [MemberNotNull([nameof(Result)])]
    public override void EnsureSuccessStatusCode()
    {
        if (!IsSuccessStatusCode)
        {
            throw new ExecutionResultException(this);
        }
    }

    /// <inheritdoc/>
    public override bool IsGeneric => true;

    /// <summary>
    /// Converts the current instance to an <see cref="ExecutionResult"/> object.
    /// </summary>
    /// <param name="result"></param>
    public static implicit operator ExecutionResult(ExecutionResult<TResult> result) =>
        result.ToExecutionResult();

    /// <summary>
    /// Converts the current instance to an <see cref="ExecutionResult{TResult}"/> object.
    /// </summary>
    /// <param name="result"></param>
    public static implicit operator ExecutionResult<TResult>(ExecutionResult result) =>
        new()
        {
            StatusCode = result.StatusCode,
            Title = result.Title,
            Detail = result.Detail,
            Location = result.Location,
            Result = result.Result is TResult resultValue ? resultValue : default,
            Errors = result.Errors,
            Headers = result.Headers,
            Extensions = result.Extensions
        };

    /// <summary>
    /// Converts the current instance to an <see cref="ExecutionResult"/> object.
    /// </summary>
    /// <returns></returns>
    public ExecutionResult ToExecutionResult() => new()
    {
        StatusCode = StatusCode,
        Title = Title,
        Detail = Detail,
        Location = Location,
        Result = Result,
        Errors = Errors,
        Headers = Headers,
        Extensions = Extensions
    };
}