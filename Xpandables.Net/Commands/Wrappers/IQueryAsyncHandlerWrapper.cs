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
namespace Xpandables.Net.Commands.Wrappers;

/// <summary>
/// Defines a wrapper for handling asynchronous queries.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
public interface IQueryAsyncHandlerWrapper<TResult>
{
    /// <summary>
    /// Handles the asynchronous query.
    /// </summary>
    /// <param name="query">The query to handle.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An asynchronous enumerable of the result.</returns>
    IAsyncEnumerable<TResult> HandleAsync(
         IQueryAsync<TResult> query,
         CancellationToken cancellationToken = default);
}
