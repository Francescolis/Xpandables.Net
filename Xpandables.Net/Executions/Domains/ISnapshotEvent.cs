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

using Xpandables.Net.Executions.Tasks;
using Xpandables.Net.States;

namespace Xpandables.Net.Executions.Domains;

/// <summary>
/// Defines a contract for a snapshot event, which includes its associated memento state and owner identifier.
/// </summary>
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
}

/// <summary>
/// Represents a snapshot of an event with its associated memento and owner.
/// </summary>
public sealed record SnapshotEvent : Event, ISnapshotEvent
{
    /// <inheritdoc />
    public required IMemento Memento { get; init; }

    /// <inheritdoc />
    public required Guid OwnerId { get; init; }
}