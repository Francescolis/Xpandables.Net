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
/// Provides a client for sending HTTP requests and receiving HTTP responses from a RESTful service using customizable
/// request and response builders.
/// </summary>
/// <remarks>This client is designed for extensibility and can be customized through the use of different request
/// and response builders, as well as attribute providers. It supports configurable timeouts and integrates with logging
/// frameworks for monitoring and diagnostics.</remarks>
/// <param name="requestBuilder">The builder responsible for constructing HTTP requests based on the provided request context.</param>
/// <param name="responseBuilder">The builder responsible for constructing HTTP responses from the received response context.</param>
/// <param name="httpClient">The HttpClient instance used to send HTTP requests and receive responses.</param>
/// <param name="attributeProvider">The provider that supplies REST attributes for the request, influencing how the request is constructed.</param>
/// <param name="options">Optional settings for configuring the RestClient's behavior, such as request timeouts.</param>
/// <param name="logger">An optional logger for logging information, warnings, and errors during request processing.</param>
public sealed partial class RestClient(
    IRestRequestBuilder requestBuilder,
    IRestResponseBuilder responseBuilder,
    HttpClient httpClient,
    IRestAttributeProvider attributeProvider,
    IOptions<RestClientOptions>? options = null,
    ILogger<RestClient>? logger = null) : IRestClient
{
    private readonly IRestAttributeProvider _attributeProvider = attributeProvider;
    private readonly RestClientOptions _options = options?.Value ?? new RestClientOptions();
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
            RestAttribute attribute = _attributeProvider.GetRestAttribute(request);

            RestRequestContext requestContext = new()
            {
                Attribute = attribute,
                Message = new(),
                Request = request,
                SerializerOptions = RestSettings.SerializerOptions,
                IsAborted = false
            };

            using RestRequest restRequest = await requestBuilder
                .BuildRequestAsync(requestContext, timeoutCts.Token)
                .ConfigureAwait(false);

            if (requestContext.IsAborted)
            {
                LogRequestFailed(_logger, new OperationCanceledException("Request was aborted by a request interceptor."), request.Name);
                return new RestResponse
                {
                    StatusCode = HttpStatusCode.RequestTimeout,
                    Version = httpClient.DefaultRequestVersion,
                    Headers = httpClient.DefaultRequestHeaders.ToElementCollection(),
                    Exception = new OperationCanceledException("Request was aborted by a request interceptor.")
                };
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
                SerializerOptions = RestSettings.SerializerOptions,
                IsAborted = requestContext.IsAborted
            };

            RestResponse restResponse = await responseBuilder
                .BuildResponseAsync(responseContext, timeoutCts.Token)
                .ConfigureAwait(false);

            LogReceivedResponse(_logger, restResponse.StatusCode, request.Name);

            return restResponse;
        }
        catch (OperationCanceledException)
            when (timeoutCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
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
            when (exception is not ArgumentNullException)
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


    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Sending {Method} request to {Uri} for {RequestName}")]
    private static partial void LogSendingRequest(ILogger logger, HttpMethod method, Uri? uri, string requestName);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Received {StatusCode} response for {RequestName}")]
    private static partial void LogReceivedResponse(ILogger logger, HttpStatusCode statusCode, string requestName);

    [LoggerMessage(EventId = 4, Level = LogLevel.Warning, Message = "Request {RequestName} timed out after {Timeout}")]
    private static partial void LogRequestTimeout(ILogger logger, string requestName, TimeSpan timeout);

    [LoggerMessage(EventId = 5, Level = LogLevel.Error, Message = "Request {RequestName} failed with exception")]
    private static partial void LogRequestFailed(ILogger logger, Exception exception, string requestName);
}