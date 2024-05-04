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

using System.Net.Http.Headers;
using System.Reflection;

using Microsoft.Extensions.Options;

using static Xpandables.Net.Http.HttpClientParameters;

namespace Xpandables.Net.Http.Builders;

/// <summary>
/// The default implementation of the 
/// <see cref="IHttpClientDispatcherFactory"/> interface.
/// </summary>
public sealed class HttpClientDispatcherFactory
    (IOptions<HttpClientOptions> options,
    IServiceProvider serviceProvider) :
    IHttpClientDispatcherFactory
{
    private readonly HttpClientOptions _options = options.Value;

    ///<inheritdoc/>
    public ValueTask<HttpRequestMessage> BuildRequestAsync<TRequest>(
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        HttpClientAttribute attribute = ResolveAttribute(request);

        attribute.Path ??= "/";
#pragma warning disable CA2000 // Dispose objects before losing scope
        HttpRequestMessage requestMessage = new()
        {
            Method = new(attribute.Method.ToString()),
            RequestUri = new(attribute.Path, UriKind.Relative)
        };
#pragma warning restore CA2000 // Dispose objects before losing scope

        requestMessage.Headers.Accept
            .Add(new MediaTypeWithQualityHeaderValue(attribute.Accept));
        requestMessage.Headers.AcceptLanguage
            .Add(new StringWithQualityHeaderValue(
                Thread.CurrentThread.CurrentCulture.Name));



        if ((attribute.Location & Location.Path) == Location.Path)
        {
            IHttpRequestPathString pathRequest = (IHttpRequestPathString)request;
            requestMessage = _options
                .GetRequestBuilderFor<IHttpRequestPathString>()
                .Build(attribute, pathRequest, requestMessage);
        }

        if ((attribute.Location & Location.Query) == Location.Query)
        {
            IHttpRequestQueryString queryRequest = (IHttpRequestQueryString)request;
            requestMessage = _options
                .GetRequestBuilderFor<IHttpRequestQueryString>()
                .Build(attribute, queryRequest, requestMessage);
        }

        if ((attribute.Location & Location.Cookie) == Location.Cookie)
        {
            IHttpRequestCookie cookieRequest = (IHttpRequestCookie)request;
            requestMessage = _options
                .GetRequestBuilderFor<IHttpRequestCookie>()
                .Build(attribute, cookieRequest, requestMessage);
        }

        if ((attribute.Location & Location.Header) == Location.Header)
        {
            IHttpRequestHeader headerRequest = (IHttpRequestHeader)request;
            requestMessage = _options
                .GetRequestBuilderFor<IHttpRequestHeader>()
                .Build(attribute, headerRequest, requestMessage);
        }

        if (attribute.IsNullable == false
            && (attribute.Location & Location.Body) == Location.Body)
        {
            if (attribute.BodyFormat == BodyFormat.ByteArray)
            {
                IHttpRequestByteArray byteRequest = (IHttpRequestByteArray)request;
                requestMessage = _options
                    .GetRequestBuilderFor<IHttpRequestByteArray>()
                    .Build(attribute, byteRequest, requestMessage);
            }

            if (attribute.BodyFormat == BodyFormat.FormUrlEncoded)
            {
                IHttpRequestFormUrlEncoded formRequest =
                    (IHttpRequestFormUrlEncoded)request;
                requestMessage = _options
                    .GetRequestBuilderFor<IHttpRequestFormUrlEncoded>()
                    .Build(attribute, formRequest, requestMessage);
            }

            if (attribute.BodyFormat == BodyFormat.Multipart)
            {
                IHttpRequestMultipart multipartRequest =
                    (IHttpRequestMultipart)request;
                requestMessage = _options
                    .GetRequestBuilderFor<IHttpRequestMultipart>()
                    .Build(attribute, multipartRequest, requestMessage);
            }

            if (attribute.BodyFormat == BodyFormat.Stream)
            {
                IHttpRequestStream streamRequest = (IHttpRequestStream)request;
                requestMessage = _options
                    .GetRequestBuilderFor<IHttpRequestStream>()
                    .Build(attribute, streamRequest, requestMessage);
            }

            if (attribute.BodyFormat == BodyFormat.String)
            {
                IHttpRequestString stringRequest = (IHttpRequestString)request;
                requestMessage = _options
                    .GetRequestBuilderFor<IHttpRequestString>()
                    .Build(attribute, stringRequest, requestMessage);
            }

            if (requestMessage.Content is not null
                && requestMessage.Content.Headers.ContentType is null)
                requestMessage.Content.Headers.ContentType
                    = new MediaTypeHeaderValue(attribute.ContentType);
        }

        if (attribute.IsSecured)
        {
            requestMessage.Headers.Authorization =
                new AuthenticationHeaderValue(attribute.Scheme);
            requestMessage.Options
                .Set(new(nameof(
                    HttpClientAttribute.IsSecured)),
                    attribute.IsSecured);
        }

        return ValueTask.FromResult(requestMessage);

        HttpClientAttribute ResolveAttribute(TRequest request)
        {
            if (request is IHttpClientAttributeProvider attributeProvider)
                return attributeProvider.Build(_options.ServiceProvider);

            return request!
                .GetType()
                .GetCustomAttribute<HttpClientAttribute>()
               ?? throw new ArgumentNullException(
                   $"{request.GetType().Name} must be decorated " +
                   $"with {nameof(HttpClientAttribute)} " +
                   $"attribute or implement " +
                   $"{nameof(IHttpClientAttributeProvider)} interface.");
        }
    }

    ///<inheritdoc/>
    public async ValueTask<HttpClientResponse> BuildResponseAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(response);

        IHttpClientResponseBuilder builder
            = _options.GetResponseBuilderFor<IHttpClientResponseBuilder>(
                serviceProvider,
                response.StatusCode);

        return await builder
            .BuildAsync(
                response,
                _options.SerializerOptions,
                cancellationToken)
            .ConfigureAwait(false);
    }

    ///<inheritdoc/>
    public async ValueTask<HttpClientResponse<TResult>>
        BuildResponseAsync<TResult>(
        HttpResponseMessage response,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(response);


        IHttpClientResponseResultBuilder<TResult> builder
            = _options.GetResponseBuilderFor
            <IHttpClientResponseResultBuilder<TResult>>(
                serviceProvider,
                response.StatusCode);

        return await builder
            .BuildAsync(
                response,
                _options.SerializerOptions,
                cancellationToken)
            .ConfigureAwait(false);
    }

    ///<inheritdoc/>
    public async ValueTask<HttpClientResponse<IAsyncEnumerable<TResult>>>
        BuildResponseResultAsync<TResult>(
        HttpResponseMessage response,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(response);

        IHttpClientResponseIAsyncResultBuilder<TResult> builder
            = _options.GetResponseBuilderFor
            <IHttpClientResponseIAsyncResultBuilder<TResult>>(
                serviceProvider,
                response.StatusCode);

        return await builder
                .BuildAsync(
                    response,
                    _options.SerializerOptions,
                    cancellationToken)
                .ConfigureAwait(false);
    }
}
