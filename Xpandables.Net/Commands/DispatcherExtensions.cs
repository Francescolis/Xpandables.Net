
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

using Xpandables.Net.Operations;
using Xpandables.Net.Primitives;

namespace Xpandables.Net.Commands;

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
    /// <returns>A task that represents an <see cref="IOperationResult{TValue}"/>.</returns>
    public static async ValueTask<IOperationResult<TResult>> GetAsync<TResult>(
        this IDispatcher dispatcher,
        IQuery<TResult> query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(dispatcher);

        try
        {
            Type queryWrapperType = typeof(QueryHandlerWrapper<,>)
                .MakeGenericType(query.GetType(), typeof(TResult));

            IQueryHandlerWrapper<TResult> handler = (IQueryHandlerWrapper<TResult>)dispatcher
                .GetRequiredService(queryWrapperType);

            return await handler.HandleAsync(query, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is not ArgumentNullException
                                                and not OperationResultException)
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

            IAsyncQueryHandlerWrapper<TResult> handler = (IAsyncQueryHandlerWrapper<TResult>)dispatcher
                .GetRequiredService(queryWrapperType);

            return handler.HandleAsync(query, cancellationToken);
        }
        catch (Exception exception) when (exception is not ArgumentNullException
                                            and not OperationResultException)
        {
            throw new OperationResultException(exception.ToOperationResult());
        }
    }
}
