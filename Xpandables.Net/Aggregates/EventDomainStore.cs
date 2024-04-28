
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

using Xpandables.Net.Primitives.I18n;
using Xpandables.Net.Primitives.Text;
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
    public async ValueTask AppendAsync<TAggregateId>(
        IEventDomain<TAggregateId> @event,
        CancellationToken cancellationToken = default)
        where TAggregateId : struct, IAggregateId<TAggregateId>
    {
        ArgumentNullException.ThrowIfNull(@event);

        await AppendEventAsync(@event, cancellationToken)
            .ConfigureAwait(false);
    }

    ///<inheritdoc/>
    public async IAsyncEnumerable<IEventDomain<TAggregateId>> ReadAsync<TAggregateId>(
        TAggregateId aggregateId,
         [EnumeratorCancellation] CancellationToken cancellationToken = default)
        where TAggregateId : struct, IAggregateId<TAggregateId>
    {
        ArgumentNullException.ThrowIfNull(aggregateId);

        EntityFilter<TEventEntity> filter = new()
        {
            Criteria = x => x.AggregateId == aggregateId.Value
            && x.AggregateIdTypeName == typeof(TAggregateId)
                .GetNameWithoutGenericArity()
        };

        EventConverter<TEventEntity> converter = Options
            .Converters
            .FirstOrDefault(x => x.CanConvert(typeof(TEventEntity)))
            .As<EventConverter<TEventEntity>>()
            ?? throw new InvalidOperationException(
                I18nXpandables.AggregateFailedToFindConverter
                    .StringFormat(
                        typeof(TEventEntity).GetNameWithoutGenericArity()));

        await foreach (TEventEntity entity in Repository
           .FetchAsync(filter, cancellationToken))
        {
            yield return converter
                .ConvertFrom(entity, Options.SerializerOptions)
                .AsRequired<IEventDomain<TAggregateId>>();
        }
    }

    ///<inheritdoc/>
    public IAsyncEnumerable<IEventDomain<TAggregateId>> ReadAsync<TAggregateId>(
       IEventFilter eventFilter,
       CancellationToken cancellationToken = default)
        where TAggregateId : struct, IAggregateId<TAggregateId>
    {
        ArgumentNullException.ThrowIfNull(eventFilter);

        return ReadEventAsync<IEventDomain<TAggregateId>>(
            eventFilter,
            cancellationToken);
    }
}
