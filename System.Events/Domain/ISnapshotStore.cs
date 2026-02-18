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

namespace System.Events.Domain;

/// <summary>
/// Defines a contract for an event store that supports storing and retrieving snapshot events in addition to standard
/// event operations.
/// </summary>
/// <remarks>Implementations of this interface provide mechanisms to persist and access snapshot events, which can
/// be used to optimize state reconstruction for aggregates or entities. This interface extends the base event store
/// functionality by enabling efficient retrieval and storage of snapshots, typically used in event-sourced systems to
/// reduce replay time.</remarks>
public interface ISnapshotStore
{
	/// <summary>
	/// Asynchronously retrieves the latest snapshot event for the specified owner.
	/// </summary>
	/// <param name="ownerId">The unique identifier of the owner whose snapshot event is to be retrieved. Cannot be null or empty.</param>
	/// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
	/// <returns>A task that represents the asynchronous operation. The task result contains an optional <see cref="EnvelopeResult"/> representing 
	/// the latest snapshot event if found; otherwise, an empty optional.</returns>
	Task<EnvelopeResult?> GetLatestSnapshotAsync(Guid ownerId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Asynchronously appends a snapshot event to the store.
	/// </summary>
	/// <param name="event">The snapshot event to be saved. Cannot be null.</param>
	/// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous save operation.</param>
	/// <returns>A task that represents the asynchronous save operation.</returns>
	Task AppendSnapshotAsync(ISnapshotEvent @event, CancellationToken cancellationToken = default);
}
