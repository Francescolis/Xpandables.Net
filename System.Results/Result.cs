/*******************************************************************************
 * Copyright (C) 2025-2026 Kamersoft
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
using System.Text.Json.Serialization;

namespace System.Results;

/// <summary>
/// Provides a base type for representing the result of an operation, including status information, error details, and
/// associated metadata.
/// </summary>
/// <remarks>This record serves as the foundation for operation result types, encapsulating common
/// properties such as HTTP status code, error collections, headers, and operation details. It is designed for
/// extensibility and serialization, allowing derived types to represent both successful and failed outcomes. Use the
/// provided properties to inspect the result, access error information, and handle operation metadata. The type
/// supports integration with HTTP-based APIs and can be used to standardize result handling across application
/// layers.</remarks>
[DebuggerDisplay($"{{{nameof(DebuggerDisplay)},nq}}")]
public partial record Result
{
	/// <summary>
	/// Initializes a new instance of the Result class for use by derived types and JSON deserialization.
	/// </summary>
	/// <remarks>This constructor is intended for use by subclasses and JSON serialization frameworks. It
	/// cannot be called directly from external assemblies.</remarks>
	[JsonConstructor]
	protected internal Result() { }

	/// <summary>
	/// Represents the HTTP status code associated with the operation result.
	/// </summary>
	public HttpStatusCode StatusCode { get; init; } = HttpStatusCode.OK;

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
	/// Derived types should override this property to provide typed storage and avoid boxing.
	/// Use <see cref="Result{TValue}"/> for strongly-typed results.</remarks>
	[MaybeNull, AllowNull]
	[JsonIgnore]
	protected internal virtual object? InternalValue { get; init; }

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
	/// Indicates whether the operation result is generic. The value is false for non-generic result types.
	/// </summary>
	public virtual bool IsGeneric => false;

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
			throw new ResultException(this);
		}
	}

	/// <summary>
	/// Retrieves the underlying value represented by the current instance.
	/// </summary>
	/// <returns>An object containing the internal value, or null if no value is set.</returns>
	public virtual object? GetUnderlyingValue() => InternalValue;

	/// <summary>
	/// Converts this <see cref="Result"/> to a <see cref="Result{TValue}"/> instance.
	/// If this instance is already of type <see cref="Result{TValue}"/>, it is returned directly (zero-copy).
	/// Otherwise, a new instance is created, copying all metadata properties.
	/// </summary>
	/// <typeparam name="TValue">The type of the value for the target result.</typeparam>
	/// <returns>A <see cref="Result{TValue}"/> containing the metadata from this result.</returns>
	public Result<TValue> ToResult<TValue>()
	{
		if (this is Result<TValue> typed)
		{
			return typed;
		}

		return new()
		{
			StatusCode = StatusCode,
			Title = Title,
			Detail = Detail,
			Location = Location,
			Errors = Errors,
			Headers = Headers,
			Extensions = Extensions,
			Exception = Exception,
			Value = InternalValue is TValue value ? value : default
		};
	}

	/// <summary>
	/// Returns a string that provides a concise, human-readable representation of the response for debugging purposes.
	/// </summary>
	protected string DebuggerDisplay => $"{(int)StatusCode} {StatusCode} - {(IsSuccess ? "Success" : "Failure")}{(Title is not null ? $": {Title}" : string.Empty)}";
}

/// <summary>
/// Provides a base type for representing the result of an operation of <typeparamref name="TValue"/>, including status information, error details, and
/// associated metadata.
/// </summary>
/// <remarks>This record serves as the foundation for operation result types, encapsulating common
/// properties such as HTTP status code, error collections, headers, and operation details. It is designed for
/// extensibility and serialization, allowing derived types to represent both successful and failed outcomes. Use the
/// provided properties to inspect the result, access error information, and handle operation metadata. The type
/// supports integration with HTTP-based APIs and can be used to standardize result handling across application
/// layers.</remarks>
/// <typeparam name="TValue">The type of the value contained in the result.</typeparam>
public record Result<TValue> : Result
{
	/// <summary>
	/// Typed backing store for <see cref="Value"/>.
	/// Avoids boxing value types during normal get/set operations.
	/// </summary>
	private protected TValue? _typedValue;

	/// <summary>
	/// Initializes a new instance of the Result class for use by derived types and JSON deserialization.
	/// </summary>
	/// <remarks>This constructor is intended for use by subclasses and JSON serialization frameworks. It
	/// should not be called directly in application code.</remarks>
	[JsonConstructor]
	protected internal Result() { }

	/// <summary>
	/// Indicates whether the operation result is generic. The value is true for generic result types.
	/// </summary>
	public sealed override bool IsGeneric => true;

	/// <summary>
	/// Provides polymorphic access to the stored value for non-generic consumers.
	/// Delegates to <see cref="_typedValue"/>, boxing only when accessed through this property.
	/// </summary>
	[MaybeNull, AllowNull]
	[JsonIgnore]
	protected internal sealed override object? InternalValue
	{
		get => _typedValue;
		init => _typedValue = value is TValue tv ? tv : default;
	}

	/// <summary>
	/// Gets or sets the value of the result.
	/// </summary>
	[MaybeNull, AllowNull]
	public TValue Value
	{
		get => _typedValue;
		protected internal init => _typedValue = value;
	}
}
