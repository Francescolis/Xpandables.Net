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

using Xpandables.Net.Http.Builders;

namespace Xpandables.Net.Http;

/// <summary>
/// Defines a factory to build <see cref="RestResponse"/>
/// </summary>
public interface IRestResponseFactory
{
    /// <summary>
    /// Asynchronously builds a response based on the provided HTTP response message.
    /// </summary>
    /// <typeparam name="TRestResponse">This type parameter specifies the type of response that will be constructed from the HTTP response.</typeparam>
    /// <param name="response">The HTTP response message used to create the response object.</param>
    /// <param name="cancellationToken">Used to signal the cancellation of the asynchronous operation.</param>
    /// <returns>Returns a task that represents the asynchronous operation, containing the constructed response.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="response"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the operation fails.</exception>
    Task<TRestResponse> BuildResponseAsync<TRestResponse>(
        HttpResponseMessage response, CancellationToken cancellationToken = default)
        where TRestResponse : RestResponseAbstract;
}

internal sealed class RestResponseFactory : Disposable, IRestResponseFactory
{
    private RestOptions _requestOptions;
    private readonly IDisposable? _disposable;
    public RestResponseFactory(IOptionsMonitor<RestOptions> options)
    {
        _requestOptions = options.CurrentValue;
        _disposable = options.OnChange(newOptions => _requestOptions = newOptions);
    }

    ///<inheritdoc/>
    public async Task<TRestResponse> BuildResponseAsync<TRestResponse>(
        HttpResponseMessage response, CancellationToken cancellationToken = default)
        where TRestResponse : RestResponseAbstract
    {
        ArgumentNullException.ThrowIfNull(response);

        IRestResponseBuilder builder =
            _requestOptions.GetResponseBuilder<TRestResponse>(response.StatusCode)
            ?? throw new InvalidOperationException("The response builder is not found.");

        RestResponseContext context = new()
        {
            Message = response,
            SerializerOptions = _requestOptions.SerializerOptions
        };

        try
        {
            return await builder
                .BuildAsync<TRestResponse>(context, cancellationToken)
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