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

using Xpandables.Net.Commands;
using Xpandables.Net.Executions;
using Xpandables.Net.Operations;

namespace Xpandables.Net.Pipelines;

/// <summary>
/// Represents an asynchronous pipeline decorator that handles 
/// <see cref="IQueryAsync{TResult}"/> request and produces a response.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public abstract class PipelineAsyncDecorator<TRequest, TResponse> :
    IPipelineAsyncDecorator<TRequest, TResponse>
    where TRequest : class, IQueryAsync<TResponse>
{
    /// <inheritdoc/>
    public IAsyncEnumerable<TResponse> HandleAsync(
        TRequest query,
        RequestAsyncHandler<TResponse> next,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return HandleCoreAsync(query, next, cancellationToken);
        }
        catch (Exception exception)
            when (exception is not ExecutionResultException)
        {
            throw new ExecutionResultException(
                exception.ToExecutionResult());
        }
    }

    /// <summary>
    /// Handles the core logic of the pipeline decorator.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="next">The next delegate in the pipeline.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An asynchronous enumerable of the response.</returns>
    protected abstract IAsyncEnumerable<TResponse> HandleCoreAsync(
        TRequest request,
        RequestAsyncHandler<TResponse> next,
        CancellationToken cancellationToken = default);
}
