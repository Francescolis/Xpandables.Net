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
using System.Text.Json;

namespace Xpandables.Net.Events;

/// <summary>
/// Represents an event associated with an entity, providing access to event metadata and data payload.
/// </summary>
/// <remarks>Implementations of this interface expose information about an entity event, including its name, fully
/// qualified name, sequence number, and associated data. The interface inherits from <see cref="IDisposable"/>,
/// indicating that resources associated with the event may need to be released when the event is no longer
/// needed.</remarks>
public interface IEntityEvent : IDisposable
{
    /// <summary>
    /// Gets the name of the event associated with the current instance.
    /// </summary>
    string EventType { get; }

    /// <summary>
    /// Gets the event full name, including any relevant namespace or path information.
    /// </summary>
    string EventFullName { get; }

    /// <summary>
    /// Gets the current sequence number associated with the instance, which is used to track the order of events.
    /// </summary>
    long Sequence { get; }

    /// <summary>
    /// Gets the JSON data associated with this instance.
    /// </summary>
    JsonDocument EventData { get; }
}
