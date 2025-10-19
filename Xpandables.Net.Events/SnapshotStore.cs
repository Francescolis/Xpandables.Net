
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

using Microsoft.Extensions.Options;

using Xpandables.Net.States;

namespace Xpandables.Net.Events;

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
    IEventStore eventStore) : IAggregateStore<TAggregate>
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
        var envelope = await eventStore
            .GetLatestSnapshotAsync(streamId, cancellationToken)
            .ConfigureAwait(false);

        if (!envelope.IsEmpty)
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
        var aggregate = TAggregate.Create();
        aggregate.Restore(snapshot.Memento);

        // Replay events after snapshot’s version (assuming Restore set StreamVersion to snapshot version)
        ReadStreamRequest request = new()
        {
            FromVersion = aggregate.StreamVersion,
            MaxCount = 0,
            StreamId = streamId
        };

        await foreach (var env in eventStore
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
