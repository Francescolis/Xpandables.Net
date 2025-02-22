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
using Xpandables.Net.Executions.Pipelines;

namespace Xpandables.Net.Executions.Tasks;

/// <summary>
/// Defines a pipeline when handling requests.
/// </summary>
public interface IPipelineRequestHandler
{
    /// <summary>
    /// Handles the specified request asynchronously.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation
    /// requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task 
    /// result contains the operation result.</returns>
    Task<IExecutionResult> HandleAsync(
        IRequest request,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// A wrapper for handling requests with decorators.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
public sealed class PipelineRequestHandler<TRequest>(
    IRequestHandler<TRequest> decoratee,
    IEnumerable<IPipelineDecorator<TRequest, IExecutionResult>> decorators) :
    IPipelineRequestHandler
    where TRequest : class, IRequest
{
    /// <summary>
    /// Handles the request asynchronously.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the operation.</returns>
    public Task<IExecutionResult> HandleAsync(
        IRequest request,
        CancellationToken cancellationToken = default)
    {
        Task<IExecutionResult> result = decorators
            .Reverse()
            .Aggregate<IPipelineDecorator<TRequest, IExecutionResult>,
            RequestHandler<IExecutionResult>>(
                Handler,
                (next, decorator) => () => decorator.HandleAsync(
                    (TRequest)request,
                    next,
                    cancellationToken))();

        return result;

        Task<IExecutionResult> Handler() =>
            decoratee.HandleAsync((TRequest)request, cancellationToken);
    }
}