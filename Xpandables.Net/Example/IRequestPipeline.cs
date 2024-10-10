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

namespace Xpandables.Net.Example;

/// <summary>  
/// Represents a delegate that processes a request and returns a response  
/// asynchronously.  
/// </summary>  
/// <typeparam name="TRequest">The type of the request.</typeparam>  
/// <typeparam name="TResponse">The type of the response.</typeparam>  
/// <param name="request">The request to process.</param>  
/// <param name="cancellationToken">A token to monitor for cancellation  
/// requests.</param>  
/// <returns>A task that represents the asynchronous operation,  
/// containing the response.</returns>  
public delegate Task<TResponse> RequestDelegate<TRequest, TResponse>(
   TRequest request,
   CancellationToken cancellationToken)
  where TResponse : IOperationResult;

/// <summary>  
/// Defines a pipeline for handling requests and responses.  
/// </summary>  
/// <typeparam name="TRequest">The type of the request.</typeparam>  
/// <typeparam name="TResponse">The type of the response.</typeparam>  
public interface IRequestPipeline<TRequest, TResponse>
  where TRequest : notnull
  where TResponse : class, IOperationResult
{
    /// <summary>  
    /// Handles the request asynchronously.  
    /// </summary>  
    /// <param name="request">The request to handle.</param>  
    /// <param name="next">The next delegate in the pipeline.</param>  
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>  
    /// <returns>A task that represents the asynchronous operation, containing the response.</returns>  
    Task<TResponse> HandleAsync(
        TRequest request,
        RequestDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken = default);
}

/// <inheritdoc/>
public class RequestHandlerPipeline<TRequest, TResponse> : IRequestPipeline<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : class, IOperationResult
{
    /// <inheritdoc/>
    public async Task<TResponse> HandleAsync(
        TRequest request,
        RequestDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken = default) =>
        // Add cross-cutting logic here, like logging, validation, etc.
        await next(request, cancellationToken);
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class LoggingPipeline<TRequest, TResponse>(ILogger<LoggingPipeline<TRequest, TResponse>> logger) : IRequestPipeline<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : class, IOperationResult
{
    private readonly ILogger<LoggingPipeline<TRequest, TResponse>> _logger = logger;

    public async Task<TResponse> HandleAsync(TRequest request, RequestDelegate<TRequest, TResponse> next, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling request: {RequestName}", typeof(TRequest).Name);

        TResponse result = await next(request, cancellationToken);

        _logger.LogInformation("Request {RequestName} handled successfully with result: {Result}", typeof(TRequest).Name, result);

        return result;
    }
}
