
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

using Xpandables.Net.Operations;
using Xpandables.Net.Primitives.I18n;
using Xpandables.Net.Primitives.Text;

namespace Xpandables.Net.Aggregates;

internal sealed class AggregateStoreSnapshot<TAggregate>(
    IAggregateStore<TAggregate> decoratee,
    IEventDomainStore eventStore,
    IEventSnapshotStore snapShotStore,
    IOptions<EventOptions> options)
    : IAggregateStoreSnapshot<TAggregate>
    where TAggregate : class, IAggregate, IOriginator
{
    public async ValueTask<IOperationResult> AppendAsync(
        TAggregate aggregate,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(aggregate);

        try
        {
            if (IsSnapshopOptionsActive(aggregate))
            {
                EventConverter<IAggregate> converter = options.Value
                    .Converters
                    .FirstOrDefault(x => x.CanConvert(typeof(IAggregate)))
                    .As<EventConverter<IAggregate>>()
                    ?? throw new InvalidOperationException(
                        I18nXpandables.AggregateFailedToFindConverter
                            .StringFormat(nameof(IAggregate)));

                IEventSnapshot snapshot = converter
                    .ConvertFrom(aggregate, options.Value.SerializerOptions)
                    .AsRequired<IEventSnapshot>();

                await snapShotStore
                   .AppendAsync(snapshot, cancellationToken)
                   .ConfigureAwait(false);
            }

            return await decoratee
              .AppendAsync(aggregate, cancellationToken)
              .ConfigureAwait(false);
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
         Guid aggregateId,
         CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(aggregateId);

        if (options.Value.SnapshotOptions.IsOff)
        {
            return await decoratee
                .ReadAsync(aggregateId, cancellationToken)
                .ConfigureAwait(false);
        }

        TAggregate? aggregate = default;

        if (await snapShotStore
            .ReadAsync(aggregateId, cancellationToken)
            .ConfigureAwait(false) is { } snapshot)
        {
            EventConverter<IAggregate> converter = options.Value
                .Converters
                .FirstOrDefault(x => x.CanConvert(typeof(IAggregate)))
                .As<EventConverter<IAggregate>>()
                ?? throw new InvalidOperationException(
                    I18nXpandables.AggregateFailedToFindConverter
                        .StringFormat(nameof(IAggregate)));

            aggregate = converter
                .ConvertTo(snapshot, options.Value.SerializerOptions)
                .AsRequired<TAggregate>();
        }

        if (aggregate is null)
        {
            return await decoratee
                .ReadAsync(aggregateId, cancellationToken)
                .ConfigureAwait(false);
        }

        // because the snapshot is not always aligned with the last events,
        // we need to add those events if available
        IEventFilter filter = new EventFilter()
        {
            AggregateId = aggregateId,
            AggregateName = typeof(TAggregate).Name,
            Version = aggregate.Version
        };

        try
        {
            await foreach (IEventDomain? @event in eventStore
                .ReadAsync(filter, cancellationToken)
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

    private bool IsSnapshopOptionsActive(IAggregate aggregate) =>
        options.Value.SnapshotOptions.IsOn
            && aggregate.Version % options.Value.SnapshotOptions.Frequency == 0
            && aggregate.Version >= options.Value.SnapshotOptions.Frequency;
}
