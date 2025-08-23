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
using System.Data.Common;

namespace Xpandables.Net.Repositories;

/// <summary>
/// Represents an abstract unit of work that encapsulates a series of operations  to be executed as a single
/// transaction. This class provides a base implementation  for managing transactional operations and repository access
/// in a consistent manner.
/// </summary>
/// <remarks>The <see cref="UnitOfWorkBase"/> class is designed to coordinate changes across multiple  repositories
/// and ensure that all operations are committed or rolled back as a single  atomic unit. Derived classes must implement
/// methods for transaction management and  repository retrieval to provide concrete functionality. <para> This class
/// also implements <see cref="IUnitOfWork"/> and <see cref="AsyncDisposable"/>  to support both synchronous and
/// asynchronous resource management. </para> <para> Typical usage involves beginning a transaction, performing
/// operations on repositories,  and then calling <see cref="SaveChanges"/> or <see cref="SaveChangesAsync"/> to persist
/// changes. Transactions can also be explicitly managed using the  <see cref="BeginTransaction"/> or <see
/// cref="UseTransaction"/> methods. </para></remarks>
public abstract class UnitOfWorkBase : AsyncDisposable, IUnitOfWork
{
    /// <inheritdoc />
    public virtual Task<IUnitOfWorkTransaction> BeginTransactionAsync(
        CancellationToken cancellationToken = default) =>
        throw new NotImplementedException("This method must be overridden in derived classes.");

    /// <inheritdoc />
    public virtual IUnitOfWorkTransaction BeginTransaction() =>
        throw new NotImplementedException("This method must be overridden in derived classes.");

    /// <inheritdoc />
    public virtual Task<IUnitOfWorkTransaction> UseTransactionAsync(
        DbTransaction transaction,
        CancellationToken cancellationToken = default) =>
        throw new NotImplementedException("This method must be overridden in derived classes.");

    /// <inheritdoc />
    public virtual IUnitOfWorkTransaction UseTransaction(DbTransaction transaction) =>
        throw new NotImplementedException("This method must be overridden in derived classes.");

    /// <inheritdoc />
    public virtual Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(0);

    /// <inheritdoc />
    public virtual int SaveChanges() => 0;

    /// <inheritdoc />
    public virtual TRepository GetRepository<TRepository>()
        where TRepository : class, IRepository =>
        throw new NotImplementedException(
            "This method must be overridden in derived classes to return the appropriate repository instance.");

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the resources used by the current instance of the class.
    /// </summary>
    /// <remarks>This method should be called when the instance is no longer needed to ensure that all
    /// resources  are properly released. If the instance is disposed, it cannot be used again.</remarks>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        if (IsDisposed)
        {
            return;
        }

        if (disposing)
        {
            // Release all managed resources here
            // Need to unregister/detach yourself from the events.
            // Always make sure the object is not null first before trying to
            // unregister/detach them!
            // Failure to unregister can be a BIG source of memory leaks
        }

        // Release all unmanaged resources here and override a finalizer below.
        // Set large fields to null.

        // Dispose has been called.
        IsDisposed = true;

        // If it is available, make the call to the
        // base class's Dispose(boolean) method
    }
}
