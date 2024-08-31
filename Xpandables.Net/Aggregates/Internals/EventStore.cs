
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

using Xpandables.Net.Aggregates.Events;
using Xpandables.Net.Distribution;

namespace Xpandables.Net.Aggregates.Internals;

/// <summary>
/// Represents the event store implementation.
/// </summary>
internal sealed class EventStore(
    IEventRepository repository,
    IOptions<EventOptions> options) : Disposable, IEventStore
{
    private IDisposable[] _disposables = [];

    ///<inheritdoc/>
    public async Task AppendAsync(
        IEvent @event,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(@event);

        IEventConverter converter = options
            .Value
            .GetEventConverterFor(@event);

        IEntityEvent entity = converter
            .ConvertTo(@event, options.Value.SerializerOptions);

        if (options
            .Value
            .DisposeEventEntityAfterPersistence)
        {
            Array.Resize(ref _disposables, _disposables.Length + 1);
            _disposables[^1] = entity;
        }

        await repository
            .InsertAsync(entity, cancellationToken)
            .ConfigureAwait(false);
    }

    ///<inheritdoc/>
    public async IAsyncEnumerable<IEvent> FetchAsync(
        IEventFilter eventFilter,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eventFilter);

        IEventConverter converter = options
            .Value
            .GetEventConverterFor(eventFilter.Type);

        await foreach (IEntityEvent entity in repository
           .FetchAsync(eventFilter, cancellationToken))
        {
            yield return converter
                .ConvertFrom(entity, options.Value.SerializerOptions);
        }
    }

    ///<inheritdoc/>
    public async Task MarkEventAsPublished(
        Guid eventId,
        Exception? exception = null,
        CancellationToken cancellationToken = default)
        => await repository
            .MarkEventsAsPublishedAsync(eventId, exception, cancellationToken)
            .ConfigureAwait(false);

    ///<inheritdoc/>
    protected sealed override async ValueTask DisposeAsync(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        if (options
            .Value
            .DisposeEventEntityAfterPersistence)
        {
            foreach (IDisposable disposable in _disposables)
            {
                disposable?.Dispose();
            }
        }

        await base.DisposeAsync(disposing)
            .ConfigureAwait(false);
    }
}
