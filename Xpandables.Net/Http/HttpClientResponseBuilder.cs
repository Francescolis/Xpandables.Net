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
/// Builds the response from the <see cref="HttpRequestMessage"/>.
/// </summary>
public abstract class HttpClientResponseBuilder
{
    /// <summary>
    /// When overridden in a derived class, determines whether the builder
    /// instance can build the response for the specified status code.
    /// </summary>
    /// <param name="statusCode">The status code of the response.</param>
    /// <param name="genericType">The type of the response.</param>
    /// <returns><see langword="true"/> if the instance can build the
    /// specified request; otherwise, <see langword="false"/>.</returns>
    public abstract bool CanBuild(
        HttpStatusCode statusCode,
        Type? genericType);

    /// <summary>
    /// Builds a response of <see cref="HttpClientResponse"/> type.
    /// </summary>
    /// <param name="httpResponse">The response message to act on.</param>
    /// <param name="options">The serialization options to use.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The built response.</returns>
    public abstract ValueTask<HttpClientResponse> BuildAsync(
        HttpResponseMessage httpResponse,
        JsonSerializerOptions options,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Builds the response from the <see cref="HttpRequestMessage"/>.
/// </summary>
public abstract class HttpClientResponseResultBuilder :
    HttpClientResponseBuilder
{
    /// <summary>
    /// Builds a response of <typeparamref name="TResult"/> type.
    /// </summary>
    /// <param name="httpResponse">The response message to act on.</param>
    /// <param name="options">The serialization options to use.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The built response.</returns>
    public abstract ValueTask<HttpClientResponse<TResult>> BuildAsync<TResult>(
        HttpResponseMessage httpResponse,
        JsonSerializerOptions options,
        CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public sealed override ValueTask<HttpClientResponse> BuildAsync(
        HttpResponseMessage httpResponse,
        JsonSerializerOptions options,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Builds the response from the <see cref="HttpRequestMessage"/> that contains
/// a result of a specific type.
/// </summary>
public abstract class HttpClientResponseIAsyncResultBuilder :
    HttpClientResponseBuilder
{
    /// <summary>
    /// Builds a response of <see cref="HttpClientResponse{TResult}"/>> type.
    /// </summary>
    /// <param name="httpResponse">The response message to act on.</param>
    /// <param name="options">The serialization options to use.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The built response.</returns>
    public abstract ValueTask<HttpClientResponse
        <IAsyncEnumerable<TResult>>> BuildAsync<TResult>(
        HttpResponseMessage httpResponse,
        JsonSerializerOptions options,
        CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public sealed override ValueTask<HttpClientResponse> BuildAsync(
        HttpResponseMessage httpResponse,
        JsonSerializerOptions options,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}