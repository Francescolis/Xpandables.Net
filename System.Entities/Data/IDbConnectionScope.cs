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
using System.Data;
using System.Data.Common;

namespace System.Entities.Data;

/// <summary>
/// Represents a scoped database connection that manages the connection lifecycle and optional transaction.
/// </summary>
/// <remarks>
/// <para>
/// This interface follows the scope pattern similar to <c>IServiceScope</c>, providing automatic 
/// disposal of the connection and any active transaction when the scope is disposed.
/// </para>
/// <para>
/// The scope ensures that:
/// <list type="bullet">
/// <item>The connection is opened when first accessed</item>
/// <item>Transactions are properly committed or rolled back</item>
/// <item>Resources are released when the scope is disposed</item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// await using var scope = await scopeFactory.CreateScopeAsync(cancellationToken);
/// await using var transaction = await scope.BeginTransactionAsync(cancellationToken);
/// 
/// // Perform database operations...
/// 
/// await transaction.CommitAsync(cancellationToken);
/// </code>
/// </example>
public interface IDbConnectionScope : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Gets the database connection managed by this scope.
    /// </summary>
    /// <remarks>
    /// The connection is opened when the scope is created. Do not dispose this connection 
    /// directly; it will be disposed when the scope is disposed.
    /// </remarks>
    DbConnection Connection { get; }

    /// <summary>
    /// Gets the current active transaction, if any.
    /// </summary>
    /// <remarks>
    /// Returns <see langword="null"/> if no transaction has been started or if the 
    /// transaction has been committed or rolled back.
    /// </remarks>
    IDataTransaction? CurrentTransaction { get; }

    /// <summary>
    /// Gets a value indicating whether a transaction is currently active.
    /// </summary>
    bool HasActiveTransaction { get; }

    /// <summary>
    /// Begins a new database transaction asynchronously.
    /// </summary>
    /// <param name="isolationLevel">The isolation level for the transaction. 
    /// Defaults to <see cref="IsolationLevel.ReadCommitted"/>.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the transaction.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a transaction is already active in this scope.
    /// </exception>
    Task<IDataTransaction> BeginTransactionAsync(
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a new database transaction synchronously.
    /// </summary>
    /// <param name="isolationLevel">The isolation level for the transaction. 
    /// Defaults to <see cref="IsolationLevel.ReadCommitted"/>.</param>
    /// <returns>The database transaction.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a transaction is already active in this scope.
    /// </exception>
    IDataTransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

    /// <summary>
    /// Creates a database command associated with this scope's connection and transaction.
    /// </summary>
    /// <returns>A new <see cref="DbCommand"/> configured with the connection and current transaction.</returns>
    DbCommand CreateCommand();
}

/// <summary>
/// Defines a factory for creating <see cref="IDbConnectionScope"/> instances.
/// </summary>
/// <remarks>
/// Use this factory to create scoped connections that manage their own lifecycle.
/// The factory is typically registered as a singleton in the dependency injection container.
/// </remarks>
public interface IDbConnectionScopeFactory
{
    /// <summary>
    /// Creates a new connection scope asynchronously with an open connection.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the connection scope.</returns>
    Task<IDbConnectionScope> CreateScopeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new connection scope synchronously with an open connection.
    /// </summary>
    /// <returns>The connection scope with an open connection.</returns>
    IDbConnectionScope CreateScope();
}
