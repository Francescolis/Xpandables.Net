
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

using Microsoft.Extensions.Options;

using Xpandables.Net.Primitives.I18n;
using Xpandables.Net.Primitives.Text;
using Xpandables.Net.Repositories;

namespace Xpandables.Net.Aggregates;

/// <summary>
/// Represents the domain event store.
/// </summary>
/// <typeparam name="TEventEntity">The type of the event entity.</typeparam>
/// <param name="repository">The repository to use.</param>
/// <param name="options">The event configuration options.</param>
public sealed class DomainEventStore<TEventEntity>(
    IRepository<TEventEntity> repository,
    IOptions<EventOptions> options)
    : Disposable, IDomainEventStore
    where TEventEntity : class, IEventEntityDomain
{
    private IDisposable[] _disposables = [];

    ///<inheritdoc/>
    public async ValueTask AppendAsync<TAggregateId>(
        IEventDomain<TAggregateId> @event,
        CancellationToken cancellationToken = default)
        where TAggregateId : struct, IAggregateId<TAggregateId>
    {
        ArgumentNullException.ThrowIfNull(@event);

        EventConverter<TEventEntity> converter = options.Value
            .Converters
            .FirstOrDefault(x => x.CanConvert(@event.GetType()))
            .As<EventConverter<TEventEntity>>()
            ?? throw new InvalidOperationException(
                I18nXpandables.AggregateFailedToFindConverter
                    .StringFormat(
                        @event.GetType().GetNameWithoutGenericArity()));

        TEventEntity entity = converter
            .ConvertTo(@event, options.Value.SerializerOptions);

        Array.Resize(ref _disposables, _disposables.Length + 1);
        _disposables[^1] = entity;

        await repository
            .InsertAsync(entity, cancellationToken)
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

        EventConverter<TEventEntity> converter = options.Value
            .Converters
            .FirstOrDefault(x => x.CanConvert(typeof(TEventEntity)))
            .As<EventConverter<TEventEntity>>()
            ?? throw new InvalidOperationException(
                I18nXpandables.AggregateFailedToFindConverter
                    .StringFormat(
                        typeof(TEventEntity).GetNameWithoutGenericArity()));

        await foreach (TEventEntity entity in repository
           .FetchAsync(filter, cancellationToken))
        {
            yield return converter
                .ConvertFrom(entity, options.Value.SerializerOptions)
                .AsRequired<IEventDomain<TAggregateId>>();
        }
    }

    ///<inheritdoc/>
    public async IAsyncEnumerable<IEventDomain<TAggregateId>> ReadAsync<TAggregateId>(
       IEventFilter eventFilter,
       [EnumeratorCancellation] CancellationToken cancellationToken = default)
        where TAggregateId : struct, IAggregateId<TAggregateId>
    {
        ArgumentNullException.ThrowIfNull(eventFilter);

        EventEntityFilter<TEventEntity> applyFilter = options.Value
            .Filters
            .FirstOrDefault(x => x.CanFilter(typeof(TEventEntity)))
            .As<EventEntityFilter<TEventEntity>>()
            ?? throw new InvalidOperationException(
                I18nXpandables.AggregateFailedToFindFilter
                    .StringFormat(
                        typeof(TEventEntity).GetNameWithoutGenericArity()));

        EventConverter<TEventEntity> converter = options.Value
            .Converters
            .FirstOrDefault(x => x.CanConvert(typeof(TEventEntity)))
            .As<EventConverter<TEventEntity>>()
            ?? throw new InvalidOperationException(
                I18nXpandables.AggregateFailedToFindConverter
                    .StringFormat(
                        typeof(TEventEntity).GetNameWithoutGenericArity()));

        EntityFilter<TEventEntity> filter = new()
        {
            Criteria = applyFilter.Filter(eventFilter),
            Paging = eventFilter.Pagination,
            OrderBy = x => x.OrderBy(o => o.Version)
        };

        await foreach (TEventEntity entity in repository
           .FetchAsync(filter, cancellationToken))
        {
            yield return converter
                .ConvertFrom(entity, options.Value.SerializerOptions)
                .AsRequired<IEventDomain<TAggregateId>>();
        }
    }

    ///<inheritdoc/>
    protected override async ValueTask DisposeAsync(bool disposing)
    {
        if (!disposing)
            return;

        foreach (IDisposable disposable in _disposables)
            disposable?.Dispose();

        await base.DisposeAsync(disposing)
            .ConfigureAwait(false);
    }
}
