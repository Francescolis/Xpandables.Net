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

namespace Xpandables.Net.Executions.Pipelines;

/// <summary>
/// Represents the next delegate to be executed on a pipeline.
/// </summary>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public delegate Task<TResponse> RequestHandler<TResponse>()
    where TResponse : notnull;

/// <summary>
/// Defines a method to handle a request in a pipeline process.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public interface IPipelineDecorator<TRequest, TResponse>
    where TRequest : class
    where TResponse : notnull
{
    /// <summary>
    /// Handles the request in the pipeline.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="next">The next handler in the pipeline.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. 
    /// The task result contains the response.</returns>
    Task<TResponse> HandleAsync(
        TRequest request,
        RequestHandler<TResponse> next,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents the next delegate to be executed on a pipeline.
/// </summary>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public delegate IAsyncEnumerable<TResponse> RequestStreamHandler<TResponse>()
    where TResponse : notnull;

/// <summary>
/// Defines a decorator for handling stream request.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public interface IPipelineStreamDecorator<TRequest, TResponse>
    where TRequest : class
    where TResponse : notnull
{
    /// <summary>
    /// Handles the stream request.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="next">The next delegate in the chain.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An asynchronous enumerable of the response.</returns>
    IAsyncEnumerable<TResponse> HandleAsync(
        TRequest request,
        RequestStreamHandler<TResponse> next,
        CancellationToken cancellationToken = default);
}