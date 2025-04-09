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
/// Asynchronously builds a response from an HTTP response message. It returns a task containing the constructed
/// response.
/// </summary>
public interface IRestResponseHandler
{
    /// <summary>
    /// Asynchronously builds a response based on the provided HTTP response message.
    /// </summary>
    /// <param name="response">The HTTP response message used to create the response object.</param>
    /// <param name="cancellationToken">Used to signal the cancellation of the asynchronous operation.</param>
    /// <returns>Returns a task that represents the asynchronous operation, containing the constructed response.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="response"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the operation fails.</exception>
    Task<RestResponse> BuildResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken = default);
}

internal sealed class RestResponseHandler : Disposable, IRestResponseHandler
{
    private RestOptions _requestOptions;
    private readonly IDisposable? _disposable;
    private readonly IRestResponseBuilder _responseBuilder;
    public RestResponseHandler(IOptionsMonitor<RestOptions> options, IRestResponseBuilder responseBuilder)
    {
        _requestOptions = options.CurrentValue;
        _disposable = options.OnChange(newOptions => _requestOptions = newOptions);
        _responseBuilder = responseBuilder;
    }

    ///<inheritdoc/>
    public async Task<RestResponse> BuildResponseAsync(
        HttpResponseMessage response, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(response);

        RestResponseContext context = new()
        {
            Message = response,
            SerializerOptions = _requestOptions.SerializerOptions
        };

        try
        {
            return await _responseBuilder
                .BuildAsync(context, cancellationToken)
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