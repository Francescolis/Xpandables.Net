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
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

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
    [EventStatusValidation(allowCustomStatuses: true)]
    public string Status { get; set; } = EventStatus.ACTIVE;

    /// <inheritdoc/>
    public string? CausationId { get; init; }

    /// <inheritdoc/>
    public string? CorrelationId { get; init; }

    /// <inheritdoc />
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    /// <inheritdoc />
    public DateTime? UpdatedOn { get; set; }

    /// <inheritdoc />
    public DateTime? DeletedOn { get; set; }

    /// <inheritdoc/>
    public bool IsDeleted => Status == EventStatus.DELETED;

    /// <inheritdoc />
    [Key]
    public required Guid KeyId { get; init; }

    /// <inheritdoc />
    public void SetStatus(string status)
    {
        Status = status;
        DeletedOn = status == EventStatus.DELETED
            ? DateTime.UtcNow
            : null;
    }
    /// <inheritdoc />
    public required string EventName { get; init; }

    /// <inheritdoc />
    public required JsonDocument EventData { get; init; }

    /// <inheritdoc />
    public long Sequence { get; init; }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the resources used by the event entity.
    /// </summary>
    /// <param name="disposing">
    /// True if the method is called directly or indirectly by user code; false if called by the
    /// runtime from within the finalizer.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            EventData?.Dispose();
        }
    }

    internal const DynamicallyAccessedMemberTypes DynamicallyAccessedMemberTypes =
         System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicConstructors
         | System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.NonPublicConstructors
         | System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicProperties
         | System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicFields
         | System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.NonPublicProperties
         | System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.NonPublicFields
         | System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.Interfaces;
}