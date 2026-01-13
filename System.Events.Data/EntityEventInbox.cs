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
namespace System.Events.Data;

/// <summary>
/// Represents an event in the inbox used for tracking the processing state, attempts, and error information of an
/// entity event within the event-driven system.
/// </summary>
/// <remarks>Use this class to monitor and manage the lifecycle of entity events that require reliable processing,
/// including retry attempts and error handling. This type is typically used in scenarios where event delivery
/// guarantees and processing status tracking are required.</remarks>
public sealed class EntityEventInbox : EntityEvent, IEntityEventInbox
{
    /// <summary>
    /// Constructs a new instance of the <see cref="EntityEventInbox" /> class.
    /// </summary>
    public EntityEventInbox() => SetStatus(EventStatus.PROCESSING);

    /// <inheritdoc/>
    public string? ErrorMessage { get; init; }

    /// <inheritdoc/>
    public int AttemptCount { get; init; }

    /// <inheritdoc/>
    public DateTime? NextAttemptOn { get; init; }

    /// <inheritdoc/>
    public Guid? ClaimId { get; init; }

    /// <inheritdoc/>
    public string Consumer { get; set; } = string.Empty;
}