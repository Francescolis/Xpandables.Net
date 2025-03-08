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
/// Represents a dispatcher that handles various operations such as fetching, 
/// sending requests.
/// </summary>
internal sealed class Dispatcher(IServiceProvider provider) : IDispatcher
{
    /// <inheritdoc/>
    public Task<IExecutionResult> SendAsync(
        IRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Type pipelineRequestHandlerType = typeof(PipelineRequestHandler<,>)
                .MakeGenericType(request.GetType(), typeof(Task<IExecutionResult>));

            IPipelineRequestHandler<Task<IExecutionResult>> handler =
                (IPipelineRequestHandler<Task<IExecutionResult>>)provider
                .GetRequiredService(pipelineRequestHandlerType);

            return handler.Handle(request, cancellationToken);
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
    {
        try
        {
            Type pipelineRequestHandlerType = typeof(PipelineRequestHandler<,>)
                .MakeGenericType(request.GetType(), typeof(IAsyncEnumerable<TResult>));

            IPipelineRequestHandler<IAsyncEnumerable<TResult>> handler =
                (IPipelineRequestHandler<IAsyncEnumerable<TResult>>)provider
                .GetRequiredService(pipelineRequestHandlerType);

            return handler.Handle(request, cancellationToken);
        }
        catch (Exception exception)
            when (exception is not ExecutionResultException)
        {
            IExecutionResult execution = exception.ToExecutionResult();
            throw new ExecutionResultException(execution);
        }
    }

    /// <inheritdoc/>
    public Task<IExecutionResult<TResult>> SendAsync<TResult>(
        IRequest<TResult> request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Type pipelineRequestHandlerType = typeof(PipelineRequestHandler<,>)
                .MakeGenericType(request.GetType(), typeof(Task<IExecutionResult<TResult>>));

            IPipelineRequestHandler<Task<IExecutionResult<TResult>>> handler =
                (IPipelineRequestHandler<Task<IExecutionResult<TResult>>>)provider
                .GetRequiredService(pipelineRequestHandlerType);

            return handler.Handle(request, cancellationToken);
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