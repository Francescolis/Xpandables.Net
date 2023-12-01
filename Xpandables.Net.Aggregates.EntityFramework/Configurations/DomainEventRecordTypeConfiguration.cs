
/************************************************************************************************************
 * Copyright (C) 2023 Francis-Black EWANE
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
************************************************************************************************************/
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Xpandables.Net.Repositories;

namespace Xpandables.Net.Aggregates.EntityFramework.Configurations;

/// <summary>
/// Defines the base <see cref="DomainEventRecord"/> configuration.
/// </summary>
public sealed class DomainEventRecordTypeConfiguration : IEntityTypeConfiguration<DomainEventRecord>
{
    ///<inheritdoc/>
    public void Configure(EntityTypeBuilder<DomainEventRecord> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Property(p => p.Id).IsRequired();
        builder.HasKey(p => p.Id);
        builder.HasIndex(p => new { p.Id, p.EventTypeName, p.Version }).IsUnique();

        builder.Property(p => p.Id);
        builder.Property(p => p.AggregateIdTypeName);
        builder.Property(p => p.AggregateId);
        builder.Property(p => p.Data);
        builder.Property(p => p.EventTypeFullName);
        builder.Property(p => p.EventTypeName);
        builder.Property(p => p.Version);

        builder.HasQueryFilter(f => f.Status == EntityStatus.ACTIVE);
    }
}
