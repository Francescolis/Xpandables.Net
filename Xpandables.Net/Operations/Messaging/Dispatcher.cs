
/************************************************************************************************************
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
************************************************************************************************************/
using Microsoft.Extensions.DependencyInjection;

namespace Xpandables.Net.Operations.Messaging;

internal sealed class Dispatcher(IServiceProvider serviceProvider) : IDispatcher
{
    private readonly IServiceProvider _serviceProvider =
        serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

    public async ValueTask<OperationResult<TResult>> GetAsync<TQuery, TResult>(
        TQuery query, CancellationToken cancellationToken = default)
        where TQuery : notnull, IQuery<TResult>
    {
        ArgumentNullException.ThrowIfNull(query);

        IQueryHandler<TQuery, TResult> handler =
            _serviceProvider.GetRequiredService<IQueryHandler<TQuery, TResult>>();

        return await handler.HandleAsync(query, cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask<OperationResult> SendAsync<TCommand>(
        TCommand command, CancellationToken cancellationToken = default)
        where TCommand : notnull, ICommand
    {
        ArgumentNullException.ThrowIfNull(command);

        ICommandHandler<TCommand> handler =
            _serviceProvider.GetRequiredService<ICommandHandler<TCommand>>();

        return await handler.HandleAsync(command, cancellationToken).ConfigureAwait(false);
    }

    object? IServiceProvider.GetService(Type serviceType) => _serviceProvider.GetService(serviceType);

    public IAsyncEnumerable<TResult> FetchAsync<TQuery, TResult>(
        TQuery query, CancellationToken cancellationToken = default)
        where TQuery : notnull, IAsyncQuery<TResult>
    {
        ArgumentNullException.ThrowIfNull(query);

        IAsyncQueryHandler<TQuery, TResult> handler =
            _serviceProvider.GetRequiredService<IAsyncQueryHandler<TQuery, TResult>>();

        return handler.HandleAsync(query, cancellationToken);
    }
}