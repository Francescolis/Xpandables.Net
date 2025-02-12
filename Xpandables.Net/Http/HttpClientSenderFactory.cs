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

using Microsoft.Extensions.Options;

using Xpandables.Net.Http.Interfaces;
using Xpandables.Net.Http.RequestBuilders;
using Xpandables.Net.Http.ResponseBuilders;

namespace Xpandables.Net.Http;

/// <summary>
/// Factory class for creating HTTP client messages.
/// </summary>
public sealed class HttpClientSenderFactory(IOptions<HttpClientOptions> options) :
    IHttpClientSenderFactory
{
    private readonly HttpClientOptions _options = options.Value;

    /// <inheritdoc/>
    public Task<HttpRequestMessage> BuildRequestAsync(
        IHttpClientRequest request,
        CancellationToken cancellationToken = default)
    {
        HttpClientAttribute attribute =
            _options.GetRequestOptions(request);

        List<IHttpClientRequestBuilder> builders =
            [.. _options.PeekRequestBuilders(request.GetType())];

        HttpClientRequestContext context = new()
        {
            Attribute = attribute,
            Message = new HttpRequestMessage(),
            Request = request,
            SerializerOptions = _options.SerializerOptions
        };

        foreach (IHttpClientRequestBuilder builder in
            builders.OrderBy(o => o.Order))
        {
            builder.Build(context);
        }

        return Task.FromResult(context.Message);
    }

    /// <inheritdoc/>
    public async Task<TResponse> BuildResponseAsync<TResponse>(
        HttpResponseMessage response,
        CancellationToken cancellationToken = default)
        where TResponse : HttpClientResponse
    {
        ArgumentNullException.ThrowIfNull(response);

        IHttpClientResponseBuilder builder =
            _options.PeekResponseBuilder<TResponse>(response.StatusCode);

        HttpClientResponseContext context = new()
        {
            Message = response,
            SerializerOptions = _options.SerializerOptions
        };

        return await builder
            .BuildAsync<TResponse>(context, cancellationToken)
            .ConfigureAwait(false);
    }
}
