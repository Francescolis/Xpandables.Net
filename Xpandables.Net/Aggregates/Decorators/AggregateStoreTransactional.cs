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
using Xpandables.Net.Primitives;
using Xpandables.Net.Transactions;

namespace Xpandables.Net.Aggregates.Decorators;

/// <summary>
/// This class acts like a decorator, allows the application author 
/// to add transactional support to the aggregate store.
/// </summary>
/// <typeparam name="TAggregate">The type of aggregate.</typeparam>
/// <typeparam name="TAggregateId">The type of aggregate Id.</typeparam>
/// <param name="decoratee">The decorated aggregate store.</param>
/// <param name="transactional">The aggregate transactional to use.</param>
public sealed class AggregateStoreTransactional<TAggregate, TAggregateId>(
    IAggregateStore<TAggregate, TAggregateId> decoratee,
    IAggregateTransactional transactional) :
    IAggregateStoreTransactional<TAggregate, TAggregateId>
    where TAggregate : class, IAggregate<TAggregateId>, ITransactionDecorator
    where TAggregateId : struct, IAggregateId<TAggregateId>
{
    ///<inheritdoc/>
    public async ValueTask<IOperationResult> AppendAsync(
        TAggregate aggregate,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(aggregate);

        try
        {
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using ITransactional disposable = await transactional
                .TransactionAsync(cancellationToken)
                .ConfigureAwait(false);
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

            disposable.Result = await decoratee
                .AppendAsync(aggregate, cancellationToken)
                .ConfigureAwait(false);

            return disposable.Result;
        }
        catch (OperationResultException operationEx)
        {
            return operationEx.Operation;
        }
        catch (Exception exception)
            when (exception is not ArgumentNullException)
        {
            return OperationResults
                .InternalError()
                .WithException(exception)
                .Build();
        }
    }

    ///<inheritdoc/>
    public ValueTask<IOperationResult<TAggregate>> ReadAsync(
        TAggregateId aggregateId,
        CancellationToken cancellationToken = default) =>
        decoratee.ReadAsync(aggregateId, cancellationToken);

}

