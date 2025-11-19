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
using Xpandables.Net.Collections;
using Xpandables.Net.ExecutionResults;

namespace Xpandables.Net.Http;

/// <summary>
/// Provides functionality for sending HTTP requests and receiving responses in a RESTful manner.
/// </summary>
/// <param name="requestBuilder">The builder used to construct REST requests.</param>
/// <param name="responseBuilder">The builder used to construct REST responses.</param>
/// <param name="httpClient">The HTTP client used to send requests and receive responses.</param>
/// <remarks>
/// RestClient acts as a sealed implementation of the <see cref="IRestClient"/> interface.
/// It utilizes an <see cref="IServiceProvider"/> for dependency resolution and an <see cref="HttpClient"/> for handling HTTP operations.
/// The class supports the sending of requests adhering to the <see cref="IRestRequest"/> interface and processes responses into <see cref="RestResponse"/> objects.
/// </remarks>
public sealed class RestClient(
    IRestRequestBuilder requestBuilder,
    IRestResponseBuilder responseBuilder,
    HttpClient httpClient) : IRestClient
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
            using RestRequest restRequest = await requestBuilder
                .BuildRequestAsync(request, cancellationToken)
                .ConfigureAwait(false);

            _response = await httpClient
                .SendAsync(restRequest.HttpRequestMessage, cancellationToken)
                .ConfigureAwait(false);

            RestResponseContext responseContext = new()
            {
                Message = _response,
                Request = request,
                SerializerOptions = RestSettings.SerializerOptions
            };

            return await responseBuilder
                .BuildResponseAsync(responseContext, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception)
            when (exception is not ArgumentNullException)
        {
            return new RestResponse
            {
                StatusCode = exception.GetHttpStatusCode(),
                Version = httpClient.DefaultRequestVersion,
                Headers = HttpClient.DefaultRequestHeaders.ToElementCollection(),
                Exception = exception
            };
        }
    }

    /// <summary>
    /// Releases resources used by the object, optionally disposing of managed resources.
    /// </summary>
    public void Dispose()
    {
        _response?.Dispose();
        _response = null;
    }
}