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

namespace System.Events.Domain;

/// <summary>
/// Represents a wrapper for an event, providing metadata and contextual information about the event.
/// </summary>
/// <remarks>The <see cref="EnvelopeResult"/> is used to encapsulate an event along with its associated metadata, 
/// such as the event's unique identifier, type, timestamp, and its position in the global event stream.  It also
/// includes optional information about the stream to which the event belongs, if applicable.</remarks>
public readonly record struct EnvelopeResult
{
    /// <summary>
    /// Gets the unique identifier for the event.
    /// </summary>
    public readonly required Guid EventId { get; init; }
    /// <summary>
    /// Gets the type of the event as a string.
    /// </summary>
    public readonly required string EventName { get; init; }

    /// <summary>
    /// Gets the date and time when the event occurred.
    /// </summary>
    public readonly required DateTimeOffset OccurredOn { get; init; }
    /// <summary>
    /// Gets the event associated with this instance.
    /// </summary>
    public readonly required IEvent Event { get; init; }
    /// <summary>
    /// Gets the global position of the entity in a sequence or stream.
    /// </summary>
    public readonly required long GlobalPosition { get; init; }
    /// <summary>
    /// Gets the unique identifier of the stream associated with this instance.
    /// </summary>
    public readonly Guid? StreamId { get; init; }
    /// <summary>
    /// Gets the name of the stream, or <see langword="null"/> if no name is specified.
    /// </summary>
    public readonly string? StreamName { get; init; }
    /// <summary>
    /// Gets the version of the stream, or <see langword="null"/> if the version is not available.
    /// </summary>
    public readonly long? StreamVersion { get; init; }
    /// <summary>
    /// Gets the identifier of the event that caused this event (for causation tracking).
    /// </summary>
    /// <remarks>Null if this event was not caused by another event.</remarks>
    public readonly Guid? CausationId { get; init; }
    /// <summary>
    /// Gets the correlation identifier for tracking related events across streams.
    /// </summary>
    /// <remarks>Null if this event is not part of a correlated flow.</remarks>
    public readonly Guid? CorrelationId { get; init; }
}
