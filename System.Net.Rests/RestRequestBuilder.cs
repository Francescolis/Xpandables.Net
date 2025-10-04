
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
using System.Net.Abstractions;
using System.Net.Http.Headers;

using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.Abstractions;

namespace Xpandables.Net.Rests;

/// <summary>
/// Provides a builder for creating REST requests by composing multiple request composers and applying REST-specific
/// attributes.
/// </summary>
/// <remarks>This class is typically used to construct HTTP requests for REST APIs by aggregating logic from
/// multiple composers. It is sealed and not intended for inheritance. Thread safety depends on the thread safety of the
/// provided composers and attribute provider.</remarks>
/// <param name="attributeProvider">An object that supplies REST attribute metadata for the request type.</param>
/// <param name="serviceProvider">The service provider used to resolve dependencies, such as request composers.</param>
public sealed class RestRequestBuilder(
    IRestAttributeProvider attributeProvider,
    IServiceProvider serviceProvider) : Disposable, IRestRequestBuilder
{
    private readonly IRestAttributeProvider _attributeProvider = attributeProvider;

    /// <inheritdoc />
    [Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "<Pending>")]
    public ValueTask<RestRequest> BuildRequestAsync<TRestRequest>(TRestRequest request,
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
            cancellationToken.ThrowIfCancellationRequested();
            composer.Compose(context);
        }

        cancellationToken.ThrowIfCancellationRequested();

        message = FinalizeHttpRequestMessage(context);

        return new ValueTask<RestRequest>(new RestRequest() { HttpRequestMessage = message });
    }

    private static HttpRequestMessage InitializeHttpRequestMessage(RestAttribute attribute)
    {
        HttpRequestMessage message = new()
        {
            Method = new HttpMethod(attribute.Method.ToString()),
            RequestUri = new Uri(attribute.Path, UriKind.Relative)
        };

        message.Headers.Accept
            .Add(new MediaTypeWithQualityHeaderValue(attribute.Accept));
        message.Headers.AcceptLanguage
            .Add(new StringWithQualityHeaderValue(
                Thread.CurrentThread.CurrentCulture.Name));

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
}