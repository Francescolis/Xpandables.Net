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

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using Microsoft.EntityFrameworkCore;

using Xpandables.Net.Optionals;
using Xpandables.Net.Repositories;

namespace Xpandables.Net.Events;

/// <summary>
/// Provides an implementation of an event store that persists domain events and snapshots using the specified data
/// context.
/// </summary>
/// <remarks>This class enables appending, reading, and managing event streams and snapshots in an event-sourced
/// system. It is designed to work with a specific data context, allowing integration with various storage backends that
/// support the DataContext abstraction. All operations are asynchronous and support cancellation via CancellationToken.
/// This class is not thread-safe; concurrent usage should be managed externally if required.</remarks>
/// <typeparam name="TDataContext">The type of the data context used for event persistence. Must inherit from DataContext.</typeparam>
/// <param name="context">The data context instance used to access the underlying event storage. Cannot be null.</param>
public sealed class EventStore<TDataContext>(TDataContext context) : DisposableAsync, IEventStore
    where TDataContext : DataContext
{
    private readonly TDataContext _db = context
        ?? throw new ArgumentNullException(nameof(context));

    ///<inheritdoc/>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
    public async Task<AppendResult> AppendToStreamAsync(
        AppendRequest request,
        CancellationToken cancellationToken = default)
    {
        var batch = request.Events.OfType<IDomainEvent>().ToArray();
        if (batch.Length == 0)
        {
            return AppendResult.Create(request.ExpectedVersion.GetValueOrDefault() + 1, request.ExpectedVersion.GetValueOrDefault());
        }

        long current = await GetStreamVersionCoreAsync(request.StreamId, cancellationToken).ConfigureAwait(false);
        if (request.ExpectedVersion.HasValue && current != request.ExpectedVersion)
        {
            // You can choose to throw early, or let DB uniqueness enforce at commit. Early throw is helpful.
            throw new InvalidOperationException(
                $"Concurrency violation for aggregate {request.StreamId}. " +
                $"Expected version {request.ExpectedVersion} but found {current}.");
        }

        var converter = EventConverter.GetConverterFor<IDomainEvent>();
        var entities = new List<EntityDomainEvent>(capacity: batch.Length);
        long next = request.ExpectedVersion.GetValueOrDefault();

        foreach (var @event in batch)
        {
            next++;

            var nextEvent = @event
                .WithStreamId(request.StreamId)
                .WithStreamVersion(next)
                .WithStreamName(@event.StreamName);

            var entity = (EntityDomainEvent)converter.ConvertEventToEntity(nextEvent);
            entities.Add(entity);
        }

        await _db.AddRangeAsync(entities, cancellationToken).ConfigureAwait(false);

        Guid[] guids = [.. entities.ConvertAll(e => e.StreamId)];
        return AppendResult.Create(guids, next, request.ExpectedVersion.GetValueOrDefault());
    }

    ///<inheritdoc/>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
    public async Task AppendSnapshotAsync(
        ISnapshotEvent snapshotEvent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(snapshotEvent);

        var converter = EventConverter.GetConverterFor<ISnapshotEvent>();
        var entity = (EntitySnapshotEvent)converter.ConvertEventToEntity(snapshotEvent);

        await _db.AddAsync(entity, cancellationToken).ConfigureAwait(false);
        // defer SaveChanges to Unit of Work
    }

    ///<inheritdoc/>
    public Task DeleteStreamAsync(DeleteStreamRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();

    ///<inheritdoc/>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
    public async Task<Optional<EnvelopeResult>> GetLatestSnapshotAsync(
        Guid ownerId,
        CancellationToken cancellationToken = default)
    {
        var last = await _db.Set<EntitySnapshotEvent>()
            .AsNoTracking()
            .Where(e => e.OwnerId == ownerId)
            .OrderByDescending(e => e.Sequence)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (last == null)
            return Optional.Empty<EnvelopeResult>();

        return Optional.Some(new EnvelopeResult
        {
            Event = EventConverter.DeserializeEntityToEvent(last),
            EventFullName = last.EventFullName,
            EventId = last.KeyId,
            EventType = last.EventType,
            GlobalPosition = last.Sequence,
            OccurredOn = last.CreatedOn,
            StreamId = last.OwnerId,
            StreamName = null,
            StreamVersion = last.Sequence
        });
    }

    ///<inheritdoc/>
    public async Task<long> GetStreamVersionAsync(Guid streamId, CancellationToken cancellationToken = default) =>
        await GetStreamVersionCoreAsync(streamId, cancellationToken).ConfigureAwait(false);

    ///<inheritdoc/>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
    public async IAsyncEnumerable<EnvelopeResult> ReadAllStreamsAsync(
        ReadAllStreamsRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var query = _db.Set<EntityDomainEvent>()
            .AsNoTracking()
            .Where(e => e.Sequence > request.FromPosition)
            .OrderBy(e => e.Sequence)
            .Take(request.MaxCount);

        await foreach (var entity in query.AsAsyncEnumerable().ConfigureAwait(false))
        {
            yield return new EnvelopeResult
            {
                Event = EventConverter.DeserializeEntityToEvent(entity),
                EventFullName = entity.EventFullName,
                EventId = entity.KeyId,
                EventType = entity.EventType,
                GlobalPosition = entity.Sequence,
                OccurredOn = entity.CreatedOn,
                StreamId = entity.StreamId,
                StreamName = entity.StreamName,
                StreamVersion = entity.StreamVersion
            };
        }
    }

    ///<inheritdoc/>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
    public async IAsyncEnumerable<EnvelopeResult> ReadStreamAsync(
        ReadStreamRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var query = _db.Set<EntityDomainEvent>()
            .AsNoTracking()
            .Where(e => e.StreamId == request.StreamId && e.StreamVersion > request.FromVersion) // exclusive lower bound
            .OrderBy(e => e.StreamVersion)
            .Take(request.MaxCount);

        await foreach (var entity in query.AsAsyncEnumerable().ConfigureAwait(false))
        {
            yield return new EnvelopeResult
            {
                Event = EventConverter.DeserializeEntityToEvent(entity),
                EventFullName = entity.EventFullName,
                EventId = entity.KeyId,
                EventType = entity.EventType,
                GlobalPosition = entity.Sequence,
                OccurredOn = entity.CreatedOn,
                StreamId = entity.StreamId,
                StreamName = entity.StreamName,
                StreamVersion = entity.StreamVersion
            };
        }
    }

    ///<inheritdoc/>
    public async Task<bool> StreamExistsAsync(Guid streamId, CancellationToken cancellationToken = default)
    {
        return await _db.Set<EntityDomainEvent>()
            .AsNoTracking()
            .AnyAsync(e => e.StreamId == streamId, cancellationToken)
            .ConfigureAwait(false);
    }

    ///<inheritdoc/>
    public IAsyncDisposable SubscribeToAllStreams(
        SubscribeToAllStreamsRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

    }

    ///<inheritdoc/>
    public IAsyncDisposable SubscribeToStream(SubscribeToStreamRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    ///<inheritdoc/>
    public Task TruncateStreamAsync(TruncateStreamRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();

    private async Task<long> GetStreamVersionCoreAsync(
        Guid streamId, CancellationToken cancellationToken)
    {
        var last = await _db.Set<EntityDomainEvent>()
            .AsNoTracking()
            .Where(e => e.StreamId == streamId)
            .OrderByDescending(e => e.StreamVersion)
            .Select(e => (long?)e.StreamVersion)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        return last ?? -1;
    }
}
