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

using Xpandables.Net.Operations;

namespace Xpandables.Net.Responsibilities.Decorators;

/// <summary>
/// A decorator that logs the handling of a request and its response.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public sealed class LoggingPipelineDecorator<TRequest, TResponse>(
    ILogger<LoggingPipelineDecorator<TRequest, TResponse>> logger) :
    PipelineDecorator<TRequest, TResponse>
    where TRequest : class
    where TResponse : IOperationResult
{
    /// <inheritdoc/>
    protected override async Task<TResponse> HandleCoreAsync(
        TRequest query,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Handling {RequestName} with request: " +
            "{@Request}", typeof(TRequest).Name, query);

        try
        {
            TResponse response = await next().ConfigureAwait(false);

            logger.LogInformation("Handled {RequestName} with response: " +
                "{@Response}", typeof(TRequest).Name, response);

            return response;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error handling {RequestName} with request: " +
                "{@Request}", typeof(TRequest).Name, query);

            throw;
        }
    }
}