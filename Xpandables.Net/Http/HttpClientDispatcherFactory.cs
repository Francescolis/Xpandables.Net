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

using Microsoft.Extensions.Options;

using Xpandables.Net.Http.RequestBuilders;
using Xpandables.Net.Http.Requests;
using Xpandables.Net.Http.ResponseBuilders;

namespace Xpandables.Net.Http;

/// <summary>
/// The default implementation of the 
/// <see cref="IHttpClientDispatcherFactory"/> interface.
/// </summary>
public sealed class HttpClientDispatcherFactory
    (IOptions<HttpClientOptions> options) :
    IHttpClientDispatcherFactory
{
    ///<inheritdoc/>
    public HttpClientOptions Options => options.Value;

    ///<inheritdoc/>
    public async Task<HttpRequestMessage> BuildRequestAsync(
        IHttpClientRequest request,
        CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        ArgumentNullException.ThrowIfNull(request);

        HttpClientAttribute attribute = Options
            .ResolveHttpClientAttribute(request);

        IReadOnlyList<IHttpClientRequestBuilder> builders = Options
            .ResolveRequestBuilders(request);

        HttpClientRequestContext context = new(
            attribute,
            request,
            new HttpRequestMessage(),
            options.Value.SerializerOptions);

        foreach (IHttpClientRequestBuilder builder in builders
            .OrderBy(o => o.Order))
        {
            builder.Build(context);
        }

        return context.RequestMessage;
    }

    ///<inheritdoc/>
    public async Task<HttpClientResponse> BuildResponseAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(response);

        IHttpClientResponseResponseBuilder builder
            = Options
                .GetResponseBuilderFor<HttpClientResponse>(
                response.StatusCode)
            .AsRequired<IHttpClientResponseResponseBuilder>();

        HttpClientResponseContext context = new(
            response,
            options.Value.SerializerOptions);

        return await builder
            .BuildAsync(context, cancellationToken)
            .ConfigureAwait(false);
    }

    ///<inheritdoc/>
    public async Task<HttpClientResponse<TResult>>
        BuildResponseAsync<TResult>(
        HttpResponseMessage response,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(response);

        IHttpClientResponseResultBuilder builder
            = Options
                .GetResponseBuilderFor(
                    typeof(HttpClientResponse<>),
                    response.StatusCode)
                .AsRequired<IHttpClientResponseResultBuilder>();

        HttpClientResponseContext context = new(
            response,
            options.Value.SerializerOptions);

        return await builder
            .BuildAsync<TResult>(context, cancellationToken)
            .ConfigureAwait(false);
    }

    ///<inheritdoc/>
    public async Task<HttpClientResponse<IAsyncEnumerable<TResult>>>
        BuildResponseResultAsync<TResult>(
        HttpResponseMessage response,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(response);

        IHttpClientResponseAsyncResultBuilder builder
             = Options
                .GetResponseBuilderFor(
                 typeof(IAsyncEnumerable<>),
                 response.StatusCode)
             .AsRequired<IHttpClientResponseAsyncResultBuilder>();

        HttpClientResponseContext context = new(
            response,
            options.Value.SerializerOptions);

        return await builder
            .BuildAsync<TResult>(context, cancellationToken)
            .ConfigureAwait(false);
    }
}
