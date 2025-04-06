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
/// Handles sending HTTP requests and receiving responses asynchronously.
/// </summary>
/// <param name="requestFactory">Used to create HTTP requests based on the provided request details.</param>
/// <param name="responseFactory">Utilized to construct response objects from the HTTP responses received.</param>
/// <param name="httpClient">Acts as the underlying client for sending HTTP requests and receiving responses.</param>
public sealed class RestClient(
    IRestRequestFactory requestFactory,
    IRestResponseFactory responseFactory,
    HttpClient httpClient) : Disposable, IRestClient
{

    /// <inheritdoc/>
    public HttpClient HttpClient => httpClient;

    /// <inheritdoc/>
    public async Task<RestResponse> SendAsync(IRestRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            using HttpRequestMessage httpRequest = await requestFactory
                .BuildRequestAsync(request, cancellationToken)
                .ConfigureAwait(false);

            using HttpResponseMessage response = await httpClient
                .SendAsync(httpRequest, cancellationToken)
                .ConfigureAwait(false);

            return await responseFactory
                .BuildResponseAsync<RestResponse>(response, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception)
            when (exception is not ArgumentNullException)
        {
            return new RestResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Result = default,
                Version = httpClient.DefaultRequestVersion,
                Headers = HttpClient.DefaultRequestHeaders.ToElementCollection(),
                Exception = exception
            };
        }
    }

    /// <inheritdoc/>
    public async Task<RestResponse<TResult>> SendAsync<TResult>(
        IRestRequest<TResult> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            using HttpRequestMessage httpRequest = await requestFactory
                .BuildRequestAsync(request, cancellationToken)
                .ConfigureAwait(false);

            using HttpResponseMessage response = await httpClient
                .SendAsync(httpRequest, cancellationToken)
                .ConfigureAwait(false);

            return await responseFactory
                .BuildResponseAsync<RestResponse<TResult>>(response, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception)
            when (exception is not ArgumentNullException)
        {
            return new RestResponse<TResult>
            {
                StatusCode = HttpStatusCode.BadRequest,
                Result = default,
                Version = httpClient.DefaultRequestVersion,
                Headers = HttpClient.DefaultRequestHeaders.ToElementCollection(),
                Exception = exception
            };
        }
    }

    /// <inheritdoc/>
    public async Task<RestResponse<IAsyncEnumerable<TResult>>> SendAsync<TResult>(
        IRestStreamRequest<TResult> request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            using HttpRequestMessage httpRequest = await requestFactory
                .BuildRequestAsync(request, cancellationToken)
                .ConfigureAwait(false);

            // Due to the fact that the result is an IAsyncEnumerable,
            // the response can not be disposed before.
            HttpResponseMessage response = await HttpClient
                .SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);

            return await responseFactory
                .BuildResponseAsync<RestResponse<IAsyncEnumerable<TResult>>>(response, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception)
            when (exception is not ArgumentNullException)
        {
            return new RestResponse<IAsyncEnumerable<TResult>>
            {
                StatusCode = HttpStatusCode.BadRequest,
                Result = default,
                Version = httpClient.DefaultRequestVersion,
                Headers = HttpClient.DefaultRequestHeaders.ToElementCollection(),
                Exception = exception
            };
        }
    }
}
