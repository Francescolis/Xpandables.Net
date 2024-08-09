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

using Xpandables.Net.Events;
using Xpandables.Net.Operations;
using Xpandables.Net.Primitives;

namespace Xpandables.Net.Aggregates;
internal sealed class AggregateAccessorSnapshot<TAggregate>(
    IAggregateAccessor<TAggregate> decoratee,
    IEventStore eventStore,
    IOptions<EventOptions> options) : IAggregateAccessor<TAggregate>
    where TAggregate : class, IAggregate, IOriginator
{
    public async Task<IOperationResult> AppendAsync(
        TAggregate aggregate,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(aggregate);

        try
        {
            if (IsSnapshopOptionsActive(aggregate))
            {
                EventAggregateConverter converter = options
                    .Value
                    .GetEventConverterFor(typeof(IAggregate))
                    .AsRequired<EventAggregateConverter>();

                IEvent @event = converter
                    .ConvertFromAggregate(
                        aggregate,
                        options.Value.SerializerOptions);

                await eventStore
                    .AppendAsync(@event, cancellationToken)
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

    public async Task<IOperationResult<TAggregate>> PeekAsync(
        Guid keyId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(keyId);

        if (options.Value.SnapshotOptions.IsOff)
        {
            return await decoratee
                .PeekAsync(keyId, cancellationToken)
                .ConfigureAwait(false);
        }

        TAggregate? aggregate = default;

        IEventFilter filter = options.Value
            .GetEventFilterFor<IEventSnapshot>();

        filter.KeyId = keyId;
        filter.Paging = Pagination.With(1, 1);

        IEvent? eventSnapshot = eventStore
            .FetchAsync(filter, cancellationToken)
            .ToBlockingEnumerable(cancellationToken)
            .FirstOrDefault();

        if (eventSnapshot is not null)
        {
            EventAggregateConverter converter = options
                .Value
                .GetEventConverterFor(typeof(IAggregate))
                .AsRequired<EventAggregateConverter>();

            aggregate = converter
                .ConvertToAggregate(
                    eventSnapshot,
                    options.Value.SerializerOptions)
                .AsRequired<TAggregate>();
        }

        if (aggregate is null)
        {
            return await decoratee
                .PeekAsync(keyId, cancellationToken)
                .ConfigureAwait(false);
        }

        // because the snapshot is not always aligned with the last events,
        // we need to add those events if available
        IEventFilter eventFilter = options.Value
                 .GetEventFilterFor<IEventDomain>();

        eventFilter.KeyId = keyId;
        eventFilter.Version = aggregate.Version;
        eventFilter.AggregateName = typeof(TAggregate).Name;

        try
        {
            await foreach (IEvent @event in eventStore
                .FetchAsync(eventFilter, cancellationToken)
                .ConfigureAwait(false))
            {
                if (@event is IEventDomain eventDomain)
                {
                    aggregate.LoadFromHistory(eventDomain);
                }
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
