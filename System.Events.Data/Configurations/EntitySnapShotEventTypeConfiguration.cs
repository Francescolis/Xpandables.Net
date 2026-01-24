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

namespace System.Events.Data.Configurations;

/// <summary>
/// Provides configuration for the Entity Framework Core mapping of the EntitySnapshotEvent entity type.
/// </summary>
/// <remarks>This configuration defines property requirements and indexes for the EntitySnapshotEvent entity. It
/// is intended to be used within the Entity Framework Core model-building process to ensure correct schema generation
/// and query performance.</remarks>
public sealed class EntitySnapShotEventTypeConfiguration : EntityEventTypeConfiguration<EntityEventSnapshot>
{
    /// <inheritdoc/>
    public sealed override void Configure(EntityTypeBuilder<EntityEventSnapshot> builder)
    {
        base.Configure(builder);
        builder.ToTable("SnapshotEvents");

        builder.Property(e => e.OwnerId).IsRequired();
        builder.Property(e => e.CausationId).HasMaxLength(64);
        builder.Property(e => e.CorrelationId).HasMaxLength(64);

        builder.HasIndex(e => e.OwnerId)
               .HasDatabaseName("IX_SnapshotEvent_OwnerId");

        builder.HasIndex(e => new { e.OwnerId, e.Sequence })
               .IsUnique()
               .HasDatabaseName("IX_SnapshotEvent_OwnerId_Sequence_Unique");

        builder.Property(e => e.UpdatedOn)
               .IsConcurrencyToken();
    }
}
