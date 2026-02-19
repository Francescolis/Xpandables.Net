/*******************************************************************************
 * Copyright (C) 2025 Kamersoft
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
using System.ComponentModel.DataAnnotations;
using System.Events.Domain;
using System.Runtime.CompilerServices;
using System.States;

using Microsoft.Extensions.Options;

namespace System.Events.Aggregates;

/// <summary>
/// Provides aggregate persistence with automatic snapshotting support, improving load and save performance for
/// aggregates by storing and restoring state from snapshots when enabled.
/// </summary>
/// <remarks>When snapshotting is enabled via <see cref="SnapshotOptions"/>, aggregates are periodically saved and
/// restored from snapshots to reduce the number of events that must be replayed. This can improve performance for
/// aggregates with long event histories. If no snapshot is available, the aggregate is loaded from the event stream as
/// usual. Thread safety and consistency are determined by the underlying stores.</remarks>
/// <typeparam name="TAggregate">The aggregate type to be persisted. Must implement <see cref="IAggregate"/>, <see
/// cref="IAggregateFactory{TAggregate}"/>, and <see cref="IOriginator"/>.</typeparam>
/// <param name="options">The snapshot configuration options. Cannot be null.</param>
/// <param name="aggregateStore">The underlying aggregate store used for persistence when snapshots are not enabled or available. Cannot be null.</param>
/// <param name="eventStore">The event store used to store and retrieve snapshot events. Cannot be null.</param>
public sealed class SnapshotStore<TAggregate>(
    IOptions<SnapshotOptions> options,
    IAggregateStore<TAggregate> aggregateStore,
    IDomainStore eventStore) : IAggregateStore<TAggregate>
    where TAggregate : class, IAggregate, IAggregateFactory<TAggregate>, IOriginator
{
    private readonly SnapshotOptions _options = options.Value
        ?? throw new ArgumentNullException(nameof(options));

    /// <inheritdoc/>
    public async Task<TAggregate> LoadAsync(Guid streamId, CancellationToken cancellationToken = default)
    {
        if (!_options.IsSnapshotEnabled)
        {
            return await aggregateStore
                .LoadAsync(streamId, cancellationToken)
                .ConfigureAwait(false);
        }

        return await ResolveFromSnapshotAsync(streamId, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task SaveAsync(TAggregate aggregate, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(aggregate);

        if (ShouldCreateSnapshot(aggregate))
        {
            await CreateSnapshotAsync(aggregate, cancellationToken).ConfigureAwait(false);
        }

        await aggregateStore
            .SaveAsync(aggregate, cancellationToken)
            .ConfigureAwait(false);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool ShouldCreateSnapshot(IAggregate aggregate) =>
        _options.IsSnapshotEnabled
        && aggregate.StreamVersion > 0
        && aggregate.StreamVersion % _options.SnapshotFrequency == 0;

    private async Task CreateSnapshotAsync(TAggregate aggregate, CancellationToken cancellationToken)
    {
        IMemento memento = aggregate.Save();

        var snapshotEvent = new SnapshotEvent
        {
            EventId = Guid.CreateVersion7(),
            Memento = memento,
            OwnerId = aggregate.StreamId
        };

        await eventStore
            .AppendSnapshotAsync(snapshotEvent, cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task<TAggregate> ResolveFromSnapshotAsync(
        Guid streamId,
        CancellationToken cancellationToken)
    {
		// Get latest snapshot once (store-agnostic)
		EnvelopeResult? envelope = await eventStore
            .GetLatestSnapshotAsync(streamId, cancellationToken)
            .ConfigureAwait(false);

        if (!envelope.HasValue)
        {
            // No snapshot: fallback
            return await aggregateStore
                .LoadAsync(streamId, cancellationToken)
                .ConfigureAwait(false);
        }

        if (envelope.Value.Event is not ISnapshotEvent snapshot)
        {
            throw new InvalidOperationException(
                $"Latest snapshot for aggregate '{streamId}' is not an ISnapshotEvent.");
        }

        // Rehydrate from snapshot
        TAggregate aggregate = TAggregate.Initialize();
        aggregate.Restore(snapshot.Memento);

        // Replay events after snapshot’s version (assuming Restore set StreamVersion to snapshot version)
        ReadStreamRequest request = new()
        {
            FromVersion = aggregate.StreamVersion,
            MaxCount = 0,
            StreamId = streamId
        };

        await foreach (EnvelopeResult env in eventStore
            .ReadStreamAsync(request, cancellationToken: cancellationToken)
            .ConfigureAwait(false))
        {
            if (env.Event is IDomainEvent domainEvent)
            {
                aggregate.LoadFromHistory(domainEvent);
            }
        }

        if (aggregate.IsEmpty)
        {
            throw new ValidationException(
                new ValidationResult(
                    $"The aggregate with ID '{streamId}' was not found.",
                    [nameof(streamId)]),
                null,
                streamId);
        }

        return aggregate;
    }
}
