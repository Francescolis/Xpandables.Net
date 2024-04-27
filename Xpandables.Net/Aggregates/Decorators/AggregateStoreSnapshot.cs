
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
using Microsoft.Extensions.Options;

using Xpandables.Net.Aggregates.DomainEvents;
using Xpandables.Net.Aggregates.SnapShots;
using Xpandables.Net.Operations;
using Xpandables.Net.Optionals;
using Xpandables.Net.Transactions;

namespace Xpandables.Net.Aggregates.Decorators;

internal sealed class AggregateStoreSnapshot<TAggregate, TAggregateId>(
    IAggregateStore<TAggregate, TAggregateId> decoratee,
    IAggregateTransactional transactional,
    IDomainEventStore eventStore,
    ISnapShotStore snapShotStore,
    IOptions<SnapShotOptions> snapShotOptions)
    : IAggregateStoreSnapshot<TAggregate, TAggregateId>
    where TAggregate : class, IAggregate<TAggregateId>, IOriginator
    where TAggregateId : struct, IAggregateId<TAggregateId>
{
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

            if (snapShotOptions.Value.IsOn
                && aggregate.Version % snapShotOptions.Value.Frequency == 0
                && aggregate.Version >= snapShotOptions.Value.Frequency)
            {
                SnapShotDescriptor descriptor = new(
                    aggregate,
                    aggregate.AggregateId,
                    aggregate.Version);

                await snapShotStore.AppendAsync(descriptor, cancellationToken)
                    .ConfigureAwait(false);
            }

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

    public async ValueTask<IOperationResult<TAggregate>> ReadAsync(
         TAggregateId aggregateId,
         CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(aggregateId);

        if (snapShotOptions.Value.IsOff)
        {
            return await decoratee
                .ReadAsync(aggregateId, cancellationToken)
                .ConfigureAwait(false);
        }

        TAggregate? aggregate = default;

        try
        {
            Optional<TAggregate> optionalResult = await snapShotStore
                .ReadAsync<TAggregate>(aggregateId.Value, cancellationToken)
                .ConfigureAwait(false);

            _ = optionalResult.Map(result => aggregate = result);
        }
        catch (Exception exception)
            when (exception is not ArgumentNullException)
        {
            return OperationResults
                .BadRequest<TAggregate>()
                .WithException(exception)
                .Build();
        }

        if (aggregate is null)
        {
            return await decoratee
                .ReadAsync(aggregateId, cancellationToken)
                .ConfigureAwait(false);
        }

        // because the snapshot is not always aligned with the last events,
        // we need to add those events if available
        DomainEventFilter filter = new()
        {
            AggregateId = aggregateId.Value,
            AggregateIdTypeName = typeof(TAggregateId).Name,
            Version = aggregate.Version
        };

        try
        {
            await foreach (IDomainEvent<TAggregateId>? @event in eventStore
                .ReadAsync<TAggregateId>(filter, cancellationToken)
                .ConfigureAwait(false))
            {
                aggregate.LoadFromHistory(@event);
            }
        }
        catch (Exception exception)
            when (exception is not ArgumentNullException)
        {
            return OperationResults
                .BadRequest<TAggregate>()
                .WithException(exception)
                .Build();
        }

        return OperationResults
            .Ok(aggregate)
            .Build();
    }
}
