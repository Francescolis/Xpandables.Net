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
using Xpandables.Net.EventSourcing.States;

namespace Xpandables.Net.EventSourcing;

/// <summary>
/// Defines the contract for an event that captures a snapshot of an object's state using a memento pattern.
/// </summary>
/// <remarks>A snapshot event represents a point-in-time capture of an aggregate's state, typically used for
/// restoring or persisting state efficiently. Implementations should ensure that the associated memento accurately
/// reflects the state at the time the snapshot was taken.</remarks>
public interface ISnapshotEvent : IEvent
{
    /// <summary>
    /// Gets the memento associated with the snapshot event.
    /// </summary>
    IMemento Memento { get; }

    /// <summary>
    /// Gets the owner of the snapshot event.
    /// </summary>
    Guid OwnerId { get; }

    /// <summary>
    /// Associates the specified owner identifier with the snapshot event.
    /// </summary>
    /// <param name="ownerId">The unique identifier of the owner to associate with the snapshot event.</param>
    /// <returns>An updated <see cref="ISnapshotEvent"/> instance with the specified owner identifier.</returns>
    ISnapshotEvent WithOwnerId(Guid ownerId);
}

/// <summary>
/// Represents an event that captures the state of an aggregate at a specific point in time for snapshotting purposes.
/// </summary>
/// <remarks>A snapshot event is typically used to optimize event-sourced systems by persisting the current state
/// of an aggregate, reducing the need to replay all historical events to reconstruct state. This record is immutable
/// and intended for use in scenarios where state recovery performance is critical.</remarks>
public sealed record SnapshotEvent : EventBase, ISnapshotEvent
{
    /// <inheritdoc />
    public required IMemento Memento { get; init; }

    /// <inheritdoc />
    public required Guid OwnerId { get; init; }

    /// <inheritdoc />
    public ISnapshotEvent WithOwnerId(Guid ownerId) => this with { OwnerId = ownerId };
}