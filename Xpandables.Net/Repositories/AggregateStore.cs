/*******************************************************************************
 * Copyright (C) 2024 Francis-Black EWANE
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

using System.ComponentModel.DataAnnotations;

using Xpandables.Net.Executions.Domains;
using Xpandables.Net.Executions.Tasks;

namespace Xpandables.Net.Repositories;

/// <summary>
/// Represents a store for managing aggregate by providing methods to append and resolve aggregates.
/// This class is designed for use with event sourcing and domain-driven design concepts.
/// </summary>
/// <typeparam name="TAggregate">
/// The type of the aggregate, which must inherit from <see cref="Aggregate"/> and have a parameterless constructor.
/// </typeparam>
public sealed class AggregateStore<TAggregate>(
    IUnitOfWorkEvent unitOfWork,
    IPublisher publisher) :
    IAggregateStore<TAggregate>
    where TAggregate : Aggregate, new()
{
    private readonly IPublisher _publisher = publisher;
    private readonly IEventStore _eventStore = unitOfWork.GetEventStore<IEventStore>();

    /// <inheritdoc />
    public async Task AppendAsync(
        TAggregate aggregate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            IReadOnlyCollection<IDomainEvent> uncommittedEvents =
                aggregate.GetUncommittedEvents();

            if (uncommittedEvents.Count == 0)
            {
                return;
            }

            await _eventStore
                .AppendAsync(uncommittedEvents, cancellationToken)
                .ConfigureAwait(false);

            Task[] tasks =
            [
                .. uncommittedEvents
                    .Select(async @event => await _publisher
                        .PublishAsync(@event, cancellationToken)
                        .ConfigureAwait(false))
            ];

            await Task.WhenAll(tasks).ConfigureAwait(false);

            aggregate.MarkEventsAsCommitted();
        }
        catch (Exception exception)
            when (exception is not ValidationException and not InvalidOperationException)
        {
            throw new InvalidOperationException(
                "Unable to append the object. See inner exception for details.",
                exception);
        }
    }

    /// <inheritdoc />
    public async Task<TAggregate> ResolveAsync(
        Guid keyId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            string aggregateTypeName = typeof(TAggregate).FullName!;

            Func<IQueryable<EntityDomainEvent>, IQueryable<EntityDomainEvent>> domainFilterFunc = query =>
                query.Where(w => w.AggregateId == keyId && w.AggregateName == aggregateTypeName)
                    .OrderBy(o => o.Version);

            TAggregate aggregate = new();

            await foreach (IDomainEvent @event in _eventStore
                .FetchAsync(domainFilterFunc, cancellationToken)
                .AsEventsAsync(cancellationToken)
                .OfType<IDomainEvent>()
                .OrderBy(x => x.Version)
                .ConfigureAwait(false))
            {
                aggregate.LoadFromHistory(@event);
            }

            if (aggregate.IsEmpty)
            {
                throw new ValidationException(new ValidationResult(
                    "The entity was not found.",
                    [nameof(keyId)]), null, keyId);
            }

            return aggregate;
        }
        catch (Exception exception)
            when (exception is not ValidationException and not InvalidOperationException)
        {
            throw new InvalidOperationException(
                "Unable to peek the entity. See inner exception for details.",
                exception);
        }
    }
}