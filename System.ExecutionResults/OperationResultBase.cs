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

using static System.ExecutionResults.OperationResult;

namespace System.ExecutionResults;

/// <summary>
/// Provides a base type for representing the result of an operation, including status information, error details, and
/// associated metadata.
/// </summary>
/// <remarks>This abstract record serves as the foundation for operation result types, encapsulating common
/// properties such as HTTP status code, error collections, headers, and operation details. It is designed for
/// extensibility and serialization, allowing derived types to represent both successful and failed outcomes. Use the
/// provided properties to inspect the result, access error information, and handle operation metadata. The type
/// supports integration with HTTP-based APIs and can be used to standardize result handling across application
/// layers.</remarks>
[DebuggerDisplay($"{{{nameof(DebuggerDisplay)}(),nq}}")]
public abstract record OperationResultBase
{
    /// <summary>
    /// Initializes a new instance of the OperationResultBase class for use by derived types and JSON deserialization.
    /// </summary>
    /// <remarks>This constructor is intended for use by subclasses and JSON serialization frameworks. It
    /// should not be called directly in application code.</remarks>
    [JsonConstructor]
    protected OperationResultBase() { }

    /// <summary>
    /// Represents the HTTP status code associated with the operation result.
    /// </summary>
    public required HttpStatusCode StatusCode { get; init; } = HttpStatusCode.OK;

    /// <summary>
    /// Represents a short, human-readable summary of the problem type.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Represents a detailed, human-readable explanation specific to this occurrence of the problem.
    /// </summary>
    public string? Detail { get; init; }

    /// <summary>
    /// Represents a URI reference that identifies a resource relevant to the operation result.
    /// </summary>
    public Uri? Location { get; init; }

    /// <summary>
    /// Represents the value associated with the operation result, which can be of any type.
    /// </summary>
    /// <remarks>This property is designed to hold the result of the operation, which may vary in type depending on the
    /// context. After deserialization, the actual value may be of <see cref="JsonElement"/> type if the source is a complex type, 
    /// requiring further processing to convert it to the desired type.</remarks>
    [MaybeNull, AllowNull]
    public object Value { get; init; }

    /// <summary>
    /// Represents a collection of errors associated with the operation result.
    /// </summary>
    public ElementCollection Errors { get; init; } = [];

    /// <summary>
    /// Represents a collection of headers associated with the operation result.
    /// The headers can include additional metadata relevant to the operation context.
    /// </summary>
    public ElementCollection Headers { get; init; } = [];

    /// <summary>
    /// Represents a collection of extensions associated with the operation result.
    /// </summary>
    public ElementCollection Extensions { get; init; } = [];

    /// <summary>
    /// Represents an exception associated with the operation result, if any.
    /// </summary>
    public Exception? Exception { get; init; }

    /// <summary>
    /// Indicates whether the operation result is generic. The value is always false for this non-generic type.
    /// </summary>
    public abstract bool IsGeneric { get; }

    /// <summary>
    /// Indicates whether the HTTP status code of the operation result signifies a successful outcome.
    /// </summary>
    public virtual bool IsSuccess => StatusCode.IsSuccess;

    /// <summary>
    /// Gets a value indicating whether the result represents a failure state.
    /// </summary>
    public virtual bool IsFailure => !IsSuccess;

    /// <summary>
    /// Ensures that the HTTP status code of the operation result indicates success.
    /// </summary>
    /// <summary>
    /// Ensures that the HTTP status code of the operation result indicates success.
    /// </summary>
    /// <exception cref="OperationResultException">Thrown if the operation result indicates a failure.</exception>
    public void EnsureSuccess()
    {
        if (IsFailure)
        {
            throw new OperationResultException(this.ToOperationResult());
        }
    }

    /// <summary>
    /// Evaluates the specified operation result and returns a corresponding status value indicating success or failure.
    /// </summary>
    /// <remarks>This operator provides a convenient way to convert an ExecutionResult to a Status based on
    /// its success state.</remarks>
    /// <param name="operation">The operation result to evaluate. Determines whether the returned status represents success or failure.</param>
    /// <returns>A Status value of Success if the operation result indicates success; otherwise, Failure.</returns>
    [SuppressMessage("Usage", "CA2225:Operator overloads have named alternates", Justification = "<Pending>")]
    public static Status operator ~(OperationResultBase operation)
    {
        ArgumentNullException.ThrowIfNull(operation);
        return operation.IsSuccess ? Status.Success : Status.Failure;
    }

    /// <summary>
    /// Converts an <see cref="OperationResultBase"/> instance to its corresponding <see cref="HttpStatusCode"/> value.
    /// </summary>
    /// <remarks>This operator enables implicit conversion from <see cref="OperationResultBase"/> to <see
    /// cref="HttpStatusCode"/>, allowing operation results to be used directly in contexts where an HTTP status code is
    /// required.</remarks>
    /// <param name="operation">The operation result to convert. Cannot be <see langword="null"/>.</param>
    public static implicit operator HttpStatusCode(OperationResultBase operation)
    {
        ArgumentNullException.ThrowIfNull(operation);
        return operation.ToHttpStatusCode();
    }

    /// <summary>
    /// Returns the HTTP status code associated with the current response.
    /// </summary>
    /// <returns>The <see cref="HttpStatusCode"/> value representing the status of the response.</returns>
    public HttpStatusCode ToHttpStatusCode() => StatusCode;

    /// <summary>
    /// Returns a string that provides a concise, human-readable representation of the response for debugging purposes.
    /// </summary>
    /// <remarks>This method is intended to assist with debugging by summarizing key response details in a
    /// single string. The format includes the numeric and textual status code, a success or failure label, and the
    /// title if it is set.</remarks>
    /// <returns>A string containing the status code, status description, success or failure indication, and the title if
    /// available.</returns>
    protected string DebuggerDisplay => $"{(int)StatusCode} {StatusCode} - {(IsSuccess ? "Success" : "Failure")}{(Title is not null ? $": {Title}" : string.Empty)}";
}

/// <summary>
/// Provides extension methods for instances of <see cref="OperationResultBase"/>.
/// </summary>
public static class OperationResultBaseExtensions
{
    /// <summary>
    /// Provides extension methods for <see cref="OperationResultBase"/> instances.
    /// </summary>
    /// <param name="operation">The operation result instance to extend.</param>"
    extension(OperationResultBase operation)
    {
        /// <summary>
        /// Creates a new <see cref="OperationResult"/> instance that represents the current operation's outcome and
        /// associated metadata.
        /// </summary>
        /// <returns>An <see cref="OperationResult"/> containing the status code, title, detail, location, value, errors,
        /// headers, extensions, and exception from the current operation.</returns>
        public OperationResult ToOperationResult() =>
            new()
            {
                StatusCode = operation.StatusCode,
                Title = operation.Title,
                Detail = operation.Detail,
                Location = operation.Location,
                Value = operation.Value,
                Errors = operation.Errors,
                Headers = operation.Headers,
                Extensions = operation.Extensions,
                Exception = operation.Exception
            };

        /// <summary>
        /// Creates a new ExecutionResultException that represents the current operation result.
        /// </summary>
        /// <returns>An ExecutionResultException initialized with the current operation result.</returns>
        public OperationResultException ToExecutionResultException() => new(operation.ToOperationResult());
    }
}