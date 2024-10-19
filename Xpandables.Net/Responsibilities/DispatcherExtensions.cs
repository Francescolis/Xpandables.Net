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

using Xpandables.Net.Events.Aggregates;
using Xpandables.Net.Operations;

namespace Xpandables.Net.Responsibilities;

/// <summary>
/// Provides extension methods for the <see cref="IDispatcher"/> interface.
/// </summary>
public static class DispatcherExtensions
{
    /// <summary>
    /// Sends a command asynchronously using the dispatcher.
    /// </summary>
    /// <typeparam name="TAggregate">The type of the aggregate.</typeparam>
    /// <param name="dispatcher">The dispatcher instance.</param>
    /// <param name="command">The command to send.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The operation result.</returns>
    public static async Task<IOperationResult> SendAsync<TAggregate>(
        this IDispatcher dispatcher,
        ICommandAggregate<TAggregate> command,
        CancellationToken cancellationToken = default)
        where TAggregate : class, IAggregate, new()
    {
        try
        {
            Type requestWrapperType = typeof(CommandAggregateHandlerWrapper<,>)
                .MakeGenericType(command.GetType(), typeof(TAggregate));

            ICommandAggregateHandlerWrapper<TAggregate> handler =
                (ICommandAggregateHandlerWrapper<TAggregate>)dispatcher
                .GetRequiredService(requestWrapperType);

            return await handler
                .HandleAsync(command, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception)
            when (exception is not OperationResultException)
        {
            return OperationResults
                .InternalServerError<TAggregate>()
                .WithException(exception)
                .Build();
        }
    }

    /// <summary>
    /// Gets a query result asynchronously using the dispatcher.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="dispatcher">The dispatcher instance.</param>
    /// <param name="query">The query to get the result for.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The operation result with the query result.</returns>
    public static async Task<IOperationResult<TResult>> GetAsync<TResult>(
        this IDispatcher dispatcher,
        IQuery<TResult> query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Type requestWrapperType = typeof(QueryHandlerWrapper<,>)
                .MakeGenericType(query.GetType(), typeof(TResult));

            IQueryHandlerWrapper<TResult> handler =
                (IQueryHandlerWrapper<TResult>)dispatcher
                .GetRequiredService(requestWrapperType);

            return await handler.HandleAsync(query, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception)
            when (exception is not OperationResultException)
        {
            return OperationResults
                .InternalServerError<TResult>()
                .WithException(exception)
                .Build();
        }
    }

    /// <summary>
    /// Fetches a query result asynchronously using the dispatcher.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="dispatcher">The dispatcher instance.</param>
    /// <param name="query">The query to fetch the result for.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An asynchronous enumerable of the query result.</returns>
    public static IAsyncEnumerable<TResult> FetchAsync<TResult>(
        this IDispatcher dispatcher,
        IQueryAsync<TResult> query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Type requestWrapperType = typeof(QueryAsyncHandlerWrapper<,>)
                .MakeGenericType(query.GetType(), typeof(TResult));

            IQueryAsyncHandlerWrapper<TResult> handler =
                (IQueryAsyncHandlerWrapper<TResult>)dispatcher
                .GetRequiredService(requestWrapperType);

            return handler.HandleAsync(query, cancellationToken);
        }
        catch (Exception exception)
        when (exception is not OperationResultException)
        {
            throw new OperationResultException(exception.ToOperationResult());
        }
    }
}
