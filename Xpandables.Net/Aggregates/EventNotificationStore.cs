
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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Xpandables.Net.Optionals;
using Xpandables.Net.Repositories;

namespace Xpandables.Net.Aggregates;

/// <summary>
/// Represents a notification store.
/// </summary>
/// <typeparam name="TEventEntity">The type of the event entity.</typeparam>
/// <param name="unitOfWork">The unit of work to use.</param>
/// <param name="options">The event configuration options to use.</param>
public sealed class EventNotificationStore<TEventEntity>(
    [FromKeyedServices(EventOptions.UnitOfWorkKey)] IUnitOfWork unitOfWork,
    IOptions<EventOptions> options) :
    EventStore<TEventEntity>(unitOfWork, options), IEventNotificationStore
    where TEventEntity : class, IEventEntityNotification
{
    ///<inheritdoc/>
    public async ValueTask AppendAsync(
        IEventNotification @event,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(@event);

        await AppendEventAsync(@event, cancellationToken)
            .ConfigureAwait(false);
    }

    ///<inheritdoc/>
    public async ValueTask AppendCloseAsync(
        Guid eventId,
        Exception? exception = default,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eventId);

        EntityFilter<TEventEntity> filter = new()
        {
            Criteria = x => x.Id == eventId
        };

        Optional<TEventEntity> entityOptional = await RepositoryRead
            .TryFindAsync(filter, cancellationToken)
            .ConfigureAwait(false);

        if (entityOptional.IsEmpty)
            return;

        TEventEntity entity = entityOptional.Value;

        entity.ErrorMessage = exception != null ? $"{exception}" : default;
        entity.SetStatus(exception != null
            ? EntityStatus.INACTIVE
            : EntityStatus.DELETED);

        await RepositoryWrite
            .UpdateAsync(entity, cancellationToken)
            .ConfigureAwait(false);

        await UnitOfWork.PersistAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    ///<inheritdoc/>
    public IAsyncEnumerable<IEventNotification> ReadAsync(
        IEventFilter eventFilter,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eventFilter);

        return ReadEventAsync<IEventNotification>(
            eventFilter,
            cancellationToken);
    }
}
