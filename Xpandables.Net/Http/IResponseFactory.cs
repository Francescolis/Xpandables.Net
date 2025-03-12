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
/// Defines a factory to build HTTP responses.
/// </summary>
public interface IResponseFactory
{
    /// <summary>
    /// Builds an HTTP response message asynchronously.
    /// </summary>
    /// <typeparam name="TResponseHttp">The type of the response.</typeparam>
    /// <param name="response">The HTTP response message.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. 
    /// The task result contains the response of type 
    /// <typeparamref name="TResponseHttp"/>.</returns>
    /// <exception cref="ArgumentNullException">The exception that is thrown 
    /// when the response is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the factory is
    /// unable to build the response.</exception>
    Task<TResponseHttp> BuildResponseAsync<TResponseHttp>(
        HttpResponseMessage response,
        CancellationToken cancellationToken = default)
        where TResponseHttp : ResponseHttp;
}

internal sealed class ResponseFactory : Disposable, IResponseFactory
{
    private RequestOptions _requestOptions;
    private readonly IDisposable? _disposable;
    public ResponseFactory(IOptionsMonitor<RequestOptions> options)
    {
        _requestOptions = options.CurrentValue;
        _disposable = options.OnChange(newOptions => _requestOptions = newOptions);
    }

    ///<inheritdoc/>
    public async Task<TResponseHttp> BuildResponseAsync<TResponseHttp>(
        HttpResponseMessage response,
        CancellationToken cancellationToken = default)
        where TResponseHttp : ResponseHttp
    {
        ArgumentNullException.ThrowIfNull(response);

        IResponseHttpBuilder builder =
            _requestOptions.PeekResponseBuilder<TResponseHttp>(response.StatusCode)
            ?? throw new InvalidOperationException("The response builder is not found.");

        ResponseContext context = new()
        {
            Message = response,
            SerializerOptions = _requestOptions.SerializerOptions
        };

        try
        {
            return await builder
                .BuildAsync<TResponseHttp>(context, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception)
            when (exception is not ArgumentNullException
                and not InvalidOperationException)
        {
            throw new InvalidOperationException(
                "The response builder failed to build the response.",
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