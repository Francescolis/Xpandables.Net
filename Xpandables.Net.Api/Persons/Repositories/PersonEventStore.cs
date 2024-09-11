
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
using Microsoft.Extensions.Options;

using Xpandables.Net.Events;
using Xpandables.Net.Primitives.Collections;
using Xpandables.Net.Repositories;

namespace Xpandables.Net.Api.Persons.Repositories;

public sealed class PersonEventStore(
    IOptions<EventOptions> options) : EventStore(options)
{
    private static readonly HashSet<IEntityEvent> _store = [];
    private static readonly HashSet<IEntityEvent> _events = [];

    public override async Task AppendEventAsync(
        IEvent @event,
        CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        IEntityEvent entity = CreateEntityEvent(@event);
        _events.Add(entity);
    }

    public override IAsyncEnumerable<IEvent> FetchEventsAsync(
        IEventFilter eventFilter,
        CancellationToken cancellationToken = default)
    {
        IAsyncEnumerable<IEntityEvent> entities =
            eventFilter.FetchAsync(_store.AsQueryable());

        return CreateEventsAsync(eventFilter, entities, cancellationToken);
    }

    public override Task MarkEventAsPublishedAsync(
        Guid eventId,
        Exception? exception = null,
        CancellationToken cancellationToken = default)
    {
        IEntityEventIntegration? entity = _store
            .OfType<IEntityEventIntegration>()
            .FirstOrDefault(x => x.Id == eventId);

        if (entity is not null)
        {
            entity.ErrorMessage = exception?.ToString();
            entity.SetStatus(EntityStatus.DELETED);
        }

        return Task.CompletedTask;
    }
    public override async Task<int> PersistEventsAsync(
        CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        int count = _events.Count;
        _events.ForEach(e => _store.Add(e));
        _events.Clear();

        return count;
    }
}