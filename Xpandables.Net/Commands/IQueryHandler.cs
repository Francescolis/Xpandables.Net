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
using Xpandables.Net.Executions;

namespace Xpandables.Net.Commands;
/// <summary>
/// Defines a handler for processing queries of type <typeparamref name="TQuery"/> 
/// and returning a result of type <typeparamref name="TResult"/>.
/// </summary>
/// <typeparam name="TQuery">The type of the query.</typeparam>
/// <typeparam name="TResult">The type of the result.</typeparam>
public interface IQueryHandler<in TQuery, TResult>
    where TQuery : class, IQuery<TResult>
{
    /// <summary>
    /// Handles the specified query asynchronously.
    /// </summary>
    /// <param name="query">The query to handle.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation
    /// requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task 
    /// result contains the operation result.</returns>
    Task<IExecutionResult<TResult>> HandleAsync(
        TQuery query,
        CancellationToken cancellationToken = default);
}
