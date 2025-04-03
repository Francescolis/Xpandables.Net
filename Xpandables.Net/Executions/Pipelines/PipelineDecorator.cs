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
/// Represents an abstract base class for pipeline decorators.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public abstract class PipelineDecorator<TRequest, TResponse> :
    IPipelineDecorator<TRequest, TResponse>
    where TRequest : class
    where TResponse : class
{
    /// <inheritdoc/>
    public Task<TResponse> HandleAsync(
        TRequest request,
        RequestHandler<TResponse> next,
        CancellationToken cancellationToken = default) =>
        HandleCoreAsync(request, next, cancellationToken);

    /// <summary>
    /// Handles the core logic of the pipeline decorator.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="next">The next delegate in the pipeline.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.
    /// the task result contains the response.</returns>
    protected abstract Task<TResponse> HandleCoreAsync(
        TRequest request,
        RequestHandler<TResponse> next,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Matches the provided operation result to the expected response type.
    /// </summary>
    /// <remarks><typeparamref name="TResponse"/> must be of type
    /// <see cref="ExecutionResult"/> or <see cref="ExecutionResult{TResult}"/>.</remarks>
    /// <param name="executionResult">The operation result to match.</param>
    /// <returns>The matched response of type TResponse.</returns>
    /// <exception cref="InvalidOperationException">The response type must be of 
    /// type ExecutionResult or ExecutionResult{T}.</exception>"
    protected Task<TResponse> MatchResponseAsync(ExecutionResult executionResult)
    {
        if (!typeof(ExecutionResult).IsAssignableFrom(typeof(ExecutionResult))
            && !typeof(ExecutionResult).IsAssignableFrom(typeof(ExecutionResult<>)))
            throw new InvalidOperationException(
                $"The response type must be of type ExecutionResult or ExecutionResult<T>.");

        if (typeof(TResponse).IsGenericType)
        {
            Type resultType = typeof(TResponse).GetGenericArguments()[0];
            return Task.FromResult((executionResult.ToExecutionResult(resultType) as TResponse)!);
        }

        return Task.FromResult((executionResult as TResponse)!);
    }
}

/// <summary>
/// Represents an abstract base class for pipeline stream decorators.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public abstract class PipelineStreamDecorator<TRequest, TResponse> :
    IPipelineStreamDecorator<TRequest, TResponse>
    where TRequest : class
    where TResponse : class
{
    /// <inheritdoc/>
    public IAsyncEnumerable<TResponse> HandleAsync(
        TRequest request,
        RequestStreamHandler<TResponse> next,
        CancellationToken cancellationToken = default) =>
        HandleCoreAsync(request, next, cancellationToken);

    /// <summary>
    /// Handles the core logic of the pipeline stream decorator.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="next">The next delegate in the pipeline.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An asynchronous enumerable that represents the asynchronous operation.</returns>
    protected abstract IAsyncEnumerable<TResponse> HandleCoreAsync(
        TRequest request,
        RequestStreamHandler<TResponse> next,
        CancellationToken cancellationToken = default);
}