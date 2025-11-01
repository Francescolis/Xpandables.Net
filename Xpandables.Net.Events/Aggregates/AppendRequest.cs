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
namespace Xpandables.Net.Events.Aggregates;

/// <summary>
/// Represents a request to append one or more events to a specific event stream with optional optimistic concurrency
/// control.
/// </summary>
public readonly record struct AppendRequest
{
    /// <summary>
    /// Gets the unique identifier of the event stream to which events will be appended.
    /// </summary>
    public readonly required Guid StreamId { get; init; }

    /// <summary>
    /// Gets the collection of events to append to the stream.
    /// </summary>
    public readonly required IEnumerable<IEvent> Events { get; init; }

    /// <summary>
    /// Gets the expected version of the stream for optimistic concurrency control.
    /// If null, no version check is performed.
    /// </summary>
    public readonly long? ExpectedVersion { get; init; }
}
