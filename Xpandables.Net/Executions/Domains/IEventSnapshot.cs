﻿
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
/// Represents an event snapshot that includes a memento and its owner.
/// </summary>
public interface IEventSnapshot : IEvent
{
    /// <summary>
    /// Gets the memento associated with the event snapshot.
    /// </summary>
    IMemento Memento { get; }

    /// <summary>
    /// Gets the owner of the event snapshot.
    /// </summary>
    Guid OwnerId { get; }
}

/// <summary>
/// Represents a snapshot of an event with its associated memento and owner.
/// </summary>
public sealed record EventSnapshot : Event, IEventSnapshot
{
    ///<inheritdoc/>
    public required IMemento Memento { get; init; }

    ///<inheritdoc/>
    public required Guid OwnerId { get; init; }
}
