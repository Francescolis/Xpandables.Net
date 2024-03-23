
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
using Xpandables.Net.Aggregates.DomainEvents;
using Xpandables.Net.Operations;
using Xpandables.Net.Primitives.I18n;

namespace Xpandables.Net.Aggregates;

/// <summary>
/// <see cref="IAggregateStore{TAggregate, TAggregateId}"/> implementation.
/// </summary>
/// <typeparam name="TAggregate">The type of aggregate.</typeparam>
/// <typeparam name="TAggregateId">The type of aggregate Id.</typeparam>
/// <remarks>
/// Initializes the aggregate store.
/// </remarks>
/// <param name="eventStore"></param>
/// <param name="eventPublisher"></param>
/// <exception cref="ArgumentNullException"></exception>
public sealed class AggregateStore<TAggregate, TAggregateId>(
    IDomainEventStore eventStore,
    IDomainEventPublisher<TAggregateId> eventPublisher)
    : IAggregateStore<TAggregate, TAggregateId>
    where TAggregate : class, IAggregate<TAggregateId>
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
            foreach (IDomainEvent<TAggregateId> @event
                in aggregate.GetUncommittedEvents())
            {
                await eventStore
                    .AppendAsync(@event, cancellationToken)
                    .ConfigureAwait(false);

                if (await eventPublisher
                    .PublishAsync((dynamic)@event, cancellationToken)
                    .ConfigureAwait(false)
                    is IOperationResult { IsFailure: true } operationResult)

                    return operationResult;
            }

            aggregate.MarkEventsAsCommitted();

            return OperationResults
                .Ok()
                .Build();
        }
        catch (OperationResultException resultException)
        {
            return resultException.OperationResult;
        }
        catch (Exception exception)
            when (exception is not ArgumentNullException)
        {
            return OperationResults
                .InternalError()
                .WithDetail(I18nXpandables.AggregateFailedToAppend)
                .WithError(nameof(Aggregate<TAggregateId>), exception)
                .Build();
        }
    }

    ///<inheritdoc/>
    public async ValueTask<IOperationResult<TAggregate>> ReadAsync(
        TAggregateId aggregateId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(aggregateId);

        try
        {
            TAggregate aggregate = AggregateExtensions
                .CreateEmptyAggregateInstance<TAggregate, TAggregateId>();

            await foreach (IDomainEvent<TAggregateId> @event in eventStore
                .ReadAsync(aggregateId, cancellationToken))
            {
                aggregate.LoadFromHistory(@event);
            }

            return aggregate.IsEmpty
                ? OperationResults.NotFound<TAggregate>().Build()
                : OperationResults.Ok(aggregate).Build();
        }
        catch (Exception exception)
            when (exception is not ArgumentNullException)
        {
            return OperationResults
                .InternalError<TAggregate>()
                .WithDetail(I18nXpandables.AggregateFailedToRead)
                .WithError(nameof(Aggregate<TAggregateId>), exception)
                .Build();
        }
    }
}