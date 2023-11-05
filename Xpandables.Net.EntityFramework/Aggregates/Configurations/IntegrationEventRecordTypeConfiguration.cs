﻿
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

using Xpandables.Net.Aggregates.Defaults;
using Xpandables.Net.Converters;
using Xpandables.Net.Repositories;

namespace Xpandables.Net.Aggregates.Configurations;

/// <summary>
/// Defines the <see cref="IntegrationEventRecord"/> configuration.
/// </summary>
public sealed class IntegrationEventRecordTypeConfiguration : IEntityTypeConfiguration<IntegrationEventRecord>
{
    ///<inheritdoc/>
    public void Configure(EntityTypeBuilder<IntegrationEventRecord> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Property(p => p.Id).IsRequired();
        builder.HasKey(p => p.Id);
        builder.HasIndex(p => p.Id).IsUnique();

        builder.Property(p => p.Data).HasJsonDocumentConversion();
        builder.Property(p => p.TypeFullName);

        builder.HasQueryFilter(f => f.Status != EntityStatus.INACTIVE);
    }
}
