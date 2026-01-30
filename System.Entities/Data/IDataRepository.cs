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

namespace System.Entities.Data;

/// <summary>
/// Marker interface for ADO.NET data repository types. Used for dependency injection 
/// registration and type discovery.
/// </summary>
public interface IDataRepository : IDisposable, IAsyncDisposable;

/// <summary>
/// Defines a generic repository interface for performing ADO.NET database operations on entities.
/// </summary>
/// <remarks>
/// <para>
/// This interface provides methods for querying, inserting, updating, and deleting entities
/// using ADO.NET with parameterized SQL. It supports <see cref="IQuerySpecification{TEntity, TResult}"/>
/// for type-safe query building and <see cref="EntityUpdater{TSource}"/> for bulk updates.
/// </para>
/// <para>
/// Unlike EF Core's <c>IRepository</c>, this interface works directly with SQL and does not
/// track entity changes. All operations are executed immediately against the database.
/// </para>
/// </remarks>
/// <typeparam name="TEntity">The type of entity to manage. Must be a class with public properties.</typeparam>
public interface IDataRepository<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TEntity> 
    : IDataRepository
    where TEntity : class
{
    /// <summary>
    /// Asynchronously retrieves entities matching the specification.
    /// </summary>
    /// <typeparam name="TResult">The type of the result projected by the specification.</typeparam>
    /// <param name="specification">The query specification defining filtering, ordering, and paging.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An async enumerable of matching results.</returns>
    IAsyncEnumerable<TResult> QueryAsync<TResult>(
        IQuerySpecification<TEntity, TResult> specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves a single entity matching the specification, or throws if not exactly one.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="specification">The query specification.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The single matching result.</returns>
    /// <exception cref="InvalidOperationException">Thrown when zero or more than one result exists.</exception>
    Task<TResult> QuerySingleAsync<TResult>(
        IQuerySpecification<TEntity, TResult> specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves a single entity matching the specification, or default if none exists.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="specification">The query specification.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The single matching result, or default if none found.</returns>
    /// <exception cref="InvalidOperationException">Thrown when more than one result exists.</exception>
    Task<TResult?> QuerySingleOrDefaultAsync<TResult>(
        IQuerySpecification<TEntity, TResult> specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves the first entity matching the specification.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="specification">The query specification.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The first matching result.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no results exist.</exception>
    Task<TResult> QueryFirstAsync<TResult>(
        IQuerySpecification<TEntity, TResult> specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves the first entity matching the specification, or default if none exists.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="specification">The query specification.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The first matching result, or default if none found.</returns>
    Task<TResult?> QueryFirstOrDefaultAsync<TResult>(
        IQuerySpecification<TEntity, TResult> specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously counts entities matching the specification.
    /// </summary>
    /// <typeparam name="TResult">The result type (used for specification compatibility).</typeparam>
    /// <param name="specification">The query specification.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The count of matching entities.</returns>
    Task<long> CountAsync<TResult>(
        IQuerySpecification<TEntity, TResult> specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously checks if any entity matches the specification.
    /// </summary>
    /// <typeparam name="TResult">The result type (used for specification compatibility).</typeparam>
    /// <param name="specification">The query specification.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><see langword="true"/> if any entity matches; otherwise, <see langword="false"/>.</returns>
    Task<bool> ExistsAsync<TResult>(
        IQuerySpecification<TEntity, TResult> specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a raw SQL query and returns results.
    /// </summary>
    /// <typeparam name="TResult">The type to map results to.</typeparam>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="parameters">Optional parameters for the query.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An async enumerable of results.</returns>
    IAsyncEnumerable<TResult> QueryRawAsync<TResult>(
        string sql,
        IEnumerable<SqlParameter>? parameters = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a raw SQL command (INSERT, UPDATE, DELETE).
    /// </summary>
    /// <param name="sql">The SQL command to execute.</param>
    /// <param name="parameters">Optional parameters for the command.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The number of rows affected.</returns>
    Task<int> ExecuteAsync(
        string sql,
        IEnumerable<SqlParameter>? parameters = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts a single entity into the database.
    /// </summary>
    /// <param name="entity">The entity to insert.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The number of rows affected (typically 1).</returns>
    Task<int> InsertAsync(
        TEntity entity,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts multiple entities into the database in a batch.
    /// </summary>
    /// <param name="entities">The entities to insert.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The number of rows affected.</returns>
    Task<int> InsertAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates entities matching the specification using the provided updater.
    /// </summary>
    /// <param name="specification">The specification defining which entities to update.</param>
    /// <param name="updater">The updater defining the SET clauses.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The number of rows affected.</returns>
    Task<int> UpdateAsync(
        IQuerySpecification<TEntity, TEntity> specification,
        EntityUpdater<TEntity> updater,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes entities matching the specification.
    /// </summary>
    /// <param name="specification">The specification defining which entities to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The number of rows affected.</returns>
    Task<int> DeleteAsync(
        IQuerySpecification<TEntity, TEntity> specification,
        CancellationToken cancellationToken = default);
}
