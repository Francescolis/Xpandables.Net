/*******************************************************************************
 * Copyright (C) 2025 Kamersoft
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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net.Http.Headers;
using System.Rests.Abstractions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace System.Rests;

/// <summary>
/// Provides a builder for constructing REST requests using a configurable set of composers and interceptors.
/// </summary>
/// <remarks>The RestRequestBuilder enables flexible and extensible construction of HTTP requests by applying a
/// pipeline of composers and interceptors. This design allows for customization of request creation, including
/// pre-processing, validation, and logging. If no composer is able to handle the provided context, an exception is
/// thrown. Interceptors can short-circuit the request building process by marking the context as aborted.</remarks>
/// <param name="composers">An enumerable collection of request composers that define how to construct the REST request based on the provided
/// context. Cannot be null and must contain at least one composer capable of handling the request context.</param>
/// <param name="logger">An optional logger used to record informational messages and errors during the request building process. If not
/// specified, a no-op logger is used.</param>
/// <param name="requestInterceptors">An optional enumerable collection of request interceptors that can inspect or modify the request context before the
/// request is built. Interceptors are executed in order of their specified priority.</param>
public sealed partial class RestRequestBuilder(
    IEnumerable<IRestRequestComposer> composers,
    ILogger<RestRequestBuilder>? logger = null,
    IEnumerable<IRestRequestInterceptor>? requestInterceptors = null) : IRestRequestBuilder
{
    private readonly IRestRequestInterceptor[] _requestInterceptors = [.. (requestInterceptors ?? []).OrderBy(i => i.Order)];
    private readonly ILogger<RestRequestBuilder> _logger = logger ?? NullLogger<RestRequestBuilder>.Instance;

    /// <inheritdoc />
    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "<Pending>")]
    public async ValueTask<RestRequest> BuildRequestAsync(
        RestRequestContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        await ExecuteRequestInterceptorsAsync(context, cancellationToken).ConfigureAwait(false);
        if (context.IsAborted)
        {
            return RestRequest.Empty;
        }

        List<IRestRequestComposer> requestComposers = [.. composers.Where(c => c.CanCompose(context))];

        if (requestComposers.Count == 0)
        {
            throw new InvalidOperationException(
                $"No request builder found for the request type {context.Request.GetType()}.");
        }

        HttpRequestMessage message = InitializeHttpRequestMessage(context.Attribute);
        cancellationToken.ThrowIfCancellationRequested();

        foreach (IRestRequestComposer composer in requestComposers)
        {
            await composer.ComposeAsync(context, cancellationToken).ConfigureAwait(false);
        }

        cancellationToken.ThrowIfCancellationRequested();

        message = FinalizeHttpRequestMessage(context);

        return new RestRequest { HttpRequestMessage = message };
    }

    private static HttpRequestMessage InitializeHttpRequestMessage(RestAttribute attribute)
    {
        ArgumentNullException.ThrowIfNull(attribute);

        string path = string.IsNullOrWhiteSpace(attribute.Path) ? "/" : attribute.Path;

        if (!Uri.TryCreate(path, UriKind.RelativeOrAbsolute, out Uri? requestUri))
        {
            throw new InvalidOperationException($"The REST path '{path}' is not a valid URI.");
        }

        HttpRequestMessage message = new()
        {
            Method = ResolveHttpMethod(attribute.Method),
            RequestUri = requestUri
        };

        message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(attribute.Accept));

        string cultureName = CultureInfo.CurrentCulture.Name;
        if (!string.IsNullOrWhiteSpace(cultureName))
        {
            message.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue(cultureName));
        }

        return message;
    }

    private static HttpRequestMessage FinalizeHttpRequestMessage(RestRequestContext context)
    {
        if (context.Message.Content is not null && context.Message.Content.Headers.ContentType is null)
        {
            context.Message.Content.Headers.ContentType
                = new MediaTypeHeaderValue(context.Attribute.ContentType);
        }

        if (!context.Attribute.IsSecured)
        {
            return context.Message;
        }

        context.Message.Options
            .Set(new HttpRequestOptionsKey<bool>(nameof(RestAttribute.IsSecured)), context.Attribute.IsSecured);

        context.Message.Headers.Authorization ??= new AuthenticationHeaderValue(context.Attribute.Scheme);

        return context.Message;
    }

    private static HttpMethod ResolveHttpMethod(RestSettings.Method method) =>
        method switch
        {
            RestSettings.Method.GET => HttpMethod.Get,
            RestSettings.Method.POST => HttpMethod.Post,
            RestSettings.Method.PUT => HttpMethod.Put,
            RestSettings.Method.DELETE => HttpMethod.Delete,
            RestSettings.Method.HEAD => HttpMethod.Head,
            RestSettings.Method.PATCH => HttpMethod.Patch,
            RestSettings.Method.OPTIONS => HttpMethod.Options,
            RestSettings.Method.TRACE => HttpMethod.Trace,
            RestSettings.Method.CONNECT => new HttpMethod(nameof(RestSettings.Method.CONNECT)),
            _ => throw new ArgumentOutOfRangeException(nameof(method), method, "Unsupported HTTP method.")
        };

    private async ValueTask ExecuteRequestInterceptorsAsync(
         RestRequestContext context,
         CancellationToken cancellationToken)
    {
        foreach (var interceptor in _requestInterceptors)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await interceptor
                .InterceptAsync(context, cancellationToken)
                .ConfigureAwait(false);

            if (context.IsAborted)
            {
                LogInterceptorShortCircuit(_logger, context.Request.Name, interceptor.GetType().Name);
            }
        }
    }

    [LoggerMessage(EventId = 6, Level = LogLevel.Debug, Message = "Request {RequestName} short-circuited by interceptor {InterceptorType}")]
    private static partial void LogInterceptorShortCircuit(ILogger logger, string requestName, string interceptorType);
}