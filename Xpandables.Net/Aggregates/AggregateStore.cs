
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
using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.Operations;
using Xpandables.Net.Primitives.I18n;
using Xpandables.Net.Repositories;

namespace Xpandables.Net.Aggregates;

/// <summary>
/// <see cref="IAggregateStore{TAggregate}"/> implementation.
/// </summary>
/// <typeparam name="TAggregate">The type of aggregate.</typeparam>
/// <remarks>
/// Initializes the aggregate store.
/// </remarks>
/// <param name="eventPublisher">The event publisher to use.</param>
/// <param name="eventStore">The event store to use.</param>
/// <param name="unitOfWork">The unit of work to use.</param>
/// <exception cref="ArgumentNullException">The <paramref name="eventPublisher"/> 
/// or <paramref name="eventStore"/> is null.</exception>"
public sealed class AggregateStore<TAggregate>(
    IEventDomainStore eventStore,
    IEventPublisher eventPublisher,
    [FromKeyedServices(EventOptions.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IAggregateStore<TAggregate>
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

                if (await eventPublisher
                    .PublishAsync((dynamic)@event, cancellationToken)
                    .ConfigureAwait(false)
                    is IOperationResult { IsFailure: true } operationResult)
                {
                    return operationResult;
                }
            }

            await unitOfWork
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
    public async Task<IOperationResult<TAggregate>> ReadAsync(
        Guid keyId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(keyId);

        try
        {
            TAggregate aggregate = AggregateExtensions
                .CreateEmptyAggregateInstance<TAggregate>();

            await foreach (IEventDomain @event in eventStore
                .ReadAsync(keyId, cancellationToken))
            {
                aggregate.LoadFromHistory(@event);
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