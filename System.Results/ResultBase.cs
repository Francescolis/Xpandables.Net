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
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json.Serialization;

namespace System.Results;

/// <summary>
/// Internal base type for result types. This type is not intended for direct use.
/// </summary>
/// <remarks>This abstract record serves as the internal foundation for operation result types. It should not be
/// referenced directly in application code. Use <see cref="Result"/> or <see cref="Result{TValue}"/> instead.</remarks>
[DebuggerDisplay($"{{{nameof(DebuggerDisplay)},nq}}")]
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract record ResultBase
{
    /// <summary>
    /// Initializes a new instance of the ResultBase class for use by derived types and JSON deserialization.
    /// </summary>
    [JsonConstructor]
    protected internal ResultBase() { }

    /// <summary>
    /// Represents the HTTP status code associated with the operation result.
    /// </summary>
    public required HttpStatusCode StatusCode { get; init; } = HttpStatusCode.OK;

    /// <summary>
    /// Represents a short, human-readable summary of the problem type.
    /// </summary>
    public string? Title { get; protected internal init; }

    /// <summary>
    /// Represents a detailed, human-readable explanation specific to this occurrence of the problem.
    /// </summary>
    public string? Detail { get; protected internal init; }

    /// <summary>
    /// Represents a URI reference that identifies a resource relevant to the operation result.
    /// </summary>
    public Uri? Location { get; protected internal init; }

    /// <summary>
    /// Represents the internal value associated with the operation result, which can be of any type.
    /// </summary>
    /// <remarks>This property is designed to hold the result of the operation internally.
    /// Derived types should expose a public <c>Value</c> property with the appropriate type if needed.</remarks>
    [MaybeNull, AllowNull]
    [JsonIgnore]
    protected internal object InternalValue { get; init; }

    /// <summary>
    /// Represents a collection of errors associated with the operation result.
    /// </summary>
    public ElementCollection Errors { get; protected internal init; } = [];

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
    [JsonIgnore]
    public Exception? Exception { get; protected internal init; }

    /// <summary>
    /// Indicates whether the operation result is generic.
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
    /// <exception cref="ResultException">Thrown if the operation result indicates a failure.</exception>
    public void EnsureSuccess()
    {
        if (IsFailure)
        {
            throw new ResultException((Result)this);
        }
    }

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