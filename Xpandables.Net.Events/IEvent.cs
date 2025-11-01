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
namespace Xpandables.Net.Events;

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