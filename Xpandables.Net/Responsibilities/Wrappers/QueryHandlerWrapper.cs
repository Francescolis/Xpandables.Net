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
using Xpandables.Net.Decorators;
using Xpandables.Net.Operations;

namespace Xpandables.Net.Responsibilities.Wrappers;

/// <summary>
/// A wrapper for handling queries with a specified query handler.
/// </summary>
/// <typeparam name="TQuery">The type of the query.</typeparam>
/// <typeparam name="TResult">The type of the result.</typeparam>
public sealed class QueryHandlerWrapper<TQuery, TResult>(
    IQueryHandler<TQuery, TResult> decoratee,
    IEnumerable<IPipelineDecorator<TQuery, IOperationResult<TResult>>> decorators) :
    IQueryHandlerWrapper<TResult>
    where TQuery : class, IQuery<TResult>
{
    /// <inheritdoc/>>
    public Task<IOperationResult<TResult>> HandleAsync(
        IQuery<TResult> query,
        CancellationToken cancellationToken = default)
    {
        Task<IOperationResult<TResult>> Handler() =>
            decoratee.HandleAsync((TQuery)query, cancellationToken);

        Task<IOperationResult<TResult>> result = decorators
            .Reverse()
            .Aggregate<IPipelineDecorator<TQuery, IOperationResult<TResult>>,
            RequestHandlerDelegate<IOperationResult<TResult>>>(
                Handler,
                (next, decorator) => () => decorator.HandleAsync(
                    (TQuery)query,
                    next,
                    cancellationToken))();

        return result;
    }
}