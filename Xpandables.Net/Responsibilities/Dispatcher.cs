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

using Xpandables.Net.Events;
using Xpandables.Net.Operations;

namespace Xpandables.Net.Responsibilities;

/// <summary>
/// Represents a dispatcher that handles various operations such as fetching, 
/// sending, and publishing events.
/// </summary>
public sealed class Dispatcher(IServiceProvider provider) : IDispatcher
{
    private readonly IEventPublisher _eventPublisher = provider
        .GetRequiredService<IEventPublisher>();

    /// <inheritdoc/>
    public IAsyncEnumerable<TResult> FetchAsync<TQuery, TResult>(
        TQuery query,
        CancellationToken cancellationToken = default)
        where TQuery : notnull, IQueryAsync<TResult>
    {
        try
        {
            IQueryAsyncHandler<TQuery, TResult> handler =
                provider.GetRequiredService<IQueryAsyncHandler<TQuery, TResult>>();

            return handler.HandleAsync(query, cancellationToken);
        }
        catch (Exception exception)
            when (exception is not OperationResultException)
        {
            IOperationResult operation = exception.ToOperationResult();
            throw new OperationResultException(operation);
        }
    }

    /// <inheritdoc/>
    public Task<IOperationResult<TResult>> GetAsync<TQuery, TResult>(
        TQuery query,
        CancellationToken cancellationToken = default)
        where TQuery : notnull, IQuery<TResult>
    {
        try
        {
            IQueryHandler<TQuery, TResult> handler =
                provider.GetRequiredService<IQueryHandler<TQuery, TResult>>();

            return handler.HandleAsync(query, cancellationToken);
        }
        catch (Exception exception)
            when (exception is not OperationResultException)
        {
            return Task.FromResult(OperationResults
                .InternalServerError<TResult>()
                .WithException(exception)
                .Build());
        }
    }

    /// <inheritdoc/>
    public Task<IOperationResult> PublishAsync<TEvent>(
        TEvent @event,
        CancellationToken cancellationToken = default) where TEvent : notnull, IEvent => throw new NotImplementedException();
    /// <inheritdoc/>
    public Task<IOperationResult<IEnumerable<EventPublished>>> PublishAsync<TEvent>(
        IEnumerable<TEvent> events,
        CancellationToken cancellationToken = default)
        where TEvent : notnull, IEvent =>
        _eventPublisher.PublishAsync(events, cancellationToken);

    /// <inheritdoc/>
    public Task<IOperationResult> SendAsync<TCommand>(
        TCommand command,
        CancellationToken cancellationToken = default)
        where TCommand : notnull, ICommand
    {
        try
        {
            ICommandHandler<TCommand> handler =
                provider.GetRequiredService<ICommandHandler<TCommand>>();

            return handler.HandleAsync(command, cancellationToken);
        }
        catch (Exception exception)
            when (exception is not OperationResultException)
        {
            return Task.FromResult(OperationResults
                .InternalServerError()
                .WithException(exception)
                .Build());
        }
    }
    object? IServiceProvider.GetService(Type serviceType)
        => provider.GetService(serviceType);
    Task<IOperationResult> IDispatcher.SendAsync<TCommand, TAggregate>(
        TCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            ICommandAggregateHandler<TCommand, TAggregate> handler =
                provider.GetRequiredService<ICommandAggregateHandler<TCommand, TAggregate>>();

            return handler.HandleAsync(command, cancellationToken);
        }
        catch (Exception exception)
            when (exception is not OperationResultException)
        {
            return Task.FromResult(OperationResults
                .InternalServerError()
                .WithException(exception)
                .Build());
        }
    }
}