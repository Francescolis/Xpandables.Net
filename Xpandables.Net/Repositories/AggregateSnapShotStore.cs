/*******************************************************************************
 * Copyright (C) 2024 Francis-Black EWANE
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
using System.Runtime.CompilerServices;

using Microsoft.Extensions.Options;

using Xpandables.Net.Events;
using Xpandables.Net.States;

namespace Xpandables.Net.Repositories;

/// <summary>
/// Represents a store for aggregate root snapshots that provides optimized
/// snapshot creation and retrieval functionality.
/// </summary>
/// <typeparam name="TAggregate">The type of the aggregate root.</typeparam>
public sealed class AggregateSnapShotStore<TAggregate> : IAggregateStore<TAggregate>
    where TAggregate : Aggregate, IOriginator, new()
{
    private readonly IAggregateStore<TAggregate> _aggregateStore;
    private readonly IEventStore _eventStore;
    private readonly SnapShotOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateSnapShotStore{TAggregate}"/> class.
    /// </summary>
    /// <param name="aggregateStore">The underlying aggregate store.</param>
    /// <param name="unitOfWork">The unit of work for event operations.</param>
    /// <param name="options">The snapshot configuration options.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
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

    /// <inheritdoc />
    public async Task AppendAsync(
        TAggregate aggregate,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(aggregate);

        try
        {
            // Create snapshot if conditions are met
            if (ShouldCreateSnapshot(aggregate))
            {
                await CreateSnapshotAsync(aggregate, cancellationToken).ConfigureAwait(false);
            }

            // Append the aggregate to the underlying store
            await _aggregateStore
                .AppendAsync(aggregate, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception) when (IsRethrownException(exception))
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException(
                $"Unable to append the aggregate with ID '{aggregate.KeyId}'. See inner exception for details.",
                exception);
        }
    }

    /// <inheritdoc />
    public async Task<TAggregate> ResolveAsync(
        Guid keyId,
        CancellationToken cancellationToken = default)
    {
        if (keyId == Guid.Empty)
        {
            throw new ArgumentException("KeyId cannot be empty.", nameof(keyId));
        }

        // If snapshots are disabled, delegate to the underlying store
        if (!_options.IsSnapshotEnabled)
        {
            return await _aggregateStore
                .ResolveAsync(keyId, cancellationToken)
                .ConfigureAwait(false);
        }

        try
        {
            return await ResolveFromSnapshotAsync(keyId, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception) when (IsRethrownException(exception))
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException(
                $"Unable to resolve the aggregate with ID '{keyId}'. See inner exception for details.",
                exception);
        }
    }

    private async Task CreateSnapshotAsync(TAggregate aggregate, CancellationToken cancellationToken)
    {
        IMemento memento = aggregate.Save();

        var snapshotEvent = new SnapshotEvent
        {
            Id = Guid.CreateVersion7(),
            Memento = memento,
            OwnerId = aggregate.KeyId
        };

        await _eventStore
            .AppendAsync(snapshotEvent, cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task<TAggregate> ResolveFromSnapshotAsync(
        Guid keyId,
        CancellationToken cancellationToken)
    {
        // Try to find the most recent snapshot
        var latestSnapshot = await FindLatestSnapshotAsync(keyId, cancellationToken)
            .ConfigureAwait(false);

        TAggregate aggregate = new();

        if (latestSnapshot is not null)
        {
            // Restore aggregate from snapshot
            aggregate.Restore(latestSnapshot.Memento);

            // Load events that occurred after the snapshot
            await LoadEventsAfterSnapshotAsync(aggregate, keyId, cancellationToken)
                .ConfigureAwait(false);
        }
        else
        {
            // No snapshot found, fallback to regular resolution
            return await _aggregateStore
                .ResolveAsync(keyId, cancellationToken)
                .ConfigureAwait(false);
        }

        if (aggregate.IsEmpty)
        {
            throw new ValidationException(
                new ValidationResult(
                    $"The aggregate with ID '{keyId}' was not found.",
                    [nameof(keyId)]),
                null,
                keyId);
        }

        return aggregate;
    }

    private async Task<ISnapshotEvent?> FindLatestSnapshotAsync(
        Guid keyId,
        CancellationToken cancellationToken)
    {
        return await _eventStore
            .FetchAsync<EntitySnapshotEvent, EntitySnapshotEvent>(query =>
                query.Where(w => w.OwnerId == keyId)
                    .OrderByDescending(o => o.Sequence)
                    .Take(1), cancellationToken)
            .AsEventsPagedAsync(cancellationToken)
            .OfType<ISnapshotEvent>()
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task LoadEventsAfterSnapshotAsync(
        TAggregate aggregate,
        Guid keyId,
        CancellationToken cancellationToken)
    {
        var eventsQuery = _eventStore
            .FetchAsync<EntityDomainEvent, EntityDomainEvent>(query =>
                query.Where(w => w.AggregateId == keyId && w.StreamVersion > aggregate.StreamVersion)
                    .OrderBy(o => o.StreamVersion), cancellationToken)
            .AsEventsPagedAsync(cancellationToken)
            .OfType<IDomainEvent>();

        await foreach (IDomainEvent domainEvent in eventsQuery
            .WithCancellation(cancellationToken)
            .ConfigureAwait(false))
        {
            aggregate.LoadFromHistory(domainEvent);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool ShouldCreateSnapshot(Aggregate aggregate) =>
        _options.IsSnapshotEnabled
        && aggregate.StreamVersion > 0
        && aggregate.StreamVersion % _options.SnapshotFrequency == 0
        && aggregate.StreamVersion >= _options.SnapshotFrequency;

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