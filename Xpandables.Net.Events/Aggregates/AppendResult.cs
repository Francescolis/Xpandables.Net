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
using Xpandables.Net.Events;
using Xpandables.Net.Events.Aggregates;

namespace Xpandables.Net.Events.Aggregates;

/// <summary>
/// Represents the result of an append operation, including the identifiers of the appended events and the stream
/// versions assigned as a result of the operation.
/// </summary>
/// <remarks>This type provides information about the events appended to a stream and the resulting stream version
/// range. It is typically used to confirm the outcome of an append operation in event sourcing or similar
/// scenarios.</remarks>
public readonly record struct AppendResult
{
    /// <summary>
    /// Represents an empty result for an append operation, indicating no events were appended.
    /// </summary>
    /// <remarks>This static instance can be used as a default or placeholder result for scenarios where no
    /// events  are appended to a stream. The <see cref="FirstAssignedStreamVersion"/> and  <see
    /// cref="LastAssignedStreamVersion"/> properties are set to their default values, and  <see cref="EventIds"/> is an
    /// empty collection.</remarks>
    public static readonly AppendResult Empty = new()
    {
        EventIds = [],
        FirstAssignedStreamVersion = 0,
        LastAssignedStreamVersion = -1
    };

    /// <summary>
    /// Creates a new instance of the <see cref="AppendResult"/> class with the specified event identifiers and stream
    /// version information.
    /// </summary>
    /// <param name="eventIds">A collection of unique identifiers representing the events to be appended. Cannot be <see langword="null"/>.</param>
    /// <param name="firstAssignedStreamVersion">The version number assigned to the first event in the stream.</param>
    /// <param name="lastAssignedStreamVersion">The version number assigned to the last event in the stream.</param>
    /// <returns>A new <see cref="AppendResult"/> instance containing the specified event identifiers and stream version
    /// information.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="eventIds"/> is <see langword="null"/>.</exception>
    public static AppendResult Create(
        IReadOnlyCollection<Guid> eventIds,
        long firstAssignedStreamVersion,
        long lastAssignedStreamVersion) =>
        new()
        {
            EventIds = eventIds ?? throw new ArgumentNullException(nameof(eventIds)),
            FirstAssignedStreamVersion = firstAssignedStreamVersion,
            LastAssignedStreamVersion = lastAssignedStreamVersion
        };

    /// <summary>
    /// Creates a new instance of the <see cref="AppendResult"/> class with the specified event IDs and stream version
    /// information.
    /// </summary>
    /// <param name="eventIds">The collection of event IDs to include in the result. Cannot be <see langword="null"/>.</param>
    /// <param name="firstAssignedStreamVersion">The first version number assigned to the stream.</param>
    /// <param name="lastAssignedStreamVersion">The last version number assigned to the stream.</param>
    /// <returns>A new <see cref="AppendResult"/> instance initialized with the provided event IDs and stream version details.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="eventIds"/> is <see langword="null"/>.</exception>
    public static AppendResult Create(
        IEnumerable<Guid> eventIds,
        long firstAssignedStreamVersion,
        long lastAssignedStreamVersion) =>
        new()
        {
            EventIds = eventIds?.ToArray() ?? throw new ArgumentNullException(nameof(eventIds)),
            FirstAssignedStreamVersion = firstAssignedStreamVersion,
            LastAssignedStreamVersion = lastAssignedStreamVersion
        };

    /// <summary>
    /// Creates a new instance of the <see cref="AppendResult"/> class with the specified event ID and stream version
    /// range.
    /// </summary>
    /// <param name="eventId">The unique identifier of the event to include in the result.</param>
    /// <param name="firstAssignedStreamVersion">The first version of the stream assigned to the event.</param>
    /// <param name="lastAssignedStreamVersion">The last version of the stream assigned to the event.</param>
    /// <returns>A new <see cref="AppendResult"/> instance containing the specified event ID and stream version range.</returns>
    public static AppendResult Create(
        Guid eventId,
        long firstAssignedStreamVersion,
        long lastAssignedStreamVersion) =>
        new()
        {
            EventIds = [eventId],
            FirstAssignedStreamVersion = firstAssignedStreamVersion,
            LastAssignedStreamVersion = lastAssignedStreamVersion
        };

    /// <summary>
    /// Creates a new instance of the <see cref="AppendResult"/> class with the specified stream version information.
    /// </summary>
    /// <param name="firstAssignedStreamVersion">The version number of the first event assigned to the stream.</param>
    /// <param name="lastAssignedStreamVersion">The version number of the last event assigned to the stream.</param>
    /// <returns>A new <see cref="AppendResult"/> instance initialized with the specified stream version information.</returns>
    public static AppendResult Create(
        long firstAssignedStreamVersion,
        long lastAssignedStreamVersion) =>
        new()
        {
            EventIds = [],
            FirstAssignedStreamVersion = firstAssignedStreamVersion,
            LastAssignedStreamVersion = lastAssignedStreamVersion
        };

    /// <summary>
    /// Gets the unique identifiers of the events that were appended.
    /// </summary>
    public readonly required IReadOnlyCollection<Guid> EventIds { get; init; }

    /// <summary>
    /// Gets the first assigned stream version after the append operation.
    /// </summary>
    public readonly long FirstAssignedStreamVersion { get; init; }

    /// <summary>
    /// Gets the last assigned stream version after the append operation.
    /// </summary>
    public readonly long LastAssignedStreamVersion { get; init; }
}