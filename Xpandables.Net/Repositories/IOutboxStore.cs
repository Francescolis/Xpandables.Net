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
/// Outbox pattern store for integration events.
/// </summary>
public interface IOutboxStore
{
    /// <summary>
    /// Enqueues an integration event into the outbox.
    /// Must not call SaveChanges; let Unit of Work flush.
    /// </summary>
    Task EnqueueAsync(IIntegrationEvent @event, CancellationToken cancellationToken = default);

    /// <summary>
    /// Claims a batch of pending events for processing.
    /// </summary>
    Task<IReadOnlyList<IIntegrationEvent>> ClaimPendingAsync(
        int batchSize,
        TimeSpan? leaseDuration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks claimed events as completed (published).
    /// </summary>
    Task CompleteAsync(IEnumerable<Guid> eventIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks claimed events as failed and schedules retry with backoff.
    /// </summary>
    Task FailAsync(IEnumerable<(Guid EventId, string Error)> failures, CancellationToken cancellationToken = default);
}
