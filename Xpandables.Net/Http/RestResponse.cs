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
using System.Diagnostics.CodeAnalysis;
using System.Net;

using Xpandables.Net.Collections;

namespace Xpandables.Net.Http;

/// <summary>
/// Represents an abstract HTTP response with properties for status code, headers, result, exception, version, and
/// reason phrase.Includes methods for checking success, failure, and equality.
/// </summary>
public class RestResponseAbstract : Disposable
{
    /// <summary>
    /// Represents the HTTP status code for a response. It is a required property that can only be set during
    /// initialization.
    /// </summary>
    public required HttpStatusCode StatusCode { get; init; }

    /// <summary>
    /// Represents the HTTP headers associated with a request or response. It is a required property that must be
    /// initialized.
    /// </summary>
    public required ElementCollection Headers { get; init; }

    /// <summary>
    /// Holds the result of an operation, which can be of any type or null. It is initialized at the time of object
    /// creation.
    /// </summary>
    public object? Result { get; init; }

    /// <summary>
    /// Represents an optional exception that may have occurred. It can be null if no exception is present.
    /// </summary>
    public Exception? Exception { get; init; }

    /// <summary>
    /// Represents the version of an assembly or application. It is a required property that can only be set during
    /// initialization.
    /// </summary>
    public required Version Version { get; init; }

    /// <summary>
    /// Represents the reason phrase associated with the HTTP response. It provides additional context about the
    /// response status.
    /// </summary>
    public string? ReasonPhrase { get; init; }

    /// <summary>
    /// Indicates whether the operation was successful based on the StatusCode. Returns true for status codes in the
    /// range of 200 to 299.
    /// </summary>
    [MemberNotNullWhen(false, nameof(Exception))]
    public bool IsSuccess => (int)StatusCode is >= 200 and <= 299;

    /// <summary>
    /// Indicates whether an operation has failed. It returns true if the operation was not successful.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Exception))]
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Indicates whether the Result is not null and is of a type other than object. This helps determine if the Result
    /// is a generic type.
    /// </summary>
    public bool IsGeneric => Result is not null && Result.GetType() != typeof(object);

    private bool _isDisposed;

    /// <summary>
    /// Releases resources used by the object, ensuring proper cleanup when no longer needed. It checks if the object
    /// has already been disposed.
    /// </summary>
    /// <param name="disposing">Indicates whether to release both managed and unmanaged resources.</param>
    protected override void Dispose(bool disposing)
    {
        if (_isDisposed)
            return;

        if (disposing)
        {
            (Result as IDisposable)?.Dispose();
            (Exception as IDisposable)?.Dispose();
        }

        _isDisposed = true;
        base.Dispose(disposing);
    }
}

/// <summary>
/// Represents the response from a RESTful service, including status code, headers, result, exception, version, and
/// reason phrase.
/// </summary>
public class RestResponse : RestResponseAbstract, IEquatable<RestResponse>
{
    /// <summary>
    /// Compares the current instance with another object for equality based on various properties.
    /// </summary>
    /// <param name="other">The object to compare against, which should be of the same type.</param>
    /// <returns>Returns true if the objects are considered equal; otherwise, false.</returns>
    public bool Equals(RestResponse? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return StatusCode == other.StatusCode &&
               Headers.Equals(other.Headers) &&
               Equals(Result, other.Result) &&
               Equals(Exception, other.Exception) &&
               Version.Equals(other.Version) &&
               string.Equals(ReasonPhrase, other.ReasonPhrase, StringComparison.Ordinal);
    }

    /// <summary>
    /// Compares the current instance with another object for equality.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns>True if the objects are considered equal; otherwise, false.</returns>
    public override bool Equals(object? obj) => Equals(obj as RestResponse);

    /// <summary>
    /// Generates a hash code for the current instance based on its properties such as StatusCode, Headers, Result,
    /// Exception, Version, and ReasonPhrase.
    /// </summary>
    /// <returns>Returns an integer that represents the hash code of the instance.</returns>
    public override int GetHashCode() => HashCode.Combine(
            StatusCode,
            Headers,
            Result,
            Exception,
            Version,
            ReasonPhrase);
}

/// <summary>
/// Represents a response from a REST operation, encapsulating a result and potential exceptions.
/// </summary>
/// <typeparam name="TResult">This type parameter defines the expected type of the result returned from the REST operation.</typeparam>
public class RestResponse<TResult> : RestResponseAbstract, IEquatable<RestResponse<TResult>>
{
    /// <summary>
    /// Gets the result of type TResult or returns the default value if not available. Allows setting the result during
    /// initialization.
    /// </summary>
    public new required TResult? Result
    {
        get => base.Result is TResult value ? value : default;
        init => base.Result = value;
    }

    /// <summary>
    /// Gets or sets the Exception property from the base class. Allows for initialization of the Exception value.
    /// </summary>
    public new Exception? Exception
    {
        get => base.Exception;
        init => base.Exception = value;
    }

    /// <summary>
    /// Indicates whether the operation was successful. When true, the Result property is guaranteed to be non-null.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Result))]
    [MemberNotNullWhen(false, nameof(Exception))]
    public new bool IsSuccess => base.IsSuccess;

    /// <summary>
    /// Indicates whether the operation has failed. Returns true if there is an exception, false if there is a result.
    /// </summary>
    [MemberNotNullWhen(false, nameof(Result))]
    [MemberNotNullWhen(true, nameof(Exception))]
    public new bool IsFailure => base.IsFailure;

    /// <summary>
    /// Compares the current instance with another object for equality.
    /// </summary>
    /// <param name="other">The object to compare with the current instance.</param>
    /// <returns>True if the objects are considered equal; otherwise, false.</returns>
    public bool Equals(RestResponse<TResult>? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return base.Equals(other) && Equals(Result, other.Result);
    }

    /// <summary>
    /// Compares the current instance with another object for equality.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns>Returns true if the objects are considered equal; otherwise, false.</returns>
    public override bool Equals(object? obj) => Equals(obj as RestResponse<TResult>);

    /// <summary>
    /// Generates a hash code for the current instance by combining the base hash code with the hash code of the Result
    /// property.
    /// </summary>
    /// <returns>Returns an integer that represents the hash code of the object.</returns>
    public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Result);

    /// <summary>
    /// Converts the current object into a RestResponse instance with its properties populated.
    /// </summary>
    /// <param name="response">The response to be converted.</param>
    public static implicit operator RestResponse(RestResponse<TResult> response) => response.ToRestResponse();

    /// <summary>
    /// Converts a RestResponse to a RestResponse of a specified result type, preserving relevant properties.
    /// </summary>
    /// <param name="response">The original response is used to extract and convert its properties into a new response object.</param>
    public static implicit operator RestResponse<TResult>(RestResponse response) =>
        new()
        {
            StatusCode = response.StatusCode,
            Headers = response.Headers,
            Result = response.Result is TResult result ? result : default,
            Exception = response.Exception,
            Version = response.Version,
            ReasonPhrase = response.ReasonPhrase
        };

    /// <summary>
    /// Converts the current object into a RestResponse instance with its properties populated.
    /// </summary>
    /// <returns>Returns a new RestResponse object containing the status code, headers, result, exception, version, and reason
    /// phrase.</returns>
    public RestResponse ToRestResponse() =>
        new()
        {
            StatusCode = StatusCode,
            Headers = Headers,
            Result = Result,
            Exception = Exception,
            Version = Version,
            ReasonPhrase = ReasonPhrase
        };
}