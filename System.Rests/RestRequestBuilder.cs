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

using Microsoft.Extensions.DependencyInjection;

namespace System.Rests;

/// <summary>
/// Provides a builder for creating REST requests by composing multiple request composers and applying REST-specific
/// attributes.
/// </summary>
/// <param name="attributeProvider">An object that supplies REST attribute metadata for the request type.</param>
/// <param name="serviceProvider">The service provider used to resolve dependencies, such as request composers.</param>
public sealed class RestRequestBuilder(
    IRestAttributeProvider attributeProvider,
    IServiceProvider serviceProvider) : IRestRequestBuilder
{
    private readonly IRestAttributeProvider _attributeProvider = attributeProvider;

    /// <inheritdoc />
    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "<Pending>")]
    public async ValueTask<RestRequest> BuildRequestAsync<TRestRequest>(TRestRequest request,
        CancellationToken cancellationToken = default)
        where TRestRequest : class, IRestRequest
    {
        ArgumentNullException.ThrowIfNull(request);

        RestAttribute attribute = _attributeProvider.GetRestAttribute(request);
        IEnumerable<IRestRequestComposer<TRestRequest>> composers = [.. serviceProvider.GetServices<IRestRequestComposer<TRestRequest>>()];

        cancellationToken.ThrowIfCancellationRequested();

        if (!composers.Any())
        {
            throw new InvalidOperationException(
                $"No request builder found for the request type {request.GetType()}.");
        }

        HttpRequestMessage message = InitializeHttpRequestMessage(attribute);

        cancellationToken.ThrowIfCancellationRequested();

        RestRequestContext context = new()
        {
            Attribute = attribute,
            Message = message,
            Request = request,
            SerializerOptions = RestSettings.SerializerOptions
        };

        foreach (IRestRequestComposer<TRestRequest> composer in composers)
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
}