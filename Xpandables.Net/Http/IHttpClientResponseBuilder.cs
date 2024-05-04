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
using System.Text.Json;

namespace Xpandables.Net.Http;

/// <summary>
/// Defines the base contract for building the response from
/// the <see cref="HttpResponseMessage"/>.
/// </summary>
public interface IHttpClientResponseBuilderBase
{
    /// <summary>
    /// Gets the response content type result being built by the current 
    /// builder instance.
    /// </summary>
    Type? Type { get; }

    /// <summary>
    /// When overridden in a derived class, determines whether the builder
    /// instance can build the response for the specified status code.
    /// </summary>
    /// <param name="targetStatusCode">The status code of the response.</param>
    /// <returns><see langword="true"/> if the instance can build the
    /// specified request; otherwise, <see langword="false"/>.</returns>
    bool CanBuild(HttpStatusCode targetStatusCode);
}

/// <summary>
/// Defines the base contract for building the response from
/// the <see cref="HttpResponseMessage"/>.
/// </summary>
public interface IHttpClientResponseBuilder : IHttpClientResponseBuilderBase
{
    /// <summary>
    /// Builds a response of <see cref="HttpClientResponse"/> type.
    /// </summary>
    /// <param name="httpResponse">The response message to act on.</param>
    /// <param name="options">The serialization options to use.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The built response.</returns>
    ValueTask<HttpClientResponse> BuildAsync(
        HttpResponseMessage httpResponse,
        JsonSerializerOptions options,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Builds the response <see cref="HttpClientResponse{TResult}"/> 
/// from the <see cref="HttpRequestMessage"/>.
/// </summary>
/// <typeparam name="TResult">The type of the result to build.</typeparam>
public interface IHttpClientResponseResultBuilder<TResult> :
    IHttpClientResponseBuilderBase
{
    /// <summary>
    /// Builds a response of <see cref="HttpClientResponse{TResult}"/>> type.
    /// </summary>
    /// <param name="httpResponse">The response message to act on.</param>
    /// <param name="options">The serialization options to use.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The built response.</returns>
    ValueTask<HttpClientResponse<TResult>> BuildAsync(
        HttpResponseMessage httpResponse,
        JsonSerializerOptions options,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Builds the response <see cref="HttpClientResponse{TResult}"/> 
/// from the <see cref="HttpRequestMessage"/>.
/// </summary>
/// <typeparam name="TResult">The type of the result to build.</typeparam>
public interface IHttpClientResponseIAsyncResultBuilder<TResult> :
    IHttpClientResponseBuilderBase
{
    /// <summary>
    /// Builds a response of <see cref="HttpClientResponse{TResult}"/>> type.
    /// </summary>
    /// <param name="httpResponse">The response message to act on.</param>
    /// <param name="options">The serialization options to use.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The built response.</returns>
    ValueTask<HttpClientResponse<IAsyncEnumerable<TResult>>> BuildAsync(
        HttpResponseMessage httpResponse,
        JsonSerializerOptions options,
        CancellationToken cancellationToken = default);
}