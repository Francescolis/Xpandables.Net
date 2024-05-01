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
/// Abstract class that represents the event store.
/// </summary>
/// <typeparam name="TEventEntity">The type of the event entity.</typeparam>
/// <param name="unitOfWork">The unit of work to use.</param>
/// <param name="options">The event configuration options to use.</param>
public abstract class EventStore<TEventEntity>(
    IUnitOfWork unitOfWork,
    IOptions<EventOptions> options) :
    Disposable
    where TEventEntity : class, IEventEntity
{
    private IDisposable[] _disposables = [];

    ///<inheritdoc/>
    protected IRepositoryRead<TEventEntity> RepositoryRead =>
        unitOfWork.GetRepositoryRead<TEventEntity>();

    ///<inheritdoc/>
    protected IRepositoryWrite<TEventEntity> RepositoryWrite =>
        unitOfWork.GetRepositoryWrite<TEventEntity>();

    ///<inheritdoc/>
    protected IUnitOfWork UnitOfWork => unitOfWork;

    ///<inheritdoc/>
    protected EventOptions Options => options.Value;

    ///<inheritdoc/>
    protected async ValueTask AppendEventAsync<TEvent>(
        TEvent @event,
        CancellationToken cancellationToken = default)
        where TEvent : class, IEvent
    {
        ArgumentNullException.ThrowIfNull(@event);

        EventConverter<TEventEntity> converter = Options
          .Converters
          .FirstOrDefault(x => x.CanConvert(typeof(TEventEntity)))
          .As<EventConverter<TEventEntity>>()
          ?? throw new InvalidOperationException(
              I18nXpandables.AggregateFailedToFindConverter
                  .StringFormat(
                      @event.GetType().GetNameWithoutGenericArity()));

        TEventEntity entity = converter
        .ConvertTo(@event, Options.SerializerOptions);

        Array.Resize(ref _disposables, _disposables.Length + 1);
        _disposables[^1] = entity;

        await RepositoryWrite
            .InsertAsync(entity, cancellationToken)
            .ConfigureAwait(false);
    }

    ///<inheritdoc/>
    protected async IAsyncEnumerable<TEvent> ReadEventAsync<TEvent>(
        IEventFilter eventFilter,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
        where TEvent : class, IEvent
    {
        ArgumentNullException.ThrowIfNull(eventFilter);

        EventEntityFilter<TEventEntity> filter = Options
                .Filters
                .FirstOrDefault(x => x.CanFilter(typeof(TEventEntity)))
                .As<EventEntityFilter<TEventEntity>>()
                ?? throw new InvalidOperationException(
                    I18nXpandables.AggregateFailedToFindFilter
                        .StringFormat(
                            typeof(TEventEntity).GetNameWithoutGenericArity()));

        EventConverter<TEventEntity> converter = Options
            .Converters
            .FirstOrDefault(x => x.CanConvert(typeof(TEventEntity)))
            .As<EventConverter<TEventEntity>>()
            ?? throw new InvalidOperationException(
                I18nXpandables.AggregateFailedToFindConverter
                    .StringFormat(
                        typeof(TEventEntity).GetNameWithoutGenericArity()));

        EntityFilter<TEventEntity> entityFilter = new()
        {
            Criteria = filter.Filter(eventFilter),
            Paging = eventFilter.Pagination,
            OrderBy = x => x.OrderBy(o => o.Version)
        };

        await foreach (TEventEntity entity in RepositoryRead
           .FetchAsync(entityFilter, cancellationToken))
        {
            yield return converter
                .ConvertFrom(entity, Options.SerializerOptions)
                .AsRequired<TEvent>();
        }
    }

    ///<inheritdoc/>
    protected sealed override async ValueTask DisposeAsync(bool disposing)
    {
        if (!disposing)
            return;

        foreach (IDisposable disposable in _disposables)
            disposable?.Dispose();

        await base.DisposeAsync(disposing)
            .ConfigureAwait(false);
    }
}
