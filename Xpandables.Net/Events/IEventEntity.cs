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

using Xpandables.Net.Repositories;

namespace Xpandables.Net.Events;

/// <summary>
/// Represents an event entity that contains event-related data.
/// </summary>
public interface IEventEntity : IEntity<Guid>, IDisposable
{
    /// <summary>
    /// Gets the name of the event.
    /// </summary>
    string EventName { get; }

    /// <summary>
    /// Gets the full name of the event.
    /// </summary>
    string EventFullName { get; }

    /// <summary>
    /// Gets the version of the event.
    /// </summary>
    ulong EventVersion { get; }

    /// <summary>
    /// Gets the data associated with the event.
    /// </summary>
    JsonDocument EventData { get; }
}