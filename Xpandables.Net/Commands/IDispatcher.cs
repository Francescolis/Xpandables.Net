
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

using Xpandables.Net.Operations;

namespace Xpandables.Net.Commands;

/// <summary>
/// Defines a set of methods to automatically dispatches commands and queries.
/// The implementation must be 
/// thread-safe when working in a multi-threaded environment.
/// </summary>
public interface IDispatcher : IServiceProvider
{
    /// <summary>
    /// Asynchronously send the command to the 
    /// <see cref="ICommandHandler{TCommand}"/> implementation handler.
    /// </summary>
    /// <param name="command">The command to act on.</param>
    /// <param name="cancellationToken">A CancellationToken to 
    /// observe while waiting for the task to complete.</param>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="command"/> is null.</exception>
    /// <returns>A task that represents an <see cref="IOperationResult"/>
    /// .</returns>
    ValueTask<IOperationResult> SendAsync<TCommand>(
        TCommand command,
        CancellationToken cancellationToken = default)
        where TCommand : notnull, ICommand;

    /// <summary>
    /// Asynchronously gets the result of the query using
    /// the <see cref="IQueryHandler{TQuery, TResult}"/> 
    /// implementation and returns a result.
    /// </summary>
    /// <typeparam name="TQuery">Type of the query</typeparam>
    /// <typeparam name="TResult">Type of the result.</typeparam>
    /// <param name="query">The query to act on.</param>
    /// <param name="cancellationToken">A CancellationToken to 
    /// observe while waiting for the task to complete.</param>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="query"/> is null.</exception>
    /// <returns>A task that represents an 
    /// <see cref="IOperationResult{TValue}"/>.</returns>
    ValueTask<IOperationResult<TResult>> GetAsync<TQuery, TResult>(
        TQuery query,
        CancellationToken cancellationToken = default)
        where TQuery : notnull, IQuery<TResult>;

    /// <summary>
    /// Asynchronously fetches the result from the query using
    /// the <see cref="IAsyncQueryHandler{TQuery, TResult}"/> implementation
    /// and returns an enumerator of <typeparamref name="TResult"/> 
    /// that can be asynchronously enumerated.
    /// </summary>
    /// <typeparam name="TQuery">Type of the query</typeparam>
    /// <typeparam name="TResult">Type of the result.</typeparam>
    /// <param name="query">The query to act on.</param>
    /// <param name="cancellationToken">A CancellationToken 
    /// to observe while waiting for the task to complete.</param>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="query"/> is null.</exception>
    /// <exception cref="OperationResultException">The operation failed
    /// .</exception>
    /// <returns>An enumerator of <typeparamref name="TResult"/> 
    /// that can be asynchronously enumerated.</returns>
    IAsyncEnumerable<TResult> FetchAsync<TQuery, TResult>(
        TQuery query,
        CancellationToken cancellationToken = default)
        where TQuery : notnull, IAsyncQuery<TResult>;
}