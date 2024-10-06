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
/// Represents a unit of work that encapsulates a set of operations to 
/// be performed as a single transaction.
/// </summary>
public interface IUnitOfWork : IAsyncDisposable
{
    /// <summary>
    /// Saves all changes made in this unit of work.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The number of state entries written to the database.</returns>
    /// <exception cref="InvalidOperationException">All exceptions 
    /// related to the operation.</exception>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the repository of the specified type.
    /// </summary>
    /// <typeparam name="TRepository">The type of the repository.</typeparam>
    /// <returns>The repository instance.</returns>
    IRepository GetRepository<TRepository>()
        where TRepository : class, IRepository;

    /// <summary>
    /// Gets the read-only repository of the specified type.
    /// </summary>
    /// <typeparam name="TRepository">The type of the repository.</typeparam>
    /// <returns>The read-only repository instance.</returns>
    IRepositoryRead GetRepositoryRead<TRepository>()
        where TRepository : class, IRepositoryRead;

    /// <summary>
    /// Gets the write-only repository of the specified type.
    /// </summary>
    /// <typeparam name="TRepository">The type of the repository.</typeparam>
    /// <returns>The write-only repository instance.</returns>
    IRepositoryWrite GetRepositoryWrite<TRepository>()
        where TRepository : class, IRepositoryWrite;
}
