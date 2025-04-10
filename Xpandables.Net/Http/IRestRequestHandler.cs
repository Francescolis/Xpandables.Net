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
using System.Net.Http.Headers;

using Xpandables.Net.Http.Builders;
using Xpandables.Net.Text;

namespace Xpandables.Net.Http;

/// <summary>
/// Asynchronously builds an HTTP request message from a provided REST request. 
/// It can be canceled using a cancellation token.
/// </summary>
public interface IRestRequestHandler<TRestRequest> : IDisposable
    where TRestRequest : class, IRestRequest
{
    /// <summary>
    /// Asynchronously builds an HTTP request message based on the provided REST request.
    /// </summary>
    /// <param name="request">The input that defines the details of the HTTP request to be created.</param>
    /// <param name="cancellationToken">Used to signal the cancellation of the asynchronous operation if needed.</param>
    /// <returns>Returns a task that represents the asynchronous operation, containing the constructed HTTP request message.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="request"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the operation fails.</exception>
    ValueTask<HttpRequestMessage> BuildRequestAsync(TRestRequest request, CancellationToken cancellationToken = default);
}

internal sealed class RestRequestHandler<TRestRequest>(
    IRestAttributeProvider attributeProvider,
    IEnumerable<IRestRequestComposer<TRestRequest>> composers) : Disposable, IRestRequestHandler<TRestRequest>
    where TRestRequest : class, IRestRequest
{
    private HttpRequestMessage _message = new();
    private readonly IRestAttributeProvider _attributeProvider = attributeProvider;
    private readonly IEnumerable<IRestRequestComposer<TRestRequest>> _composers = composers;

    ///<inheritdoc/>
    public ValueTask<HttpRequestMessage> BuildRequestAsync(TRestRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        _RestAttribute attribute = _attributeProvider.GetRestAttribute(request);

        cancellationToken.ThrowIfCancellationRequested();

        if (!_composers.Any())
            throw new InvalidOperationException(
                $"No request builder found for the request type {request.GetType()}.");

        _message = InitializeHttpRequestMessage(attribute);

        cancellationToken.ThrowIfCancellationRequested();

        RestRequestContext<TRestRequest> context = new()
        {
            Attribute = attribute,
            Message = _message,
            Request = request,
            SerializerOptions = DefaultSerializerOptions.Defaults
        };

        foreach (IRestRequestComposer<TRestRequest> composer in _composers)
        {
            composer.Compose(context);
        }

        cancellationToken.ThrowIfCancellationRequested();

        _message = FinalizeHttpRequestMessage(context);

        return new(_message);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _message?.Dispose();
        }

        base.Dispose(disposing);
    }

    private static HttpRequestMessage InitializeHttpRequestMessage(_RestAttribute attribute)
    {
        attribute.Path ??= "/";
        HttpRequestMessage message = new()
        {
            Method = new(attribute.Method.ToString()),
            RequestUri = new(attribute.Path, UriKind.Relative)
        };

        message.Headers.Accept
            .Add(new MediaTypeWithQualityHeaderValue(attribute.Accept));
        message.Headers.AcceptLanguage
            .Add(new StringWithQualityHeaderValue(
                Thread.CurrentThread.CurrentCulture.Name));

        return message;
    }

    private static HttpRequestMessage FinalizeHttpRequestMessage(RestRequestContext<TRestRequest> context)
    {
        if (context.Message.Content is not null)
        {
            context.Message.Content.Headers.ContentType
                = new MediaTypeHeaderValue(context.Attribute.ContentType);
        }

        if (context.Attribute.IsSecured)
        {
            context.Message.Options
                .Set(new(nameof(
                    RestAttribute.IsSecured)),
                    context.Attribute.IsSecured);

            if (context.Message.Headers.Authorization is null)
            {
                context.Message.Headers.Authorization =
                    new AuthenticationHeaderValue(context.Attribute.Scheme);
            }
        }

        return context.Message;
    }
}