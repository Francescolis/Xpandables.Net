
/************************************************************************************************************
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
using System.Runtime.CompilerServices;
using System.Text.Json;

using Microsoft.EntityFrameworkCore;

using Xpandables.Net.Aggregates.Defaults;
using Xpandables.Net.Aggregates.DomainEvents;
using Xpandables.Net.Extensions;

namespace Xpandables.Net.Aggregates;

/// <summary>
/// <see cref="IDomainEventStore{TDomainEventRecord}"/> implementation.
/// </summary>
/// <remarks>
/// Initializes the event store.
/// </remarks>
/// <param name="dataContext"></param>
/// <param name="serializerOptions"></param>
/// <exception cref="ArgumentNullException"></exception>
public sealed class DomainEventStore(DomainDataContext dataContext, JsonSerializerOptions serializerOptions)
    : Disposable, IDomainEventStore<DomainEventRecord>
{
    private IDisposable[] _disposables = [];

    ///<inheritdoc/>
    public async ValueTask AppendAsync<TAggregateId>(
        IDomainEvent<TAggregateId> @event,
        CancellationToken cancellationToken = default)
        where TAggregateId : struct, IAggregateId<TAggregateId>
    {
        ArgumentNullException.ThrowIfNull(@event);

        DomainEventRecord disposable = DomainEventRecord.FromDomainEvent(@event, serializerOptions);
        Array.Resize(ref _disposables, _disposables.Length + 1);
        _disposables[^1] = disposable;

        await dataContext.Events.AddAsync(disposable, cancellationToken).ConfigureAwait(false);
    }

    ///<inheritdoc/>
    public async IAsyncEnumerable<IDomainEvent<TAggregateId>> ReadAsync<TAggregateId>(
        TAggregateId aggregateId,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
        where TAggregateId : struct, IAggregateId<TAggregateId>
    {
        ArgumentNullException.ThrowIfNull(aggregateId);

        string aggregateIdName = typeof(TAggregateId).GetNameWithoutGenericArity();

        await foreach (DomainEventRecord entity in
            dataContext.Events
            .Where(e => e.AggregateId == aggregateId.Value && e.AggregateIdName == aggregateIdName)
            .OrderBy(e => e.Version)
            .AsAsyncEnumerable())
        {
            if (DomainEventRecord.ToDomainEventRecord<TAggregateId>(entity, serializerOptions) is IDomainEvent<TAggregateId> { } @event)
                yield return @event;
        }
    }

    ///<inheritdoc/>
    public IAsyncEnumerable<TResult> ReadAsync<TResult>(
       IDomainEventFilter<DomainEventRecord, TResult> filter,
       CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        return filter.GetQueryableFiltered(dataContext.Events).AsAsyncEnumerable();
    }

    ///<inheritdoc/>
    protected override async ValueTask DisposeAsync(bool disposing)
    {
        if (!disposing)
            return;

        foreach (IDisposable disposable in _disposables)
            disposable?.Dispose();

        await base.DisposeAsync(disposing).ConfigureAwait(false);
    }
}
