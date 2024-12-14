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
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace Xpandables.Net.Http;
/// <summary>
/// Represents a response from an HTTP client.
/// </summary>
public class HttpClientResponse : Disposable, IEquatable<HttpClientResponse>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HttpClientResponse"/> class.
    /// </summary>
    public HttpClientResponse() { }

    /// <summary>  
    /// Initializes a new instance of the <see cref="HttpClientResponse"/> 
    /// class with the specified parameters.  
    /// </summary>  
    /// <param name="statusCode">The status code of the HTTP response.</param>  
    /// <param name="headers">The headers of the HTTP response.</param>  
    /// <param name="result">The result of the HTTP response.</param>  
    /// <param name="exception">The exception associated with the HTTP 
    /// response, if any.</param>  
    /// <param name="version">The HTTP protocol version used by the response.</param>  
    /// <param name="reasonPhrase">The reason phrase which typically is sent 
    /// by servers together with the status code.</param>  
    [SetsRequiredMembers]
    public HttpClientResponse(
       HttpStatusCode statusCode,
       NameValueCollection headers,
       object? result,
       HttpClientException? exception,
       Version version,
       string? reasonPhrase)
    {
        StatusCode = statusCode;
        Headers = headers;
        Result = result;
        Exception = exception;
        Version = version;
        ReasonPhrase = reasonPhrase;
    }

    /// <summary>  
    /// Gets the status code of the HTTP response.  
    /// </summary>  
    public required HttpStatusCode StatusCode { get; init; }
    /// <summary>  
    /// Gets the headers of the HTTP response.  
    /// </summary>  
    public required NameValueCollection Headers { get; init; }
    /// <summary>
    /// Gets the result of the HTTP response.
    /// </summary>
    [MaybeNull, AllowNull]
    public object Result { get; init; }
    /// <summary>
    /// Gets the exception associated with the HTTP response, if any.
    /// </summary>
    public HttpClientException? Exception { get; init; }
    /// <summary>  
    /// Gets the HTTP protocol version used by the response.  
    /// </summary>  
    public required Version Version { get; init; }
    /// <summary>  
    /// Gets the reason phrase which typically is sent by servers 
    /// together with the status code.  
    /// </summary>  
    public string? ReasonPhrase { get; init; }

    /// <summary>  
    /// Gets a value indicating whether the HTTP response is valid 
    /// (status code is between 200 and 299).  
    /// </summary>  
    [MemberNotNullWhen(false, nameof(Exception))]
    public bool IsSuccessStatusCode => (int)StatusCode is >= 200 and <= 299;

    /// <summary>  
    /// Gets a value indicating whether the HTTP response is not valid 
    /// (status code is not between 200 and 299).  
    /// </summary>  
    [MemberNotNullWhen(true, nameof(Exception))]
    public bool IsFailureStatusCode => !IsSuccessStatusCode;

    /// <inheritdoc/>
    public bool Equals(HttpClientResponse? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return StatusCode == other.StatusCode &&
               Equals(Headers, other.Headers) &&
               Equals(Result, other.Result) &&
               Equals(Exception, other.Exception) &&
               Equals(Version, other.Version) &&
               ReasonPhrase == other.ReasonPhrase;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) =>
        Equals(obj as HttpClientResponse);

    /// <inheritdoc/>
    public override int GetHashCode() =>
        HashCode.Combine(StatusCode,
                         Headers,
                         Result,
                         Exception,
                         Version,
                         ReasonPhrase);

    ///<inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            _isDisposed = true;

            if (disposing)
            {
                (Result as IDisposable)?.Dispose();
            }

            base.Dispose(disposing);
        }
    }

    private bool _isDisposed;

    ///<inheritdoc/>
    protected override async ValueTask DisposeAsync(bool disposing)
    {
        if (!_isDisposed)
        {
            _isDisposed = true;

            if (Result is IAsyncDisposable disposable)
            {
                await disposable.DisposeAsync().ConfigureAwait(false);
            }
            else
            {
                (Result as IDisposable)?.Dispose();
            }

            await base.DisposeAsync(disposing).ConfigureAwait(false);
        }
    }

    internal virtual bool IsGeneric => false;
}

/// <summary>
/// Represents a response from an HTTP client with a specific result type.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
public class HttpClientResponse<TResult> :
    HttpClientResponse, IEquatable<HttpClientResponse<TResult>>
{
    /// <summary>  
    /// Initializes a new instance of the 
    /// <see cref="HttpClientResponse{TResult}"/> class.  
    /// </summary>  
    public HttpClientResponse() { }

    /// <summary>  
    /// Initializes a new instance of the <see cref="HttpClientResponse{TResult}"/> 
    /// class with the specified parameters.  
    /// </summary>  
    /// <param name="statusCode">The status code of the HTTP response.</param>  
    /// <param name="headers">The headers of the HTTP response.</param>  
    /// <param name="result">The result of the HTTP response.</param>  
    /// <param name="exception">The exception associated with the HTTP 
    /// response, if any.</param>  
    /// <param name="version">The HTTP protocol version used by the response.</param>  
    /// <param name="reasonPhrase">The reason phrase which typically is sent 
    /// by servers together with the status code.</param>  
    [SetsRequiredMembers]
    public HttpClientResponse(
        HttpStatusCode statusCode,
        NameValueCollection headers,
        TResult? result,
        HttpClientException? exception,
        Version version,
        string? reasonPhrase)
        : base(statusCode, headers, result, exception, version, reasonPhrase) { }

    /// <summary>
    /// Gets the result of the HTTP response.
    /// </summary>
    [MaybeNull, AllowNull]
    public new TResult Result
    {
        get => base.Result is TResult value ? value : default;
        init => base.Result = value;
    }

    /// <summary>  
    /// Gets a value indicating whether the HTTP response is valid 
    /// (status code is between 200 and 299).  
    /// </summary>  
    [MemberNotNullWhen(true, nameof(Result))]
    public new bool IsSuccessStatusCode => base.IsSuccessStatusCode;

    /// <summary>  
    /// Gets a value indicating whether the HTTP response is not valid 
    /// (status code is not between 200 and 299).  
    /// </summary>  
    [MemberNotNullWhen(true, nameof(Result))]
    public new bool IsFailureStatusCode => base.IsFailureStatusCode;

    internal override bool IsGeneric => true;

    /// <inheritdoc/>
    public bool Equals(HttpClientResponse<TResult>? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return base.Equals(other)
            && EqualityComparer<TResult>.Default.Equals(Result, other.Result);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) =>
        Equals(obj as HttpClientResponse<TResult>);

    /// <inheritdoc/>
    public override int GetHashCode() =>
        HashCode.Combine(base.GetHashCode(), Result);
}