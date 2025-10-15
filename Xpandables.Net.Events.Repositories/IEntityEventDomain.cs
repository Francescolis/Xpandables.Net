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
namespace Xpandables.Net.Events;

/// <summary>
/// Defines the contract for an event that is associated with a specific stream and version within a domain-driven
/// design context.
/// </summary>
/// <remarks>Implementations of this interface represent events that are part of an event stream, typically used
/// in event sourcing or domain event scenarios. The interface provides access to the stream's unique identifier,
/// version, and name, enabling consumers to track and process events in the correct order and context.</remarks>
public interface IEntityEventDomain : IEntityEvent
{
    /// <summary>
    /// Gets the unique identifier for the stream.
    /// </summary>
    Guid StreamId { get; }

    /// <summary>
    /// Gets the stream version of the event.
    /// </summary>
    long StreamVersion { get; }

    /// <summary>
    /// Gets the name of the stream associated with this instance.
    /// </summary>
    string StreamName { get; init; }
}