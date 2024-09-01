
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

    protected override Task DoAppendAsync(
        IEntityEvent entity,
        CancellationToken cancellationToken = default)
    {
        _events.Add(entity);
        return Task.CompletedTask;
    }
    protected override IAsyncEnumerable<IEntityEvent> DoFetchAsync(
        IEventFilter eventFilter,
        CancellationToken cancellationToken = default)
        => eventFilter
            .ApplyQueryable(_store.AsQueryable());
    protected override Task DoMarkEventsAsPublishedAsync(
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
    protected override Task DoPersistAsync(
        CancellationToken cancellationToken = default)
    {
        _events.ForEach(e => _store.Add(e));
        _events.Clear();
        return Task.CompletedTask;
    }
}