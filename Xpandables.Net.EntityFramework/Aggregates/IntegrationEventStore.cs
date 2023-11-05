
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
using System.Text.Json;

using Microsoft.EntityFrameworkCore;

using Xpandables.Net.Aggregates.Defaults;
using Xpandables.Net.Aggregates.IntegrationEvents;
using Xpandables.Net.Primitives;

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
#pragma warning disable CA2213 // Disposable fields should be disposed
    private readonly DomainDataContext _dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
#pragma warning restore CA2213 // Disposable fields should be disposed
    private readonly JsonSerializerOptions _serializerOptions = serializerOptions ?? throw new ArgumentNullException(nameof(serializerOptions));

    ///<inheritdoc/>
    public async ValueTask AppendAsync(IIntegrationEvent @event, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(@event);

        IntegrationEventRecord disposable = IntegrationEventRecord.FromIntegrationEvent(@event, _serializerOptions);
        Array.Resize(ref _disposables, _disposables.Length + 1);
        _disposables[^1] = disposable;

        await _dataContext.Notifications.AddAsync(disposable, cancellationToken).ConfigureAwait(false);
    }

    ///<inheritdoc/>
    public async ValueTask DeleteAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eventId);

        await _dataContext.Notifications
            .Where(e => e.Id == eventId)
            .ExecuteDeleteAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    ///<inheritdoc/>
    public IAsyncEnumerable<IIntegrationEvent> ReadAsync(
        Pagination pagination,
        CancellationToken cancellationToken = default)
    {
        return _dataContext.Notifications
        .AsNoTracking()
        .Skip(pagination.Index * pagination.Size)
        .Take(pagination.Size)
        .Select(e => IntegrationEventRecord.ToIntegrationEvent(e, _serializerOptions))
        .Where(w => w.IsNotEmpty)
        .Select(s => s.Value)
        .AsAsyncEnumerable();
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
