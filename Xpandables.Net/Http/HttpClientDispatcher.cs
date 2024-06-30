
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
using System.Net;

namespace Xpandables.Net.Http;

/// <summary>
/// Default implementation of the <see cref="IHttpClientDispatcher"/> interface.
/// </summary>
public sealed class HttpClientDispatcherDefault(
    HttpClient httpClient,
    IHttpClientDispatcherFactory dispatcherFactory)
    : HttpClientDispatcher(httpClient, dispatcherFactory)
{
}

/// <summary>
/// This helper class allows the application 
/// author to implement the <see cref="IHttpClientDispatcher"/> interface.
/// </summary>
///<inheritdoc/>
public abstract class HttpClientDispatcher(
    HttpClient httpClient,
    IHttpClientDispatcherFactory dispatcherFactory)
    : Disposable, IHttpClientDispatcher
{
    ///<inheritdoc/>
    public HttpClient HttpClient => httpClient;

    ///<inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            httpClient.Dispose();
        }

        base.Dispose(disposing);
    }

    ///<inheritdoc/>
    public virtual async Task<HttpClientResponse<IAsyncEnumerable<TResult>>>
        SendAsync<TResult>(
        IHttpClientAsyncRequest<TResult> request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using HttpRequestMessage httpRequest = await dispatcherFactory
                .BuildRequestAsync(
                    request,
                    cancellationToken)
                .ConfigureAwait(false);

            // Due to the fact that the result is an IAsyncEnumerable,
            // the response can not be disposed before.
            HttpResponseMessage response = await HttpClient
                .SendAsync(
                    httpRequest,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken)
                .ConfigureAwait(false);

            return await dispatcherFactory
                .BuildResponseResultAsync<TResult>(
                    response,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception)
            when (exception is not ArgumentNullException)
        {
            return new HttpClientResponse<IAsyncEnumerable<TResult>>(
                HttpStatusCode.BadRequest,
                HttpClient.DefaultRequestHeaders.ReadHttpHeaders(),
                default,
                HttpClient.DefaultRequestVersion,
                default,
                new HttpClientException(exception.Message, exception));
        }
    }

    ///<inheritdoc/>
    public virtual async Task<HttpClientResponse> SendAsync(
        IHttpClientRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using HttpRequestMessage httpRequest = await dispatcherFactory
                .BuildRequestAsync(
                    request,
                    cancellationToken)
                .ConfigureAwait(false);

            using HttpResponseMessage response = await HttpClient
                .SendAsync(httpRequest, cancellationToken)
                .ConfigureAwait(false);

            return await dispatcherFactory
                .BuildResponseAsync(response, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception)
            when (exception is not ArgumentNullException)
        {
            return new HttpClientResponse(
                HttpStatusCode.BadRequest,
                httpClient.DefaultRequestHeaders.ReadHttpHeaders(),
                httpClient.DefaultRequestVersion,
                default,
                new HttpClientException(exception.Message, exception));
        }
    }

    ///<inheritdoc/>
    public virtual async Task<HttpClientResponse<TResult>> SendAsync<TResult>(
        IHttpClientRequest<TResult> request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using HttpRequestMessage httpRequest = await dispatcherFactory
                .BuildRequestAsync(
                    request,
                    cancellationToken)
                .ConfigureAwait(false);

            using HttpResponseMessage response = await HttpClient
                .SendAsync(
                    httpRequest,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken)
                .ConfigureAwait(false);

            return await dispatcherFactory
                .BuildResponseAsync<TResult>(response, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception)
            when (exception is not ArgumentNullException)
        {
            return new HttpClientResponse<TResult>(
                HttpStatusCode.BadRequest,
                HttpClient.DefaultRequestHeaders.ReadHttpHeaders(),
                default,
                HttpClient.DefaultRequestVersion,
                default,
                new HttpClientException(exception.Message, exception));
        }
    }
}