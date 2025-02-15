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
using System.Net;

namespace Xpandables.Net.Http;
/// <summary>
/// Represents an abstract base class for dispatching HTTP client requests.
/// </summary>
/// <param name="requestFactory">The request factory instance.</param>
/// <param name="responseFactory">The response factory instance.</param>
/// <param name="httpClient">The HTTP client instance.</param>
public abstract class RequestHttpSender(
    IRequestFactory requestFactory,
    IResponseFactory responseFactory,
    HttpClient httpClient) : Disposable, IRequestHttpSender
{
    ///<inheritdoc/>
    public HttpClient HttpClient => httpClient;

    ///<inheritdoc/>
    public async Task<ResponseHttp> SendAsync(
        IRequestHttp request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using HttpRequestMessage httpRequest = await requestFactory
                .BuildRequestAsync(
                    request,
                    cancellationToken)
                .ConfigureAwait(false);

            using HttpResponseMessage response = await HttpClient
                .SendAsync(httpRequest, cancellationToken)
                .ConfigureAwait(false);

            return await responseFactory
                .BuildResponseAsync<ResponseHttp>(
                    response,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception)
            when (exception is not ArgumentNullException)
        {
            return new ResponseHttp
            {
                Exception = new RequestException(exception.Message, exception),
                Headers = HttpClient.DefaultRequestHeaders.ToNameValueCollection(),
                StatusCode = HttpStatusCode.BadRequest,
                Version = HttpClient.DefaultRequestVersion
            };
        }
    }

    /// <inheritdoc/>
    public async Task<ResponseHttp<TResult>> SendAsync<TResult>(
        IRequestHttp<TResult> request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using HttpRequestMessage httpRequest = await requestFactory
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

            return await responseFactory
                .BuildResponseAsync<ResponseHttp<TResult>>(
                    response,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception)
            when (exception is not ArgumentNullException)
        {
            return new ResponseHttp<TResult>
            {
                Exception = new RequestException(exception.Message, exception),
                Headers = HttpClient.DefaultRequestHeaders.ToNameValueCollection(),
                StatusCode = HttpStatusCode.BadRequest,
                Version = HttpClient.DefaultRequestVersion
            };
        }
    }

    /// <inheritdoc/>
    public async Task<ResponseHttp<IAsyncEnumerable<TResult>>> SendAsync<TResult>(
        IRequestHttpAsync<TResult> request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using HttpRequestMessage httpRequest = await requestFactory
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

            return await responseFactory
                .BuildResponseAsync<ResponseHttp<IAsyncEnumerable<TResult>>>(
                    response,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception)
            when (exception is not ArgumentNullException)
        {
            return new ResponseHttp<IAsyncEnumerable<TResult>>
            {
                Exception = new RequestException(exception.Message, exception),
                Headers = HttpClient.DefaultRequestHeaders.ToNameValueCollection(),
                StatusCode = HttpStatusCode.BadRequest,
                Version = HttpClient.DefaultRequestVersion
            };
        }
    }
}
