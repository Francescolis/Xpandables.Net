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

using Microsoft.Extensions.Options;

using Xpandables.Net.Http.Builders;

namespace Xpandables.Net.Http;

/// <summary>
/// Defines a factory to build <see cref="HttpRequestMessage"/>>.
/// </summary>
public interface IRestRequestFactory
{
    /// <summary>
    /// Asynchronously builds an HTTP request message based on the provided REST request.
    /// </summary>
    /// <param name="request">The input that defines the details of the HTTP request to be created.</param>
    /// <param name="cancellationToken">Used to signal the cancellation of the asynchronous operation if needed.</param>
    /// <returns>Returns a task that represents the asynchronous operation, containing the constructed HTTP request message.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="request"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the operation fails.</exception>
    Task<HttpRequestMessage> BuildRequestAsync(IRestRequest request, CancellationToken cancellationToken = default);
}

internal sealed class RestRequestFactory : Disposable, IRestRequestFactory
{
    private RestOptions _requestOptions;
    private readonly IDisposable? _disposable;
    private HttpRequestMessage _message = default!;
    public RestRequestFactory(IOptionsMonitor<RestOptions> options)
    {
        _requestOptions = options.CurrentValue;
        _disposable = options.OnChange(newOptions => _requestOptions = newOptions);
    }

    ///<inheritdoc/>
    public Task<HttpRequestMessage> BuildRequestAsync(IRestRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        MapRestAbstractAttribute attribute = _requestOptions.GetMapHttp(request);

        List<IRestRequestBuilder> builders = attribute.RequestBuilder switch
        {
            not null => [attribute.RequestBuilder],
            _ => [.. _requestOptions.GetAllRequestBuilders(request.GetType())]
        };

        if (builders.Count == 0)
            throw new InvalidOperationException(
                $"No request builder found for the request type {request.GetType()}.");

        attribute.Path ??= "/";
        _message = new()
        {
            Method = new(attribute.Method.ToString()),
            RequestUri = new(attribute.Path, UriKind.Relative)
        };

        _message.Headers.Accept
            .Add(new MediaTypeWithQualityHeaderValue(attribute.Accept));
        _message.Headers.AcceptLanguage
            .Add(new StringWithQualityHeaderValue(
                Thread.CurrentThread.CurrentCulture.Name));

        RestRequestContext context = new()
        {
            Attribute = attribute,
            Message = _message,
            Request = request,
            SerializerOptions = _requestOptions.SerializerOptions
        };

        try
        {
            foreach (IRestRequestBuilder builder in builders)
            {
                builder.Build(context);
            }

            if (context.Message.Content is not null)
            {
                context.Message.Content.Headers.ContentType
                    = new MediaTypeHeaderValue(context.Attribute.ContentType);
            }

            if (context.Attribute.IsSecured)
            {
                context.Message.Options
                    .Set(new(nameof(
                        MapRestAttribute.IsSecured)),
                        context.Attribute.IsSecured);

                if (context.Message.Headers.Authorization is null)
                {
                    context.Message.Headers.Authorization =
                        new AuthenticationHeaderValue(context.Attribute.Scheme);
                }
            }

            return Task.FromResult(context.Message);
        }
        catch (Exception exception)
            when (exception is not ArgumentNullException
                and not InvalidOperationException)
        {
            throw new InvalidOperationException(
                "An error occurred while building the request message.",
                exception);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _disposable?.Dispose();
            _message?.Dispose();
        }

        base.Dispose(disposing);
    }
}