
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
using System.Runtime.CompilerServices;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Xpandables.Net.Repositories;

namespace Xpandables.Net.Aggregates;

/// <summary>
/// Represents the domain event store.
/// </summary>
/// <typeparam name="TEventEntity">The type of the event entity.</typeparam>
/// <param name="unitOfWork">The unit of work to use.</param>
/// <param name="options">The event configuration options.</param>
public sealed class EventDomainStore<TEventEntity>(
    [FromKeyedServices(EventOptions.UnitOfWorkKey)] IUnitOfWork unitOfWork,
    IOptions<EventOptions> options) :
    EventStore<TEventEntity>(unitOfWork, options), IEventDomainStore
    where TEventEntity : class, IEventEntityDomain
{
    ///<inheritdoc/>
    public async Task AppendAsync(
        IEventDomain @event,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(@event);

        await AppendEventAsync(@event, cancellationToken)
            .ConfigureAwait(false);
    }

    ///<inheritdoc/>
    public async IAsyncEnumerable<IEventDomain>
        ReadAsync(
        Guid aggregateId,
         [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(aggregateId);

        EntityFilter<TEventEntity> filter = new()
        {
            Criteria = x => x.AggregateId == aggregateId
        };

        EventConverter<TEventEntity> converter = Options
            .GetEventConverterFor<EventConverter<TEventEntity>>(
                typeof(TEventEntity));

        await foreach (TEventEntity entity in RepositoryRead
           .FetchAsync(filter, cancellationToken))
        {
            yield return converter
                .ConvertFrom(entity, Options.SerializerOptions)
                .AsRequired<IEventDomain>();
        }
    }

    ///<inheritdoc/>
    public IAsyncEnumerable<IEventDomain> ReadAsync(
       IEventFilter eventFilter,
       CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eventFilter);

        return ReadEventAsync<IEventDomain>(
            eventFilter,
            cancellationToken);
    }
}
