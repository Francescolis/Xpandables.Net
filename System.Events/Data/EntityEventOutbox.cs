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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace System.Events.Data;

/// <summary>
/// Represents an event that is stored in the outbox for reliable processing and eventual dispatch to external systems.
/// </summary>
/// <remarks>Use this type to track events that require guaranteed delivery or retry logic. The outbox pattern
/// helps ensure that events are not lost and are processed even in the presence of failures. This class includes
/// properties for tracking delivery attempts, scheduling retries, and recording error information.</remarks>
[Table("OutboxEvents")]
public sealed class EntityEventOutbox : EntityEvent, IEntityEventOutbox
{
    /// <summary>
    /// Constructs a new instance of the <see cref="EntityEventOutbox" /> class.
    /// </summary>
    public EntityEventOutbox() => SetStatus(EventStatus.PENDING.Value);

    /// <inheritdoc/>
    [Column("ErrorMessage")]
    [StringLength(4000)]
    public string? ErrorMessage { get; set; }

    /// <inheritdoc/>
    [Column("AttemptCount")]
    public int AttemptCount { get; set; }

    /// <inheritdoc/>
    [Column("NextAttemptOn")]
    public DateTime? NextAttemptOn { get; set; }

    /// <inheritdoc/>
    [Column("ClaimId")]
    public Guid? ClaimId { get; set; }

    /// <inheritdoc />
    [Column("EventData")]
    public required string EventData { get; init; }
}