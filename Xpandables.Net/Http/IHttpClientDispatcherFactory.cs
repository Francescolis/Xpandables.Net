﻿/*******************************************************************************
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
using Xpandables.Net.Http.Requests;

namespace Xpandables.Net.Http;

/// <summary>
/// Represents a factory that creates <see cref="HttpRequestMessage"/> 
/// and <see cref="HttpClientResponse"/> instances for 
/// <see cref="IHttpClientDispatcher"/>.
/// </summary>
public interface IHttpClientDispatcherFactory
{
    /// <summary>
    /// Gets the options of the <see cref="IHttpClientDispatcherFactory"/>.
    /// </summary>
    HttpClientOptions Options { get; }

    /// <summary>
    /// Builds a request message from the specified request object.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<HttpRequestMessage> BuildRequestAsync(
        IHttpClientRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Builds a <see cref="HttpClientResponse"/> from the 
    /// specified response.
    /// </summary>
    /// <param name="response">The response message.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<HttpClientResponse> BuildResponseAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Builds a <see cref="HttpClientResponse{TResult}"/> from the
    /// specified response.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="response">The response message.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<HttpClientResponse<TResult>> BuildResponseAsync<TResult>(
        HttpResponseMessage response,
               CancellationToken cancellationToken = default);

    /// <summary>
    /// Builds a <see cref="HttpClientResponse{TResult}"/> from the
    /// specified response.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="response">The response message.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<HttpClientResponse<IAsyncEnumerable<TResult>>>
        BuildResponseResultAsync<TResult>(
            HttpResponseMessage response,
               CancellationToken cancellationToken = default);
}
