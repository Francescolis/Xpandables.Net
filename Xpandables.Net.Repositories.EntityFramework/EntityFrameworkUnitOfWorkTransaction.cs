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

using Microsoft.EntityFrameworkCore.Storage;

using Xpandables.Net.Repositories;

namespace Xpandables.Net.Repositories;

/// <summary>
/// Provides an implementation of the unit of work transaction pattern using an Entity Framework database transaction.
/// </summary>
/// <remarks>This class enables transactional operations within a unit of work pattern by wrapping an Entity
/// Framework IDbContextTransaction. It ensures that commit and rollback operations are properly delegated to the
/// underlying database transaction. Instances of this class are intended to be used within a single transaction scope
/// and should be disposed when no longer needed.</remarks>
/// <param name="transaction">The underlying Entity Framework database transaction to be managed by this unit of work transaction. Cannot be null.</param>
public sealed class EntityFrameworkUnitOfWorkTransaction(IDbContextTransaction transaction) : DisposableAsync, IUnitOfWorkTransaction
{
    private readonly IDbContextTransaction _transaction = transaction;

    /// <inheritdoc/>
    public DbTransaction Transaction => _transaction.GetDbTransaction();

    /// <inheritdoc/>
    public void CommitTransaction() => _transaction.Commit();

    /// <inheritdoc/>
    public Task CommitTransactionAsync(CancellationToken cancellationToken = default) =>
        _transaction.CommitAsync(cancellationToken);

    /// <inheritdoc/>
    public void Dispose() => _transaction.Dispose();

    /// <inheritdoc/>
    public void RollbackTransaction() => _transaction.Rollback();

    /// <inheritdoc/>
    public Task RollbackTransactionAsync(CancellationToken cancellationToken = default) =>
        _transaction.RollbackAsync(cancellationToken);

    /// <inheritdoc/>
    protected override ValueTask DisposeAsync(bool disposing) =>
        disposing ? _transaction.DisposeAsync() : ValueTask.CompletedTask;
}
