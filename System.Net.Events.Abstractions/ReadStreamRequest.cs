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
/// Represents a request to read a sequence of events from a specific event stream.
/// </summary>
public readonly record struct ReadStreamRequest
{
    /// <summary>
    /// Initializes a new instance of the ReadStreamRequest class.
    /// </summary>
    public ReadStreamRequest()
    {
    }

    /// <summary>
    /// Gets the identifier of the event stream to read from.
    /// </summary>
    public readonly required Guid StreamId { get; init; }

    /// <summary>
    /// Gets the starting version to read from (inclusive).
    /// </summary>
    public readonly required long FromVersion { get; init; } = 0;

    /// <summary>
    /// Gets the maximum number of events to read. A value of 0 indicates no limit.
    /// </summary>
    public readonly required int MaxCount { get; init; } = 0;
}

/// <summary>
/// Represents a request to read events from all streams, specifying the starting position and maximum number of events
/// to retrieve.
/// </summary>
public readonly record struct ReadAllStreamsRequest
{
    /// <summary>
    /// Initializes a new instance of the ReadAllRequest class.
    /// </summary>
    public ReadAllStreamsRequest()
    {
    }
    /// <summary>
    /// Gets the starting global position to read from (inclusive).
    /// </summary>
    public readonly required long FromPosition { get; init; } = 0;

    /// <summary>
    /// Gets the maximum number of events to read. A value of 0 indicates no limit.
    /// </summary>
    public readonly required int MaxCount { get; init; } = 0;
}
