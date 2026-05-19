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
using System.Net;
using System.Text.Json.Serialization;

namespace System.Results;

/// <summary>
/// Represents the result of an operation that has failed, providing error details and contextual information.
/// </summary>
/// <remarks>Use this type to capture and convey information about failed operations, including associated
/// exceptions, error collections, and descriptive messages. The properties of this result provide structured data for
/// diagnostics and user feedback. This type is not generic and always indicates a failure state.</remarks>
public sealed record FailureResult : Result
{
	/// <summary>
	/// Represents the HTTP status code associated with the operation result.
	/// </summary>
	public HttpStatusCode StatusCode
	{
		get => field;
		init => field = value.Failure();
	}

	/// <summary>
	/// Represents a short, human-readable summary of the problem type.
	/// </summary>
	public string? Title { get; init; }

	/// <summary>
	/// Represents a detailed, human-readable explanation specific to this occurrence of the problem.
	/// </summary>
	public string? Detail { get; init; }

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
	[JsonIgnore]
	public Exception? Exception { get; init; }

	/// <summary>
	/// Creates a new FailureResult that combines the details of the current instance with those of the specified
	/// FailureResult.
	/// </summary>
	/// <remarks>If the specified FailureResult provides non-empty Title or Detail values, they replace those in the
	/// current instance. Errors, headers, extensions, and exception information from the specified FailureResult are
	/// combined with those of the current instance.</remarks>
	/// <param name="other">The FailureResult whose properties and error information are merged into the current instance. Cannot be null.</param>
	/// <returns>A new FailureResult containing merged titles, details, errors, headers, extensions, and exception information from
	/// both instances.</returns>
	public FailureResult Merge(FailureResult other)
	{
		ArgumentNullException.ThrowIfNull(other);
		FailureResult failure = this;

		failure = failure.WithStatusCode(other.StatusCode);
		if (!string.IsNullOrEmpty(other.Title))
		{
			failure = failure.WithTitle(other.Title);
		}
		if (!string.IsNullOrEmpty(other.Detail))
		{
			failure = failure.WithDetail(other.Detail);
		}

		failure = failure.WithErrors(other.Errors);
		failure = failure.WithHeaders(other.Headers);
		failure = failure.WithExtensions(other.Extensions);
		if (other.Exception is not null)
		{
			failure = failure.WithException(other.Exception);
		}

		return failure;
	}
}

/// <summary>
/// Provides extension methods for creating and modifying instances of the FailureResult type with additional error
/// information, headers, details, and extensions.
/// </summary>
/// <remarks>These extension methods enable a fluent style for updating FailureResult objects with custom titles,
/// details, exceptions, headers, errors, and extension data. Each method returns a new FailureResult instance with the
/// specified modifications, preserving immutability. These methods throw an ArgumentNullException if any required
/// argument is null.</remarks>
public static class FailureResultExtensions
{
	/// <summary>
	/// Creates a new FailureResult instance with the specified HTTP status code.
	/// </summary>
	/// <param name="this">The FailureResult instance to update. Cannot be null.</param>
	/// <param name="statusCode">The HTTP status code to assign to the returned FailureResult.</param>
	/// <returns>A new FailureResult instance with the specified status code. The original instance is not modified.</returns>
	public static FailureResult WithStatusCode(this FailureResult @this, HttpStatusCode statusCode)
	{
		ArgumentNullException.ThrowIfNull(@this);
		return @this with { StatusCode = statusCode };
	}

	/// <summary>
	/// Creates a new FailureResult instance with the specified title.
	/// </summary>
	/// <param name="this">The FailureResult instance to copy and update.</param>
	/// <param name="title">The title to assign to the new FailureResult. Cannot be null.</param>
	/// <returns>A new FailureResult instance with the Title property set to the specified value.</returns>
	public static FailureResult WithTitle(this FailureResult @this, string title)
	{
		ArgumentNullException.ThrowIfNull(@this);
		ArgumentNullException.ThrowIfNull(title);

		return @this with { Title = title };
	}

	/// <summary>
	/// Creates a new FailureResult instance with the specified detail message.
	/// </summary>
	/// <param name="this">The FailureResult instance to copy and update.</param>
	/// <param name="detail">The detail message to associate with the new FailureResult. Cannot be null.</param>
	/// <returns>A new FailureResult instance with the Detail property set to the specified value.</returns>
	public static FailureResult WithDetail(this FailureResult @this, string detail)
	{
		ArgumentNullException.ThrowIfNull(@this);
		ArgumentNullException.ThrowIfNull(detail);

		return @this with { Detail = detail };
	}

	/// <summary>
	/// Creates a new FailureResult instance with the specified exception attached.
	/// </summary>
	/// <param name="this">The FailureResult to augment with the exception. Cannot be null.</param>
	/// <param name="exception">The exception to associate with the FailureResult. Cannot be null.</param>
	/// <returns>A new FailureResult instance that includes the specified exception.</returns>
	public static FailureResult WithException(this FailureResult @this, Exception exception)
	{
		ArgumentNullException.ThrowIfNull(@this);
		ArgumentNullException.ThrowIfNull(exception);

		Exception? existingException = @this.Exception;

		if (existingException is not null)
		{
			if (existingException is AggregateException aggregate)
			{
				existingException = new AggregateException(aggregate.InnerExceptions.Append(exception));
			}
			else
			{
				existingException = new AggregateException(existingException, exception);
			}
		}
		else
		{
			existingException = exception;
		}

		return @this with { Exception = existingException };
	}

	/// <summary>
	/// Adds a header with the specified key and value to the failure result and returns a new instance with the updated
	/// headers.
	/// </summary>
	/// <remarks>This method does not modify the original FailureResult instance; instead, it returns a new instance
	/// with the additional header. If a header with the same key already exists, the behavior depends on the
	/// implementation of the Headers collection.</remarks>
	/// <param name="this">The failure result to which the header will be added. Cannot be null.</param>
	/// <param name="key">The key of the header to add. Cannot be null.</param>
	/// <param name="value">The value of the header to add. Cannot be null.</param>
	/// <returns>A new FailureResult instance that includes the specified header.</returns>
	public static FailureResult WithHeader(this FailureResult @this, string key, string value)
	{
		ArgumentNullException.ThrowIfNull(@this);
		ArgumentNullException.ThrowIfNull(key);
		ArgumentNullException.ThrowIfNull(value);

		ElementCollection headers = @this.Headers;
		headers.Add(key, value);
		return @this with { Headers = headers };
	}

	/// <summary>
	/// Returns a new FailureResult instance with the specified headers added to the existing headers.
	/// </summary>
	/// <param name="this">The FailureResult instance to which the headers will be added.</param>
	/// <param name="headers">The collection of headers to add. Cannot be null.</param>
	/// <returns>A new FailureResult instance containing the combined headers.</returns>
	public static FailureResult WithHeaders(this FailureResult @this, ElementCollection headers)
	{
		ArgumentNullException.ThrowIfNull(@this);

		ElementCollection newHeaders = @this.Headers;
		headers.AddRange(headers);
		return @this with { Headers = newHeaders };
	}

	/// <summary>
	/// Adds an error entry with the specified key and error message to the failure result.
	/// </summary>
	/// <param name="this">The failure result to which the error will be added. Cannot be null.</param>
	/// <param name="key">The key that identifies the error. Cannot be null.</param>
	/// <param name="error">The error message to associate with the specified key. Cannot be null.</param>
	/// <returns>A new FailureResult instance that includes the specified error entry.</returns>
	public static FailureResult WithError(this FailureResult @this, string key, string error)
	{
		ArgumentNullException.ThrowIfNull(@this);
		ArgumentNullException.ThrowIfNull(key);
		ArgumentNullException.ThrowIfNull(error);

		ElementCollection errors = @this.Errors;
		errors.Add(key, error);
		return @this with { Errors = errors };
	}

	/// <summary>
	/// Adds the specified collection of errors to the current failure result and returns a new instance with the combined
	/// errors.
	/// </summary>
	/// <param name="this">The failure result to which errors will be added. Cannot be null.</param>
	/// <param name="errors">The collection of errors to add to the failure result. Cannot be null.</param>
	/// <returns>A new FailureResult instance containing the combined errors from the original result and the specified collection.</returns>
	public static FailureResult WithErrors(this FailureResult @this, ElementCollection errors)
	{
		ArgumentNullException.ThrowIfNull(@this);

		ElementCollection newErrors = @this.Errors;
		errors.AddRange(errors);
		return @this with { Errors = newErrors };
	}

	/// <summary>
	/// Returns a new FailureResult instance with the specified extension key and value added to its Extensions collection.
	/// </summary>
	/// <remarks>This method does not modify the original FailureResult instance. Instead, it returns a new instance
	/// with the updated Extensions collection.</remarks>
	/// <param name="this">The FailureResult instance to which the extension will be added. Cannot be null.</param>
	/// <param name="key">The key of the extension to add. Cannot be null.</param>
	/// <param name="value">The value of the extension to add. Cannot be null.</param>
	/// <returns>A new FailureResult instance that includes the specified extension key and value.</returns>
	public static FailureResult WithExtension(this FailureResult @this, string key, string value)
	{
		ArgumentNullException.ThrowIfNull(@this);
		ArgumentNullException.ThrowIfNull(key);
		ArgumentNullException.ThrowIfNull(value);

		ElementCollection extensions = @this.Extensions;
		extensions.Add(key, value);
		return @this with { Extensions = extensions };
	}

	/// <summary>
	/// Returns a new FailureResult instance with the specified extensions added to its existing collection.
	/// </summary>
	/// <param name="this">The FailureResult instance to which the extensions will be added.</param>
	/// <param name="extensions">The collection of extensions to add. Cannot be null.</param>
	/// <returns>A new FailureResult instance containing the combined extensions.</returns>
	public static FailureResult WithExtensions(this FailureResult @this, ElementCollection extensions)
	{
		ArgumentNullException.ThrowIfNull(@this);

		ElementCollection newExtensions = @this.Extensions;
		extensions.AddRange(extensions);
		return @this with { Headers = newExtensions };
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
	/// <summary>
	/// Represents the HTTP status code associated with the operation result.
	/// </summary>
	public HttpStatusCode StatusCode
	{
		get => field;
		init => field = value.Failure();
	}

	/// <summary>
	/// Represents a short, human-readable summary of the problem type.
	/// </summary>
	public string? Title { get; init; }

	/// <summary>
	/// Represents a detailed, human-readable explanation specific to this occurrence of the problem.
	/// </summary>
	public string? Detail { get; init; }

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
	[JsonIgnore]
	public Exception? Exception { get; init; }

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
			Errors = failure.Errors,
			Headers = failure.Headers,
			Extensions = failure.Extensions,
			Exception = failure.Exception
		};
	}
}

/// <summary>
/// Provides extension methods for building and modifying <see cref="FailureResult{TValue}"/> instances in a fluent manner.
/// </summary>
/// <remarks>These extension methods enable a fluent API for constructing failure results with various properties
/// such as titles, details, exceptions, headers, errors, and extensions. Each method returns a new instance with
/// the specified property set, following an immutable pattern.</remarks>
public static class FailureResultOfValueExtensions
{
	/// <summary>
	/// Creates a new FailureResult result instance with the specified HTTP status code.
	/// </summary>
	/// <param name="this">The FailureResult instance to update. Cannot be null.</param>
	/// <param name="statusCode">The HTTP status code to assign to the returned FailureResult.</param>
	/// <returns>A new FailureResult instance with the specified status code. The original instance is not modified.</returns>
	public static FailureResult<TValue> WithStatusCode<TValue>(this FailureResult<TValue> @this, HttpStatusCode statusCode)
	{
		ArgumentNullException.ThrowIfNull(@this);
		return @this with { StatusCode = statusCode };
	}

	/// <summary>
	/// Sets the title for a failure result, providing a short, human-readable summary of the problem type.
	/// </summary>
	/// <typeparam name="TValue">The type of the value associated with the result.</typeparam>
	/// <param name="this">The failure result instance to modify. Cannot be <see langword="null"/>.</param>
	/// <param name="title">The title to set. Cannot be <see langword="null"/>.</param>
	/// <returns>A new <see cref="FailureResult{TValue}"/> instance with the specified title.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="this"/> or <paramref name="title"/> is <see langword="null"/>.</exception>
	public static FailureResult<TValue> WithTitle<TValue>(this FailureResult<TValue> @this, string title)
	{
		ArgumentNullException.ThrowIfNull(@this);
		ArgumentNullException.ThrowIfNull(title);

		return @this with { Title = title };
	}

	/// <summary>
	/// Sets the detail for a failure result, providing a detailed, human-readable explanation specific to this occurrence of the problem.
	/// </summary>
	/// <typeparam name="TValue">The type of the value associated with the result.</typeparam>
	/// <param name="this">The failure result instance to modify. Cannot be <see langword="null"/>.</param>
	/// <param name="detail">The detail message to set. Cannot be <see langword="null"/>.</param>
	/// <returns>A new <see cref="FailureResult{TValue}"/> instance with the specified detail.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="this"/> or <paramref name="detail"/> is <see langword="null"/>.</exception>
	public static FailureResult<TValue> WithDetail<TValue>(this FailureResult<TValue> @this, string detail)
	{
		ArgumentNullException.ThrowIfNull(@this);
		ArgumentNullException.ThrowIfNull(detail);

		return @this with { Detail = detail };
	}

	/// <summary>
	/// Associates an exception with a failure result, capturing the underlying error that caused the failure.
	/// </summary>
	/// <typeparam name="TValue">The type of the value associated with the result.</typeparam>
	/// <param name="this">The failure result instance to modify. Cannot be <see langword="null"/>.</param>
	/// <param name="exception">The exception to associate with the failure. Cannot be <see langword="null"/>.</param>
	/// <returns>A new <see cref="FailureResult{TValue}"/> instance with the specified exception.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="this"/> or <paramref name="exception"/> is <see langword="null"/>.</exception>
	public static FailureResult<TValue> WithException<TValue>(this FailureResult<TValue> @this, Exception exception)
	{
		ArgumentNullException.ThrowIfNull(@this);
		ArgumentNullException.ThrowIfNull(exception);

		return @this with { Exception = exception };
	}

	/// <summary>
	/// Adds a single header to the failure result, providing additional metadata relevant to the operation context.
	/// </summary>
	/// <typeparam name="TValue">The type of the value associated with the result.</typeparam>
	/// <param name="this">The failure result instance to modify. Cannot be <see langword="null"/>.</param>
	/// <param name="key">The header key. Cannot be <see langword="null"/>.</param>
	/// <param name="value">The header value. Cannot be <see langword="null"/>.</param>
	/// <returns>A new <see cref="FailureResult{TValue}"/> instance with the specified header added.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="this"/>, <paramref name="key"/>, or <paramref name="value"/> is <see langword="null"/>.</exception>
	public static FailureResult<TValue> WithHeader<TValue>(this FailureResult<TValue> @this, string key, string value)
	{
		ArgumentNullException.ThrowIfNull(@this);
		ArgumentNullException.ThrowIfNull(key);
		ArgumentNullException.ThrowIfNull(value);

		ElementCollection headers = @this.Headers;
		headers.Add(key, value);
		return @this with { Headers = headers };
	}

	/// <summary>
	/// Adds multiple headers to the failure result, providing additional metadata relevant to the operation context.
	/// </summary>
	/// <typeparam name="TValue">The type of the value associated with the result.</typeparam>
	/// <param name="this">The failure result instance to modify. Cannot be <see langword="null"/>.</param>
	/// <param name="headers">The collection of headers to add.</param>
	/// <returns>A new <see cref="FailureResult{TValue}"/> instance with the specified headers added.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="this"/> is <see langword="null"/>.</exception>
	public static FailureResult<TValue> WithHeaders<TValue>(this FailureResult<TValue> @this, ElementCollection headers)
	{
		ArgumentNullException.ThrowIfNull(@this);

		ElementCollection newHeaders = @this.Headers;
		headers.AddRange(headers);
		return @this with { Headers = newHeaders };
	}

	/// <summary>
	/// Adds a single error to the failure result, capturing specific validation or business rule failures.
	/// </summary>
	/// <typeparam name="TValue">The type of the value associated with the result.</typeparam>
	/// <param name="this">The failure result instance to modify. Cannot be <see langword="null"/>.</param>
	/// <param name="key">The error key or field name. Cannot be <see langword="null"/>.</param>
	/// <param name="error">The error message. Cannot be <see langword="null"/>.</param>
	/// <returns>A new <see cref="FailureResult{TValue}"/> instance with the specified error added.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="this"/>, <paramref name="key"/>, or <paramref name="error"/> is <see langword="null"/>.</exception>
	public static FailureResult<TValue> WithError<TValue>(this FailureResult<TValue> @this, string key, string error)
	{
		ArgumentNullException.ThrowIfNull(@this);
		ArgumentNullException.ThrowIfNull(key);
		ArgumentNullException.ThrowIfNull(error);

		ElementCollection errors = @this.Errors;
		errors.Add(key, error);
		return @this with { Errors = errors };
	}

	/// <summary>
	/// Adds multiple errors to the failure result, capturing specific validation or business rule failures.
	/// </summary>
	/// <typeparam name="TValue">The type of the value associated with the result.</typeparam>
	/// <param name="this">The failure result instance to modify. Cannot be <see langword="null"/>.</param>
	/// <param name="errors">The collection of errors to add.</param>
	/// <returns>A new <see cref="FailureResult{TValue}"/> instance with the specified errors added.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="this"/> is <see langword="null"/>.</exception>
	public static FailureResult<TValue> WithErrors<TValue>(this FailureResult<TValue> @this, ElementCollection errors)
	{
		ArgumentNullException.ThrowIfNull(@this);

		ElementCollection newErrors = @this.Errors;
		errors.AddRange(errors);
		return @this with { Errors = newErrors };
	}

	/// <summary>
	/// Adds a single extension to the failure result, providing custom metadata for specialized scenarios.
	/// </summary>
	/// <typeparam name="TValue">The type of the value associated with the result.</typeparam>
	/// <param name="this">The failure result instance to modify. Cannot be <see langword="null"/>.</param>
	/// <param name="key">The extension key. Cannot be <see langword="null"/>.</param>
	/// <param name="value">The extension value. Cannot be <see langword="null"/>.</param>
	/// <returns>A new <see cref="FailureResult{TValue}"/> instance with the specified extension added.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="this"/>, <paramref name="key"/>, or <paramref name="value"/> is <see langword="null"/>.</exception>
	public static FailureResult<TValue> WithExtension<TValue>(this FailureResult<TValue> @this, string key, string value)
	{
		ArgumentNullException.ThrowIfNull(@this);
		ArgumentNullException.ThrowIfNull(key);
		ArgumentNullException.ThrowIfNull(value);

		ElementCollection extensions = @this.Extensions;
		extensions.Add(key, value);
		return @this with { Extensions = extensions };
	}

	/// <summary>
	/// Adds multiple extensions to the failure result, providing custom metadata for specialized scenarios.
	/// </summary>
	/// <typeparam name="TValue">The type of the value associated with the result.</typeparam>
	/// <param name="this">The failure result instance to modify. Cannot be <see langword="null"/>.</param>
	/// <param name="extensions">The collection of extensions to add.</param>
	/// <returns>A new <see cref="FailureResult{TValue}"/> instance with the specified extensions added.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="this"/> is <see langword="null"/>.</exception>
	public static FailureResult<TValue> WithExtensions<TValue>(this FailureResult<TValue> @this, ElementCollection extensions)
	{
		ArgumentNullException.ThrowIfNull(@this);

		ElementCollection newExtensions = @this.Extensions;
		extensions.AddRange(extensions);
		return @this with { Headers = newExtensions };
	}

}
