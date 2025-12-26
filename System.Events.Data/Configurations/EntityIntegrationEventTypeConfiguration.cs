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
        builder.ToTable("IntegrationEvents", "Events");

        builder.Property(e => e.ClaimId).IsRequired(false);
        builder.Property(e => e.AttemptCount).HasDefaultValue(0);
        builder.Property(e => e.NextAttemptOn).IsRequired(false);
        builder.Property(e => e.ErrorMessage).IsRequired(false);
        builder.Property(e => e.CausationId);
        builder.Property(e => e.CorrelationId);

        builder.Property(e => e.UpdatedOn)
               .IsConcurrencyToken();

        builder.HasIndex(e => new { e.Status, e.NextAttemptOn })
               .HasDatabaseName("IX_IntegrationEvent_Status_NextAttemptOn");

        builder.HasIndex(e => e.ClaimId)
               .HasDatabaseName("IX_IntegrationEvent_ClaimId");

        builder.HasIndex(e => new { e.Status, e.NextAttemptOn, e.Sequence })
               .HasDatabaseName("IX_IntegrationEvent_Processing");

        builder.HasIndex(e => new { e.Status, e.AttemptCount, e.NextAttemptOn })
               .HasDatabaseName("IX_IntegrationEvent_Retry");
    }
}
