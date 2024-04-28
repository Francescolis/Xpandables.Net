
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
using Xpandables.Net.Primitives.I18n;
using Xpandables.Net.Primitives.Text;
using Xpandables.Net.Repositories;

namespace Xpandables.Net.Aggregates;

/// <summary>
/// Represents a snapshot store that stores the snapshot of the aggregate.
/// </summary>
/// <typeparam name="TEventEntity">The type of the event entity.</typeparam>
/// <param name="unitOfWork">The unit of work to use.</param>
/// <param name="options">The event configuration options to use.</param>
public sealed class SnapshotStore<TEventEntity>(
    [FromKeyedServices(EventOptions.UnitOfWorkKey)] IUnitOfWork unitOfWork,
    IOptions<EventOptions> options) :
    EventStore<TEventEntity>(unitOfWork, options), ISnapshotStore
    where TEventEntity : class, IEventEntitySnapshot
{
    ///<inheritdoc/>
    public async ValueTask AppendAsync(
        IEventSnapshot @event,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(@event);

        await AppendEventAsync(@event, cancellationToken)
            .ConfigureAwait(false);
    }

    ///<inheritdoc/>
    public async ValueTask<IEventSnapshot?> ReadAsync(
        Guid objectId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(objectId);

        EntityFilter<TEventEntity> filter = new()
        {
            Criteria = x => x.ObjectId == objectId,
            OrderBy = x => x.OrderByDescending(o => o.Version),
        };

        Optional<TEventEntity> entityOptional = await Repository
            .TryFindAsync(filter, cancellationToken)
            .ConfigureAwait(false);

        if (entityOptional.IsEmpty)
            return default;

        TEventEntity entity = entityOptional.Value;

        EventConverter<TEventEntity> converter = Options
            .Converters
            .FirstOrDefault(x => x.CanConvert(typeof(TEventEntity)))
            .As<EventConverter<TEventEntity>>()
            ?? throw new InvalidOperationException(
                I18nXpandables.AggregateFailedToFindConverter
                    .StringFormat(typeof(TEventEntity)
                        .GetNameWithoutGenericArity()));

        IEventSnapshot @event = (IEventSnapshot)converter
            .ConvertFrom(entity, Options.SerializerOptions);

        return @event;
    }
}