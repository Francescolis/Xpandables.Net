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

using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Xpandables.Net.Repositories;

namespace Xpandables.Net.Executions.Domains;

/// <summary>
/// Provides configuration for the <see cref="EntityDomainEvent" /> entity type.
/// </summary>
public sealed class EntityDomainEventTypeConfiguration : EntityEventTypeConfiguration<EntityDomainEvent>
{
    /// <inheritdoc />
    public sealed override void Configure(EntityTypeBuilder<EntityDomainEvent> builder)
    {
        base.Configure(builder);

        _ = builder.HasIndex(e => new { e.KeyId, e.AggregateId, e.Name, e.Version });
        _ = builder.Property(e => e.AggregateId).IsRequired();
        _ = builder.Property(e => e.AggregateName).IsRequired().HasMaxLength(short.MaxValue / 8);
    }
}
