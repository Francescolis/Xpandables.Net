
/*******************************************************************************
 * Copyright (C) 2023 Francis-Black EWANE
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

using Xpandables.Net.Operations;
using Xpandables.Net.Primitives;

namespace Xpandables.Net.Commands;

internal sealed class Dispatcher(IServiceProvider serviceProvider)
    : IDispatcher
{
    public async ValueTask<IOperationResult<TResult>> GetAsync<TQuery, TResult>(
        TQuery query,
        CancellationToken cancellationToken = default)
        where TQuery : notnull, IQuery<TResult>
    {
        ArgumentNullException.ThrowIfNull(query);

        try
        {
            IQueryHandler<TQuery, TResult> handler =
              serviceProvider
              .GetRequiredService<IQueryHandler<TQuery, TResult>>();

            return await handler
                .HandleAsync(query, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception)
            when (exception is not ArgumentNullException
                and not OperationResultException)
        {
            return OperationResults
                .InternalError<TResult>()
                .WithError(ElementEntry.UndefinedKey, exception)
                .Build();
        }
    }

    public async ValueTask<IOperationResult> SendAsync<TCommand>(
        TCommand command,
        CancellationToken cancellationToken = default)
        where TCommand : notnull, ICommand
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            ICommandHandler<TCommand> handler =
        serviceProvider
        .GetRequiredService<ICommandHandler<TCommand>>();

            return await handler
                .HandleAsync(command, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception)
            when (exception is not ArgumentNullException
                and not OperationResultException)
        {
            return OperationResults
                .InternalError()
                .WithError(ElementEntry.UndefinedKey, exception)
                .Build();
        }
    }

    object? IServiceProvider.GetService(Type serviceType)
        => serviceProvider.GetService(serviceType);

    public IAsyncEnumerable<TResult> FetchAsync<TQuery, TResult>(
        TQuery query,
        CancellationToken cancellationToken = default)
        where TQuery : notnull, IAsyncQuery<TResult>
    {
        ArgumentNullException.ThrowIfNull(query);

        try
        {
            IAsyncQueryHandler<TQuery, TResult> handler =
        serviceProvider
        .GetRequiredService<IAsyncQueryHandler<TQuery, TResult>>();

            return handler.HandleAsync(query, cancellationToken);
        }
        catch (Exception exception)
            when (exception is not ArgumentNullException
                and not OperationResultException)
        {
            IOperationResult operationResult = exception.ToOperationResult();
            throw new OperationResultException(operationResult);
        }
    }
}