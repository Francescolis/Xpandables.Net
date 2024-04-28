
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

using Xpandables.Net.Operations;
using Xpandables.Net.Optionals;
using Xpandables.Net.Primitives.I18n;
using Xpandables.Net.Primitives.Text;
using Xpandables.Net.Repositories;

namespace Xpandables.Net.Aggregates;

/// <summary>
/// Represents a snapshot store that stores the snapshot of the aggregate.
/// </summary>
/// <typeparam name="TEventEntity">The type of the event entity.</typeparam>
/// <param name="repository">The repository to use.</param>
/// <param name="options">The event configuration options to use.</param>
public sealed class SnapshotStore<TEventEntity>(
    IRepository<TEventEntity> repository,
    IOptions<EventOptions> options) :
    Disposable, ISnapshotStore
    where TEventEntity : class, IEventEntitySnapshot
{
    private IDisposable[] _disposables = [];

    ///<inheritdoc/>
    public async ValueTask<IOperationResult> AppendAsync(
        IEventSnapshot @event,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(@event);

        try
        {
            EventConverter<TEventEntity> converter = options.Value
              .Converters
              .FirstOrDefault(x => x.CanConvert(@event.GetType()))
              .As<EventConverter<TEventEntity>>()
              ?? throw new InvalidOperationException(
                  I18nXpandables.AggregateFailedToFindConverter
                      .StringFormat(
                          @event.GetType().GetNameWithoutGenericArity()));

            TEventEntity entity = converter
                .ConvertTo(@event, options.Value.SerializerOptions);

            Array.Resize(ref _disposables, _disposables.Length + 1);
            _disposables[^1] = entity;

            await repository
                .InsertAsync(entity, cancellationToken)
                .ConfigureAwait(false);

            return OperationResults
                .Ok()
                .Build();
        }
        catch (Exception exception)
            when (exception is not ArgumentNullException)
        {
            return exception.ToOperationResult();
        }
    }

    ///<inheritdoc/>
    public async ValueTask<IOperationResult<IEventSnapshot>> ReadAsync(
        Guid objectId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(objectId);

        EntityFilter<TEventEntity> filter = new()
        {
            Criteria = x => x.ObjectId == objectId
        };

        Optional<TEventEntity> entityOptional = await repository
            .TryFindAsync(filter, cancellationToken)
            .ConfigureAwait(false);

        if (entityOptional.IsEmpty)
            return OperationResults
                .NotFound<IEventSnapshot>()
                .WithError(
                    nameof(Snapshot),
                    I18nXpandables.AggregateFailedToFindSnapshot
                        .StringFormat(objectId))
                .Build();

        TEventEntity entity = entityOptional.Value;

        EventConverter<TEventEntity> converter = options.Value
            .Converters
            .FirstOrDefault(x => x.CanConvert(typeof(TEventEntity)))
            .As<EventConverter<TEventEntity>>()
            ?? throw new InvalidOperationException(
                I18nXpandables.AggregateFailedToFindConverter
                    .StringFormat(typeof(TEventEntity)
                        .GetNameWithoutGenericArity()));

        IEventSnapshot @event = (IEventSnapshot)converter
            .ConvertFrom(entity, options.Value.SerializerOptions);

        return OperationResults
            .Ok(@event)
            .Build();
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