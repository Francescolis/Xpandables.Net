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
using System.Data;
using System.Data.Common;

namespace Xpandables.Net.Repositories;

/// <summary>
/// Represents a transaction within a unit of work pattern, providing methods to commit or rollback changes
/// asynchronously.
/// </summary>
/// <remarks>This interface is designed to be used in scenarios where changes need to be committed or rolled back
/// as a single transaction. Implementations should ensure that resources are properly disposed of when the transaction
/// is completed or disposed.</remarks>
public interface IUnitOfWorkTransaction : IAsyncDisposable
{
    /// <summary>
    /// Commits the current transaction asynchronously.
    /// </summary>
    /// <remarks>This method finalizes the transaction, making all changes permanent. If the operation is
    /// canceled via the <paramref name="cancellationToken"/>, the task will be marked as canceled.</remarks>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. 
    /// The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous commit operation.</returns>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously rolls back the current transaction.
    /// </summary>
    /// <remarks>This method should be called to undo all operations performed in the current transaction. If
    /// the transaction has already been committed or rolled back, calling this method will have no effect.</remarks>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. 
    /// The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous rollback operation.</returns>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a unit of work that encapsulates a set of operations to 
/// be performed as a single transaction.
/// </summary>
public interface IUnitOfWork : IAsyncDisposable
{
    /// <summary>
    /// Gets a value indicating whether the operation is transactional.
    /// </summary>
    bool IsTransactional { get; }

    /// <summary>
    /// Saves all changes made in this unit of work.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The number of state entries written to the database.</returns>
    /// <exception cref="InvalidOperationException">All exceptions 
    /// related to the operation.</exception>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a new transaction asynchronously.
    /// </summary>
    /// <param name="cancellationToken"> A token to cancel the operation.</param>
    /// <remarks>The transaction must be disposed of using the returned <see cref="IAsyncDisposable"/> object
    /// to ensure that resources are released properly. This method is typically used to group a series of operations
    /// that should be executed as a single unit of work.</remarks>
    /// <returns>An <see cref="IUnitOfWorkTransaction"/> that represents the transaction. 
    /// Dispose of this object to commit or roll back
    /// the transaction.</returns>
    Task<IUnitOfWorkTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Associates the specified transaction with the current database context for asynchronous operations.
    /// </summary>
    /// <remarks>This method allows you to execute database operations within the context of the provided
    /// transaction. Ensure that the transaction is properly managed and disposed of after use to avoid resource
    /// leaks.</remarks>
    /// <param name="transaction">The <see cref="IDbTransaction"/> to be used for database operations.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. 
    /// The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>An <see cref="IUnitOfWorkTransaction"/> that represents the transaction. 
    /// Dispose of this object to commit or roll back the transaction.</returns>
    Task<IUnitOfWorkTransaction> UseTransactionAsync(
        DbTransaction transaction,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the repository of the specified type.
    /// </summary>
    /// <typeparam name="TRepository">The type of the repository.</typeparam>
    /// <returns>The repository instance.</returns>
    TRepository GetRepository<TRepository>()
        where TRepository : class, IRepository;
}

/// <summary>
/// Represents a unit of work that encapsulates a set of operations to 
/// be performed as a single transaction with a specific context.
/// </summary>
/// <typeparam name="TDataContext">The type of the context.</typeparam>
public interface IUnitOfWork<TDataContext> : IUnitOfWork
    where TDataContext : class
{
}