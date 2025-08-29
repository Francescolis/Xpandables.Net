
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
using Xpandables.Net.Events;

namespace Xpandables.Net.Repositories;

/// <summary>
/// Event-sourcing oriented event store API.
/// Provides stream appends with optimistic concurrency, stream/global reads, and snapshot support.
/// </summary>
public interface IEventStore : IAsyncDisposable
{
    /// <summary>
    /// Appends domain events to an aggregate stream with optimistic concurrency.
    /// </summary>
    /// <param name="aggregateId">Aggregate identifier.</param>
    /// <param name="aggregateName">Aggregate CLR name (usually typeof(T).Name or FullName).</param>
    /// <param name="expectedVersion">Expected current stream version (-1 for new stream).</param>
    /// <param name="events">Events to append (in order).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>AppendResult including assigned stream version range.</returns>
    Task<AppendResult> AppendToStreamAsync(
        Guid aggregateId,
        string aggregateName,
        long expectedVersion,
        IEnumerable<IDomainEvent> events,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads domain events in a specific aggregate stream from a given version.
    /// </summary>
    IAsyncEnumerable<EventEnvelope> ReadStreamAsync(
        Guid aggregateId,
        long fromVersion = -1,
        int maxCount = int.MaxValue,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads all events globally from a given store position (sequence).
    /// </summary>
    IAsyncEnumerable<EventEnvelope> ReadAllAsync(
        long fromPosition = 0,
        int maxCount = 4096,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current stream version for an aggregate, or -1 if no stream exists.
    /// </summary>
    Task<long> GetStreamVersionAsync(
        Guid aggregateId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Appends a snapshot event for an aggregate.
    /// </summary>
    Task AppendSnapshotAsync(
        Guid aggregateId,
        ISnapshotEvent snapshot,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads the latest snapshot event for an aggregate, if any.
    /// </summary>
    Task<EventEnvelope?> ReadLatestSnapshotAsync(
        Guid aggregateId,
        CancellationToken cancellationToken = default);
}