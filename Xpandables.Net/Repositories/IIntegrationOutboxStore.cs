
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
/// Defines a contract for managing and retrieving integration events from an outbox store.
/// </summary>
/// <remarks>This interface is typically used to support the outbox pattern, which ensures reliable delivery of
/// integration events by persisting them in a durable store before processing. Implementations of this interface should
/// handle concurrency and ensure that claimed events are not processed by multiple consumers simultaneously.</remarks>
public interface IIntegrationOutboxStore
{
    /// <summary>
    /// Claims a batch of pending integration events for processing.
    /// </summary>
    /// <remarks>This method is typically used to retrieve a batch of events from a queue or store for
    /// processing.  The caller is responsible for ensuring that the claimed events are processed
    /// appropriately.</remarks>
    /// <param name="batchSize">The maximum number of integration events to claim in this operation. Must be greater than zero.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A read-only list of claimed integration events. The list will be empty if no pending events are available.</returns>
    Task<IReadOnlyList<IIntegrationEvent>> ClaimPendingAsync(
        int batchSize, CancellationToken cancellationToken = default);
}
