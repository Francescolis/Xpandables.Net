
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

using System.Text.Json;

using Xpandables.Net.IntegrationEvents;
using Xpandables.Net.Primitives;
using Xpandables.Net.Repositories;

namespace Xpandables.Net.Aggregates;

/// <summary>
/// <see cref="IIntegrationEventStore"/> implementation.
/// </summary>
/// <remarks>
/// Initializes the event store.
/// </remarks>
/// <param name="dataContext"></param>
/// <param name="serializerOptions"></param>
/// <exception cref="ArgumentNullException"></exception>
public sealed class IntegrationEventStore(
    DomainDataContext dataContext,
    JsonSerializerOptions serializerOptions) : Disposable, IIntegrationEventStore
{
    private IDisposable[] _disposables = [];

    ///<inheritdoc/>
    public async ValueTask AppendAsync(
        IIntegrationEvent @event,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(@event);

        IntegrationEventRecord disposable = IntegrationEventRecord
            .FromIntegrationEvent(@event, serializerOptions);

        Array.Resize(ref _disposables, _disposables.Length + 1);
        _disposables[^1] = disposable;

        await dataContext.Integrations.AddAsync(disposable, cancellationToken)
            .ConfigureAwait(false);
    }

    ///<inheritdoc/>
    public async ValueTask SetErrorAsync(
        Guid eventId,
        Exception exception,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eventId);

        await dataContext.Integrations
            .Where(e => e.Id == eventId)
            .ExecuteUpdateAsync(e => e
                .SetProperty(p => p.ErrorMessage, p => $"{exception}")
                .SetProperty(p => p.UpdatedOn, p => DateTime.UtcNow)
                .SetProperty(p => p.Status, p => EntityStatus.INACTIVE),
                cancellationToken)
            .ConfigureAwait(false);
    }

    ///<inheritdoc/>
    public async ValueTask MarkAsProcessedAsync(
        Guid eventId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eventId);

        await dataContext.Integrations
            .Where(e => e.Id == eventId)
            .ExecuteUpdateAsync(e => e
                .SetProperty(p => p.ErrorMessage, p => null)
                .SetProperty(p => p.UpdatedOn, p => DateTime.UtcNow)
                .SetProperty(p => p.DeletedOn, p => DateTime.UtcNow)
                .SetProperty(p => p.Status, p => EntityStatus.DELETED),
                cancellationToken)
            .ConfigureAwait(false);
    }

    ///<inheritdoc/>
    public IAsyncEnumerable<IIntegrationEvent> ReadAsync(
        Pagination pagination,
        CancellationToken cancellationToken = default)
    {
        return dataContext.Integrations
            .AsNoTracking()
            .Where(e => e.ErrorMessage == null && e.DeletedOn == null && e.Status == EntityStatus.ACTIVE)
            .Skip(pagination.Index * pagination.Size)
            .Take(pagination.Size)
            .OrderBy(o => o.Id)
            .Select(e => IntegrationEventRecord.ToIntegrationEvent(e, serializerOptions))
            .OfType<IIntegrationEvent>()
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
