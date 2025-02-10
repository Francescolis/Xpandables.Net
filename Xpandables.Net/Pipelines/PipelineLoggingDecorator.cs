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
using Microsoft.Extensions.Logging;

using Xpandables.Net.Executions;

namespace Xpandables.Net.Pipelines;

/// <summary>
/// A decorator that logs the handling of a request and its response.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public sealed class PipelineLoggingDecorator<TRequest, TResponse>(
    ILogger<PipelineLoggingDecorator<TRequest, TResponse>> logger) :
    PipelineDecorator<TRequest, TResponse>
    where TRequest : class
    where TResponse : IExecutionResult
{
    /// <inheritdoc/>
    protected override async Task<TResponse> HandleCoreAsync(
        TRequest query,
        RequestHandler<TResponse> next,
        CancellationToken cancellationToken = default)
    {
#pragma warning disable CA1848 // Use the LoggerMessage delegates
        logger.LogInformation("Handling {RequestName} with request: " +
            "{@Request}", typeof(TRequest).Name, query);
#pragma warning restore CA1848 // Use the LoggerMessage delegates

        try
        {
            TResponse response = await next().ConfigureAwait(false);

#pragma warning disable CA1848 // Use the LoggerMessage delegates
            logger.LogInformation("Handled {RequestName} with response: " +
                "{@Response}", typeof(TRequest).Name, response);
#pragma warning restore CA1848 // Use the LoggerMessage delegates

            return response;
        }
        catch (Exception exception)
        {
#pragma warning disable CA1848 // Use the LoggerMessage delegates
            logger.LogError(exception, "Error handling {RequestName} with request: " +
                "{@Request}", typeof(TRequest).Name, query);
#pragma warning restore CA1848 // Use the LoggerMessage delegates

            throw;
        }
    }
}