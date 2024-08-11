
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
using Xpandables.Net.Events;
using Xpandables.Net.Primitives.Collections;

namespace Xpandables.Net.Api.Persistence;

public sealed class RepositoryPerson : IRepositoryEvent
{
    private static readonly HashSet<IEntityEvent> _store = [];
    private static readonly HashSet<IEntityEvent> _events = [];

    public Task InsertAsync(
        IEntityEvent entity,
        CancellationToken cancellationToken = default)
    {
        _events.Add(entity);
        return Task.CompletedTask;
    }

    public IAsyncEnumerable<IEntityEvent> FetchAsync(
        IEventFilter eventFilter,
        CancellationToken cancellationToken = default)
        => eventFilter
            .ApplyQueryable(_store.AsQueryable());

    public Task MarkEventsAsPublishedAsync(
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
        }

        return Task.CompletedTask;
    }
    public Task PersistAsync(
        CancellationToken cancellationToken = default)
    {
        _events.ForEach(e => _store.Add(e));
        _events.Clear();
        return Task.CompletedTask;
    }
}