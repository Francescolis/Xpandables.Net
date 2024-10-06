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
namespace Xpandables.Net.Repositories;

/// <summary>
/// Provides an interface for reading entities from a repository.
/// </summary>
public interface IRepositoryRead
{
    /// <summary>
    /// Fetches entities from the repository based on the specified filter.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="entityFilter">The filter to apply to the entities.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An asynchronous enumerable of the result type.</returns>
    IAsyncEnumerable<TResult> FetchAsync<TEntity, TResult>(
        IEntityFilter<TEntity, TResult> entityFilter,
        CancellationToken cancellationToken = default)
        where TEntity : class, IEntity;
}
