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
/// Represents a unit of work transaction that encapsulates a database transaction.
/// </summary>
/// <remarks>This class provides methods to commit or roll back the underlying database transaction,  ensuring
/// proper transactional behavior. It also implements asynchronous disposal to  ensure that resources are released
/// properly in both synchronous and asynchronous contexts.</remarks>
/// <param name="dbTransaction"></param>
public sealed class UnitOfWorkTransaction(DbTransaction dbTransaction) : DisposableAsync, IUnitOfWorkTransaction
{
    private readonly DbTransaction _dbTransaction = dbTransaction;
    private bool _committed;
    private bool _rollback;
    private bool _exception;
    private bool _isDisposed;

    /// <inheritdoc/>
    public DbTransaction Transaction => _dbTransaction;

    /// <inheritdoc/>
    public void CommitTransaction()
    {
        ValidateCanCommit();

        _committed = true;
        _rollback = false;

        try
        {
            _dbTransaction.Commit();
        }
        catch
        {
            _exception = true;
            throw;
        }
    }

    /// <inheritdoc/>
    public void RollbackTransaction()
    {
        ValidateCanRollback();

        _rollback = true;
        _committed = false;

        try
        {
            _dbTransaction.Rollback();
        }
        catch
        {
            _exception = true;
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        ValidateCanCommit();

        _committed = true;
        _rollback = false;

        try
        {
            await _dbTransaction
                .CommitAsync(cancellationToken)
                .ConfigureAwait(false);
        }
        catch
        {
            _exception = true;
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        ValidateCanRollback();

        _rollback = true;
        _committed = false;

        try
        {
            await _dbTransaction
                .RollbackAsync(cancellationToken)
                .ConfigureAwait(false);
        }
        catch
        {
            _exception = true;
            throw;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!_isDisposed)
        {
            try
            {
                if (_exception && !_rollback)
                {
                    RollbackTransaction();
                }
                else if (!_exception && !_committed)
                {
                    try
                    {
                        CommitTransaction();
                    }
                    catch
                    {
                        RollbackTransaction();
                        throw;
                    }
                }
            }
            finally
            {
                _dbTransaction?.Dispose();
            }

            _isDisposed = true;
        }
    }

    /// <inheritdoc/>
    protected override async ValueTask DisposeAsync(bool disposing)
    {
        if (disposing && !_isDisposed)
        {
            try
            {
                if (_exception && !_rollback)
                {
                    await RollbackTransactionAsync().ConfigureAwait(false);
                }
                else if (!_exception && !_committed)
                {
                    try
                    {
                        await CommitTransactionAsync().ConfigureAwait(false);
                    }
                    catch
                    {
                        await RollbackTransactionAsync().ConfigureAwait(false);
                        throw;
                    }
                }
            }
            finally
            {
                await _dbTransaction.DisposeAsync().ConfigureAwait(false);
            }

            _isDisposed = true;
        }

        await base.DisposeAsync(disposing).ConfigureAwait(false);
    }

    private void ValidateCanRollback()
    {
        if (_dbTransaction.Connection is null)
        {
            throw new InvalidOperationException("No transaction is in progress.");
        }
        if (_rollback)
        {
            throw new InvalidOperationException("Transaction has already been rolled back.");
        }
        if (_committed)
        {
            throw new InvalidOperationException("Transaction has already been committed.");
        }
        if (_dbTransaction.Connection.State != ConnectionState.Open)
        {
            throw new InvalidOperationException("Transaction connection is not open.");
        }
    }

    private void ValidateCanCommit()
    {
        if (_dbTransaction.Connection is null)
        {
            throw new InvalidOperationException("No transaction is in progress.");
        }
        if (_committed)
        {
            throw new InvalidOperationException("Transaction has already been committed.");
        }
        if (_rollback)
        {
            throw new InvalidOperationException("Transaction has already been rolled back.");
        }
        if (_dbTransaction.Connection.State != ConnectionState.Open)
        {
            throw new InvalidOperationException("Transaction connection is not open.");
        }
    }
}
