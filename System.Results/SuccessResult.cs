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
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace System.Results;

/// <summary>
/// Represents a result that indicates a successful operation without an associated value.
/// </summary>
/// <remarks>Use this type to signal that an operation completed successfully when no additional data needs to be
/// returned. This is typically used in scenarios where only the success or failure state is relevant, and no result
/// value is required.</remarks>
public sealed record SuccessResult : Result
{
	/// <summary>
	/// Represents the HTTP status code associated with the operation result.
	/// </summary>
	public required HttpStatusCode StatusCode
	{
		get => field;
		init => field = value.Success();
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
	/// Represents a URI reference that identifies a resource relevant to the operation result.
	/// </summary>
	public Uri? Location { get; init; }

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal object? Value { get; init; }

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
	/// Gets the current value held by the instance.
	/// </summary>
	/// <returns>The value associated with the instance, or null if no value is set.</returns>
	[SuppressMessage("Design", "CA1024:Use properties where appropriate", Justification = "<Pending>")]
	public object? GetValue() => Value;
}

/// <summary>
/// Provides extension methods for creating modified copies of SuccessResult instances with updated properties or
/// collections.
/// </summary>
/// <remarks>These methods enable a fluent approach to updating SuccessResult instances by returning new objects
/// with the specified changes applied. The original instance is never modified. This class is intended to simplify the
/// process of customizing SuccessResult objects in a safe and immutable manner.</remarks>
public static class SuccessResultExtensions
{
	/// <summary>
	/// Returns a new SuccessResult instance with the specified title set.
	/// </summary>
	/// <remarks>This method does not modify the original instance. Instead, it returns a new instance with the
	/// updated title.</remarks>
	/// <param name="this">The SuccessResult instance to copy and update. Cannot be null.</param>
	/// <param name="title">The title to assign to the new SuccessResult instance. Cannot be null.</param>
	/// <returns>A new SuccessResult instance with the Title property set to the specified value.</returns>
	public static SuccessResult WithTitle(this SuccessResult @this, string title)
	{
		ArgumentNullException.ThrowIfNull(@this);
		ArgumentNullException.ThrowIfNull(title);

		return @this with { Title = title };
	}

	/// <summary>
	/// Creates a copy of the current SuccessResult with the specified detail message.
	/// </summary>
	/// <remarks>This method does not modify the original instance. It returns a new object with the updated
	/// detail.</remarks>
	/// <param name="this">The SuccessResult instance to copy.</param>
	/// <param name="detail">The detail message to associate with the result. Cannot be null.</param>
	/// <returns>A new SuccessResult instance with the Detail property set to the specified value.</returns>
	public static SuccessResult WithDetail(this SuccessResult @this, string detail)
	{
		ArgumentNullException.ThrowIfNull(@this);
		ArgumentNullException.ThrowIfNull(detail);

		return @this with { Detail = detail };
	}

	/// <summary>
	/// Returns a new SuccessResult instance with the specified location set.
	/// </summary>
	/// <remarks>This method does not modify the original instance; it returns a new instance with the updated
	/// Location property.</remarks>
	/// <param name="this">The SuccessResult instance to copy and update. Cannot be null.</param>
	/// <param name="location">The URI to assign to the Location property. Cannot be null.</param>
	/// <returns>A new SuccessResult instance with the Location property set to the specified URI.</returns>
	public static SuccessResult WithLocation(this SuccessResult @this, Uri location)
	{
		ArgumentNullException.ThrowIfNull(@this);
		ArgumentNullException.ThrowIfNull(location);

		return @this with { Location = location };
	}

	/// <summary>
	/// Returns a new SuccessResult instance with the specified header key and value added to the Headers collection.
	/// </summary>
	/// <remarks>This method does not modify the original SuccessResult instance. Instead, it returns a new instance
	/// with the updated Headers collection.</remarks>
	/// <param name="this">The SuccessResult instance to which the header will be added. Cannot be null.</param>
	/// <param name="key">The name of the header to add. Cannot be null.</param>
	/// <param name="value">The value of the header to add. Cannot be null.</param>
	/// <returns>A new SuccessResult instance that includes the specified header in its Headers collection.</returns>
	public static SuccessResult WithHeader(this SuccessResult @this, string key, string value)
	{
		ArgumentNullException.ThrowIfNull(@this);
		ArgumentNullException.ThrowIfNull(key);
		ArgumentNullException.ThrowIfNull(value);

		ElementCollection headers = @this.Headers;
		headers.Add(key, value);
		return @this with { Headers = headers };
	}

	/// <summary>
	/// Returns a new SuccessResult instance with the specified headers added to the existing collection.
	/// </summary>
	/// <param name="this">The SuccessResult instance to which headers will be added.</param>
	/// <param name="headers">The collection of headers to add. Cannot be null.</param>
	/// <returns>A new SuccessResult instance containing the combined headers.</returns>
	public static SuccessResult WithHeaders(this SuccessResult @this, ElementCollection headers)
	{
		ArgumentNullException.ThrowIfNull(@this);

		ElementCollection newHeaders = @this.Headers;
		headers.AddRange(headers);
		return @this with { Headers = newHeaders };
	}

	/// <summary>
	/// Adds a key-value pair to the Extensions collection of the specified SuccessResult and returns a new instance with
	/// the updated extensions.
	/// </summary>
	/// <remarks>This method does not modify the original SuccessResult instance. Instead, it returns a new instance
	/// with the updated Extensions collection. If the specified key already exists in the Extensions collection, an
	/// exception may be thrown.</remarks>
	/// <param name="this">The SuccessResult instance to which the extension will be added. Cannot be null.</param>
	/// <param name="key">The key of the extension to add. Cannot be null.</param>
	/// <param name="value">The value of the extension to add. Cannot be null.</param>
	/// <returns>A new SuccessResult instance with the specified extension added to its Extensions collection.</returns>
	public static SuccessResult WithExtension(this SuccessResult @this, string key, string value)
	{
		ArgumentNullException.ThrowIfNull(@this);
		ArgumentNullException.ThrowIfNull(key);
		ArgumentNullException.ThrowIfNull(value);

		ElementCollection extensions = @this.Extensions;
		extensions.Add(key, value);
		return @this with { Extensions = extensions };
	}

	/// <summary>
	/// Returns a new SuccessResult instance with the specified extensions added to its existing collection.
	/// </summary>
	/// <param name="this">The SuccessResult instance to which the extensions will be added. Cannot be null.</param>
	/// <param name="extensions">The collection of extensions to add to the SuccessResult. Cannot be null.</param>
	/// <returns>A new SuccessResult instance containing the combined extensions.</returns>
	public static SuccessResult WithExtensions(this SuccessResult @this, ElementCollection extensions)
	{
		ArgumentNullException.ThrowIfNull(@this);

		ElementCollection newExtensions = @this.Extensions;
		extensions.AddRange(extensions);
		return @this with { Headers = newExtensions };
	}
}


/// <summary>
/// Represents a successful result that contains a value of the specified type.
/// </summary>
/// <remarks>Use this type to indicate the successful completion of an operation and to provide the resulting
/// value. The <see cref="Value"/> property holds the value associated with the success. This type is typically returned
/// from methods that follow a result pattern, distinguishing between success and failure cases.</remarks>
/// <typeparam name="TValue">The type of the value contained in the result.</typeparam>
public sealed record SuccessResult<TValue> : Result<TValue>
{
	/// <summary>
	/// Represents the HTTP status code associated with the operation result.
	/// </summary>
	public required HttpStatusCode StatusCode
	{
		get => field;
		init => field = value.Success();
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
	/// Represents a URI reference that identifies a resource relevant to the operation result.
	/// </summary>
	public Uri? Location { get; init; }

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
	/// Gets or sets the value of the result.
	/// </summary>
	[MaybeNull, AllowNull]
	public required TValue Value { get; init; }

	/// <summary>
	/// Converts a generic <see cref="SuccessResult{TValue}"/> instance to a non-generic <see cref="SuccessResult"/>
	/// instance, preserving response metadata and value.
	/// </summary>
	/// <remarks>This operator allows seamless conversion when only the response metadata and value are needed
	/// without the generic type parameter. The <see cref="Value"/> property is preserved as an object.</remarks>
	/// <param name="success">The generic success result to convert. Cannot be null.</param>
	[SuppressMessage("Usage", "CA2225:Operator overloads have named alternates", Justification = "<Pending>")]
	public static implicit operator SuccessResult(SuccessResult<TValue> success)
	{
		ArgumentNullException.ThrowIfNull(success);
		return new()
		{
			Headers = success.Headers,
			StatusCode = success.StatusCode,
			Location = success.Location,
			Detail = success.Detail,
			Extensions = success.Extensions,
			Title = success.Title,
			Value = success.Value
		};
	}

	/// <summary>
	/// Converts a non-generic SuccessResult instance to a generic <see cref="SuccessResult{TValue}"/> instance, copying all relevant
	/// properties.
	/// </summary>
	/// <remarks>Use this operator to convert a non-generic success result to a generic one when a strongly typed
	/// value is required. The Value property is cast to the specified generic type parameter.</remarks>
	/// <param name="success">The SuccessResult instance to convert. Cannot be null.</param>
	[SuppressMessage("Usage", "CA2225:Operator overloads have named alternates", Justification = "<Pending>")]
	public static implicit operator SuccessResult<TValue>(SuccessResult success)
	{
		ArgumentNullException.ThrowIfNull(success);
		return new()
		{
			Headers = success.Headers,
			StatusCode = success.StatusCode,
			Location = success.Location,
			Detail = success.Detail,
			Extensions = success.Extensions,
			Title = success.Title,
			Value = (TValue?)success.Value
		};
	}
}

/// <summary>
/// Provides extension methods for creating modified copies of SuccessResult{TValue} instances with updated metadata,
/// such as title, detail, location, headers, or extensions.
/// </summary>
/// <remarks>These extension methods enable fluent modification of SuccessResult{TValue} objects by returning new
/// instances with the specified changes applied. The original instance is not modified. All methods require non-null
/// arguments and will throw an ArgumentNullException if any required parameter is null.</remarks>
public static class SuccessResultOfTValueExtensions
{
	/// <summary>
	/// Returns a new SuccessResult{TValue} instance with the specified title set.
	/// </summary>
	/// <typeparam name="TValue">The type of the value contained in the success result.</typeparam>
	/// <param name="this">The  SuccessResult{TValue} instance to update. Cannot be null.</param>
	/// <param name="value">The title to assign to the result. Cannot be null.</param>
	/// <returns>A new  SuccessResult{TValue} instance with the Title property set to the specified value.</returns>
	public static SuccessResult<TValue> WithTitle<TValue>(this SuccessResult<TValue> @this, TValue value)
	{
		ArgumentNullException.ThrowIfNull(@this);
		ArgumentNullException.ThrowIfNull(value);

		return @this with { Value = value };
	}

	/// <summary>
	/// Returns a new SuccessResult{TValue} instance with the specified title set.
	/// </summary>
	/// <typeparam name="TValue">The type of the value contained in the success result.</typeparam>
	/// <param name="this">The  SuccessResult{TValue} instance to update. Cannot be null.</param>
	/// <param name="title">The title to assign to the result. Cannot be null.</param>
	/// <returns>A new  SuccessResult{TValue} instance with the Title property set to the specified value.</returns>
	public static SuccessResult<TValue> WithTitle<TValue>(this SuccessResult<TValue> @this, string title)
	{
		ArgumentNullException.ThrowIfNull(@this);
		ArgumentNullException.ThrowIfNull(title);

		return @this with { Title = title };
	}

	/// <summary>
	/// Creates a new instance of the current success result with the specified detail message.
	/// </summary>
	/// <remarks>This method does not modify the original result. Instead, it returns a new instance with the
	/// updated detail.</remarks>
	/// <typeparam name="TValue">The type of the value contained in the success result.</typeparam>
	/// <param name="this">The success result to augment with additional detail. Cannot be null.</param>
	/// <param name="detail">The detail message to associate with the result. Cannot be null.</param>
	/// <returns>A new <see cref="SuccessResult{TValue}"/> instance with the specified detail message.</returns>
	public static SuccessResult<TValue> WithDetail<TValue>(this SuccessResult<TValue> @this, string detail)
	{
		ArgumentNullException.ThrowIfNull(@this);
		ArgumentNullException.ThrowIfNull(detail);

		return @this with { Detail = detail };
	}

	/// <summary>
	/// Returns a new  SuccessResult{TValue} instance with the specified location set.
	/// </summary>
	/// <remarks>This method does not modify the original instance. Instead, it returns a new instance with the
	/// updated Location property.</remarks>
	/// <typeparam name="TValue">The type of the value contained in the success result.</typeparam>
	/// <param name="this">The  SuccessResult{TValue} instance to copy and update. Cannot be null.</param>
	/// <param name="location">The URI to assign to the Location property. Cannot be null.</param>
	/// <returns>A new  SuccessResult{TValue} instance with the Location property set to the specified URI.</returns>
	public static SuccessResult<TValue> WithLocation<TValue>(this SuccessResult<TValue> @this, Uri location)
	{
		ArgumentNullException.ThrowIfNull(@this);
		ArgumentNullException.ThrowIfNull(location);

		return @this with { Location = location };
	}

	/// <summary>
	/// Returns a new  SuccessResult{TValue} instance with the specified header key and value added to its Headers
	/// collection.
	/// </summary>
	/// <remarks>This method does not modify the original  SuccessResult{TValue} instance. Instead, it returns a new
	/// instance with the updated Headers collection.</remarks>
	/// <typeparam name="TValue">The type of the value contained in the SuccessResult.</typeparam>
	/// <param name="this">The  SuccessResult{TValue} to which the header will be added. Cannot be null.</param>
	/// <param name="key">The key of the header to add. Cannot be null.</param>
	/// <param name="value">The value of the header to add. Cannot be null.</param>
	/// <returns>A new  SuccessResult{TValue} instance that includes the specified header.</returns>
	public static SuccessResult<TValue> WithHeader<TValue>(this SuccessResult<TValue> @this, string key, string value)
	{
		ArgumentNullException.ThrowIfNull(@this);
		ArgumentNullException.ThrowIfNull(key);
		ArgumentNullException.ThrowIfNull(value);

		ElementCollection headers = @this.Headers;
		headers.Add(key, value);
		return @this with { Headers = headers };
	}

	/// <summary>
	/// Returns a new  SuccessResult{TValue} instance with the specified headers added to the existing headers.
	/// </summary>
	/// <remarks>This method does not modify the original  SuccessResult{TValue} instance. Instead, it returns a new
	/// instance with the combined headers.</remarks>
	/// <typeparam name="TValue">The type of the value contained in the success result.</typeparam>
	/// <param name="this">The  SuccessResult{TValue} instance to which the headers will be added. Cannot be null.</param>
	/// <param name="headers">The collection of headers to add. Cannot be null.</param>
	/// <returns>A new  SuccessResult{TValue} instance that includes the specified headers in addition to the existing headers.</returns>
	public static SuccessResult<TValue> WithHeaders<TValue>(this SuccessResult<TValue> @this, ElementCollection headers)
	{
		ArgumentNullException.ThrowIfNull(@this);

		ElementCollection newHeaders = @this.Headers;
		headers.AddRange(headers);
		return @this with { Headers = newHeaders };
	}

	/// <summary>
	/// Returns a new  SuccessResult{TValue} instance with the specified extension key and value added to its Extensions
	/// collection.
	/// </summary>
	/// <remarks>This method does not modify the original  SuccessResult{TValue} instance. Instead, it returns a new
	/// instance with the updated Extensions collection.</remarks>
	/// <typeparam name="TValue">The type of the value contained in the SuccessResult.</typeparam>
	/// <param name="this">The  SuccessResult{TValue} to which the extension will be added. Cannot be null.</param>
	/// <param name="key">The key of the extension to add. Cannot be null.</param>
	/// <param name="value">The value of the extension to add. Cannot be null.</param>
	/// <returns>A new S SuccessResult{TValue} instance that includes the specified extension key and value.</returns>
	public static SuccessResult<TValue> WithExtension<TValue>(this SuccessResult<TValue> @this, string key, string value)
	{
		ArgumentNullException.ThrowIfNull(@this);
		ArgumentNullException.ThrowIfNull(key);
		ArgumentNullException.ThrowIfNull(value);

		ElementCollection extensions = @this.Extensions;
		extensions.Add(key, value);
		return @this with { Extensions = extensions };
	}

	/// <summary>
	/// Returns a new  SuccessResult{TValue} instance with the specified extensions added to the existing collection.
	/// </summary>
	/// <typeparam name="TValue">The type of the value contained in the success result.</typeparam>
	/// <param name="this">The  SuccessResult{TValue}instance to which the extensions will be added. Cannot be null.</param>
	/// <param name="extensions">The collection of extensions to add. Cannot be null.</param>
	/// <returns>A new  SuccessResult{TValue} instance that includes the specified extensions in its collection.</returns>
	public static SuccessResult<TValue> WithExtensions<TValue>(this SuccessResult<TValue> @this, ElementCollection extensions)
	{
		ArgumentNullException.ThrowIfNull(@this);

		ElementCollection newExtensions = @this.Extensions;
		extensions.AddRange(extensions);
		return @this with { Headers = newExtensions };
	}

}
