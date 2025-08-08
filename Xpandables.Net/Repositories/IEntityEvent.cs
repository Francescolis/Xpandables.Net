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

namespace Xpandables.Net.Repositories;

/// <summary>
/// Represents an event entity with a unique identifier, providing access to event-specific details such as name, full
/// name, version, and associated data.
/// </summary>
/// <remarks>This interface extends <see cref="IEntity{Guid}"/> to include event-specific properties and
/// implements <see cref="IDisposable"/> for managing resources.</remarks>
public interface IEntityEvent : IEntity<Guid>, IDisposable
{
    /// <summary>
    /// Gets the name of the event.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the full name of the event.
    /// </summary>
    string FullName { get; }

    /// <summary>
    /// Gets the version of the event.
    /// </summary>
    ulong Version { get; }

    /// <summary>
    /// Gets the sequence number of the event, which is used to track the order of events.
    /// </summary>
    ulong Sequence { get; }

    /// <summary>
    /// Gets the data associated with the event.
    /// </summary>
    JsonDocument Data { get; }
}