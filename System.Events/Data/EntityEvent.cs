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
using System.Entities;

namespace System.Events.Data;

/// <summary>
/// Represents a domain event associated with an entity, providing event metadata and data payload for event-driven
/// operations.
/// </summary>
/// <remarks>EntityEvent serves as a base class for events that capture changes or actions related to entities in
/// the domain. It includes metadata such as the event name, fully qualified name, and a structured data payload.
/// Instances are typically used in event sourcing, auditing, or integration scenarios where tracking entity changes is
/// required. The class implements IDisposable to ensure proper release of resources associated with the event
/// data.</remarks>
public abstract class EntityEvent : IEntityEvent
{
    /// <summary>
    /// Initializes a new instance of <see cref="EntityEvent" />.
    /// </summary>
    protected EntityEvent() { }

    /// <inheritdoc />
    [Column("Status")]
    [StringLength(50)]
    [EntityStatusValidation(allowCustomStatuses: true)]
    public string Status { get; set; } = EntityStatus.ACTIVE.Value;

    /// <inheritdoc/>
    [Column("CausationId")]
    [StringLength(100)]
    public string? CausationId { get; init; }

    /// <inheritdoc/>
    [Column("CorrelationId")]
    [StringLength(100)]
    public string? CorrelationId { get; init; }

    /// <inheritdoc />
    [Column("CreatedOn")]
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    /// <inheritdoc />
    [Column("UpdatedOn")]
    public DateTime? UpdatedOn { get; set; }

    /// <inheritdoc />
    [Column("DeletedOn")]
    public DateTime? DeletedOn { get; set; }

    /// <inheritdoc/>
    [NotMapped]
    public bool IsDeleted => Status == EntityStatus.DELETED.Value;

    /// <inheritdoc />
    [Key]
    [Column("KeyId")]
    public required Guid KeyId { get; init; }

    /// <inheritdoc />
    public void SetStatus(string status)
    {
        Status = status;
        DeletedOn = status == EntityStatus.DELETED.Value
            ? DateTime.UtcNow
            : null;
    }
    /// <inheritdoc />
    [Column("EventName")]
    [StringLength(500)]
    public required string EventName { get; init; }

    /// <inheritdoc />
    [Column("Sequence"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Sequence { get; init; }
}