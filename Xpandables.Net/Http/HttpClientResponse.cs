
/*******************************************************************************
 * Copyright (C) 2023 Francis-Black EWANE
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

using Xpandables.Net.Primitives;

namespace Xpandables.Net.Http;

/// <summary>
/// Represents an HTTP client response. Implements <see cref="IDisposable"/> 
/// and <see cref="IAsyncDisposable"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of <see cref="HttpClientResponse"/> class 
/// with exception and status code.
/// </remarks>
/// <param name="statusCode">The status code of the response.</param>
/// <param name="headers">All the headers of the response.</param>
/// <param name="version">The response version.</param>
/// <param name="reasonPhrase">the reason phrase which typically is s
/// ent by servers together with the status code.</param>
/// <param name="exception">The handled exception of the response.</param>
/// <exception cref="ArgumentNullException">The 
/// <paramref name="exception"/> is null.</exception>
public class HttpClientResponse(
    HttpStatusCode statusCode,
    NameValueCollection headers,
    Version? version = default,
    string? reasonPhrase = default,
    HttpClientException? exception = default) : Disposable
{

    /// <summary>
    /// Gets the <see cref="HttpClientException"/> that holds the 
    /// handled exception.
    /// </summary>
    [AllowNull]
    public HttpClientException Exception { get; } = exception;

    /// <summary>
    /// Determines whether or not the instance contains an exception.
    /// </summary>
    /// <param name="exception">The target exception if true.</param>
    /// <returns>returns <see langword="true"/>, 
    /// otherwise <see langword="false"/>.</returns>
    public bool IsAnException([NotNullWhen(true)] out HttpClientException? exception)
    {
        exception = Exception;
        return exception is not null;
    }

    /// <summary>
    /// Gets the HTTP response version.
    /// </summary>
    public Version? Version { get; } = version;

    /// <summary>
    /// Gets the reason phrase which typically is sent 
    /// by servers together with the status code.
    /// </summary>
    public string? ReasonPhrase { get; } = reasonPhrase;

    /// <summary>
    /// Gets all headers of the HTTP response.
    /// </summary>
    public NameValueCollection Headers { get; } = headers;

    /// <summary>
    /// Gets the response status code.
    /// </summary>
    public HttpStatusCode StatusCode { get; } = statusCode;

    /// <summary>
    /// Determines whether or not the response status is valid.
    /// Returns <see langword="true"/> if so, otherwise <see langword="false"/>.
    /// </summary>
    [MemberNotNullWhen(false, nameof(Exception))]
    public bool IsValid => StatusCode.IsSuccessStatusCode();

    /// <summary>
    /// Determines whether or not the response status is not valid.
    /// Returns <see langword="true"/> if so, otherwise <see langword="false"/>.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Exception))]
    public bool IsNotValid => StatusCode.IsFailureStatusCode();

    internal virtual bool IsGeneric => false;
}

/// <summary>
///  Represents an HTTP Rest client response of a specific type result. 
///  Implements <see cref="IDisposable"/> and <see cref="IAsyncDisposable"/>.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
/// <remarks>
/// Initializes a new instance of <see cref="HttpClientResponse{TResult}"/> 
/// class with exception and status code.
/// </remarks>
/// <param name="statusCode">The status code of the response.</param>
/// <param name="headers">All the headers of the response.</param>
/// <param name="result">The optional result value.</param>
/// <param name="version">The response version.</param>
/// <param name="reasonPhrase">the reason phrase which typically is sent by 
/// servers together with the status code.</param>
/// <param name="exception">The handled exception of the response.</param>
/// <exception cref="ArgumentNullException">The 
/// <paramref name="exception"/> is null.</exception>
public class HttpClientResponse<TResult>(
    HttpStatusCode statusCode,
    NameValueCollection headers,
    TResult? result = default,
    Version? version = default,
    string? reasonPhrase = default,
    HttpClientException? exception = default)
    : HttpClientResponse(statusCode, headers, version, reasonPhrase, exception)
{

    /// <summary>
    /// Gets the HTTP response content.
    /// </summary>
    [AllowNull]
    public TResult Result { get; } = result;

    private bool _isDisposed;

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

    internal override bool IsGeneric => false;
}

/// <summary>
/// Provides with extension methods for <see cref="HttpClientResponse"/>.
/// </summary>
public static class HttpClientResponseExtensions
{
    /// <summary>
    /// Determines whether the status code of the current 
    /// <see cref="HttpClientResponse"/> instance is <see cref="HttpStatusCode.OK"/>.
    /// </summary>
    /// <param name="this">The current <see cref="HttpClientResponse"/>
    /// instance.</param>
    /// <returns><see langword="true"/> if status code is 
    /// <see cref="HttpStatusCode.OK"/>, otherwise <see langword="false"/>.</returns>
    public static bool IsStatusCodeOK(this HttpClientResponse @this)
        => @this.IsHttpStatusCode(HttpStatusCode.OK);

    /// <summary>
    /// Determines whether the status code of the current 
    /// <see cref="HttpClientResponse"/> instance is <see cref="HttpStatusCode.Created"/>.
    /// </summary>
    /// <param name="this">The current <see cref="HttpClientResponse"/> 
    /// instance.</param>
    /// <returns><see langword="true"/> if status code is 
    /// <see cref="HttpStatusCode.Created"/>, otherwise <see langword="false"/>.</returns>
    public static bool IsStatusCodeCreated(this HttpClientResponse @this)
        => @this.IsHttpStatusCode(HttpStatusCode.Created);

    /// <summary>
    /// Determines whether the status code of the current 
    /// <see cref="HttpClientResponse"/> instance is <see cref="HttpStatusCode.Accepted"/>.
    /// </summary>
    /// <param name="this">The current <see cref="HttpClientResponse"/> 
    /// instance.</param>
    /// <returns><see langword="true"/> if status code is 
    /// <see cref="HttpStatusCode.Accepted"/>, otherwise <see langword="false"/>.</returns>
    public static bool IsStatusCodeAccepted(this HttpClientResponse @this)
        => @this.IsHttpStatusCode(HttpStatusCode.Accepted);

    /// <summary>
    /// Determines whether the status code of the current 
    /// <see cref="HttpClientResponse"/> instance matches the specified one.
    /// </summary>
    /// <param name="this">The current <see cref="HttpClientResponse"/> 
    /// instance.</param>
    /// <param name="statusCode">The status code to match.</param>
    /// <returns><see langword="true"/> if instance status code matches 
    /// the expected value, otherwise <see langword="false"/>.</returns>
    public static bool IsHttpStatusCode(
        this HttpClientResponse @this,
        HttpStatusCode statusCode)
    {
        ArgumentNullException.ThrowIfNull(@this);
        return @this.StatusCode == statusCode;
    }
}
