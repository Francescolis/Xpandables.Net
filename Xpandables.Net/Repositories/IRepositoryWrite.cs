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
using System.Linq.Expressions;

namespace Xpandables.Net.Repositories;

/// <summary>
/// Represents a set of methods to write objects to a data store.
/// </summary>
/// <typeparam name="TEntity">The entity object type.</typeparam>
public interface IRepositoryWrite<TEntity>
    where TEntity : class, IEntity
{
    /// <summary>
    /// Marks the specified entity to be inserted to the data storage 
    /// on persistence according to the database provider/ORM.
    /// </summary>
    /// <param name="entity">The entity to be added and persisted.</param>
    /// <param name="cancellationToken">A CancellationToken to observe while 
    /// waiting for the task to complete.</param>
    /// <returns>A task that represents an  asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="entity"/> is null.</exception>
    /// <exception cref="OperationCanceledException">If 
    /// the <see cref="CancellationToken" /> is canceled.</exception>
    ValueTask InsertAsync(
        TEntity entity,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks the specified entities to be inserted to the data storage 
    /// on persistence according to the database provider/ORM.
    /// </summary>
    /// <param name="entities">The collection of entities 
    /// to be added and persisted.</param>
    /// <param name="cancellationToken">A CancellationToken 
    /// to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents an  asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="entities"/> is null.</exception>
    /// <exception cref="OperationCanceledException">
    /// If the <see cref="CancellationToken" /> is canceled.</exception>
    ValueTask InsertManyAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks the specified entity to be updated to the data storage
    /// on persistence according to the database provider/ORM.
    /// </summary>
    /// <param name="entity">The entity to be updated when persisted.</param>
    /// <param name="cancellationToken">A CancellationToken 
    /// to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents an  asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="entity"/> is null.</exception>
    /// <exception cref="OperationCanceledException">
    /// If the <see cref="CancellationToken" /> is canceled.</exception>
    ValueTask UpdateAsync(
        TEntity entity,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks the specified entities to be updated to the data 
    /// storage on persistence according to the database provider/ORM.
    /// </summary>
    /// <param name="entities">The collection of entities to be updated 
    /// when persisted.</param>
    /// <param name="cancellationToken">A CancellationToken to observe
    /// while waiting for the task to complete.</param>
    /// <returns>A task that represents an  asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="entities"/> is null.</exception>
    /// <exception cref="OperationCanceledException">
    /// If the <see cref="CancellationToken" /> is canceled.</exception>
    ValueTask UpdateManyAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the entities matching the filter using the 
    /// specified updater and marks the target entities to be updated
    /// to the data storage on persistence according to the database provider/ORM.
    /// </summary>
    /// <param name="filter">Defines a set of criteria that 
    /// entity should meet to be deleted.</param>
    /// <param name="updater">The expression update that returns 
    /// an object that contains only the updated properties and values.
    /// The returned type can be an anonymous type.</param>
    /// <param name="cancellationToken">A CancellationToken 
    /// to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents an  asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="filter"/> or <paramref name="updater"/> is null.</exception>
    /// <exception cref="OperationCanceledException">
    /// If the <see cref="CancellationToken" /> is canceled.</exception>
    ValueTask UpdateManyAsync(
        IEntityFilter<TEntity> filter,
        Expression<Func<TEntity, object>> updater,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks the objects matching the predicate as deleted 
    /// and will be removed according to the database provider/ORM.
    /// </summary>
    /// <param name="filter">Defines a set of criteria that 
    /// entity should meet to be deleted.</param>
    /// <param name="cancellationToken">A CancellationToken 
    /// to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents an  asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="filter"/> is null.</exception>
    /// <exception cref="OperationCanceledException">
    /// If the <see cref="CancellationToken" /> is canceled.</exception>
    ValueTask DeleteAsync(
        IEntityFilter<TEntity> filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks the entity as deleted and will be removed according 
    /// to the database provider/ORM.
    /// </summary>
    /// <param name="entity">Defines the entity to be marked as deleted.</param>
    /// <param name="cancellationToken">A CancellationToken 
    /// to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents an  asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="entity"/> is null.</exception>
    /// <exception cref="OperationCanceledException">
    /// If the <see cref="CancellationToken" /> is canceled.</exception>
    ValueTask DeleteAsync(
        TEntity entity,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a set of methods to write objects to a data store.
/// </summary>
/// <typeparam name="TEntity">The entity object type.</typeparam>
/// <typeparam name="TDataContext">The data context type.</typeparam>
public interface IRepositoryWrite<TEntity, TDataContext>
    : IRepositoryWrite<TEntity>
    where TEntity : class, IEntity
    where TDataContext : class
{
}