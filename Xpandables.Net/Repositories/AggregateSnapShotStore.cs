/*******************************************************************************
 * Copyright (C) ...
 * Licensed under the Apache License, Version 2.0
********************************************************************************/
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

using Microsoft.Extensions.Options;

using Xpandables.Net.Events;
using Xpandables.Net.States;

namespace Xpandables.Net.Repositories;

/// <summary>
/// Decorates an aggregate store with snapshot creation and fast load from latest snapshot.
/// </summary>
/// <typeparam name="TAggregate">Aggregate type.</typeparam>
public sealed class AggregateSnapShotStore<TAggregate> : IAggregateStore<TAggregate>
    where TAggregate : class, IAggregate, IOriginator, new()
{
    private readonly IAggregateStore<TAggregate> _aggregateStore;
    private readonly IEventStore _eventStore;
    private readonly SnapShotOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateSnapShotStore{TAggregate}"/> class,  which provides
    /// functionality for managing aggregate snapshots in conjunction with an aggregate store and event store.
    /// </summary>
    /// <param name="aggregateStore">The aggregate store used to persist and retrieve aggregates. Cannot be <see langword="null"/>.</param>
    /// <param name="unitOfWork">The unit of work that provides access to the event store. Cannot be <see langword="null"/>.</param>
    /// <param name="options">The configuration options for snapshot behavior. The <see cref="SnapShotOptions"/> value cannot be <see
    /// langword="null"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="aggregateStore"/>, <paramref name="unitOfWork"/>, or <paramref name="options"/> is
    /// <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="options"/> value is <see langword="null"/>.</exception>
    public AggregateSnapShotStore(
        IAggregateStore<TAggregate> aggregateStore,
        IUnitOfWorkEvent unitOfWork,
        IOptions<SnapShotOptions> options)
    {
        _aggregateStore = aggregateStore ?? throw new ArgumentNullException(nameof(aggregateStore));
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(options);

        _eventStore = unitOfWork.GetEventStore<IEventStore>();
        _options = options.Value ?? throw new ArgumentException("Options value cannot be null.", nameof(options));

        ValidateOptions();
    }

    /// <summary>
    /// Persists the aggregate (via underlying store) and, if frequency matches, appends a snapshot.
    /// Both snapshot and events are committed by the unit of work.
    /// </summary>
    public async Task SaveAsync(
        TAggregate aggregate,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(aggregate);

        try
        {
            // Snapshot first: both snapshot and domain events are tracked and committed together.
            if (ShouldCreateSnapshot(aggregate))
            {
                await CreateSnapshotAsync(aggregate, cancellationToken).ConfigureAwait(false);
            }

            await _aggregateStore
                .SaveAsync(aggregate, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception) when (IsRethrownException(exception))
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException(
                $"Unable to save the aggregate with ID '{aggregate.KeyId}'. See inner exception for details.",
                exception);
        }
    }

    /// <summary>
    /// Loads the aggregate: if snapshots enabled and present, restore from latest snapshot, then replay events after it.
    /// Otherwise, fallback to underlying store.
    /// </summary>
    public async Task<TAggregate> LoadAsync(
        Guid aggregateId,
        CancellationToken cancellationToken = default)
    {
        if (aggregateId == Guid.Empty)
        {
            throw new ArgumentException("AggregateId cannot be empty.", nameof(aggregateId));
        }

        if (!_options.IsSnapshotEnabled)
        {
            return await _aggregateStore
                .LoadAsync(aggregateId, cancellationToken)
                .ConfigureAwait(false);
        }

        try
        {
            return await ResolveFromSnapshotAsync(aggregateId, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception) when (IsRethrownException(exception))
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException(
                $"Unable to load the aggregate with ID '{aggregateId}'. See inner exception for details.",
                exception);
        }
    }

    private async Task CreateSnapshotAsync(TAggregate aggregate, CancellationToken cancellationToken)
    {
        IMemento memento = aggregate.Save();

        // Assuming SnapshotEvent implements ISnapshotEvent and converters are registered.
        var snapshotEvent = new SnapshotEvent
        {
            Id = Guid.CreateVersion7(),
            Memento = memento,
            OwnerId = aggregate.KeyId
        };

        await _eventStore
            .AppendSnapshotAsync(aggregate.KeyId, snapshotEvent, cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task<TAggregate> ResolveFromSnapshotAsync(
        Guid aggregateId,
        CancellationToken cancellationToken)
    {
        // Get latest snapshot once (store-agnostic)
        var envelope = await _eventStore
            .ReadLatestSnapshotAsync(aggregateId, cancellationToken)
            .ConfigureAwait(false);

        if (!envelope.HasValue)
        {
            // No snapshot: fallback
            return await _aggregateStore
                .LoadAsync(aggregateId, cancellationToken)
                .ConfigureAwait(false);
        }

        if (envelope.Value.Event is not ISnapshotEvent snapshot)
        {
            throw new InvalidOperationException(
                $"Latest snapshot for aggregate '{aggregateId}' is not an ISnapshotEvent.");
        }

        // Rehydrate from snapshot
        var aggregate = new TAggregate();
        aggregate.Restore(snapshot.Memento);

        // Replay events after snapshot’s version (assuming Restore set StreamVersion to snapshot version)
        await foreach (var env in _eventStore
            .ReadStreamAsync(aggregateId, fromVersion: aggregate.StreamVersion, cancellationToken: cancellationToken)
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
                    $"The aggregate with ID '{aggregateId}' was not found.",
                    [nameof(aggregateId)]),
                null,
                aggregateId);
        }

        return aggregate;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool ShouldCreateSnapshot(IAggregate aggregate) =>
        _options.IsSnapshotEnabled
        && aggregate.StreamVersion > 0
        && aggregate.StreamVersion % _options.SnapshotFrequency == 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsRethrownException(Exception exception) =>
        exception is InvalidOperationException
        or ValidationException
        or ArgumentException
        or ArgumentNullException
        or OperationCanceledException;

    private void ValidateOptions()
    {
        if (_options.IsSnapshotEnabled && _options.SnapshotFrequency <= 0)
        {
            throw new InvalidOperationException(
                "SnapshotFrequency must be greater than 0 when snapshots are enabled.");
        }
    }
}