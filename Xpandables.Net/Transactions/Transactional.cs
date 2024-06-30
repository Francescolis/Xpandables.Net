/*******************************************************************************
 * Copyright (C) 2023 Francis-Black EWANE
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
using System.Runtime.ExceptionServices;

using Xpandables.Net.Operations;

namespace Xpandables.Net.Transactions;

/// <summary>
/// Abstract class that provides a transactional store implementation.
/// </summary>
public abstract class Transactional : Disposable, ITransactional
{
    private CancellationToken _cancellationToken;

    ///<inheritdoc/>
    public required IOperationResult Result { get; set; }

    ///<inheritdoc/>
    public async Task<ITransactional> TransactionAsync(
        CancellationToken cancellationToken = default)
    {
        _cancellationToken = cancellationToken;
        AppDomain.CurrentDomain.FirstChanceException +=
            Transaction_FirstChanceExceptionHanlder;

        await BeginTransactionAsync(_cancellationToken)
            .ConfigureAwait(false);

        return this;
    }

    ///<inheritdoc/>
    protected sealed override async ValueTask DisposeAsync(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        AppDomain.CurrentDomain.FirstChanceException -=
            Transaction_FirstChanceExceptionHanlder;

        await CompleteTransactionAsync(_cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Begins the transaction.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    protected abstract Task BeginTransactionAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Completes the transaction.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    protected abstract Task CompleteTransactionAsync(
        CancellationToken cancellationToken = default);

    private void Transaction_FirstChanceExceptionHanlder(
        object? sender,
        FirstChanceExceptionEventArgs e)
        => Result = e.Exception.ToOperationResult();
}
