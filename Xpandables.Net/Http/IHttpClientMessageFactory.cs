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
using Xpandables.Net.Http.Interfaces;

namespace Xpandables.Net.Http;
/// <summary>
/// Defines methods to build HTTP request and response messages.
/// </summary>
public interface IHttpClientMessageFactory
{
    /// <summary>
    /// Builds an HTTP request message asynchronously.
    /// </summary>
    /// <param name="request">The HTTP client request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. 
    /// The task result contains the HTTP request message.</returns>
    Task<HttpRequestMessage> BuildRequestAsync(
        IHttpClientRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Builds an HTTP response message asynchronously.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="response">The HTTP response message.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. 
    /// The task result contains the response of type <typeparamref name="TResponse"/>.</returns>
    Task<TResponse> BuildResponseAsync<TResponse>(
        HttpResponseMessage response,
        CancellationToken cancellationToken = default)
        where TResponse : HttpClientResponse;
}
