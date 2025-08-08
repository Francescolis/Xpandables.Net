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

using Xpandables.Net.Repositories;

namespace Xpandables.Net.Executions.Domains;

/// <summary>
/// Provides configuration for the <see cref="EntityDomainEvent" /> entity type.
/// </summary>
public sealed class EntityDomainEventTypeConfiguration : IEntityTypeConfiguration<EntityDomainEvent>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<EntityDomainEvent> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        _ = builder.HasKey(e => e.KeyId);
        _ = builder.HasIndex(e => new { e.KeyId, e.AggregateId, e.Name, e.Version });
        _ = builder.Property(e => e.KeyId).IsRequired();
        _ = builder.Property(e => e.AggregateId).IsRequired();
        _ = builder.Property(e => e.AggregateName).IsRequired().HasMaxLength(500);
        _ = builder.Property(e => e.Name).IsRequired().HasMaxLength(100);
        _ = builder.Property(e => e.FullName).IsRequired().HasMaxLength(byte.MaxValue);
        _ = builder.Property(e => e.Version).IsRequired();
        _ = builder.Property(e => e.Data).IsRequired();
        _ = builder.Property(e => e.Status).IsRequired().HasMaxLength(50);
        _ = builder.Property(e => e.CreatedOn).IsRequired();
        _ = builder.Property(e => e.UpdatedOn).IsRequired(false);
        _ = builder.Property(e => e.DeletedOn).IsRequired(false);

        _ = builder.HasQueryFilter(e => e.Status != EntityStatus.DELETED.Value);
    }
}