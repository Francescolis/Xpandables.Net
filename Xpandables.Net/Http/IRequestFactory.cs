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

namespace Xpandables.Net.Http;

/// <summary>
/// Defines a factory to build <see cref="HttpRequestMessage"/>>.
/// </summary>
public interface IRequestFactory
{
    /// <summary>
    /// Builds an <see cref="HttpRequestMessage"/> message asynchronously.
    /// </summary>
    /// <param name="request">The HTTP client request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. 
    /// The task result contains the HTTP request message.</returns>
    /// <exception cref="ArgumentNullException">The request is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the factory is
    /// unable to build the request.</exception>
    Task<HttpRequestMessage> BuildRequestAsync(
        IRequestHttp request,
        CancellationToken cancellationToken = default);
}

internal sealed class RequestFactory : Disposable, IRequestFactory
{
    private RequestOptions _requestOptions;
    private readonly IDisposable? _disposable;
    public RequestFactory(IOptionsMonitor<RequestOptions> options)
    {
        _requestOptions = options.CurrentValue;
        _disposable = options.OnChange(newOptions => _requestOptions = newOptions);
    }

    ///<inheritdoc/>
    public Task<HttpRequestMessage> BuildRequestAsync(
        IRequestHttp request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        RequestDefinitionAttribute attribute = _requestOptions.GetRequestDefinition(request);

        List<IRequestHttpBuilder> builders =
            [.. _requestOptions.PeekRequestBuilders(request.GetType())];

        if (builders.Count == 0)
            throw new InvalidOperationException(
                $"No request builder found for the request type {request.GetType()}.");

        RequestContext context = new()
        {
            Attribute = attribute,
            Message = new HttpRequestMessage(),
            Request = request,
            SerializerOptions = _requestOptions.SerializerOptions
        };

        try
        {
            foreach (IRequestHttpBuilder builder in builders)
            {
                builder.Build(context);
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
        }

        base.Dispose(disposing);
    }
}