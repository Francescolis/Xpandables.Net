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
/// <param name="factory">The factory to create HTTP client messages.</param>
/// <param name="httpClient">The HTTP client instance.</param>
public abstract class HttpClientDispatcher(
    IHttpClientMessageFactory factory,
    HttpClient httpClient) : Disposable, IHttpClientDispatcher
{
    private readonly IHttpClientMessageFactory _factory = factory;

    ///<inheritdoc/>
    public HttpClient HttpClient => httpClient;

    ///<inheritdoc/>
    public async Task<HttpClientResponse> SendAsync(
        IHttpClientRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using HttpRequestMessage httpRequest = await _factory
                .BuildRequestAsync(
                    request,
                    cancellationToken)
                .ConfigureAwait(false);

            using HttpResponseMessage response = await HttpClient
                .SendAsync(httpRequest, cancellationToken)
                .ConfigureAwait(false);

            return await _factory
                .BuildResponseAsync<HttpClientResponse>(
                    response,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception)
            when (exception is not ArgumentNullException)
        {
            return new HttpClientResponse
            {
                Exception = new HttpClientException(exception.Message, exception),
                Headers = [], // TODO: Add headers
                StatusCode = HttpStatusCode.BadRequest,
                Version = HttpClient.DefaultRequestVersion
            };
        }
    }
}
