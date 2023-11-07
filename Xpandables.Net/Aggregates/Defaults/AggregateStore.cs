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
using Xpandables.Net.Aggregates.DomainEvents;
using Xpandables.Net.Aggregates.IntegrationEvents;
using Xpandables.Net.Operations;
using Xpandables.Net.Operations.Messaging;

namespace Xpandables.Net.Aggregates.Defaults;

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
/// <param name="eventOutbox"></param>
/// <exception cref="ArgumentNullException"></exception>
public sealed class AggregateStore<TAggregate, TAggregateId>(
    IDomainEventStore<DomainEventRecord> eventStore,
    ITransientPublisher eventPublisher,
    IIntegrationEventOutbox eventOutbox) : IAggregateStore<TAggregate, TAggregateId>
    where TAggregate : class, IAggregate<TAggregateId>
    where TAggregateId : struct, IAggregateId<TAggregateId>
{
    private readonly IDomainEventStore<DomainEventRecord> _eventStore = eventStore
        ?? throw new ArgumentNullException(nameof(eventStore));
    private readonly ITransientPublisher _eventPublisher = eventPublisher
        ?? throw new ArgumentNullException(nameof(eventPublisher));
    private readonly IIntegrationEventOutbox _eventOutbox = eventOutbox
        ?? throw new ArgumentNullException(nameof(eventOutbox));

    ///<inheritdoc/>
    public async ValueTask<OperationResult> AppendAsync(
        TAggregate aggregate,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(aggregate);

        try
        {
            foreach (IDomainEvent<TAggregateId> @event in aggregate.GetUncommittedEvents())
            {
                await _eventStore.AppendAsync(@event, cancellationToken).ConfigureAwait(false);

                OperationResult operationResult = await _eventPublisher
                    .PublishAsync((dynamic)@event, cancellationToken)
                    .ConfigureAwait(false);

                if (operationResult.IsFailure)
                    return operationResult;
            }

            if (await _eventOutbox.AppendAsync(cancellationToken)
                .ConfigureAwait(false) is { IsFailure: true } failedOperation)
                return failedOperation;

            aggregate.MarkEventsAsCommitted();

            return OperationResults
                .Ok()
                .Build();
        }
        catch (Exception exception) when (exception is not ArgumentNullException)
        {
            return OperationResults
                .InternalError()
                .WithDetail("Failed to append Aggregate")
                .WithError(nameof(Aggregate<TAggregateId>), exception)
                .Build();
        }
    }

    ///<inheritdoc/>
    public async ValueTask<OperationResult<TAggregate>> ReadAsync(
        TAggregateId aggregateId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(aggregateId);

        try
        {
            TAggregate aggregate = AggregateExtensions.CreateEmptyAggregateInstance<TAggregate, TAggregateId>();

            await foreach (var @event in _eventStore.ReadAsync(aggregateId, cancellationToken))
            {
                aggregate.LoadFromHistory(@event);
            }

            return aggregate.IsEmpty
                ? OperationResults.NotFound<TAggregate>().Build()
                : OperationResults.Ok(aggregate).Build();
        }
        catch (Exception exception) when (exception is not ArgumentNullException)
        {
            return OperationResults
                .InternalError<TAggregate>()
                .WithDetail("Failed to read Aggregate")
                .WithError(nameof(Aggregate<TAggregateId>), exception)
                .Build();
        }
    }
}