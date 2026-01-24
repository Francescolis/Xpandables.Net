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
using System.Diagnostics.CodeAnalysis;

namespace System.Entities;

/// <summary>
/// Represents a unit of work that encapsulates a set of operations to 
/// be performed as a single transaction.
/// /// <para> For best practices, consider using directly the target data access technology (e.g., Entity Framework Core,
/// Hibernate, Dapper) to leverage its full capabilities and optimizations).</para>
/// </summary>
public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Returns the repository of the specified type.
    /// </summary>
    /// <typeparam name="TRepository">The type of the repository.</typeparam>
    /// <returns>The repository instance.</returns>
    TRepository GetRepository<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TRepository>()
       where TRepository : class, IRepository;

    /// <summary>
    /// Begins a new transaction asynchronously.
    /// </summary>
    /// <param name="cancellationToken"> A token to cancel the operation.</param>
    /// <remarks>The transaction must be disposed of using the returned <see cref="IAsyncDisposable"/> object
    /// to ensure that resources are released properly. This method is typically used to group a series of operations
    /// that should be executed as a single unit of work.</remarks>
    /// <returns>An <see cref="IUnitOfWorkTransaction"/> that represents the transaction. 
    /// Dispose of this object to commit or roll back the transaction.</returns>
    Task<IUnitOfWorkTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a new transaction within the current unit of work.
    /// </summary>
    /// <remarks>The returned transaction must be disposed to either commit or roll back the changes.  Ensure
    /// that only one transaction is active at a time within the same unit of work.</remarks>
    /// <returns>An <see cref="IUnitOfWorkTransaction"/> instance representing the transaction.
    /// Use this object to manage the transaction's lifecycle.
    /// Dispose of this object to commit or roll back the transaction.</returns>
    IUnitOfWorkTransaction BeginTransaction();

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
    /// Associates an existing <see cref="DbTransaction"/> with the current unit of work.
    /// </summary>
    /// <remarks>This method allows the unit of work to operate within the context of a pre-existing database
    /// transaction. The caller is responsible for managing the lifecycle of the provided transaction, including
    /// committing or rolling it back.</remarks>
    /// <param name="transaction">The <see cref="DbTransaction"/> to be used by the unit of work. Cannot be <see langword="null"/>.</param>
    /// <returns>An <see cref="IUnitOfWorkTransaction"/> instance that represents the scope of the transaction.
    /// Dispose of this object to commit or roll back the transaction.</returns>
    IUnitOfWorkTransaction UseTransaction(DbTransaction transaction);

    /// <summary>
    /// Saves all changes made in this unit of work.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The number of state entries written to the database.</returns>
    /// <exception cref="InvalidOperationException">All exceptions 
    /// related to the operation.</exception>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves all changes made in the current context to the underlying data store.
    /// </summary>
    /// <remarks>This method commits any tracked changes, such as additions, updates, or deletions, to the
    /// data store. If no changes are detected, the method performs no operation and returns 0.</remarks>
    /// <returns>The number of state entries written to the data store. Returns 0 if no changes were made.</returns>
    int SaveChanges();
}

/// <summary>
/// Represents a unit of work that encapsulates a set of operations to 
/// be performed as a single transaction with a specific context.
/// </summary>
/// <typeparam name="TDataContext">The type of the context.</typeparam>
public interface IUnitOfWork<TDataContext> : IUnitOfWork
    where TDataContext : class;