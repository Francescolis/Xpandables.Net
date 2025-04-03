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
/// A wrapper for applying pipeline on requests.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public sealed class PipelineRequestHandler<TRequest, TResponse>(
    IHandler<TRequest, Task<TResponse>> decoratee,
    IEnumerable<IPipelineDecorator<TRequest, TResponse>> decorators) :
    IPipelineRequestHandler<TRequest, TResponse>
    where TRequest : class
    where TResponse : class
{
    /// <inheritdoc/>
    public Task<TResponse> HandleAsync(
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        Task<TResponse> result = decorators
           .Reverse()
           .Aggregate<IPipelineDecorator<TRequest, TResponse>,
           RequestHandler<TResponse>>(
               Handler,
               (next, decorator) => () => decorator.HandleAsync(
                   request,
                   next,
                   cancellationToken))();

        return result;

        Task<TResponse> Handler() => decoratee.Handle(request, cancellationToken);
    }
}

/// <summary>
/// A wrapper for applying pipeline on stream requests.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public sealed class PipelineStreamRequestHandler<TRequest, TResponse>(
    IHandler<TRequest, IAsyncEnumerable<TResponse>> decoratee,
    IEnumerable<IPipelineStreamDecorator<TRequest, TResponse>> decorators) :
    IPipelineStreamRequestHandler<TRequest, TResponse>
    where TRequest : class, IStreamRequest<TResponse>
    where TResponse : class
{
    /// <inheritdoc/>
    public IAsyncEnumerable<TResponse> HandleAsync(
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        IAsyncEnumerable<TResponse> result = decorators
            .Reverse()
            .Aggregate<IPipelineStreamDecorator<TRequest, TResponse>,
            RequestStreamHandler<TResponse>>(
                Handler,
                (next, decorator) => () => decorator.HandleAsync(
                    request,
                    next,
                    cancellationToken))();

        return result;

        IAsyncEnumerable<TResponse> Handler() => decoratee.Handle(request, cancellationToken);
    }
}