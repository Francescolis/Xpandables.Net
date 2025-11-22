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
/// Represents the result of an execution operation that produces a strongly typed value.
/// </summary>
/// <remarks>Use this type to capture the outcome of an operation that returns a value, along with status
/// information, errors, and additional metadata. The generic parameter allows callers to access the result value in a
/// type-safe manner. Inherits all metadata and status properties from the base ExecutionResult type.</remarks>
/// <typeparam name="TResult">The type of the value returned by the execution operation.</typeparam>
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public readonly record struct ExecutionResult<TResult>
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
    public readonly TResult Value { get; init; }

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
    /// Indicates whether the execution result is generic. The value is always true for this generic type.
    /// </summary>
    public readonly bool IsGeneric => true;

    /// <summary>
    /// Indicates whether the HTTP status code of the execution result signifies a successful outcome.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Value))]
    public readonly bool IsSuccess => StatusCode.IsSuccess;

    /// <summary>
    /// Ensures that the HTTP status code of the execution result indicates success.
    /// </summary>
    /// <exception cref="ExecutionResultException">Thrown if the execution result indicates a failure.</exception>
    public readonly void EnsureSuccess()
    {
        if (!IsSuccess)
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
    public static implicit operator HttpStatusCode(ExecutionResult<TResult> execution) =>
        execution.ToHttpStatusCode();

    /// <summary>
    /// Converts an instance of the generic <see cref="ExecutionResult{TResult}"/> to a non-generic ExecutionResult.
    /// </summary>
    /// <remarks>This implicit conversion allows seamless use of <see cref="ExecutionResult{TResult}"/> where an
    /// ExecutionResult is expected. The conversion preserves the execution outcome and any associated data, but
    /// type-specific information may be lost.</remarks>
    /// <param name="execution">The generic execution result to convert. Cannot be null.</param>
    public static implicit operator ExecutionResult(ExecutionResult<TResult> execution) =>
        execution.ToExecutionResult();

    /// <summary>
    /// Defines an implicit conversion from <see cref="ExecutionResult{TResult}"/> to
    /// <see cref="ExecutionResult"/>. This allows for seamless transformation of a generic
    /// execution result to its base form.
    /// </summary>
    /// <param name="execution">The <see cref="ExecutionResult{TResult}"/> instance to be converted to <see cref="ExecutionResult"/>.
    /// </param>
    /// <returns>A new instance of <see cref="ExecutionResult"/> populated with the data from the given <see cref="ExecutionResult{TResult}"/>.</returns>
    public static implicit operator ExecutionResult<TResult>(ExecutionResult execution)
    {
        return new()
        {
            StatusCode = execution.StatusCode,
            Title = execution.Title,
            Detail = execution.Detail,
            Location = execution.Location,
            Value = execution.Value is TResult resultValue ? resultValue : default,
            Errors = execution.Errors,
            Headers = execution.Headers,
            Extensions = execution.Extensions
        };
    }

    /// <summary>
    /// Creates a new <see cref="ExecutionResult"/> instance that represents the current state of this object.
    /// </summary>
    /// <returns>An <see cref="ExecutionResult"/> containing the status code, title, detail, location, value, errors, headers,
    /// and extensions from this object.</returns>
    public ExecutionResult ToExecutionResult() => new()
    {
        StatusCode = StatusCode,
        Title = Title,
        Detail = Detail,
        Location = Location,
        Value = Value,
        Errors = Errors,
        Headers = Headers,
        Extensions = Extensions
    };

    private string GetDebuggerDisplay()
        => $"{(int)StatusCode} {StatusCode} - {(IsSuccess ? "Success" : "Failure")}{(Title is not null ? $": {Title}" : string.Empty)}";
}