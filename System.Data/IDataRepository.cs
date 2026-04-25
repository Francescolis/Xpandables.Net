/*******************************************************************************
 * Copyright (C) 2025-2026 Kamersoft
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

namespace System.Data;

/// <summary>
/// Marker interface for ADO.NET data repository types. Used for dependency injection 
/// registration and type discovery.
/// </summary>
public interface IDataRepository : IDisposable, IAsyncDisposable;

/// <summary>
/// Defines a generic repository interface for performing ADO.NET database operations on data.
/// </summary>
/// <remarks>
/// <para>
/// This interface provides methods for querying, inserting, updating, and deleting data
/// using ADO.NET with parameterized SQL. It supports <see cref="IDataSpecification{Tdata, TResult}"/>
/// for type-safe query building and <see cref="DataUpdater{TSource}"/> for bulk updates.
/// </para>
/// <para>
/// Unlike EF Core's <c>IdataRepository</c>, this interface works directly with SQL and does not
/// track data changes. All operations are executed immediately against the database.
/// </para>
/// </remarks>
/// <typeparam name="TData">The type of data to manage. Must be a class with public properties.</typeparam>
public interface IDataRepository<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TData> : IDataRepository
	where TData : class
{
	/// <summary>
	/// Asynchronously retrieves data matching the specification.
	/// </summary>
	/// <typeparam name="TResult">The type of the result projected by the specification.</typeparam>
	/// <param name="specification">The query specification defining filtering, ordering, and paging.</param>
	/// <param name="cancellationToken">A token to cancel the operation.</param>
	/// <returns>An async enumerable of matching results.</returns>
	IAsyncEnumerable<TResult> QueryAsync<TResult>(IDataSpecification<TData, TResult> specification, CancellationToken cancellationToken = default);

	/// <summary>
	/// Executes an asynchronous, paged query using the specified query specification and returns a sequence of results.
	/// </summary>
	/// <remarks>Use this method to efficiently retrieve large datasets in pages without loading all results
	/// into memory at once. The returned enumerable supports asynchronous iteration and paging, which is suitable for
	/// scenarios where data volume may be significant.</remarks>
	/// <typeparam name="TResult">The type of the result elements returned by the query.</typeparam>
	/// <param name="specification">The query specification that defines the criteria and projection for the query operation.</param>
	/// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous query operation.</param>
	/// <returns>An asynchronous paged enumerable that yields result elements of type TResult according to the specified query
	/// specification.</returns>
	IAsyncPagedEnumerable<TResult> QueryPagedAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TResult>(
		IDataSpecification<TData, TResult> specification, CancellationToken cancellationToken = default);

	/// <summary>
	/// Asynchronously retrieves a single data matching the specification, or throws if not exactly one.
	/// </summary>
	/// <typeparam name="TResult">The type of the result.</typeparam>
	/// <param name="specification">The query specification.</param>
	/// <param name="cancellationToken">A token to cancel the operation.</param>
	/// <returns>The single matching result.</returns>
	/// <exception cref="InvalidOperationException">Thrown when zero or more than one result exists.</exception>
	Task<TResult> QuerySingleAsync<TResult>(IDataSpecification<TData, TResult> specification, CancellationToken cancellationToken = default);

	/// <summary>
	/// Asynchronously retrieves a single data matching the specification, or default if none exists.
	/// </summary>
	/// <typeparam name="TResult">The type of the result.</typeparam>
	/// <param name="specification">The query specification.</param>
	/// <param name="cancellationToken">A token to cancel the operation.</param>
	/// <returns>The single matching result, or default if none found.</returns>
	/// <exception cref="InvalidOperationException">Thrown when more than one result exists.</exception>
	Task<TResult?> QuerySingleOrDefaultAsync<TResult>(IDataSpecification<TData, TResult> specification, CancellationToken cancellationToken = default);

	/// <summary>
	/// Asynchronously retrieves the first data matching the specification.
	/// </summary>
	/// <typeparam name="TResult">The type of the result.</typeparam>
	/// <param name="specification">The query specification.</param>
	/// <param name="cancellationToken">A token to cancel the operation.</param>
	/// <returns>The first matching result.</returns>
	/// <exception cref="InvalidOperationException">Thrown when no results exist.</exception>
	Task<TResult> QueryFirstAsync<TResult>(IDataSpecification<TData, TResult> specification, CancellationToken cancellationToken = default);

	/// <summary>
	/// Asynchronously retrieves the first data matching the specification, or default if none exists.
	/// </summary>
	/// <typeparam name="TResult">The type of the result.</typeparam>
	/// <param name="specification">The query specification.</param>
	/// <param name="cancellationToken">A token to cancel the operation.</param>
	/// <returns>The first matching result, or default if none found.</returns>
	Task<TResult?> QueryFirstOrDefaultAsync<TResult>(IDataSpecification<TData, TResult> specification, CancellationToken cancellationToken = default);

	/// <summary>
	/// Asynchronously counts data matching the specification.
	/// </summary>
	/// <typeparam name="TResult">The result type (used for specification compatibility).</typeparam>
	/// <param name="specification">The query specification.</param>
	/// <param name="cancellationToken">A token to cancel the operation.</param>
	/// <returns>The count of matching data.</returns>
	Task<long> CountAsync<TResult>(IDataSpecification<TData, TResult> specification, CancellationToken cancellationToken = default);

	/// <summary>
	/// Asynchronously checks if any data matches the specification.
	/// </summary>
	/// <typeparam name="TResult">The result type (used for specification compatibility).</typeparam>
	/// <param name="specification">The query specification.</param>
	/// <param name="cancellationToken">A token to cancel the operation.</param>
	/// <returns><see langword="true"/> if any data matches; otherwise, <see langword="false"/>.</returns>
	Task<bool> ExistsAsync<TResult>(IDataSpecification<TData, TResult> specification, CancellationToken cancellationToken = default);

	/// <summary>
	/// Executes a raw SQL query and returns results.
	/// </summary>
	/// <typeparam name="TResult">The type to map results to.</typeparam>
	/// <param name="sql">The SQL query to execute.</param>
	/// <param name="parameters">Optional parameters for the query.</param>
	/// <param name="cancellationToken">A token to cancel the operation.</param>
	/// <returns>An async enumerable of results.</returns>
	IAsyncEnumerable<TResult> QueryRawAsync<TResult>(string sql, IEnumerable<SqlParameter>? parameters = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Executes a raw SQL query and maps each row using a custom mapper function.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Use this overload when the default property-name-based mapping is insufficient,
	/// such as when working with constructor-based DTOs, computed columns, or complex projections.
	/// </para>
	/// <para>
	/// The <paramref name="mapper"/> delegate receives the <see cref="Common.DbDataReader"/> positioned
	/// on the current row and must return a fully constructed <typeparamref name="TResult"/> instance.
	/// The reader is only valid during the delegate invocation and must not be stored or used asynchronously.
	/// </para>
	/// </remarks>
	/// <typeparam name="TResult">The type of the result projected by the mapper.</typeparam>
	/// <param name="sql">The SQL query to execute.</param>
	/// <param name="mapper">A function that maps each <see cref="Common.DbDataReader"/> row to a <typeparamref name="TResult"/> instance.</param>
	/// <param name="parameters">Optional parameters for the query.</param>
	/// <param name="cancellationToken">A token to cancel the operation.</param>
	/// <returns>An async enumerable of mapped results.</returns>
	IAsyncEnumerable<TResult> QueryRawAsync<TResult>(string sql, Func<Common.DbDataReader, TResult> mapper, IEnumerable<SqlParameter>? parameters = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Executes a raw SQL query and maps each row using an expression selector,
	/// reusing the same mapping pipeline as <see cref="DataSpecification"/>-based queries.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The raw SQL must select all columns required by the <typeparamref name="TData"/> entity type.
	/// The <paramref name="selector"/> expression is then applied client-side to project each
	/// materialized entity into <typeparamref name="TResult"/>.
	/// </para>
	/// <para>
	/// This overload is useful when you need the full expression-based mapping (including
	/// constructor binding and <see cref="System.ComponentModel.DataAnnotations.Schema.ColumnAttribute"/> resolution)
	/// but with hand-written SQL.
	/// </para>
	/// </remarks>
	/// <typeparam name="TResult">The type of the result projected by the selector.</typeparam>
	/// <param name="sql">The SQL query to execute. Must select all columns for the <typeparamref name="TData"/> entity.</param>
	/// <param name="selector">An expression that projects each <typeparamref name="TData"/> entity to a <typeparamref name="TResult"/> instance.</param>
	/// <param name="parameters">Optional parameters for the query.</param>
	/// <param name="cancellationToken">A token to cancel the operation.</param>
	/// <returns>An async enumerable of projected results.</returns>
	IAsyncEnumerable<TResult> QueryRawAsync<TResult>(string sql, Linq.Expressions.Expression<Func<TData, TResult>> selector, IEnumerable<SqlParameter>? parameters = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Executes a raw SQL command (INSERT, UPDATE, DELETE).
	/// </summary>
	/// <param name="sql">The SQL command to execute.</param>
	/// <param name="parameters">Optional parameters for the command.</param>
	/// <param name="cancellationToken">A token to cancel the operation.</param>
	/// <returns>The number of rows affected.</returns>
	Task<int> ExecuteAsync(string sql, IEnumerable<SqlParameter>? parameters = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Inserts a single data into the database.
	/// </summary>
	/// <param name="data">The data to insert.</param>
	/// <param name="cancellationToken">A token to cancel the operation.</param>
	/// <returns>The number of rows affected (typically 1).</returns>
	Task<int> InsertAsync(TData data, CancellationToken cancellationToken = default);

	/// <summary>
	/// Inserts multiple data into the database in a batch.
	/// </summary>
	/// <param name="data">The data to insert.</param>
	/// <param name="cancellationToken">A token to cancel the operation.</param>
	/// <returns>The number of rows affected.</returns>
	Task<int> InsertAsync(IEnumerable<TData> data, CancellationToken cancellationToken = default);

	/// <summary>
	/// Asynchronously inserts the specified data into the data store and returns its generated identity value.
	/// </summary>
	/// <typeparam name="TIdentity">The type of the identity value returned after insertion, such as an integer or GUID.</typeparam>
	/// <param name="data">The data to insert into the data store. Cannot be null.</param>
	/// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
	/// <returns>A task that represents the asynchronous insert operation. The task result contains the identity value generated
	/// for the inserted data.</returns>
	Task<TIdentity> InsertAsync<TIdentity>(
		TData data,
		CancellationToken cancellationToken = default)
		where TIdentity : struct;

	/// <summary>
	/// Updates data matching the specification using the provided updater.
	/// </summary>
	/// <param name="specification">The specification defining which data to update.</param>
	/// <param name="updater">The updater defining the SET clauses.</param>
	/// <param name="cancellationToken">A token to cancel the operation.</param>
	/// <returns>The number of rows affected.</returns>
	Task<int> UpdateAsync(IDataSpecification<TData, TData> specification, DataUpdater<TData> updater, CancellationToken cancellationToken = default);

	/// <summary>
	/// Deletes data matching the specification.
	/// </summary>
	/// <param name="specification">The specification defining which data to delete.</param>
	/// <param name="cancellationToken">A token to cancel the operation.</param>
	/// <returns>The number of rows affected.</returns>
	Task<int> DeleteAsync(IDataSpecification<TData, TData> specification, CancellationToken cancellationToken = default);
}
