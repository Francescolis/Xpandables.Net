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
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Xpandables.Net.Events.Repositories;

namespace Xpandables.Net.Events.Configurations;

/// <summary>
/// Configures the entity type mapping for the EntityDomainEvent in the data model.
/// </summary>
/// <remarks>This configuration defines required properties and key constraints for the EntityDomainEvent entity
/// when using Entity Framework Core. It ensures that StreamId, StreamVersion, and StreamName are required fields, sets
/// a maximum length for StreamName, and establishes primary and unique composite keys for event stream
/// identification.</remarks>
public sealed class EntityDomainEventTypeConfiguration : EntityEventTypeConfiguration<EntityDomainEvent>
{
    /// <inheritdoc/>
    public sealed override void Configure(EntityTypeBuilder<EntityDomainEvent> builder)
    {
        base.Configure(builder);
        builder.ToTable("DomainEvents");

        builder.Property(e => e.StreamId).IsRequired();
        builder.Property(e => e.StreamVersion).IsRequired();
        builder.Property(e => e.StreamName).IsRequired();

        // PRIMARY KEY is already configured in base class as KeyId
        // Create unique constraint for event sourcing optimistic concurrency
        // This database-level constraint prevents duplicate events in the same stream
        builder.HasIndex(e => new { e.StreamId, e.StreamVersion })
               .IsUnique()
               .HasDatabaseName("IX_DomainEvent_StreamId_StreamVersion_Unique");

        // Additional index for stream-based queries (reading all events for a stream)
        builder.HasIndex(e => e.StreamId)
               .HasDatabaseName("IX_DomainEvent_StreamId");

        // Index for stream name queries (useful for querying by aggregate type)
        builder.HasIndex(e => e.StreamName)
               .HasDatabaseName("IX_DomainEvent_StreamName");

        // Cross-database concurrency control using UpdatedOn
        // Prevents lost updates when events are modified (rare in event sourcing, but useful)
        builder.Property(e => e.UpdatedOn)
               .IsConcurrencyToken();
    }

}
