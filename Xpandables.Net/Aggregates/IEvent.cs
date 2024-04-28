/*******************************************************************************
 * Copyright (C) 2023 Francis-Black EWANE
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
namespace Xpandables.Net.Aggregates;

/// <summary>
/// Defines a marker interface to be used to mark an object to act as an event.
/// </summary>
public interface IEvent
{
    /// <summary>
    /// Gets When the event occurred.
    /// </summary>
    DateTimeOffset OccurredOn { get; init; }

    /// <summary>
    /// Gets the event identifier.
    /// </summary>
    Guid Id { get; init; }

    /// <summary>
    /// Gets the version of the event or associated object.
    /// </summary>
    ulong Version { get; init; }
}


/// <summary>
/// Represents the base class for all events.
/// </summary>
public abstract record class Event : IEvent
{
    ///<inheritdoc/>
    public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;

    ///<inheritdoc/>
    public Guid Id { get; init; } = Guid.NewGuid();

    ///<inheritdoc/>
    public ulong Version { get; init; }
}