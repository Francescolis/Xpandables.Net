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
using Xpandables.Net.Optionals;

namespace Xpandables.Net.Repositories;

/// <summary>
/// Represents a set of methods to read objects from a data store.
/// </summary>
/// <typeparam name="TEntity">The entity object type.</typeparam>
public interface IRepositoryRead<TEntity>
    where TEntity : class
{
    /// <summary>
    /// Tries to return an entity of the <typeparamref name="TEntity"/> 
    /// type that matches the key.
    /// If not found, returns an empty result.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <param name="key">Defines the key that entity should 
    /// meet to be returned.</param>
    /// <param name="cancellationToken">A CancellationToken t
    /// o observe while waiting for the task to complete.</param>
    /// <returns>A task that represents an optional that may 
    /// contain a value if found of <typeparamref name="TEntity"/> 
    /// type that meets the criteria or empty if not found.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="key"/> is null.</exception>
    /// <exception cref="OperationCanceledException">
    /// If the <see cref="CancellationToken" /> is canceled.</exception>
    /// <exception cref="InvalidOperationException"> The operation failed.
    /// See inner exception.</exception>
    Task<Optional<TEntity>> TryFindByKeyAsync<TKey>(
        TKey key,
        CancellationToken cancellationToken = default)
        where TKey : notnull, IComparable;

    /// <summary>
    /// Tries to return the first entity of the 
    /// <typeparamref name="TEntity"/> type that matches the filter.
    /// If not found, returns an empty optional.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="filter">A function to test each element 
    /// for a condition.</param>
    /// <param name="cancellationToken">A CancellationToken 
    /// to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents an optional that may 
    /// contain a value if found of <typeparamref name="TResult"/> 
    /// type that meets the criteria or empty if not found.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="filter"/> is null.</exception>
    /// <exception cref="OperationCanceledException"
    /// >If the <see cref="CancellationToken" /> is canceled.</exception>
    /// <exception cref="InvalidOperationException"> The operation failed.
    /// See inner exception.</exception>
    Task<Optional<TResult>> TryFindAsync<TResult>(
        IEntityFilter<TEntity, TResult> filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns an enumerable of <typeparamref name="TResult"/> type that match 
    /// the criteria and that can be asynchronously enumerated.
    /// If no result found, returns an empty enumerable.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="filter">A function to test each element for a condition.</param>
    /// <param name="cancellationToken">A CancellationToken 
    /// to observe while waiting for the task to complete.</param>
    /// <returns>A collection of <typeparamref name="TEntity"/> 
    /// that can be asynchronously enumerated.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="filter"/> is null.</exception>
    /// <exception cref="OperationCanceledException">
    /// If the <see cref="CancellationToken" /> is canceled.</exception>
    /// <exception cref="InvalidOperationException"> The operation failed.
    /// See inner exception.</exception>
    IAsyncEnumerable<TResult> FetchAsync<TResult>(
        IEntityFilter<TEntity, TResult> filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously returns the number of elements in a sequence 
    /// that satisfy a condition.
    /// </summary>
    /// <param name="filter">A function to test each element 
    /// for a condition.</param>
    /// <param name="cancellationToken">A CancellationToken 
    /// to observe while waiting for the task to complete.</param>
    /// <returns> A task that represents the asynchronous operation. 
    /// The task result contains the 
    /// number of elements in the sequence that satisfy 
    /// the condition in the predicate function.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="filter"/> is null.</exception>
    /// <exception cref="OperationCanceledException">If 
    /// the <see cref="CancellationToken" /> is canceled.</exception>
    /// <exception cref="InvalidOperationException"> The operation failed.
    /// See inner exception.</exception>
    Task<int> CountAsync(
        IEntityFilter<TEntity> filter,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a set of methods to read objects from a data store.
/// </summary>
/// <typeparam name="TEntity">The entity object type.</typeparam>
/// <typeparam name="TDataContext">The data context type.</typeparam>
public interface IRepositoryRead<TEntity, TDataContext>
    : IRepositoryRead<TEntity>
    where TEntity : class
    where TDataContext : class
{
}