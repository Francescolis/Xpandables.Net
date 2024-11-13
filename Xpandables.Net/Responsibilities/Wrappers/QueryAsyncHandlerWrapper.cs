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
using Xpandables.Net.Responsibilities.Decorators;

namespace Xpandables.Net.Responsibilities.Wrappers;

/// <summary>
/// A wrapper class for handling asynchronous queries with decorators.
/// </summary>
/// <typeparam name="TQuery">The type of the query.</typeparam>
/// <typeparam name="TResult">The type of the result.</typeparam>
public sealed class QueryAsyncHandlerWrapper<TQuery, TResult>(
    IQueryAsyncHandler<TQuery, TResult> decoratee,
    IEnumerable<IAsyncPipelineDecorator<TQuery, TResult>> decorators) :
    IQueryAsyncHandlerWrapper<TResult>
    where TQuery : class, IQueryAsync<TResult>
{
    /// <inheritdoc/>
    public IAsyncEnumerable<TResult> HandleAsync(
        IQueryAsync<TResult> query,
        CancellationToken cancellationToken = default)
    {
        IAsyncEnumerable<TResult> results = decorators
            .Reverse()
            .Aggregate<IAsyncPipelineDecorator<TQuery, TResult>,
            RequestAsyncHandler<TResult>>(
                Handler,
                (next, decorator) => () => decorator.HandleAsync(
                    (TQuery)query,
                    next,
                    cancellationToken))();

        return results;

        IAsyncEnumerable<TResult> Handler() =>
            decoratee.HandleAsync((TQuery)query, cancellationToken);
    }
}
