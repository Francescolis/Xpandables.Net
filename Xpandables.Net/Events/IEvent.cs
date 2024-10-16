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
/// Represents an event with a timestamp, version, and unique identifier.
/// </summary>
public interface IEvent
{
    /// <summary>
    /// Gets the date and time when the event occurred.
    /// </summary>
    DateTimeOffset OccurredOn { get; init; }

    /// <summary>
    /// Gets the version of the event.
    /// </summary>
    int EventVersion { get; init; }

    /// <summary>
    /// Gets the unique identifier of the event.
    /// </summary>
    Guid EventId { get; init; }
}
