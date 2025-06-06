﻿/*******************************************************************************
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

using System.Net;

using Microsoft.Extensions.DependencyInjection;

namespace Xpandables.Net.Executions.Rests;

/// <summary>
/// Provides functionality for sending HTTP requests and receiving responses in a RESTful manner.
/// </summary>
/// <remarks>
/// RestClient acts as a sealed implementation of the <see cref="IRestClient"/> interface and derives from the <see cref="Disposable"/> base class.
/// It utilizes an <see cref="IServiceProvider"/> for dependency resolution and an <see cref="HttpClient"/> for handling HTTP operations.
/// The class supports the sending of requests adhering to the <see cref="IRestRequest"/> interface and processes responses into <see cref="RestResponse"/> objects.
/// </remarks>
public sealed class RestClient(IServiceProvider serviceProvider, HttpClient httpClient) : Disposable, IRestClient
{
    private HttpResponseMessage? _response;

    /// <inheritdoc />
    public HttpClient HttpClient => httpClient;

    /// <inheritdoc />
    public async Task<RestResponse> SendAsync<TRestRequest>(TRestRequest request,
        CancellationToken cancellationToken = default)
        where TRestRequest : class, IRestRequest
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            IRestRequestBuilder<TRestRequest> requestBuilder = serviceProvider
                .GetRequiredService<IRestRequestBuilder<TRestRequest>>();

            using HttpRequestMessage httpRequest = await requestBuilder
                .BuildRequestAsync(request, cancellationToken)
                .ConfigureAwait(false);

            _response = await httpClient
                .SendAsync(httpRequest, cancellationToken)
                .ConfigureAwait(false);

            IRestResponseBuilder<TRestRequest> responseBuilder = serviceProvider
                .GetRequiredService<IRestResponseBuilder<TRestRequest>>();

            return await responseBuilder
                .BuildResponseAsync(request, _response, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception)
            when (exception is not ArgumentNullException)
        {
            return new RestResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Version = httpClient.DefaultRequestVersion,
                Headers = HttpClient.DefaultRequestHeaders.ToElementCollection(),
                Exception = exception
            };
        }
    }

    /// <summary>
    /// Releases resources used by the object, optionally disposing of managed resources.
    /// </summary>
    /// <param name="disposing">Indicates whether to release both managed and unmanaged resources.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _response?.Dispose();
            _response = null;
        }

        base.Dispose(disposing);
    }
}