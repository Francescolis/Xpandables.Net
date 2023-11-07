﻿/************************************************************************************************************
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
************************************************************************************************************/
using Xpandables.Net.Aggregates;
using Xpandables.Net.Aggregates.Defaults;
using Xpandables.Net.Aggregates.Snapshots;
using Xpandables.Net.Operations;

namespace Xpandables.Net.Decorators;

internal sealed class SnapshotStoreDecorator<TAggregate, TAggregateId>(
    IAggregateStore<TAggregate, TAggregateId> decoratee,
    IDomainEventStore<DomainEventRecord> eventStore,
    ISnapshotStore snapShotStore,
    SnapShotOptions snapShotOptions) : IAggregateStore<TAggregate, TAggregateId>
    where TAggregate : class, IAggregate<TAggregateId>, IOriginator
    where TAggregateId : struct, IAggregateId<TAggregateId>
{
    private readonly IAggregateStore<TAggregate, TAggregateId> _decoratee = decoratee
        ?? throw new ArgumentNullException(nameof(decoratee));
    private readonly IDomainEventStore<DomainEventRecord> _eventStore = eventStore
        ?? throw new ArgumentNullException(nameof(eventStore));
    private readonly ISnapshotStore _snapShotStore = snapShotStore
        ?? throw new ArgumentNullException(nameof(snapShotStore));
    private readonly SnapShotOptions _snapShotOptions = snapShotOptions
        ?? throw new ArgumentNullException(nameof(snapShotOptions));

    public async ValueTask<OperationResult> AppendAsync(
        TAggregate aggregate,
        CancellationToken cancellationToken = default)
    {
        if (_snapShotOptions.IsOn
            && aggregate.Version % _snapShotOptions.Frequency == 0
            && aggregate.Version >= _snapShotOptions.Frequency)
        {
            try
            {
                SnapShotDescriptor descriptor = new(
                    aggregate,
                    aggregate.AggregateId,
                    aggregate.Version);

                await _snapShotStore.PersistAsSnapShotAsync(descriptor, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception exception) when (exception is not ArgumentNullException)
            {
                return OperationResults
                    .BadRequest()
                    .WithError(nameof(SnapshotStoreDecorator<TAggregate, TAggregateId>), exception)
                    .Build();
            }
        }

        return await _decoratee.AppendAsync(aggregate, cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask<OperationResult<TAggregate>> ReadAsync(
         TAggregateId aggregateId,
         CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(aggregateId);

        if (_snapShotOptions.IsOff)
            return await _decoratee.ReadAsync(aggregateId, cancellationToken)
                .ConfigureAwait(false);

        TAggregate? aggregate = default;
        try
        {
            var optionalResult = await _snapShotStore
                .ReadFromSnapShotAsync<TAggregate>(aggregateId.Value, cancellationToken)
                .ConfigureAwait(false);

            optionalResult.Map(result => aggregate = result);
        }
        catch (Exception exception) when (exception is not ArgumentNullException)
        {
            return OperationResults
                .BadRequest<TAggregate>()
                .WithError(nameof(SnapshotStoreDecorator<TAggregate, TAggregateId>), exception)
                .Build();
        }

        if (aggregate is null)
            return await _decoratee
                .ReadAsync(aggregateId, cancellationToken)
                .ConfigureAwait(false);

        // because the snapshot is not aligned with the last events,
        // we need to add those events if available
        var filter = new DomainEventFilter
        {
            AggregateId = aggregateId.Value,
            Criteria = x => x.Version > aggregate.Version,
            OrderBy = x => x.OrderBy(o => o.Version)
        };

        try
        {
            await foreach (var eventRecord in _eventStore.ReadAsync(filter, cancellationToken)
                           .ConfigureAwait(false))
            {
                DomainEventRecord.ToDomainEventRecord<TAggregateId>(eventRecord, default)
                    .Map(@event => aggregate.LoadFromHistory(@event));
            }
        }
        catch (Exception exception) when (exception is not ArgumentNullException)
        {
            return OperationResults
                .BadRequest<TAggregate>()
                .WithError(nameof(SnapshotStoreDecorator<TAggregate, TAggregateId>), exception)
                .Build();
        }

        return OperationResults
            .Ok(aggregate)
            .Build();
    }
}