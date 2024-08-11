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

using System.Reflection;

using Microsoft.Extensions.Options;

using Xpandables.Net.Http.RequestBuilders;
using Xpandables.Net.Http.Requests;
using Xpandables.Net.Http.ResponseBuilders;

namespace Xpandables.Net.Http;

/// <summary>
/// The default implementation of the 
/// <see cref="IHttpClientDistributorFactory"/> interface.
/// </summary>
public sealed class HttpClientDisributorFactory
    (IOptions<HttpClientOptions> options) :
    IHttpClientDistributorFactory
{
    ///<inheritdoc/>
    public HttpClientOptions Options => options.Value;

    ///<inheritdoc/>
    public Task<HttpRequestMessage> BuildRequestAsync(
        IHttpClientRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        HttpClientAttribute attribute = ResolveAttribute(request);
        List<IHttpClientRequestBuilder> builders = ResolveRequestBuilders(request);
        HttpClientRequestContext context = new(
            attribute,
            request,
            new HttpRequestMessage(),
            options.Value.SerializerOptions);

        foreach (IHttpClientRequestBuilder builder in builders
            .OrderBy(o => o.Order))
        {
            if (builder.CanBuild(request.GetType()))
            {
                builder.Build(context);
            }
        }

        return Task.FromResult(context.RequestMessage);

        HttpClientAttribute ResolveAttribute(IHttpClientRequest request)
        {
            if (request is IHttpClientAttributeProvider attributeProvider)
            {
                return attributeProvider.Build(Options.ServiceProvider);
            }

            return request!
                .GetType()
                .GetCustomAttribute<HttpClientAttribute>()
               ?? throw new ArgumentNullException(
                   $"{request.GetType().Name} must be decorated " +
                   $"with {nameof(HttpClientAttribute)} " +
                   $"attribute or implement " +
                   $"{nameof(IHttpClientAttributeProvider)} interface.");
        }

        List<IHttpClientRequestBuilder> ResolveRequestBuilders(
            IHttpClientRequest request)
        {
            List<IHttpClientRequestBuilder> types = Options
                .RequestBuilders
                .Where(x => x.CanBuild(request.GetType()))
                .ToList();

            //// add the HttpRequestMessage start builder
            //types.Add(Options.GetRequestBuilderFor<IHttpRequest>());

            //// add the HttpRequestMessage complete builder
            //types.Add(Options.GetRequestBuilderFor<IHttpRequestComplete>());

            return types;
        }
    }

    ///<inheritdoc/>
    public async Task<HttpClientResponse> BuildResponseAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(response);

        IHttpClientResponseBuilder<HttpClientResponse> builder
            = Options.GetResponseBuilderFor<HttpClientResponse>(
                typeof(HttpClientResponse),
                response.StatusCode)
            .AsRequired<IHttpClientResponseBuilder<HttpClientResponse>>();

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

        IHttpClientResponseBuilder<HttpClientResponse<TResult>> builder
            = Options.GetResponseBuilderFor<TResult>(
                typeof(HttpClientResponse<TResult>),
                response.StatusCode)
            .AsRequired<IHttpClientResponseBuilder<HttpClientResponse<TResult>>>();

        HttpClientResponseContext context = new(
            response,
            options.Value.SerializerOptions);

        return await builder
            .BuildAsync(context, cancellationToken)
            .ConfigureAwait(false);
    }

    ///<inheritdoc/>
    public async Task<HttpClientResponse<IAsyncEnumerable<TResult>>>
        BuildResponseResultAsync<TResult>(
        HttpResponseMessage response,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(response);

        IHttpClientResponseBuilder<HttpClientResponse<IAsyncEnumerable<TResult>>> builder
             = Options.GetResponseBuilderFor<TResult>(
                 typeof(HttpClientResponse<IAsyncEnumerable<TResult>>),
                 response.StatusCode)
             .AsRequired<IHttpClientResponseBuilder<HttpClientResponse<IAsyncEnumerable<TResult>>>>();

        HttpClientResponseContext context = new(
            response,
            options.Value.SerializerOptions);

        return await builder
            .BuildAsync(context, cancellationToken)
            .ConfigureAwait(false);
    }
}
