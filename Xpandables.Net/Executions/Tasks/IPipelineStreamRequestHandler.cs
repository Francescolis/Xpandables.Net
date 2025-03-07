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
/// Defines a pipeline when handling stream requests.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
public interface IPipelineStreamRequestHandler<TResult>
{
    /// <summary>
    /// Handles the stream request.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An asynchronous enumerable of the result.</returns>
    IAsyncEnumerable<TResult> HandleAsync(
         IStreamRequest<TResult> request,
         CancellationToken cancellationToken = default);
}

/// <summary>
/// A wrapper class for handling stream requests with decorators.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResult">The type of the result.</typeparam>
public sealed class PipelineQueryAsyncHandler<TRequest, TResult>(
    IStreamRequestHandler<TRequest, TResult> decoratee,
    IEnumerable<IPipelineAsyncDecorator<TRequest, TResult>> decorators) :
    IPipelineStreamRequestHandler<TResult>
    where TRequest : class, IStreamRequest<TResult>
{
    /// <inheritdoc/>
    public IAsyncEnumerable<TResult> HandleAsync(
        IStreamRequest<TResult> request,
        CancellationToken cancellationToken = default)
    {
        IAsyncEnumerable<TResult> results = decorators
            .Reverse()
            .Aggregate<IPipelineAsyncDecorator<TRequest, TResult>,
            RequestStreamHandler<TResult>>(
                Handler,
                (next, decorator) => () => decorator.HandleAsync(
                    (TRequest)request,
                    next,
                    cancellationToken))();

        return results;

        IAsyncEnumerable<TResult> Handler() =>
            decoratee.HandleAsync((TRequest)request, cancellationToken);
    }
}
