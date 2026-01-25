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
using System.Collections;
using System.Net;
using System.Rests.Abstractions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace System.Rests;

/// <summary>
/// Provides functionality for sending HTTP requests and receiving responses in a RESTful manner.
/// </summary>
/// <param name="requestBuilder">The builder used to construct REST requests.</param>
/// <param name="responseBuilder">The builder used to construct REST responses.</param>
/// <param name="httpClient">The HTTP client used to send requests and receive responses.</param>
/// <param name="options">The REST client options for timeout, retry, and logging configuration.</param>
/// <param name="requestInterceptors">Optional collection of request interceptors.</param>
/// <param name="responseInterceptors">Optional collection of response interceptors.</param>
/// <param name="logger">Optional logger for request/response logging.</param>
/// <remarks>
/// The <see cref="HttpClient"/> is typically managed by the dependency injection container via 
/// <see cref="IHttpClientFactory"/> and should not be disposed by the consumer.
/// </remarks>
public sealed partial class RestClient(
    IRestRequestBuilder requestBuilder,
    IRestResponseBuilder responseBuilder,
    HttpClient httpClient,
    IOptions<RestClientOptions>? options = null,
    IEnumerable<IRestRequestInterceptor>? requestInterceptors = null,
    IEnumerable<IRestResponseInterceptor>? responseInterceptors = null,
    ILogger<RestClient>? logger = null) : IRestClient
{
    private readonly RestClientOptions _options = options?.Value ?? new RestClientOptions();
    private readonly IRestRequestInterceptor[] _requestInterceptors = (requestInterceptors ?? [])
        .OrderBy(i => i.Order)
        .ToArray();
    private readonly IRestResponseInterceptor[] _responseInterceptors = (responseInterceptors ?? [])
        .OrderBy(i => i.Order)
        .ToArray();
    private readonly ILogger<RestClient> _logger = logger ?? NullLogger<RestClient>.Instance;

    /// <inheritdoc />
    public HttpClient HttpClient => httpClient;

    /// <inheritdoc />
    public async Task<RestResponse> SendAsync<TRestRequest>(TRestRequest request,
        CancellationToken cancellationToken = default)
        where TRestRequest : class, IRestRequest
    {
        ArgumentNullException.ThrowIfNull(request);

        // Create a linked cancellation token with timeout
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(_options.Timeout);

        try
        {
            using RestRequest restRequest = await requestBuilder
                .BuildRequestAsync(request, timeoutCts.Token)
                .ConfigureAwait(false);

            // Create context for interceptors
            RestRequestContext requestContext = new()
            {
                Attribute = RestAttributeProvider.GetRestAttributeFromRequest(request),
                Message = restRequest.HttpRequestMessage,
                Request = request,
                SerializerOptions = RestSettings.SerializerOptions
            };

            // Execute request interceptors
            RestResponse? shortCircuitResponse = await ExecuteRequestInterceptorsAsync(
                requestContext, timeoutCts.Token).ConfigureAwait(false);

            if (shortCircuitResponse is not null)
            {
                LogShortCircuitedResponse(_logger, request.Name, shortCircuitResponse.StatusCode);
                return await ExecuteResponseInterceptorsAsync(
                    shortCircuitResponse, request, timeoutCts.Token).ConfigureAwait(false);
            }

            LogSendingRequest(_logger, restRequest.HttpRequestMessage.Method, 
                restRequest.HttpRequestMessage.RequestUri, request.Name);

            using HttpResponseMessage response = await httpClient
                .SendAsync(restRequest.HttpRequestMessage, timeoutCts.Token)
                .ConfigureAwait(false);

            RestResponseContext responseContext = new()
            {
                Message = response,
                Request = request,
                SerializerOptions = RestSettings.SerializerOptions
            };

            RestResponse restResponse = await responseBuilder
                .BuildResponseAsync(responseContext, timeoutCts.Token)
                .ConfigureAwait(false);

            LogReceivedResponse(_logger, restResponse.StatusCode, request.Name);

            // Execute response interceptors
            return await ExecuteResponseInterceptorsAsync(
                restResponse, request, timeoutCts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            // Timeout occurred (not user cancellation)
            LogRequestTimeout(_logger, request.Name, _options.Timeout);

            return new RestResponse
            {
                StatusCode = HttpStatusCode.RequestTimeout,
                Version = httpClient.DefaultRequestVersion,
                Headers = httpClient.DefaultRequestHeaders.ToElementCollection(),
                Exception = new TimeoutException($"Request timed out after {_options.Timeout}.")
            };
        }
        catch (Exception exception)
            when (exception is not ArgumentNullException
                and not OperationCanceledException
                and not InvalidOperationException)
        {
            LogRequestFailed(_logger, exception, request.Name);

            return new RestResponse
            {
                StatusCode = exception.GetHttpStatusCode(),
                Version = httpClient.DefaultRequestVersion,
                Headers = httpClient.DefaultRequestHeaders.ToElementCollection(),
                Exception = exception
            };
        }
    }

    private async ValueTask<RestResponse?> ExecuteRequestInterceptorsAsync(
        RestRequestContext context,
        CancellationToken cancellationToken)
    {
        foreach (var interceptor in _requestInterceptors)
        {
            cancellationToken.ThrowIfCancellationRequested();

            RestResponse? response = await interceptor
                .InterceptAsync(context, cancellationToken)
                .ConfigureAwait(false);

            if (response is not null)
            {
                LogInterceptorShortCircuit(_logger, context.Request.Name, interceptor.GetType().Name);
                return response;
            }
        }

        return null;
    }

    private async ValueTask<RestResponse> ExecuteResponseInterceptorsAsync(
        RestResponse response,
        IRestRequest request,
        CancellationToken cancellationToken)
    {
        RestResponse currentResponse = response;

        foreach (var interceptor in _responseInterceptors)
        {
            cancellationToken.ThrowIfCancellationRequested();

            currentResponse = await interceptor
                .InterceptAsync(currentResponse, request, cancellationToken)
                .ConfigureAwait(false);
        }

        return currentResponse;
    }

        [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Sending {Method} request to {Uri} for {RequestName}")]
        private static partial void LogSendingRequest(ILogger logger, HttpMethod method, Uri? uri, string requestName);

        [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Received {StatusCode} response for {RequestName}")]
        private static partial void LogReceivedResponse(ILogger logger, HttpStatusCode statusCode, string requestName);

        [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "Request {RequestName} short-circuited with status {StatusCode}")]
        private static partial void LogShortCircuitedResponse(ILogger logger, string requestName, HttpStatusCode statusCode);

        [LoggerMessage(EventId = 4, Level = LogLevel.Warning, Message = "Request {RequestName} timed out after {Timeout}")]
        private static partial void LogRequestTimeout(ILogger logger, string requestName, TimeSpan timeout);

        [LoggerMessage(EventId = 5, Level = LogLevel.Error, Message = "Request {RequestName} failed with exception")]
        private static partial void LogRequestFailed(ILogger logger, Exception exception, string requestName);

        [LoggerMessage(EventId = 6, Level = LogLevel.Debug, Message = "Request {RequestName} short-circuited by interceptor {InterceptorType}")]
        private static partial void LogInterceptorShortCircuit(ILogger logger, string requestName, string interceptorType);
    }