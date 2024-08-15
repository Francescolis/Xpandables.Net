
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

using Xpandables.Net.Aggregates.Events;
using Xpandables.Net.Distribution;
using Xpandables.Net.Operations;
using Xpandables.Net.Primitives.I18n;

namespace Xpandables.Net.Aggregates;

/// <summary>
/// <see cref="IAggregateAccessor{TAggregate}"/> implementation.
/// </summary>
/// <typeparam name="TAggregate">The type of aggregate.</typeparam>
/// <remarks>
/// Initializes the aggregate store.
/// </remarks>
/// <param name="publisher">The event publisher to use.</param>
/// <param name="eventStore">The event store to use.</param>
/// <param name="repository">The repository event to use.</param>
/// <param name="options">The event configuration options to use.</param>
/// <exception cref="ArgumentNullException">The <paramref name="publisher"/> 
/// or <paramref name="eventStore"/> is null.</exception>"
public sealed class AggregateAccessor<TAggregate>(
    IRepositoryEvent repository,
    IEventStore eventStore,
    IEventPublisher publisher,
    IOptions<EventOptions> options) :
    IAggregateAccessor<TAggregate>
    where TAggregate : class, IAggregate
{
    ///<inheritdoc/>
    public async Task<IOperationResult> AppendAsync(
        TAggregate aggregate,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(aggregate);

        try
        {
            foreach (IEventDomain @event
                in aggregate.GetUncommittedEvents())
            {
                await eventStore
                    .AppendAsync(@event, cancellationToken)
                    .ConfigureAwait(false);

                if (await publisher
                    .PublishAsync((dynamic)@event, cancellationToken)
                    .ConfigureAwait(false)
                    is IOperationResult { IsFailure: true } operationResult)
                {
                    return operationResult;
                }
            }

            await repository
                .PersistAsync(cancellationToken)
                .ConfigureAwait(false);

            aggregate.MarkEventsAsCommitted();

            return OperationResults
                .Ok()
                .Build();
        }
        catch (OperationResultException resultException)
        {
            return resultException.Operation;
        }
        catch (Exception exception)
            when (exception is not ArgumentNullException)
        {
            return OperationResults
                .InternalError()
                .WithDetail(I18nXpandables.AggregateFailedToAppend)
                .WithException(exception)
                .Build();
        }
    }

    ///<inheritdoc/>
    public async Task<IOperationResult<TAggregate>> PeekAsync(
        Guid keyId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            TAggregate aggregate = AggregateExtensions
                .CreateEmptyAggregateInstance<TAggregate>();

            IEventFilter filter = options.Value
                .GetEventFilterFor<IEventDomain>();

            filter.KeyId = keyId;

            await foreach (IEvent @event in eventStore
                .FetchAsync(filter, cancellationToken))
            {
                if (@event is IEventDomain eventDomain)
                {
                    aggregate.LoadFromHistory(eventDomain);
                }
            }

            return aggregate.IsEmpty
                ? OperationResults
                    .NotFound<TAggregate>()
                    .WithError(
                        nameof(keyId),
                        I18nXpandables.HttpStatusCodeNotFound)
                    .Build()
                : OperationResults.Ok(aggregate).Build();
        }
        catch (Exception exception)
            when (exception is not ArgumentNullException)
        {
            return OperationResults
                .InternalError<TAggregate>()
                .WithDetail(I18nXpandables.AggregateFailedToRead)
                .WithException(exception)
                .Build();
        }
    }
}
