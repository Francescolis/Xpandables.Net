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

namespace Xpandables.Net.Events.Configurations;

/// <summary>
/// Provides configuration for the Entity Framework Core mapping of the EntitySnapshotEvent entity type.
/// </summary>
/// <remarks>This configuration defines property requirements and indexes for the EntitySnapshotEvent entity. It
/// is intended to be used within the Entity Framework Core model-building process to ensure correct schema generation
/// and query performance.</remarks>
public sealed class EntitySnapShotEventTypeConfiguration : EntityEventTypeConfiguration<EntitySnapshotEvent>
{
    /// <inheritdoc/>
    public sealed override void Configure(EntityTypeBuilder<EntitySnapshotEvent> builder)
    {
        base.Configure(builder);
        builder.ToTable("SnapshotEvents");

        builder.Property(e => e.OwnerId).IsRequired();

        // Single index for OwnerId lookups (find all snapshots for an aggregate)
        builder.HasIndex(e => e.OwnerId)
               .HasDatabaseName("IX_SnapshotEvent_OwnerId");

        // Unique constraint: only one snapshot per owner at a specific sequence
        // This prevents duplicate snapshots and ensures data integrity
        // Sequence represents the aggregate version at which the snapshot was taken
        builder.HasIndex(e => new { e.OwnerId, e.Sequence })
               .IsUnique()
               .HasDatabaseName("IX_SnapshotEvent_OwnerId_Sequence_Unique");

        // Cross-database concurrency control
        builder.Property(e => e.UpdatedOn)
               .IsConcurrencyToken();
    }

}
