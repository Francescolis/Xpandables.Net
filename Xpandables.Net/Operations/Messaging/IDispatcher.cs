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

/// <summary>
/// Defines a set of methods to automatically dispatches commands and queries.
/// The implementation must be thread-safe when working in a multi-threaded environment.
/// </summary>
public interface IDispatcher : IServiceProvider
{
    /// <summary>
    /// Asynchronously send the command to the <see cref="ICommandHandler{TCommand}"/> implementation handler.
    /// </summary>
    /// <param name="command">The command to act on.</param>
    /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="command"/> is null.</exception>
    /// <exception cref="OperationResultException">The operation failed.</exception>
    /// <exception cref="InvalidOperationException">The operation failed. See inner exception.</exception>
    /// <returns>A task that represents an <see cref="OperationResult"/>.</returns>
    ValueTask<OperationResult> SendAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : notnull, ICommand;

    /// <summary>
    /// Asynchronously gets the result of the query using
    /// the <see cref="IQueryHandler{TQuery, TResult}"/> implementation and returns a result.
    /// </summary>
    /// <typeparam name="TQuery">Type of the query</typeparam>
    /// <typeparam name="TResult">Type of the result.</typeparam>
    /// <param name="query">The query to act on.</param>
    /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="query"/> is null.</exception>
    /// <exception cref="OperationResultException">The operation failed.</exception>
    /// <exception cref="InvalidOperationException">The operation failed. See inner exception.</exception>
    /// <returns>A task that represents an <see cref="OperationResult{TValue}"/>.</returns>
    ValueTask<OperationResult<TResult>> GetAsync<TQuery, TResult>(
        TQuery query, CancellationToken cancellationToken = default)
        where TQuery : notnull, IQuery<TResult>;

    /// <summary>
    /// Asynchronously fetches the result from the query using
    /// the <see cref="IAsyncQueryHandler{TQuery, TResult}"/> implementation
    /// and returns an enumerator of <typeparamref name="TResult"/> that can be asynchronously enumerated.
    /// </summary>
    /// <typeparam name="TQuery">Type of the query</typeparam>
    /// <typeparam name="TResult">Type of the result.</typeparam>
    /// <param name="query">The query to act on.</param>
    /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="query"/> is null.</exception>
    /// <exception cref="OperationResultException">The operation failed.</exception>
    /// <exception cref="InvalidOperationException">The operation failed. See inner exception.</exception>
    /// <returns>An enumerator of <typeparamref name="TResult"/> that can be asynchronously enumerated.</returns>
    IAsyncEnumerable<TResult> FetchAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default)
        where TQuery : notnull, IAsyncQuery<TResult>;
}

/// <summary>
/// Provides with extension method for the dispatcher.
/// </summary>
public static class DispatcherExtensions
{
    /// <summary>
    /// Asynchronously fetches the result from the query using
    /// the <see cref="IQueryHandler{TQuery, TResult}"/> implementation and returns a result.
    /// </summary>
    /// <typeparam name="TResult">Type of the result.</typeparam>
    /// <param name="dispatcher">The target dispatcher instance.</param>
    /// <param name="query">The query to act on.</param>
    /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="query"/> is null.</exception>
    /// <exception cref="OperationResultException">The operation failed.</exception>
    /// <exception cref="InvalidOperationException">The operation failed. See inner exception.</exception>
    /// <returns>A task that represents an <see cref="OperationResult{TValue}"/>.</returns>
    public static async ValueTask<OperationResult<TResult>> GetAsync<TResult>(
        this IDispatcher dispatcher,
        IQuery<TResult> query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(dispatcher);

        try
        {
            Type queryWrapperType = typeof(QueryHandlerWrapper<,>)
                .MakeGenericType(query.GetType(), typeof(TResult));

            var handler = (IQueryHandlerWrapper<TResult>)dispatcher
                .GetRequiredService(queryWrapperType);

            return await handler.HandleAsync(query, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is not ArgumentNullException
                                                and not OperationResultException
                                                and not InvalidOperationException)
        {
            return OperationResults
                .InternalError<TResult>()
                .WithError(ElementEntry.UndefinedKey, exception)
                .Build();
        }
    }

    /// <summary>
    /// Asynchronously fetches the result from the query
    /// using the <see cref="IAsyncQueryHandler{TQuery, TResult}"/> implementation
    /// and returns an enumerator of <typeparamref name="TResult"/> that can be asynchronously enumerated.
    /// </summary>
    /// <typeparam name="TResult">Type of the result.</typeparam>
    /// <param name="dispatcher">The target dispatcher instance.</param>
    /// <param name="query">The query to act on.</param>
    /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="query"/> is null.</exception>
    /// <exception cref="OperationResultException">The operation failed.</exception>
    /// <exception cref="InvalidOperationException">The operation failed. See inner exception.</exception>
    /// <returns>An enumerator of <typeparamref name="TResult"/> that can be asynchronously enumerated.</returns>
    public static IAsyncEnumerable<TResult> FetchAsync<TResult>(
        this IDispatcher dispatcher,
        IAsyncQuery<TResult> query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(dispatcher);

        try
        {
            Type queryWrapperType = typeof(AsyncQueryHandlerWrapper<,>)
                .MakeGenericType(query.GetType(), typeof(TResult));

            var handler = (IAsyncQueryHandlerWrapper<TResult>)dispatcher
                .GetRequiredService(queryWrapperType);

            return handler.HandleAsync(query, cancellationToken);
        }
        catch (Exception exception) when (exception is not ArgumentNullException
                                            and not OperationResultException
                                            and not InvalidOperationException)
        {
            throw new InvalidOperationException(exception.Message, exception);
        }
    }
}

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