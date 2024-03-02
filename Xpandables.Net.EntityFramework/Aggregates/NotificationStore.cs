
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
using Microsoft.EntityFrameworkCore;

using System.Linq.Expressions;
using System.Text.Json;

using Xpandables.Net.Aggregates.Notifications;
using Xpandables.Net.Expressions;
using Xpandables.Net.Repositories;

namespace Xpandables.Net.Aggregates;

/// <summary>
/// <see cref="INotificationStore"/> implementation.
/// </summary>
/// <remarks>
/// Initializes the notification store.
/// </remarks>
/// <param name="dataContext"></param>
/// <param name="serializerOptions"></param>
/// <exception cref="ArgumentNullException"></exception>
public sealed class NotificationStore(
    DomainDataContext dataContext,
    JsonSerializerOptions serializerOptions) : Disposable, INotificationStore
{
    private IDisposable[] _disposables = [];

    ///<inheritdoc/>
    public async ValueTask AppendAsync(
        INotification @event,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(@event);

        EntityNotification disposable = EntityNotification
            .ToEntityNotification(@event, serializerOptions);

        Array.Resize(ref _disposables, _disposables.Length + 1);
        _disposables[^1] = disposable;

        _ = await dataContext.Notifications
            .AddAsync(disposable, cancellationToken)
            .ConfigureAwait(false);
    }

    ///<inheritdoc/>
    public async ValueTask AppendCloseAsync(
        Guid eventId,
        Exception? exception = default,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eventId);

        _ = await dataContext.Notifications
            .Where(e => e.Id == eventId)
            .ExecuteUpdateAsync(e => e
                .SetProperty(p => p.ErrorMessage, p => exception != null ? $"{exception}" : default)
                .SetProperty(p => p.UpdatedOn, p => DateTime.UtcNow)
                .SetProperty(p => p.Status, p => exception != null ? EntityStatus.INACTIVE : EntityStatus.DELETED),
                cancellationToken)
            .ConfigureAwait(false);
    }

    ///<inheritdoc/>
    public IAsyncEnumerable<INotification> ReadAsync(
        INotificationFilter filter,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        return BuildQueryExpression(filter, dataContext.Notifications.AsNoTracking())
             .Select(s => EntityNotification.ToNotification(s, serializerOptions))
             .OfType<INotification>()
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

    private static IQueryable<EntityNotification> BuildQueryExpression(
          INotificationFilter filter,
          IQueryable<EntityNotification> eventRecords)
    {
        QueryExpression<EntityNotification, bool> expression = QueryExpressionFactory.Create<EntityNotification>();

        if (filter.Id is not null)
            expression = expression.And(x => x.Id == filter.Id);

        if (filter.EventTypeName is not null)
            expression = expression.And(x =>
            EF.Functions.Like(x.TypeFullName, $"%{filter.EventTypeName}%"));

        if (filter.FromCreatedOn is not null)
            expression = expression.And(x =>
            x.CreatedOn >= filter.FromCreatedOn.Value);

        if (filter.ToCreatedOn is not null)
            expression = expression.And(x =>
            x.CreatedOn <= filter.ToCreatedOn.Value);

        if (filter.Status is not null)
            expression = expression.And(x =>
            EF.Functions.Like(x.Status, $"%{filter.Status}%"));

        if (filter.OnError is not null)
            expression = expression.And(x =>
                x.ErrorMessage != null == filter.OnError.Value);

        if (filter.DataCriteria is not null)
        {
            _ = Expression.Invoke(
                filter.DataCriteria,
                Expression.PropertyOrField(
                    EventFilterEntityVisitor.EventEntityParameter,
                    nameof(EntityNotification.Data)));

            Expression<Func<EntityNotification, bool>> dataCriteria = Expression.Lambda<Func<EntityNotification, bool>>(
                EventFilterEntityVisitor.EventEntityVisitor.Visit(filter.DataCriteria.Body),
                EventFilterEntityVisitor.EventEntityVisitor.Parameter);

            expression = expression.And(dataCriteria);
        }

        eventRecords = eventRecords
            .Where(expression)
            .OrderBy(o => o.CreatedOn);

        if (filter.Pagination is not null)
            eventRecords = eventRecords
                .Skip(filter.Pagination.Value.Index * filter.Pagination.Value.Size)
                .Take(filter.Pagination.Value.Size);

        return eventRecords;
    }
}
