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
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net.Abstractions;
using System.Net.Abstractions.Collections;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace System.Net.ExecutionResults;

/// <summary>
/// Represents the base type for execution results, encapsulating status, value, errors, headers, and related metadata
/// for an operation outcome.
/// </summary>
/// <remarks>This abstract record provides a common structure for representing the result of an operation,
/// including HTTP status information, error details, and extensibility points. It is intended for use as a base type
/// for more specific execution result types. The type is not intended to be used directly in application code and is
/// primarily used for internal or framework-level scenarios, such as serialization and result handling.</remarks>
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
[SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "<Pending>")]
[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract record _ExecutionResult
{
    /// <summary>
    /// Contains the key for the exception in the <see cref="ElementCollection" />.
    /// </summary>
    public const string ExceptionKey = "Exception";

    /// <summary>
    /// Initializes a new instance of the ExecutionResult class for JSON deserialization.
    /// </summary>
    /// <remarks>This constructor is intended for use by JSON serialization frameworks to create instances of
    /// the ExecutionResult class during deserialization. It should not be called directly in application
    /// code.</remarks>
    [JsonConstructor]
    protected _ExecutionResult() { }

    /// <summary>
    /// Represents the HTTP status code associated with the execution result.
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
    /// Represents a URI reference that identifies a resource relevant to the execution result.
    /// </summary>
    public Uri? Location { get; init; }

    /// <summary>
    /// Represents the value associated with the execution result, which can be of any type.
    /// </summary>
    /// <remarks>This property is designed to hold the result of the execution, which may vary in type depending on the
    /// context. After deserialization, the actual value may be of <see cref="JsonElement"/> type if the source is a complex type, 
    /// requiring further processing to convert it to the desired type.</remarks>
    [MaybeNull, AllowNull]
    public object Value { get; init; }

    /// <summary>
    /// Represents a collection of errors associated with the execution result.
    /// </summary>
    public ElementCollection Errors { get; init; } = [];

    /// <summary>
    /// Represents a collection of headers associated with the execution result.
    /// The headers can include additional metadata relevant to the execution context.
    /// </summary>
    public ElementCollection Headers { get; init; } = [];

    /// <summary>
    /// Represents a collection of extensions associated with the execution result.
    /// </summary>
    public ElementCollection Extensions { get; init; } = [];

    /// <summary>
    /// Represents an exception associated with the execution result, if any.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ElementEntry? Exception => Errors[ExceptionKey];

    /// <summary>
    /// Indicates whether the execution result is generic (i.e., not tied to a specific type).
    /// </summary>
    public virtual bool IsGeneric => false;

    /// <summary>
    /// Indicates whether the HTTP status code of the execution result signifies a successful outcome.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Value))]
    public virtual bool IsSuccess => StatusCode.IsSuccess;

    /// <summary>
    /// Ensures that the HTTP status code of the execution result indicates success.
    /// </summary>
    public abstract void EnsureSuccess();

    /// <summary>
    /// Returns the HTTP status code associated with the current response.
    /// </summary>
    /// <returns>The <see cref="HttpStatusCode"/> value representing the status of the response.</returns>
    public HttpStatusCode ToHttpStatusCode() => StatusCode;

    /// <summary>
    /// Converts an ExecutionResult instance to its corresponding HttpStatusCode value.
    /// </summary>
    /// <remarks>This operator enables implicit conversion from _ExecutionResult to HttpStatusCode, allowing
    /// _ExecutionResult objects to be used where an HttpStatusCode is expected.</remarks>
    /// <param name="executionResult">The _ExecutionResult instance to convert. Cannot be null.</param>
    public static implicit operator HttpStatusCode(_ExecutionResult executionResult)
    {
        ArgumentNullException.ThrowIfNull(executionResult);
        return executionResult.ToHttpStatusCode();
    }

    private string GetDebuggerDisplay()
        => $"{(int)StatusCode} {StatusCode} - {(IsSuccess ? "Success" : "Failure")}{(Title is not null ? $": {Title}" : string.Empty)}";
}

/// <summary>
/// Represents the result of an execution, including status information, error details, and additional metadata.
/// </summary>
/// <remarks>This record serves as a base type for execution results that encapsulate HTTP status codes,
/// error collections, and optional metadata such as titles, details, and locations. Derived types can provide specific
/// behaviors for success and error handling. The class is designed to facilitate consistent handling of execution
/// outcomes, including error reporting and extension data. Thread safety and mutability depend on the usage of the
/// derived types and the contained collections.</remarks>
[JsonConverter(typeof(ExecutionResultJsonConverterFactory))]
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public sealed partial record ExecutionResult : _ExecutionResult
{
    /// <summary>
    /// Initializes a new instance of the ExecutionResult class for JSON deserialization.
    /// </summary>
    /// <remarks>This constructor is intended for use by JSON serialization frameworks to create instances of
    /// the ExecutionResult class during deserialization. It should not be called directly in application
    /// code.</remarks>
    [JsonConstructor]
    public ExecutionResult() { }

    /// <inheritdoc/>
    public sealed override bool IsGeneric => false;

    /// <summary>
    /// Ensures that the HTTP status code of the execution result indicates success.
    /// </summary>
    public sealed override void EnsureSuccess()
    {
        if (this.IsFailure)
        {
            throw new ExecutionResultException(this);
        }
    }

    /// <summary>
    /// Converts an instance of the non-generic ExecutionResult to an <see cref="ExecutionResult{TResult}"/> implicitly.
    /// </summary>
    /// <remarks>This operator enables seamless conversion between the non-generic and generic forms of
    /// ExecutionResult when working with APIs that expect <see cref="ExecutionResult{TResult}"/>.</remarks>
    /// <param name="executionResult">The ExecutionResult instance to convert. Cannot be null.</param>
    public static implicit operator ExecutionResult<object>(ExecutionResult executionResult)
    {
        ArgumentNullException.ThrowIfNull(executionResult);
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

/// <summary>
/// Represents the result of an execution operation that produces a strongly typed value.
/// </summary>
/// <remarks>Use this type to capture the outcome of an operation that returns a value, along with status
/// information, errors, and additional metadata. The generic parameter allows callers to access the result value in a
/// type-safe manner. Inherits all metadata and status properties from the base ExecutionResult type.</remarks>
/// <typeparam name="TResult">The type of the value returned by the execution operation.</typeparam>
[JsonConverter(typeof(ExecutionResultJsonConverterFactory))]
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public sealed record ExecutionResult<TResult> : _ExecutionResult
{
    /// <summary>
    /// Initializes a new instance of the ExecutionResult class for JSON deserialization.
    /// </summary>
    /// <remarks>This constructor is intended for use by JSON serialization frameworks to create instances of
    /// the ExecutionResult class during deserialization. It should not be called directly in application
    /// code.</remarks>
    [JsonConstructor]
    public ExecutionResult() { }

    /// <summary>
    /// Gets the result value represented by this instance.
    /// </summary>
    [MaybeNull, AllowNull]
    public new TResult Value
    {
        get => (TResult?)base.Value;
        init => base.Value = value;
    }

    /// <inheritdoc />
    public sealed override bool IsGeneric => true;

    /// <inheritdoc />
    [MemberNotNullWhen(true, nameof(Value))]
    public sealed override bool IsSuccess => StatusCode.IsSuccess;

    /// <inheritdoc />
    [MemberNotNull([nameof(Value)])]
    public override void EnsureSuccess()
    {
        if (!IsSuccess)
        {
            throw new ExecutionResultException(this);
        }
    }

    /// <summary>
    /// Converts an instance of the generic <see cref="ExecutionResult{TResult}"/> to a non-generic ExecutionResult.
    /// </summary>
    /// <remarks>This implicit conversion allows seamless use of <see cref="ExecutionResult{TResult}"/> where an
    /// ExecutionResult is expected. The conversion preserves the execution outcome and any associated data, but
    /// type-specific information may be lost.</remarks>
    /// <param name="executionResult">The generic execution result to convert. Cannot be null.</param>
    public static implicit operator ExecutionResult(ExecutionResult<TResult> executionResult)
    {
        ArgumentNullException.ThrowIfNull(executionResult);
        return executionResult.ToExecutionResult();
    }

    /// <summary>
    /// Defines an implicit conversion from <see cref="ExecutionResult{TResult}"/> to
    /// <see cref="ExecutionResult"/>. This allows for seamless transformation of a generic
    /// execution result to its base form.
    /// </summary>
    /// <param name="executionResult">    /// The <see cref="ExecutionResult{TResult}"/> instance to be converted to <see cref="ExecutionResult"/>.
    /// </param>
    /// <returns>
    /// A new instance of <see cref="ExecutionResult"/> populated with the data from the given
    /// <see cref="ExecutionResult{TResult}"/>.
    /// </returns>
    public static implicit operator ExecutionResult<TResult>(ExecutionResult executionResult)
    {
        ArgumentNullException.ThrowIfNull(executionResult);

        return new()
        {
            StatusCode = executionResult.StatusCode,
            Title = executionResult.Title,
            Detail = executionResult.Detail,
            Location = executionResult.Location,
            Value = executionResult.Value is TResult resultValue ? resultValue : default,
            Errors = executionResult.Errors,
            Headers = executionResult.Headers,
            Extensions = executionResult.Extensions
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