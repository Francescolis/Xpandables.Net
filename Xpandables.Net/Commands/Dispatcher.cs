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

using Xpandables.Net.Commands.Wrappers;
using Xpandables.Net.Executions;

namespace Xpandables.Net.Commands;

/// <summary>
/// Represents a dispatcher that handles various operations such as fetching, 
/// sending command and queries.
/// </summary>
public sealed class Dispatcher(IServiceProvider provider) : IDispatcher
{
    /// <inheritdoc/>
    public Task<IExecutionResult> SendAsync(
        ICommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Type commandWrapperType = typeof(CommandHandlerWrapper<>)
                .MakeGenericType(command.GetType());

            ICommandHandlerWrapper handler =
                (ICommandHandlerWrapper)provider
                .GetRequiredService(commandWrapperType);

            return handler.HandleAsync(command, cancellationToken);
        }
        catch (Exception exception)
            when (exception is not ExecutionResultException)
        {
            return Task.FromResult(exception.ToExecutionResult());
        }
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<TResult> SendAsync<TResult>(
        IQueryAsync<TResult> query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Type requestWrapperType = typeof(QueryAsyncHandlerWrapper<,>)
                .MakeGenericType(query.GetType(), typeof(TResult));

            IQueryAsyncHandlerWrapper<TResult> handler =
                (IQueryAsyncHandlerWrapper<TResult>)provider
                .GetRequiredService(requestWrapperType);

            return handler.HandleAsync(query, cancellationToken);
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
        IQuery<TResult> query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Type requestWrapperType = typeof(QueryHandlerWrapper<,>)
                .MakeGenericType(query.GetType(), typeof(TResult));

            IQueryHandlerWrapper<TResult> handler =
                (IQueryHandlerWrapper<TResult>)provider
                .GetRequiredService(requestWrapperType);

            return handler.HandleAsync(query, cancellationToken);
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