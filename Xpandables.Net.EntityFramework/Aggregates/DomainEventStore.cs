﻿
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
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.RegularExpressions;

using Microsoft.EntityFrameworkCore;

using Xpandables.Net.Aggregates.Defaults;
using Xpandables.Net.Aggregates.DomainEvents;
using Xpandables.Net.Extensions;
using Xpandables.Net.Operations.Expressions;
using Xpandables.Net.Repositories;

namespace Xpandables.Net.Aggregates;

/// <summary>
/// <see cref="IDomainEventStore"/> implementation.
/// </summary>
/// <remarks>
/// Initializes the event store.
/// </remarks>
/// <param name="dataContext"></param>
/// <param name="serializerOptions"></param>
/// <exception cref="ArgumentNullException"></exception>
public sealed class DomainEventStore(DomainDataContext dataContext, JsonSerializerOptions serializerOptions)
    : Disposable, IDomainEventStore
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
    public IAsyncEnumerable<IDomainEvent<TAggregateId>> ReadAsync<TAggregateId>(
        TAggregateId aggregateId,
         CancellationToken cancellationToken = default)
        where TAggregateId : struct, IAggregateId<TAggregateId>
    {
        ArgumentNullException.ThrowIfNull(aggregateId);

        string aggregateIdName = typeof(TAggregateId).GetNameWithoutGenericArity();

        return dataContext.Events
            .AsNoTracking()
            .Where(e => e.AggregateId == aggregateId.Value && e.AggregateIdName == aggregateIdName)
            .OrderBy(e => e.Version)
            .Select(s => DomainEventRecord.ToDomainEventRecord<TAggregateId>(s, serializerOptions))
            .OfType<IDomainEvent<TAggregateId>>()
            .AsAsyncEnumerable();
    }

    ///<inheritdoc/>
    public IAsyncEnumerable<IDomainEvent<TAggregateId>> ReadAsync<TAggregateId>(
       DomainEventFilterCriteria filter,
#pragma warning disable CA1725 // Parameter names should match base declaration
       CancellationToken cancellation = default)
#pragma warning restore CA1725 // Parameter names should match base declaration
        where TAggregateId : struct, IAggregateId<TAggregateId>
    {
        ArgumentNullException.ThrowIfNull(filter);

        var expression = QueryExpressionFactory.Create<DomainEventRecord>();

        if (filter.AggregateId is not null)
            expression = expression.And(x => x.AggregateId == filter.AggregateId.Value);

        if (filter.AggregateIdName is not null)
            expression = expression.And(x => Regex.IsMatch(filter.AggregateIdName, x.AggregateIdName));

        if (filter.Id is not null)
            expression = expression.And(x => x.Id == filter.Id);

        if (filter.EventTypeName is not null)
            expression = expression.And(x => Regex.IsMatch(filter.EventTypeName, x.TypeName));

        if (filter.Version is not null)
            expression = expression.And(x => x.Version > filter.Version.Value);

        if (filter.FromCreatedOn is not null)
            expression = expression.And(x => x.CreatedOn >= filter.FromCreatedOn.Value);

        if (filter.ToCreatedOn is not null)
            expression = expression.And(x => x.CreatedOn <= filter.ToCreatedOn.Value);

        if (filter.DataCriteria is not null)
        {
            _ = Expression.Invoke(
                filter.DataCriteria,
                Expression.PropertyOrField(
                    DomainEventFilterCriteria.EventEntityParameter,
                    nameof(DomainEventRecord.Data)));

            var dataCriteria = Expression.Lambda<Func<DomainEventRecord, bool>>(
                DomainEventFilterCriteria.EventEntityVisitor.Visit(filter.DataCriteria.Body),
                DomainEventFilterCriteria.EventEntityVisitor.Parameter);

            expression = expression.And(dataCriteria);
        }

        return dataContext.Events
             .AsNoTracking()
             .Where(expression)
             .OrderBy(e => e.Version)
             .Select(s => DomainEventRecord.ToDomainEventRecord<TAggregateId>(s, serializerOptions))
             .OfType<IDomainEvent<TAggregateId>>()
             .AsAsyncEnumerable();
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
