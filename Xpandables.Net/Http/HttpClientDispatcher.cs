
/************************************************************************************************************
 * Copyright (C) 2023 Francis-Black EWANE
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
************************************************************************************************************/
using System.Net;
using System.Text.Json;

namespace Xpandables.Net.Http;
// Used as default implementation for IHttpClientDispatcher
internal sealed class DefaultHttpClientDispatcher(
    IHttpClientBuildProvider httpClientBuildProvider,
    HttpClient httpClient,
    JsonSerializerOptions? jsonSerializerOptions)
    : HttpClientDispatcher(httpClientBuildProvider, httpClient, jsonSerializerOptions)
{
}

/// <summary>
/// This helper class allows the application author to implement the <see cref="IHttpClientDispatcher"/> interface.
/// </summary>
///<inheritdoc/>
public abstract class HttpClientDispatcher(
    IHttpClientBuildProvider httpClientBuildProvider,
    HttpClient httpClient,
    JsonSerializerOptions? jsonSerializerOptions) : Disposable, IHttpClientDispatcher
{
    private readonly IHttpClientRequestBuilder _httpRestClientRequestBuilder = httpClientBuildProvider?.RequestBuilder
            ?? throw new ArgumentNullException(nameof(httpClientBuildProvider));
    private readonly IHttpClientResponseBuilder _httpRestClientResponseBuilder = httpClientBuildProvider?.ResponseBuilder
            ?? throw new ArgumentNullException(nameof(httpClientBuildProvider));
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    private readonly JsonSerializerOptions? _jsonSerializerOptions = jsonSerializerOptions;

    ///<inheritdoc/>
    public HttpClient HttpClient => _httpClient;

    ///<inheritdoc/>
    public JsonSerializerOptions? SerializerOptions => _jsonSerializerOptions;

    ///<inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _httpClient.Dispose();
        }

        base.Dispose(disposing);
    }

    ///<inheritdoc/>
    public virtual async ValueTask<HttpClientResponse<IAsyncEnumerable<TResult>>> SendAsync<TResult>(
        IHttpClientAsyncRequest<TResult> request,
        JsonSerializerOptions? serializerOptions = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using HttpRequestMessage httpRequest = await _httpRestClientRequestBuilder
                .BuildHttpRequestAsync(request, _httpClient, _jsonSerializerOptions)
                .ConfigureAwait(false);

            // Due to the fact that the result is an IAsyncEnumerable, the response can not be disposed before.
            HttpResponseMessage response = await _httpClient
                .SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);

            return await _httpRestClientResponseBuilder
                .BuildHttpResponseAsync<TResult>(response, _jsonSerializerOptions, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is ArgumentNullException
                                           or ArgumentException
                                           or InvalidOperationException
                                           or OperationCanceledException
                                           or HttpRequestException
                                           or TaskCanceledException
                                           or TimeoutException
                                           or WebException)
        {
            return new HttpClientResponse<IAsyncEnumerable<TResult>>(
                HttpStatusCode.BadRequest,
                _httpClient.DefaultRequestHeaders.ReadHttpHeaders(),
                default,
                _httpClient.DefaultRequestVersion,
                default,
                new HttpClientException(exception.Message, exception));
        }
    }

    ///<inheritdoc/>
    public virtual async ValueTask<HttpClientResponse> SendAsync(
        IHttpClientRequest request,
        JsonSerializerOptions? serializerOptions = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using HttpRequestMessage httpRequest = await _httpRestClientRequestBuilder
                 .BuildHttpRequestAsync(request, _httpClient, _jsonSerializerOptions)
                 .ConfigureAwait(false);

            using HttpResponseMessage response = await _httpClient
                .SendAsync(httpRequest, cancellationToken)
                .ConfigureAwait(false);

            return await _httpRestClientResponseBuilder
                .BuildHttpResponse(response, _jsonSerializerOptions, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is ArgumentNullException
                                        or ArgumentException
                                        or InvalidOperationException
                                        or OperationCanceledException
                                        or HttpRequestException
                                        or TaskCanceledException
                                        or TimeoutException
                                        or WebException)
        {
            return new HttpClientResponse(
                HttpStatusCode.BadRequest,
                _httpClient.DefaultRequestHeaders.ReadHttpHeaders(),
                _httpClient.DefaultRequestVersion,
                default,
                new HttpClientException(exception.Message, exception));
        }
    }

    ///<inheritdoc/>
    public virtual async ValueTask<HttpClientResponse<TResult>> SendAsync<TResult>(
        IHttpClientRequest<TResult> request,
        JsonSerializerOptions? serializerOptions = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using HttpRequestMessage httpRequest = await _httpRestClientRequestBuilder
                .BuildHttpRequestAsync(request, _httpClient, _jsonSerializerOptions)
                .ConfigureAwait(false);

            using HttpResponseMessage response = await _httpClient
                .SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);

            return await _httpRestClientResponseBuilder
                .BuildHttpResponse<TResult>(response, _jsonSerializerOptions, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is ArgumentNullException
                                        or ArgumentException
                                        or InvalidOperationException
                                        or OperationCanceledException
                                        or HttpRequestException
                                        or TaskCanceledException
                                        or TimeoutException
                                        or WebException)
        {
            return new HttpClientResponse<TResult>(
                HttpStatusCode.BadRequest,
                _httpClient.DefaultRequestHeaders.ReadHttpHeaders(),
                default,
                _httpClient.DefaultRequestVersion,
                default,
                new HttpClientException(exception.Message, exception));
        }
    }
}