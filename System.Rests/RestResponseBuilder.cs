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

using System.Net;
using System.Rests.Abstractions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace System.Rests;

/// <summary>
/// Provides functionality to build REST responses using a collection of response composers and optional response
/// interceptors.
/// </summary>
/// <remarks>If the response context is aborted, an empty response is returned. If no suitable composer is found
/// for the context, an InvalidOperationException is thrown. Response interceptors are executed in the order provided,
/// and the process may be short-circuited if the context is aborted during interception.</remarks>
/// <param name="composers">An enumerable collection of response composers used to generate responses based on the provided context. Cannot be
/// null.</param>
/// <param name="responseInterceptors">An optional enumerable collection of response interceptors that can modify or short-circuit the response before it
/// is returned.</param>
/// <param name="logger">An optional logger instance used to record informational messages and errors during the response building process.</param>
public sealed partial class RestResponseBuilder(
    IEnumerable<IRestResponseComposer> composers,
    IEnumerable<IRestResponseInterceptor>? responseInterceptors = null,
    ILogger<RestResponseBuilder>? logger = null) : IRestResponseBuilder
{
    private readonly ILogger<RestResponseBuilder> _logger = logger ?? NullLogger<RestResponseBuilder>.Instance;
    private readonly IRestResponseInterceptor[] _responseInterceptors = [.. (responseInterceptors ?? []).OrderBy(i => i.Order)];

    /// <inheritdoc />
    public async ValueTask<RestResponse> BuildResponseAsync(
        RestResponseContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.IsAborted)
        {
            return RestResponse.Empty;
        }

        IRestResponseComposer composer =
            composers.FirstOrDefault(c => c.CanCompose(context))
            ?? throw new InvalidOperationException(
                $"{nameof(BuildResponseAsync)}: No composer found for the provided context.");

        try
        {
			RestResponse response = await composer
                .ComposeAsync(context, cancellationToken)
                .ConfigureAwait(false);

            return await ExecuteResponseInterceptorsAsync(context, response, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception)
            when (exception is not ArgumentNullException
                      and not OperationCanceledException
                      and not InvalidOperationException)
        {
            throw new InvalidOperationException(
                "The response builder failed to build the response.",
                exception);
        }
    }

    private async ValueTask<RestResponse> ExecuteResponseInterceptorsAsync(
        RestResponseContext context,
        RestResponse response,
        CancellationToken cancellationToken)
    {
        RestResponse currentResponse = response;
        foreach (IRestResponseInterceptor interceptor in _responseInterceptors)
        {
            cancellationToken.ThrowIfCancellationRequested();

            currentResponse = await interceptor
                .InterceptAsync(context, currentResponse, cancellationToken)
                .ConfigureAwait(false);

            if (context.IsAborted)
            {
                LogShortCircuitedResponse(_logger, context.Request.Name, currentResponse.StatusCode);
            }
        }

        return currentResponse;
    }

    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "Request {RequestName} short-circuited with status {StatusCode}")]
    private static partial void LogShortCircuitedResponse(ILogger logger, string requestName, HttpStatusCode statusCode);
}