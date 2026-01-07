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
namespace System.Events;

/// <summary>
/// Represents an event model that contains basic event details such as occurrence time, version, and unique identifier.
/// </summary>
public interface IEvent
{
    /// <summary>
    /// Gets the date and time when the event occurred.
    /// </summary>
    DateTimeOffset OccurredOn { get; init; }

    /// <summary>
    /// Gets the unique identifier of the event.
    /// </summary>
    Guid EventId { get; init; }

    /// <summary>
    /// Gets the identifier of the event that caused this event (for causation tracking).
    /// </summary>
    /// <remarks>Null if this event was not caused by another event.</remarks>
    string? CausationId { get; init; }

    /// <summary>
    /// Gets the correlation identifier for tracking related events across streams.
    /// </summary>
    /// <remarks>
    /// For HTTP flows this is typically the full W3C <c>traceparent</c> header value.
    /// Null if this event is not part of a correlated flow.
    /// </remarks>
    string? CorrelationId { get; init; }

    /// <summary>
    /// Gets the name of the event type represented by this instance.
    /// </summary>
    /// <remarks>This method returns the name of the class as the event name. Override this method in derived
    /// classes if a custom event name is required.</remarks>
    /// <returns>A string containing the name of the event type. The value corresponds to the runtime type name of the current
    /// object.</returns>
    public string GetEventName() => GetType().Name;

    /// <summary>
    /// Tries to parse <see cref="CausationId"/> as a <see cref="Guid"/>.
    /// </summary>
    /// <param name="causationId">The parsed guid when successful; otherwise <see cref="Guid.Empty"/>.</param>
    /// <returns><see langword="true"/> when the value exists and is a valid <see cref="Guid"/>; otherwise <see langword="false"/>.</returns>
    public bool TryGetCausationGuidId(out Guid causationId)
    {
        causationId = Guid.Empty;

        if (string.IsNullOrWhiteSpace(CausationId))
        {
            return false;
        }

        return Guid.TryParse(CausationId, out causationId);
    }

    /// <summary>
    /// Tries to parse <see cref="CorrelationId"/> as a <see cref="Guid"/>.
    /// </summary>
    /// <param name="correlationId">The parsed guid when successful; otherwise <see cref="Guid.Empty"/>.</param>
    /// <returns><see langword="true"/> when the value exists and is a valid <see cref="Guid"/>; otherwise <see langword="false"/>.</returns>
    public bool TryGetCorrelationGuidId(out Guid correlationId)
    {
        correlationId = Guid.Empty;

        if (string.IsNullOrWhiteSpace(CorrelationId))
        {
            return false;
        }

        return Guid.TryParse(CorrelationId, out correlationId);
    }
}

/// <summary>
/// Provides an abstract base implementation for event models, encapsulating common properties such as
/// occurrence time, version, and unique identifier.
/// </summary>
public abstract record EventBase : IEvent
{
    /// <inheritdoc/>
    public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;

    /// <inheritdoc/>
    /// <remarks>It's based on the <see cref="Guid.CreateVersion7()"/>.</remarks>
    public Guid EventId { get; init; } = Guid.CreateVersion7();

    /// <inheritdoc />
    public string? CausationId { get; init; }

    /// <inheritdoc />
    public string? CorrelationId { get; init; }
}