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
using Microsoft.EntityFrameworkCore;

using Xpandables.Net.Aggregates;
using Xpandables.Net.Aggregates.Events;
using Xpandables.Net.Distribution;

namespace Xpandables.Net.Repositories;

/// <summary>
/// Represents the repository event.
/// </summary>
public sealed class RepositoryEvent(
    DataContextEvent context) : IEventRepository
{
    ///<inheritdoc/>
    public IAsyncEnumerable<IEntityEvent> FetchAsync(
        IEventFilter eventFilter,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eventFilter);

        return eventFilter.Type switch
        {
            Type type when type == typeof(IEventDomain)
                => DoFetchAsync(eventFilter, context.Domains.AsNoTracking()),
            Type type when type == typeof(IEventIntegration)
                => DoFetchAsync(eventFilter, context.Integrations.AsNoTracking()),
            Type type when type == typeof(IEventSnapshot)
                => DoFetchAsync(eventFilter, context.Snapshots.AsNoTracking()),
            _ => throw new InvalidOperationException(
                $"The type {eventFilter.Type} is not supported.")
        };

        static IAsyncEnumerable<TEntityEvent> DoFetchAsync<TEntityEvent>(
            IEventFilter filter,
            IQueryable<TEntityEvent> queryable)
        {
            IQueryable<TEntityEvent> queryableResult =
                filter
                    .Apply(queryable)
                    .OfType<TEntityEvent>();

            return queryableResult
                .AsAsyncEnumerable();
        }
    }

    ///<inheritdoc/>
    public async Task InsertAsync(
        IEntityEvent entity,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        _ = await context
            .AddAsync(entity, cancellationToken)
            .ConfigureAwait(false);
    }

    ///<inheritdoc/>
    public async Task MarkEventsAsPublishedAsync(
        Guid eventId,
        Exception? exception = null,
        CancellationToken cancellationToken = default) =>
        _ = await context
                .Integrations
                .Where(x => x.Id == eventId)
                .ExecuteUpdateAsync(setters =>
                    setters
                        .SetProperty(p => p.Status, EntityStatus.DELETED)
                        .SetProperty(p => p.UpdatedOn, DateTime.UtcNow)
                        .SetProperty(
                            p => p.ErrorMessage,
                            exception != null
                                ? exception.ToString()
                                : null),
                                cancellationToken)
                .ConfigureAwait(false);

    ///<inheritdoc/>
    public async Task PersistAsync(CancellationToken cancellationToken = default)
        => await context
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);
}
