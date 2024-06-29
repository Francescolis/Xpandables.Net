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
using Xpandables.Net.Operations;

namespace Xpandables.Net.Transactions;

/// <summary>
/// Describe a contract used to define a transactional context.
/// </summary>
/// <remarks>Implement this interface for example to provide a transactional 
/// context for persistence.
/// You may derive from <see cref="Transactional"/> class to
/// implement a custom behavior.</remarks>
public interface ITransactional : IAsyncDisposable
{
    /// <summary>
    /// Gets or sets the transactional result.
    /// </summary>
    IOperationResult Result { get; set; }

    ///<summary>
    /// Manage a transactional context for persistence.
    ///</summary>
    ///<param name="cancellationToken">A cancellation token to observe while 
    ///waiting for the task to complete.</param>
    ///<remarks>The method must be used with a <see langword="using"/> 
    ///context.</remarks>
    ValueTask<ITransactional> TransactionAsync(
        CancellationToken cancellationToken = default);
}