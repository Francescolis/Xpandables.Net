/*******************************************************************************
 * Copyright (C) 2025 Kamersoft
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
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace System.Entities;

/// <summary>
/// Defines a generic repository interface for performing asynchronous CRUD operations on entities.
/// <para> For best practices, consider using directly the target data access technology (e.g., Entity Framework Core,
/// Hibernate, Dapper) to leverage its full capabilities and optimizations).</para>
/// </summary>
/// <remarks>This interface provides methods for fetching, inserting, updating, and deleting entities in a data
/// store. It supports asynchronous operations and allows for query customization through the use of LINQ expressions.
/// Implementations of this interface should handle the underlying data access logic.</remarks>
public interface IRepository : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Gets or sets a value indicating whether the repository operations are executed within a unit of work.
    /// </summary>
    bool IsUnitOfWorkEnabled { get; set; }

    /// <summary>
    /// Asynchronously retrieves a sequence of results from the data source based on the specified filter.
    /// </summary>
    /// <remarks>
    /// <para>The query is executed asynchronously and results are streamed as they become available. The
    /// returned sequence is not materialized in memory; results are fetched on demand. This method is suitable for
    /// processing large result sets efficiently.</para>
    /// <para><b>Warning:</b> Do not call materializing methods (e.g., ToList(), ToArray()) inside the filter function,
    /// as this defeats deferred execution and may cause performance issues.</para>
    /// </remarks>
    /// <typeparam name="TEntity">The type of the entity to query from the data source. Must be a reference type.</typeparam>
    /// <typeparam name="TResult">The type of the result projected by the filter.</typeparam>
    /// <param name="filter">A function that applies filtering and projection to the queryable collection of entities, returning the desired
    /// result set.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>An asynchronous sequence of results matching the filter criteria. The sequence is streamed and may be empty if
    /// no results are found.</returns>
    IAsyncEnumerable<TResult> FetchAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TEntity, TResult>(
        Func<IQueryable<TEntity>, IQueryable<TResult>> filter,
        CancellationToken cancellationToken = default)
        where TEntity : class;

    /// <summary>
    /// Asynchronously retrieves a single result from the data source based on the specified filter.
    /// </summary>
    /// <remarks>
    /// <para>Returns the single element matching the filter, or throws if zero or more than one element exists.</para>
    /// <para><b>Warning:</b> Do not call materializing methods inside the filter function.</para>
    /// </remarks>
    /// <typeparam name="TEntity">The type of the entity to query from the data source. Must be a reference type.</typeparam>
    /// <typeparam name="TResult">The type of the result projected by the filter.</typeparam>
    /// <param name="filter">A function that applies filtering and projection to the queryable collection of entities.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The single result matching the filter criteria.</returns>
    /// <exception cref="InvalidOperationException">Thrown when zero or more than one element matches the filter.</exception>
    Task<TResult> FetchSingleAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TEntity, TResult>(
        Func<IQueryable<TEntity>, IQueryable<TResult>> filter,
        CancellationToken cancellationToken = default)
        where TEntity : class;

    /// <summary>
    /// Asynchronously retrieves a single result from the data source, or a default value if no result is found.
    /// </summary>
    /// <remarks>
    /// <para>Returns the single element matching the filter, default if none exists, or throws if more than one element exists.</para>
    /// <para><b>Warning:</b> Do not call materializing methods inside the filter function.</para>
    /// </remarks>
    /// <typeparam name="TEntity">The type of the entity to query from the data source. Must be a reference type.</typeparam>
    /// <typeparam name="TResult">The type of the result projected by the filter.</typeparam>
    /// <param name="filter">A function that applies filtering and projection to the queryable collection of entities.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The single result matching the filter criteria, or default if none found.</returns>
    /// <exception cref="InvalidOperationException">Thrown when more than one element matches the filter.</exception>
    Task<TResult?> FetchSingleOrDefaultAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TEntity, TResult>(
        Func<IQueryable<TEntity>, IQueryable<TResult>> filter,
        CancellationToken cancellationToken = default)
        where TEntity : class;

    /// <summary>
    /// Asynchronously retrieves the first result from the data source based on the specified filter.
    /// </summary>
    /// <remarks>
    /// <para>Returns the first element matching the filter, or throws if no elements exist.</para>
    /// <para><b>Warning:</b> Do not call materializing methods inside the filter function.</para>
    /// </remarks>
    /// <typeparam name="TEntity">The type of the entity to query from the data source. Must be a reference type.</typeparam>
    /// <typeparam name="TResult">The type of the result projected by the filter.</typeparam>
    /// <param name="filter">A function that applies filtering and projection to the queryable collection of entities.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The first result matching the filter criteria.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no elements match the filter.</exception>
    Task<TResult> FetchFirstAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TEntity, TResult>(
        Func<IQueryable<TEntity>, IQueryable<TResult>> filter,
        CancellationToken cancellationToken = default)
        where TEntity : class;

    /// <summary>
    /// Asynchronously retrieves the first result from the data source, or a default value if no result is found.
    /// </summary>
    /// <remarks>
    /// <para>Returns the first element matching the filter, or default if none exists.</para>
    /// <para><b>Warning:</b> Do not call materializing methods inside the filter function.</para>
    /// </remarks>
    /// <typeparam name="TEntity">The type of the entity to query from the data source. Must be a reference type.</typeparam>
    /// <typeparam name="TResult">The type of the result projected by the filter.</typeparam>
    /// <param name="filter">A function that applies filtering and projection to the queryable collection of entities.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The first result matching the filter criteria, or default if none found.</returns>
    Task<TResult?> FetchFirstOrDefaultAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TEntity, TResult>(
        Func<IQueryable<TEntity>, IQueryable<TResult>> filter,
        CancellationToken cancellationToken = default)
        where TEntity : class;

    /// <summary>
    /// Asynchronously adds one or more entities of the specified type to the data store.
    /// </summary>
    /// <remarks>If the operation is canceled via the provided cancellation token, the returned task will be
    /// in a canceled state. The entities are not persisted until the operation completes successfully.</remarks>
    /// <typeparam name="TEntity">The type of entities to add. Must be a reference type.</typeparam>
    /// <param name="entities">A collection of entities to add to the data store. Cannot be null or contain null elements.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous add operation, containing the number of entities added.</returns>
    Task<int> AddAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TEntity>(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default)
        where TEntity : class;

    /// <summary>
    /// Asynchronously updates the specified entities in the data store.
    /// </summary>
    /// <typeparam name="TEntity">The type of entities to update. Must be a reference type.</typeparam>
    /// <param name="entities">A collection of entities to update. Each entity must not be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the update operation.</param>
    /// <returns>A task that represents the asynchronous update operation, containing the number of entities updated.</returns>
    Task<int> UpdateAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TEntity>(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default)
        where TEntity : class;

    /// <summary>
    /// Asynchronously updates entities of type TEntity that match the specified filter using the provided update
    /// expression.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entities to update. Must be a reference type.</typeparam>
    /// <param name="filter">A function that applies filtering logic to an IQueryable of TEntity, selecting which entities will be updated.</param>
    /// <param name="updateExpression">An expression that defines how the selected entities should be updated. The expression maps each entity to its
    /// updated values.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. The operation is canceled if the token is triggered.</param>
    /// <returns>A task that represents the asynchronous update operation, containing the number of entities updated.</returns>
    Task<int> UpdateAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TEntity>(
        Func<IQueryable<TEntity>, IQueryable<TEntity>> filter,
        Expression<Func<TEntity, TEntity>> updateExpression,
        CancellationToken cancellationToken = default)
        where TEntity : class;

    /// <summary>
    /// Asynchronously updates entities of type TEntity that match the specified filter.
    /// </summary>
    /// <remarks>The update is applied to all entities returned by the filter. The operation is performed
    /// asynchronously and may be cancelled using the provided cancellation token.</remarks>
    /// <typeparam name="TEntity">The type of the entities to update. Must be a reference type.</typeparam>
    /// <param name="filter">A function that applies a filter to the set of entities, returning the entities to be updated.</param>
    /// <param name="updateAction">An action that defines the update to apply to each filtered entity.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous update operation, containing the number of entities updated.</returns>
    Task<int> UpdateAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TEntity>(
        Func<IQueryable<TEntity>, IQueryable<TEntity>> filter,
        Action<TEntity> updateAction,
        CancellationToken cancellationToken = default)
        where TEntity : class;

    /// <summary>
    /// Asynchronously updates entities of type TEntity that match the specified filter using a fluent updater.
    /// </summary>
    /// <remarks>This method uses the fluent updater pattern to specify multiple property updates in a single
    /// operation. It provides an efficient way to perform bulk updates without loading entities into memory.</remarks>
    /// <typeparam name="TEntity">The type of the entities to update. Must be a reference type.</typeparam>
    /// <param name="filter">A function that applies filtering logic to select which entities will be updated.</param>
    /// <param name="updater">A fluent updater that specifies the property updates to apply.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous bulk update operation, containing the number of entities updated.</returns>
    /// <exception cref="ArgumentNullException">Thrown when filter or updater is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the updater contains no property updates.</exception>
    [RequiresDynamicCode("Dynamic code generation is required for this method.")]
    [RequiresUnreferencedCode("Calls MakeGenericMethod which may require unreferenced code.")]
    Task<int> UpdateAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TEntity>(
        Func<IQueryable<TEntity>, IQueryable<TEntity>> filter,
        EntityUpdater<TEntity> updater,
        CancellationToken cancellationToken = default)
        where TEntity : class;

    /// <summary>
    /// Deletes entities from the repository based on a filter.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="filter">The filter to apply to the entities to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, containing the number of entities deleted.</returns>
    Task<int> DeleteAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TEntity>(
        Func<IQueryable<TEntity>, IQueryable<TEntity>> filter,
        CancellationToken cancellationToken = default)
        where TEntity : class;
}

/// <summary>
/// Defines a repository interface for data operations within a specified data context type.
/// </summary>
/// <typeparam name="TDataContext">The type of the data context within which the repository operates. 
/// Must be a reference type.</typeparam>
public interface IRepository<TDataContext> : IRepository
    where TDataContext : class;