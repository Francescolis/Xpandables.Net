
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
/// Represents a unit of work transaction that encapsulates a database transaction,  providing methods to commit or roll
/// back changes synchronously or asynchronously.
/// </summary>
/// <remarks>This interface is designed to manage the lifecycle of a database transaction within a unit of work
/// pattern.  It ensures that all operations performed within the transaction scope are either committed as a single
/// unit  or rolled back in case of failure. Implementations of this interface should guarantee proper disposal of 
/// resources by implementing both <see cref="IDisposable"/> and <see cref="IAsyncDisposable"/>.</remarks>
public interface IUnitOfWorkTransaction : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Gets the underlying database transaction.
    /// </summary>
    DbTransaction Transaction { get; }

    /// <summary>
    /// Commits the current transaction, finalizing all operations performed within the transaction scope.
    /// </summary>
    /// <remarks>Once the transaction is committed, all changes made during the transaction become permanent. 
    /// This method should only be called after all necessary operations within the transaction have been
    /// completed.</remarks>
    void CommitTransaction();

    /// <summary>
    /// Rolls back the current transaction, undoing any changes made since the transaction began.
    /// </summary>
    /// <remarks>This method reverts all operations performed within the scope of the current transaction.  It
    /// should be called only if the transaction is in progress and needs to be canceled due to an error or other
    /// condition. Calling this method on a transaction that has already been committed or rolled back may result in an
    /// exception.</remarks>
    void RollbackTransaction();

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
