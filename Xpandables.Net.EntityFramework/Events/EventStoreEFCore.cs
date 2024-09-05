
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
using Microsoft.Extensions.Options;

using Xpandables.Net.Aggregates;
using Xpandables.Net.Repositories;

namespace Xpandables.Net.Events;

/// <summary>
/// Represents the <see cref="IEventStore"/> using EFCore.
/// </summary>
/// <param name="context">The target event data context.</param>
/// <param name="options">The event options.</param>
public sealed class EventStoreEFCore(
    IOptions<EventOptions> options,
    DataContextEvent context) : EventStore(options)
{
    /// <inheritdoc/>
    protected override async Task AppendCoreAsync(
        IEntityEvent entity,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        _ = await context
            .AddAsync(entity, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override IQueryable GetQueryableCore(IEventFilter eventFilter)
        => eventFilter.Type switch
        {
            Type type when type == typeof(IEventDomain)
                => context.Domains.AsNoTracking(),
            Type type when type == typeof(IEventIntegration)
                => context.Integrations.AsNoTracking(),
            Type type when type == typeof(IEventSnapshot)
                => context.Snapshots.AsNoTracking(),
            _ => throw new InvalidOperationException(
                $"The type {eventFilter.Type} is not supported.")
        };

    /// <inheritdoc/>
    public override async Task MarkEventAsPublishedAsync(
        Guid eventId,
        Exception? exception = null,
        CancellationToken cancellationToken = default)
        => _ = await context
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

    /// <inheritdoc/>
    public override async Task PersistAsync(
        CancellationToken cancellationToken = default)
        => await context
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);
}
