
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
using System.Linq.Expressions;

namespace Xpandables.Net.Repositories;

/// <summary>
/// Represents a set of methods to read/write objects from a data store.
/// </summary>
/// <remarks>This is considered as anti-pattern, the EFCore DbContext for example already provides abstract data access.</remarks>
/// <typeparam name="TEntity">The entity object type.</typeparam>
public interface IRepository<TEntity>
    where TEntity : class, IEntity
{
    /// <summary>
    /// Tries to return an entity of the <typeparamref name="TEntity"/> type that matches the key.
    /// If not found, returns the <see langword="default"/> value of the type.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <param name="key">Defines the key that entity should meet to be returned.</param>
    /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents an object of <typeparamref name="TEntity"/> 
    /// type that meets the criteria or <see langword="default"/> if not found.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="key"/> is null.</exception>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    ValueTask<TEntity?> TryFindByKeyAsync<TKey>(TKey key, CancellationToken cancellationToken = default)
        where TKey : notnull, IComparable;

    /// <summary>
    /// Tries to return the first entity of the <typeparamref name="TEntity"/> type that matches the filter.
    /// If not found, returns the <see langword="default"/> value of the type.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="filter">A function to test each element for a condition.</param>
    /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents an object of <typeparamref name="TEntity"/> type 
    /// that meets the criteria or <see langword="default"/> if not found.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="filter"/> is null.</exception>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    ValueTask<TResult?> TryFindAsync<TResult>(
        IEntityFilter<TEntity, TResult> filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns an enumerable of <typeparamref name="TResult"/> type that match 
    /// the criteria and that can be asynchronously enumerated.
    /// If no result found, returns an empty enumerable.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="filter">A function to test each element for a condition.</param>
    /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
    /// <returns>A collection of <typeparamref name="TEntity"/> that can be asynchronously enumerated.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="filter"/> is null.</exception>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    IAsyncEnumerable<TResult> FetchAsync<TResult>(
        IEntityFilter<TEntity, TResult> filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously returns the number of elements in a sequence that satisfy a condition.
    /// </summary>
    /// <param name="filter">A function to test each element for a condition.</param>
    /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
    /// <returns> A task that represents the asynchronous operation. The task result contains the 
    /// number of elements in the sequence that satisfy the condition in the predicate function.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="filter"/> is null.</exception>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    ValueTask<int> CountAsync(
        IEntityFilter<TEntity> filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks the specified entity to be inserted to the data storage on persistence according to the database provider/ORM.
    /// </summary>
    /// <param name="entity">The entity to be added and persisted.</param>
    /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents an  asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="entity"/> is null.</exception>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    ValueTask InsertAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks the specified entities to be inserted to the data storage on persistence according to the database provider/ORM.
    /// </summary>
    /// <param name="entities">The collection of entities to be added and persisted.</param>
    /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents an  asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="entities"/> is null.</exception>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    ValueTask InsertManyAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks the specified entity to be updated to the data storage on persistence according to the database provider/ORM.
    /// </summary>
    /// <param name="entity">The entity to be updated when persisted.</param>
    /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents an  asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="entity"/> is null.</exception>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    ValueTask UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks the specified entities to be updated to the data storage on persistence according to the database provider/ORM.
    /// </summary>
    /// <param name="entities">The collection of entities to be updated when persisted.</param>
    /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents an  asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="entities"/> is null.</exception>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    ValueTask UpdateManyAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the entities matching the filter using the specified updater and marks the target entities to be updated
    /// to the data storage on persistence according to the database provider/ORM.
    /// </summary>
    /// <param name="filter">Defines a set of criteria that entity should meet to be deleted.</param>
    /// <param name="updater">The expression update that returns an object that contains only the updated properties and values.
    /// The returned type can be an anonymous type.</param>
    /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents an  asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="filter"/> or <paramref name="updater"/> is null.</exception>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    ValueTask UpdateManyAsync(
        IEntityFilter<TEntity> filter,
        Expression<Func<TEntity, object>> updater,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks the objects matching the predicate as deleted and will be removed according to the database provider/ORM.
    /// </summary>
    /// <param name="filter">Defines a set of criteria that entity should meet to be deleted.</param>
    /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents an  asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="filter"/> is null.</exception>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    ValueTask DeleteAsync(IEntityFilter<TEntity> filter, CancellationToken cancellationToken = default);
}