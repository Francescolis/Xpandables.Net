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
/// Represents a database transaction with explicit commit and rollback operations.
/// </summary>
/// <remarks>
/// <para>
/// This interface wraps a <see cref="DbTransaction"/> and provides a consistent API for 
/// transaction management in ADO.NET operations.
/// </para>
/// <para>
/// Transactions should be committed explicitly using <see cref="CommitAsync"/> or <see cref="Commit"/>.
/// If the transaction is disposed without being committed, it will be rolled back automatically.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// await using var transaction = await scope.BeginTransactionAsync(cancellationToken);
/// try
/// {
///     // Perform database operations...
///     await transaction.CommitAsync(cancellationToken);
/// }
/// catch
/// {
///     await transaction.RollbackAsync(cancellationToken);
///     throw;
/// }
/// </code>
/// </example>
public interface IDataTransaction : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Gets the underlying <see cref="DbTransaction"/>.
    /// </summary>
    /// <remarks>
    /// Use this property when you need to pass the transaction to methods that 
    /// require a <see cref="DbTransaction"/> directly.
    /// </remarks>
    DbTransaction DbTransaction { get; }

    /// <summary>
    /// Gets the isolation level of the transaction.
    /// </summary>
    IsolationLevel IsolationLevel { get; }

    /// <summary>
    /// Gets a value indicating whether the transaction has been completed 
    /// (either committed or rolled back).
    /// </summary>
    bool IsCompleted { get; }

    /// <summary>
    /// Gets a value indicating whether the transaction was committed successfully.
    /// </summary>
    bool IsCommitted { get; }

    /// <summary>
    /// Gets a value indicating whether the transaction was rolled back.
    /// </summary>
    bool IsRolledBack { get; }

    /// <summary>
    /// Commits the transaction asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous commit operation.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the transaction has already been completed.
    /// </exception>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the transaction synchronously.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the transaction has already been completed.
    /// </exception>
    void Commit();

    /// <summary>
    /// Rolls back the transaction asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous rollback operation.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the transaction has already been completed.
    /// </exception>
    Task RollbackAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the transaction synchronously.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the transaction has already been completed.
    /// </exception>
    void Rollback();
}
