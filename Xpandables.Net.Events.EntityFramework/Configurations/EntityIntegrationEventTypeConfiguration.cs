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
/// Provides the Entity Framework Core configuration for the EntityIntegrationEvent entity type.
/// </summary>
/// <remarks>This configuration defines property requirements, default values, and indexes for the
/// EntityIntegrationEvent entity when used with Entity Framework Core. It customizes how the entity is mapped to the
/// database schema, including optional properties and index definitions.</remarks>
public sealed class EntityIntegrationEventTypeConfiguration : EntityEventTypeConfiguration<EntityIntegrationEvent>
{
    /// <inheritdoc/>
    public sealed override void Configure(EntityTypeBuilder<EntityIntegrationEvent> builder)
    {
        base.Configure(builder);
        builder.ToTable("IntegrationEvents");

        builder.Property(e => e.ClaimId).IsRequired(false);
        builder.Property(e => e.AttemptCount).HasDefaultValue(0);
        builder.Property(e => e.NextAttemptOn).IsRequired(false);
        builder.Property(e => e.ErrorMessage).IsRequired(false).HasMaxLength(int.MaxValue);

        // Cross-database concurrency control using UpdatedOn timestamp
        // Critical for outbox pattern to prevent double-processing
        builder.Property(e => e.UpdatedOn)
               .IsConcurrencyToken();

        // Index for processing pending events (most common query)
        // Query pattern: WHERE Status = 'PENDING' AND (NextAttemptOn IS NULL OR NextAttemptOn <= NOW)
        // Note: Filtered indexes syntax varies by database
        // PostgreSQL: Use quoted identifiers
        // SQL Server: Use unquoted identifiers
        // For maximum compatibility, use partial index only if needed
        builder.HasIndex(e => new { e.Status, e.NextAttemptOn })
               .HasDatabaseName("IX_IntegrationEvent_Status_NextAttemptOn");

        // Index for claimed events lookup
        // Query pattern: WHERE ClaimId = @claimId
        builder.HasIndex(e => e.ClaimId)
               .HasDatabaseName("IX_IntegrationEvent_ClaimId");

        // Composite index for outbox polling pattern with ordering
        // Query pattern: WHERE Status IN ('PENDING', 'ONERROR') ORDER BY Sequence
        builder.HasIndex(e => new { e.Status, e.NextAttemptOn, e.Sequence })
               .HasDatabaseName("IX_IntegrationEvent_Processing");

        // Index for retry mechanism (find failed events to retry)
        // Query pattern: WHERE Status = 'ONERROR' AND NextAttemptOn <= NOW
        builder.HasIndex(e => new { e.Status, e.AttemptCount, e.NextAttemptOn })
               .HasDatabaseName("IX_IntegrationEvent_Retry");
    }

}
