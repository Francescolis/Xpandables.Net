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
using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace System.ExecutionResults;

/// <summary>
/// Represents the result of an execution, including status information, error details, and additional metadata.
/// </summary>
/// <remarks>This record serves as a base type for execution results that encapsulate HTTP status codes,
/// error collections, and optional metadata such as titles, details, and locations.The class is designed to facilitate consistent handling of execution
/// outcomes, including error reporting and extension data.</remarks>
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public readonly partial record struct ExecutionResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExecutionResult"/> struct.
    /// </summary>
    /// <remarks>This constructor is intended for use by JSON serialization frameworks to create instances of
    /// the ExecutionResult struct during deserialization. It should not be called directly in application
    /// code.</remarks>
    [JsonConstructor]
    public ExecutionResult() { }

    /// <summary>
    /// Represents the HTTP status code associated with the execution result.
    /// </summary>
    public readonly required HttpStatusCode StatusCode { get; init; } = HttpStatusCode.OK;

    /// <summary>
    /// Represents a short, human-readable summary of the problem type.
    /// </summary>
    public readonly string? Title { get; init; }

    /// <summary>
    /// Represents a detailed, human-readable explanation specific to this occurrence of the problem.
    /// </summary>
    public readonly string? Detail { get; init; }

    /// <summary>
    /// Represents a URI reference that identifies a resource relevant to the execution result.
    /// </summary>
    public readonly Uri? Location { get; init; }

    /// <summary>
    /// Represents the value associated with the execution result, which can be of any type.
    /// </summary>
    /// <remarks>This property is designed to hold the result of the execution, which may vary in type depending on the
    /// context. After deserialization, the actual value may be of <see cref="JsonElement"/> type if the source is a complex type, 
    /// requiring further processing to convert it to the desired type.</remarks>
    [MaybeNull, AllowNull]
    public readonly object Value { get; init; }

    /// <summary>
    /// Represents a collection of errors associated with the execution result.
    /// </summary>
    public readonly ElementCollection Errors { get; init; } = [];

    /// <summary>
    /// Represents a collection of headers associated with the execution result.
    /// The headers can include additional metadata relevant to the execution context.
    /// </summary>
    public readonly ElementCollection Headers { get; init; } = [];

    /// <summary>
    /// Represents a collection of extensions associated with the execution result.
    /// </summary>
    public readonly ElementCollection Extensions { get; init; } = [];

    /// <summary>
    /// Represents an exception associated with the execution result, if any.
    /// </summary>
    public readonly Exception? Exception { get; init; }

    /// <summary>
    /// Indicates whether the execution result is generic. The value is always false for this non-generic type.
    /// </summary>
    public static bool IsGeneric => false;

    /// <summary>
    /// Indicates whether the HTTP status code of the execution result signifies a successful outcome.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Value))]
    public readonly bool IsSuccess => StatusCode.IsSuccess;

    /// <summary>
    /// Ensures that the HTTP status code of the execution result indicates success.
    /// </summary>
    /// <summary>
    /// Ensures that the HTTP status code of the execution result indicates success.
    /// </summary>
    /// <exception cref="ExecutionResultException">Thrown if the execution result indicates a failure.</exception>
    public readonly void EnsureSuccess()
    {
        if (this.IsFailure)
        {
            throw new ExecutionResultException(this);
        }
    }

    /// <summary>
    /// Returns the HTTP status code associated with the current response.
    /// </summary>
    /// <returns>The <see cref="HttpStatusCode"/> value representing the status of the response.</returns>
    public HttpStatusCode ToHttpStatusCode() => StatusCode;

    /// <summary>
    /// Converts an ExecutionResult instance to its corresponding HttpStatusCode value.
    /// </summary>
    /// <remarks>This operator enables implicit conversion from _ExecutionResult to HttpStatusCode, allowing
    /// ExecutionResult objects to be used where an HttpStatusCode is expected.</remarks>
    /// <param name="execution">The ExecutionResult instance to convert. Cannot be null.</param>
    public static implicit operator HttpStatusCode(ExecutionResult execution) =>
        execution.ToHttpStatusCode();

    /// <summary>
    /// Converts an instance of the non-generic ExecutionResult to an <see cref="ExecutionResult{TResult}"/> implicitly.
    /// </summary>
    /// <remarks>This operator enables seamless conversion between the non-generic and generic forms of
    /// ExecutionResult when working with APIs that expect <see cref="ExecutionResult{TResult}"/>.</remarks>
    /// <param name="executionResult">The ExecutionResult instance to convert. Cannot be null.</param>
    public static implicit operator ExecutionResult<object>(ExecutionResult executionResult)
    {
        return executionResult.ToExecutionResult();
    }

    /// <summary>
    /// Creates a new <see langword="ExecutionResult{object}"/> instance that represents the current operation result.
    /// </summary>
    /// <typeparam name="TResult">The type of the value contained in the execution result.</typeparam>
    /// <returns>An <see langword="ExecutionResult{object}"/> containing the detail, errors, extensions, headers, location, value,
    /// status code, and title from the current instance.</returns>
    public ExecutionResult<TResult> ToExecutionResult<TResult>() =>
        new()
        {
            Detail = Detail,
            Errors = Errors,
            Extensions = Extensions,
            Headers = Headers,
            Location = Location,
            Value = Value is TResult result ? result : default,
            StatusCode = StatusCode,
            Title = Title
        };

    /// <summary>
    /// Creates a new <see langword="ExecutionResult{object}"/> instance that represents the current operation result.
    /// </summary>
    /// <returns>An <see langword="ExecutionResult{object}"/> containing the detail, errors, extensions, headers, location, value,
    /// status code, and title from the current instance.</returns>
    public ExecutionResult<object> ToExecutionResult() =>
        new()
        {
            Detail = Detail,
            Errors = Errors,
            Extensions = Extensions,
            Headers = Headers,
            Location = Location,
            Value = Value,
            StatusCode = StatusCode,
            Title = Title
        };

    private string GetDebuggerDisplay()
        => $"{(int)StatusCode} {StatusCode} - {(IsSuccess ? "Success" : "Failure")}{(Title is not null ? $": {Title}" : string.Empty)}";
}