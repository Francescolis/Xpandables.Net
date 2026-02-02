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
using System.Data.Common;

namespace System.Data;

/// <summary>
/// Provides a default implementation of <see cref="IDataTransaction"/> that wraps a <see cref="DbTransaction"/>.
/// </summary>
/// <remarks>
/// <para>
/// This implementation ensures that the transaction is rolled back if disposed without being committed.
/// It also tracks the transaction state to prevent multiple commits or rollbacks.
/// </para>
/// </remarks>
/// <param name="transaction">The underlying database transaction to wrap.</param>
/// <param name="onCompleted">Optional callback invoked when the transaction is completed (committed or rolled back).</param>
/// <exception cref="ArgumentNullException">Thrown when <paramref name="transaction"/> is null.</exception>
public sealed class DataTransaction(DbTransaction transaction, Action? onCompleted = null) : IDataTransaction
{
    private readonly DbTransaction _transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
    private readonly Action? _onCompleted = onCompleted;
    private bool _isDisposed;

    /// <inheritdoc />
    public DbTransaction DbTransaction => _transaction;

    /// <inheritdoc />
    public IsolationLevel IsolationLevel => _transaction.IsolationLevel;

    /// <inheritdoc />
    public bool IsCompleted { get; private set; }

    /// <inheritdoc />
    public bool IsCommitted { get; private set; }

    /// <inheritdoc />
    public bool IsRolledBack { get; private set; }

    /// <inheritdoc />
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfCompleted();

        await _transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        MarkAsCommitted();
    }

    /// <inheritdoc />
    public void Commit()
    {
        ThrowIfCompleted();

        _transaction.Commit();
        MarkAsCommitted();
    }

    /// <inheritdoc />
    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfCompleted();

        await _transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
        MarkAsRolledBack();
    }

    /// <inheritdoc />
    public void Rollback()
    {
        ThrowIfCompleted();

        _transaction.Rollback();
        MarkAsRolledBack();
    }

    /// <inheritdoc />
#pragma warning disable CA1031 // Intentionally catching all exceptions during disposal to ensure cleanup completes
    public void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;

        // If not completed, rollback the transaction
        if (!IsCompleted)
        {
            try
            {
                _transaction.Rollback();
                MarkAsRolledBack();
            }
            catch (Exception)
            {
                // Ignore rollback errors during disposal - the transaction may already be
                // in an invalid state and we cannot do anything about it during cleanup
            }
        }

        _transaction.Dispose();
    }
#pragma warning restore CA1031

    /// <inheritdoc />
#pragma warning disable CA1031 // Intentionally catching all exceptions during disposal to ensure cleanup completes
    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;

        // If not completed, rollback the transaction
        if (!IsCompleted)
        {
            try
            {
                await _transaction.RollbackAsync().ConfigureAwait(false);
                MarkAsRolledBack();
            }
            catch (Exception)
            {
                // Ignore rollback errors during disposal - the transaction may already be
                // in an invalid state and we cannot do anything about it during cleanup
            }
        }

        await _transaction.DisposeAsync().ConfigureAwait(false);
    }
#pragma warning restore CA1031

    private void ThrowIfCompleted()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        if (IsCompleted)
        {
            var state = IsCommitted ? "committed" : "rolled back";
            throw new InvalidOperationException(
                $"The transaction has already been {state}. A completed transaction cannot be modified.");
        }
    }

    private void MarkAsCommitted()
    {
        IsCompleted = true;
        IsCommitted = true;
        _onCompleted?.Invoke();
    }

    private void MarkAsRolledBack()
    {
        IsCompleted = true;
        IsRolledBack = true;
        _onCompleted?.Invoke();
    }
}
