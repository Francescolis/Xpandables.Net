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

using Microsoft.Extensions.DependencyInjection;

namespace Xpandables.Net.Executions.Tasks;

/// <summary>
/// Represents a mediator that handles various operations such as fetching, 
/// sending requests.
/// </summary>
internal sealed class Mediator(IServiceProvider provider) : IMediator
{
    /// <inheritdoc/>
    public Task<ExecutionResult> SendAsync(
        IRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Type pipelineRequestHandlerType = typeof(IPipelineRequestHandler<,>)
                .MakeGenericType(request.GetType(), typeof(ExecutionResult));

            IPipelineRequestHandler<ExecutionResult> handler =
                (IPipelineRequestHandler<ExecutionResult>)provider
                .GetRequiredService(pipelineRequestHandlerType);

            return handler.HandleAsync(request, cancellationToken);
        }
        catch (Exception exception)
            when (exception is not ExecutionResultException)
        {
            return Task.FromResult(exception.ToExecutionResult());
        }
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<TResult> SendAsync<TResult>(
        IStreamRequest<TResult> request,
        CancellationToken cancellationToken = default)
        where TResult : notnull
    {
        try
        {
            Type pipelineRequestHandlerType = typeof(IPipelineStreamRequestHandler<,>)
                .MakeGenericType(request.GetType(), typeof(TResult));

            IPipelineStreamRequestHandler<TResult> handler =
                (IPipelineStreamRequestHandler<TResult>)provider
                .GetRequiredService(pipelineRequestHandlerType);

            return handler.HandleAsync(request, cancellationToken);
        }
        catch (Exception exception)
            when (exception is not ExecutionResultException)
        {
            ExecutionResult execution = exception.ToExecutionResult();
            throw new ExecutionResultException(execution);
        }
    }

    /// <inheritdoc/>
    public Task<ExecutionResult<TResult>> SendAsync<TResult>(
        IRequest<TResult> request,
        CancellationToken cancellationToken = default)
        where TResult : notnull
    {
        try
        {
            Type pipelineRequestHandlerType = typeof(IPipelineRequestHandler<,>)
                .MakeGenericType(request.GetType(), typeof(ExecutionResult<TResult>));

            IPipelineRequestHandler<ExecutionResult<TResult>> handler =
                (IPipelineRequestHandler<ExecutionResult<TResult>>)provider
                .GetRequiredService(pipelineRequestHandlerType);

            return handler.HandleAsync(request, cancellationToken);
        }
        catch (Exception exception)
            when (exception is not ExecutionResultException)
        {
            return Task.FromResult(exception
                .ToExecutionResult()
                .ToExecutionResult<TResult>());
        }
    }
}