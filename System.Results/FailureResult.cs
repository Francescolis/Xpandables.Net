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

namespace System.Results;

/// <summary>
/// Represents the result of an operation that has failed, providing error details and contextual information.
/// </summary>
/// <remarks>Use this type to capture and convey information about failed operations, including associated
/// exceptions, error collections, and descriptive messages. The properties of this result provide structured data for
/// diagnostics and user feedback. This type is not generic and always indicates a failure state.</remarks>
public sealed record FailureResult : Result
{
    /// <inheritdoc/>
    public sealed override bool IsGeneric => false;

    /// <inheritdoc/>
    public sealed override bool IsFailure => true;

    /// <inheritdoc/>
    public sealed override bool IsSuccess => false;

    /// <summary>
    /// Represents an exception associated with the result, if any.
    /// </summary>
    public new Exception? Exception { get; init; }

    /// <summary>
    /// Represents a collection of errors associated with the result.
    /// </summary>
    public required new ElementCollection Errors { get; init; } = [];

    /// <summary>
    /// Represents a short, human-readable summary of the problem type.
    /// </summary>
    public new string? Title { get; init; }

    /// <summary>
    /// Represents a detailed, human-readable explanation specific to this occurrence of the problem.
    /// </summary>
    public new string? Detail { get; init; }

    /// <summary>
    /// Converts a <see cref="FailureResult"/> instance to a <see cref="FailureResult{Object}"/> instance, copying all
    /// relevant properties.
    /// </summary>
    /// <remarks>This operator enables implicit conversion from a non-generic <see cref="FailureResult"/> to a
    /// generic <see cref="FailureResult{Object}"/>. All properties are copied to the new instance. This is useful when
    /// working with APIs that expect the generic form.</remarks>
    /// <param name="failure">The <see cref="FailureResult"/> instance to convert. Cannot be null.</param>
    [Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2225:Operator overloads have named alternates", Justification = "<Pending>")]
    public static implicit operator FailureResult<object>(FailureResult failure)
    {
        ArgumentNullException.ThrowIfNull(failure);
        return new()
        {
            StatusCode = failure.StatusCode,
            Title = failure.Title,
            Detail = failure.Detail,
            Location = failure.Location,
            Errors = failure.Errors,
            Headers = failure.Headers,
            Extensions = failure.Extensions,
            Exception = failure.Exception
        };
    }
}

/// <summary>
/// Represents a generic result that indicates a failed operation and contains error information, details, and an
/// optional exception.
/// </summary>
/// <remarks>Use this type to represent the outcome of an operation that did not succeed, providing structured
/// error details and context. The error information can be accessed through the Errors property, and additional context
/// may be available via Title, Detail, and Exception. This type is typically returned from methods that follow a
/// result-based error handling pattern.</remarks>
/// <typeparam name="TValue">The type of the value associated with the result, if applicable.</typeparam>
public sealed record FailureResult<TValue> : Result<TValue>
{
    /// <inheritdoc/>
    public sealed override bool IsFailure => true;

    /// <inheritdoc/>
    public sealed override bool IsSuccess => false;

    /// <summary>
    /// Represents an exception associated with the result, if any.
    /// </summary>
    public new Exception? Exception { get; init; }

    /// <summary>
    /// Represents a collection of errors associated with the result.
    /// </summary>
    public required new ElementCollection Errors { get; init; } = [];

    /// <summary>
    /// Represents a short, human-readable summary of the problem type.
    /// </summary>
    public new string? Title { get; init; }

    /// <summary>
    /// Represents a detailed, human-readable explanation specific to this occurrence of the problem.
    /// </summary>
    public new string? Detail { get; init; }

    /// <summary>
    /// Converts a generic failure result to a non-generic <see cref="FailureResult"/> instance, preserving all error
    /// details.
    /// </summary>
    /// <remarks>Use this operator to simplify handling of failure results when the value type is not needed.
    /// All error information, including status code, title, details, errors, headers, extensions, and exception, is
    /// retained in the conversion.</remarks>
    /// <param name="failure">The generic failure result to convert. Cannot be <see langword="null"/>.</param>
    [Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2225:Operator overloads have named alternates", Justification = "<Pending>")]
    public static implicit operator FailureResult(FailureResult<TValue> failure)
    {
        ArgumentNullException.ThrowIfNull(failure);

        return new()
        {
            StatusCode = failure.StatusCode,
            Title = failure.Title,
            Detail = failure.Detail,
            Location = failure.Location,
            Errors = failure.Errors,
            Headers = failure.Headers,
            Extensions = failure.Extensions,
            Exception = failure.Exception
        };
    }

    /// <summary>
    /// Converts a non-generic FailureResult instance to a FailureResult generic instance, preserving all failure
    /// details.
    /// </summary>
    /// <remarks>This operator enables seamless conversion from a non-generic failure result to a generic
    /// failure result, allowing code that expects FailureResult generic to handle failures without loss of information.
    /// All properties from the original FailureResult are copied to the new instance.</remarks>
    /// <param name="failure">The FailureResult instance to convert. Cannot be null.</param>
    [Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2225:Operator overloads have named alternates", Justification = "<Pending>")]
    public static implicit operator FailureResult<TValue>(FailureResult failure)
    {
        ArgumentNullException.ThrowIfNull(failure);
        return new()
        {
            StatusCode = failure.StatusCode,
            Title = failure.Title,
            Detail = failure.Detail,
            Location = failure.Location,
            Errors = failure.Errors,
            Headers = failure.Headers,
            Extensions = failure.Extensions,
            Exception = failure.Exception
        };
    }

    /// <summary>
    /// Converts a <see langword="FailureResult{object}"/> instance to a <see cref="FailureResult{TValue}"/> instance,
    /// preserving failure details.
    /// </summary>
    /// <remarks>This operator allows failure results with an object payload to be cast to a generic failure
    /// result of any type, enabling consistent error handling across different value types. All failure details,
    /// including status code, title, detail, location, errors, headers, extensions, and exception, are copied to the
    /// new instance.</remarks>
    /// <param name="failure">The <see langword="FailureResult{object}"/> instance containing failure information to convert. Cannot be null.</param>
    [Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2225:Operator overloads have named alternates", Justification = "<Pending>")]
    public static implicit operator FailureResult<TValue>(FailureResult<object> failure)
    {
        ArgumentNullException.ThrowIfNull(failure);
        return new()
        {
            StatusCode = failure.StatusCode,
            Title = failure.Title,
            Detail = failure.Detail,
            Location = failure.Location,
            Errors = failure.Errors,
            Headers = failure.Headers,
            Extensions = failure.Extensions,
            Exception = failure.Exception
        };
    }
}