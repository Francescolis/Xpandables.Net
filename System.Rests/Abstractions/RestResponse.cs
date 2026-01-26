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
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace System.Rests.Abstractions;

/// <summary>
/// Provides an abstract base class for HTTP response representations, encapsulating status code, headers, result data,
/// exception information, and versioning details for RESTful operations.
/// </summary>
/// <remarks>This class is intended to be inherited by concrete response types that represent the outcome of HTTP
/// requests in a RESTful context. It exposes properties for accessing the HTTP status code, headers, result payload,
/// and any exception that may have occurred during the operation. The class also includes properties to determine
/// whether the response indicates success or failure, and supports proper resource cleanup through the IDisposable
/// pattern. Instances of this class are typically created and initialized by HTTP client implementations or REST
/// frameworks.</remarks>
[SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "<Pending>")]
[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class Response : IDisposable
{
    /// <summary>
    /// Gets or sets a value indicating whether this instance is disposed.
    /// </summary>
    /// <value>
    ///  <c>true</c> if this instance is disposed; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>Default initialization for 
    /// a <see cref="bool"/> is <c>false</c>.</remarks>
    protected bool IsDisposed { get; set; }

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
    // ReSharper disable once MemberCanBeProtected.Global
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
    /// Indicates whether the HTTP status code of the execution result signifies a successful outcome.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Result))]
    public virtual bool IsSuccess => StatusCode.IsSuccess;

    /// <summary>
    /// Indicates whether an operation has failed. It returns true if the operation was not successful.
    /// </summary>
    public virtual bool IsFailure => StatusCode.IsFailure;

    /// <summary>
    /// Indicates whether the response is generic (i.e., not tied to a specific type).
    /// </summary>
    public virtual bool IsGeneric => false;

    ///<inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the object and optionally releases the managed resources.
    /// </summary>
    /// <remarks>This method is called by both the public Dispose method and the finalizer. When disposing is
    /// true, this method disposes managed resources in addition to unmanaged resources. Override this method to release
    /// resources specific to the derived class.</remarks>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (IsDisposed)
        {
            return;
        }

        if (disposing)
        {
            (Result as IDisposable)?.Dispose();
        }

        // Dispose has been called.
        IsDisposed = true;
    }
}


/// <summary>
/// Represents a non-generic response from a RESTful operation, containing status information, headers, and optional
/// error details.
/// </summary>
/// <remarks>Use this type when the response does not include a strongly-typed result value. To convert to a
/// generic response type, use the provided conversion methods or implicit operator. This class provides properties to
/// inspect the outcome of the REST operation, including success or failure status and any associated
/// exception.</remarks>
public sealed class RestResponse : Response
{
    /// <summary>
    /// Represents a static, read-only instance of an empty HTTP response with a status code of NoContent (204).
    /// </summary>
    /// <remarks>Use this instance as a default response when no content is available to return. The response
    /// has an empty collection of headers and uses HTTP version 1.1.</remarks>
    public static readonly RestResponse Empty = new()
    {
        StatusCode = HttpStatusCode.NoContent,
        Headers = [],
        Version = new Version(1, 1)
    };

    /// <inheritdoc/>
    public sealed override bool IsGeneric => false;

    /// <inheritdoc/>
    [MemberNotNullWhen(false, nameof(Exception))]
    public sealed override bool IsSuccess => StatusCode.IsSuccess;

    /// <inheritdoc/>
    [MemberNotNullWhen(true, nameof(Exception))]
    public sealed override bool IsFailure => StatusCode.IsFailure;

    /// <summary>
    /// Allows for initialization of the Exception value.
    /// </summary>
    public new Exception? Exception
    {
        get => base.Exception;
        init => base.Exception = value;
    }

    /// <summary>
    /// Converts a non-generic RestResponse instance to a generic <see cref="RestResponse{TResult}"/> instance.
    /// </summary>
    /// <remarks>This implicit conversion allows seamless use of RestResponse where a <see cref="RestResponse{TResult}"/> is
    /// expected. The content and status information from the original response are preserved in the resulting generic
    /// instance.</remarks>
    /// <param name="response">The RestResponse instance to convert. Cannot be null.</param>
    public static implicit operator RestResponse<object>(RestResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);
        return response.ToRestResponse();
    }

    /// <summary>
    /// Creates a new <see cref="RestResponse{TResult}"/> instance that represents the current response state.
    /// </summary>
    /// <returns>A <see cref="RestResponse{TResult}"/> containing the status code, headers, result, exception, version, and reason
    /// phrase from the current response.</returns>
    public RestResponse<TResult> ToRestResponse<TResult>() =>
       new()
       {
           StatusCode = StatusCode,
           Headers = Headers,
           Result = Result is TResult result ? result : default,
           Exception = Exception,
           Version = Version,
           ReasonPhrase = ReasonPhrase
       };

    /// <summary>
    /// Creates a new <see cref="RestResponse{Object}"/> instance that represents the current response state.
    /// </summary>
    /// <returns>A <see cref="RestResponse{Object}"/> containing the status code, headers, result, exception, version, and reason
    /// phrase from the current response.</returns>
    public RestResponse<object> ToRestResponse() =>
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

/// <summary>
/// Represents a REST response that contains a strongly typed result value and associated response metadata.
/// </summary>
/// <remarks>Use this class to work with REST responses where the result is expected to be of a specific type. The
/// generic parameter enables type-safe access to the response content, while also providing access to status code,
/// headers, exceptions, and other response details. This class is typically used in scenarios where deserialization of
/// the response body to a specific type is required.</remarks>
/// <typeparam name="TResult">The type of the result value returned by the REST response.</typeparam>
public sealed class RestResponse<TResult> : Response
{
    /// <inheritdoc/>
    public sealed override bool IsGeneric => true;

    /// <inheritdoc/>
    [MemberNotNullWhen(false, nameof(Exception))]
    [MemberNotNullWhen(true, nameof(Result))]
    public sealed override bool IsSuccess => StatusCode.IsSuccess;

    /// <inheritdoc/>
    [MemberNotNullWhen(true, nameof(Exception))]
    [MemberNotNullWhen(false, nameof(Result))]
    public sealed override bool IsFailure => StatusCode.IsFailure;

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
    /// Converts the current object into a RestResponse instance with its properties populated.
    /// </summary>
    /// <param name="response">The response to be converted.</param>
    public static implicit operator RestResponse(RestResponse<TResult> response)
    {
        ArgumentNullException.ThrowIfNull(response);
        return response.ToRestResponse();
    }

    /// <summary>
    /// Converts a non-generic RestResponse to a generic <see cref="RestResponse{TResult}"/> instance.
    /// </summary>
    /// <remarks>If the Result property of the input response is not of type TResult, the Result property of
    /// the returned <see cref="RestResponse{TResult}"/> will be set to the default value of TResult.</remarks>
    /// <param name="response">The RestResponse to convert. Cannot be null.</param>
    public static implicit operator RestResponse<TResult>(RestResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);
        return new()
        {
            StatusCode = response.StatusCode,
            Headers = response.Headers,
            Result = response.Result is TResult result ? result : default,
            Exception = response.Exception,
            Version = response.Version,
            ReasonPhrase = response.ReasonPhrase
        };
    }

    /// <summary>
    /// Converts a generic <see cref="RestResponse{TResult}"/> to a <see cref="RestResponse"/> with the specified result
    /// type.
    /// </summary>
    /// <returns>A <see cref="RestResponse"/> containing the status code, headers, exception, version, and reason phrase
    /// from the original response.</returns>
    public RestResponse ToRestResponse()
    {
        return new()
        {
            StatusCode = StatusCode,
            Headers = Headers,
            Result = Result,
            Exception = Exception,
            Version = Version,
            ReasonPhrase = ReasonPhrase
        };
    }
}
