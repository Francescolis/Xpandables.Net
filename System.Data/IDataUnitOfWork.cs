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

namespace System.Data;

/// <summary>
/// Defines a unit of work pattern for ADO.NET operations, providing transaction management
/// and repository access within a single database connection scope.
/// </summary>
/// <remarks>
/// <para>
/// Unlike EF Core's unit of work which tracks changes and commits them on SaveChanges,
/// ADO.NET operations execute immediately. This unit of work provides:
/// <list type="bullet">
/// <item>Shared database connection across repositories</item>
/// <item>Explicit transaction management with commit/rollback</item>
/// <item>Repository factory for creating typed repositories</item>
/// </list>
/// </para>
/// <para>
/// For transactional operations, use <see cref="BeginTransactionAsync"/> to start a transaction,
/// perform operations through repositories, then commit or rollback.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// await using var unitOfWork = await unitOfWorkFactory.CreateAsync(cancellationToken);
/// await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
/// 
/// var orderRepo = unitOfWork.GetRepository&lt;Order&gt;();
/// var itemRepo = unitOfWork.GetRepository&lt;OrderItem&gt;();
/// 
/// await orderRepo.InsertAsync(order, cancellationToken);
/// await itemRepo.InsertAsync(items, cancellationToken);
/// 
/// await transaction.CommitAsync(cancellationToken);
/// </code>
/// </example>
public interface IDataUnitOfWork : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Gets the connection scope managed by this unit of work.
    /// </summary>
    IDataDbConnectionScope ConnectionScope { get; }

    /// <summary>
    /// Gets the current active transaction, if any.
    /// </summary>
    IDataTransaction? CurrentTransaction { get; }

    /// <summary>
    /// Gets a value indicating whether a transaction is currently active.
    /// </summary>
    bool HasActiveTransaction { get; }

    /// <summary>
    /// Gets a repository for the specified entity type.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <returns>A repository instance for the entity type.</returns>
    IDataRepository<TEntity> GetRepository<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TEntity>()
        where TEntity : class;

    /// <summary>
    /// Begins a new database transaction asynchronously.
    /// </summary>
    /// <param name="isolationLevel">The isolation level for the transaction.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The transaction instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when a transaction is already active.</exception>
    Task<IDataTransaction> BeginTransactionAsync(
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a new database transaction synchronously.
    /// </summary>
    /// <param name="isolationLevel">The isolation level for the transaction.</param>
    /// <returns>The transaction instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when a transaction is already active.</exception>
    IDataTransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
}

/// <summary>
/// Factory for creating <see cref="IDataUnitOfWork"/> instances.
/// </summary>
public interface IDataUnitOfWorkFactory
{
    /// <summary>
    /// Creates a new unit of work asynchronously with an open connection.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A new unit of work instance.</returns>
    Task<IDataUnitOfWork> CreateAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new unit of work synchronously with an open connection.
    /// </summary>
    /// <returns>A new unit of work instance.</returns>
    IDataUnitOfWork Create();
}
