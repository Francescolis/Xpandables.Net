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
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

using Xpandables.Net.Tasks.Collections;

namespace Xpandables.Net.Tasks.ExecutionResults;

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
    [JsonIgnore]
    public Exception? Exception { get; init; }

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
