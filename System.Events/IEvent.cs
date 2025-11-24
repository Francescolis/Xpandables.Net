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
    /// Gets the name of the event type represented by this instance.
    /// </summary>
    /// <remarks>This method returns the name of the class as the event name. Override this method in derived
    /// classes if a custom event name is required.</remarks>
    /// <returns>A string containing the name of the event type. The value corresponds to the runtime type name of the current
    /// object.</returns>
    public string GetEventName() => GetType().Name;
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
}