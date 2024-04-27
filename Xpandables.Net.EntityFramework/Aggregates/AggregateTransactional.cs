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

using Microsoft.EntityFrameworkCore.Storage;

using Xpandables.Net.Transactions;

namespace Xpandables.Net.Aggregates;

/// <summary>
/// The default implementation of <see cref="IAggregateTransactional"/> type.
/// </summary>
public sealed class AggregateTransactional(
    DataContextDomain dataContext) :
    Transactional, IAggregateTransactional
{
    private IDbContextTransaction? _transaction;

    ///<inheritdoc/>
    protected override async ValueTask BeginTransactionAsync(
        CancellationToken cancellationToken = default)
    {
        _transaction = await dataContext
            .Database
            .BeginTransactionAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    ///<inheritdoc/>
    protected override async ValueTask CompleteTransactionAsync(
        CancellationToken cancellationToken = default)
    {
        if (_transaction is not null)
            if (Result.IsSuccess)
                await _transaction
                    .CommitAsync(cancellationToken)
                    .ConfigureAwait(false);
            else
                await _transaction
                    .RollbackAsync(cancellationToken)
                    .ConfigureAwait(false);
    }
}
