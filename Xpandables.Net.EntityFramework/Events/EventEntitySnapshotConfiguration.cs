﻿
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

namespace Xpandables.Net.Events;

/// <summary>
/// Configuration class for the <see cref="EventEntitySnapshot"/> entity.
/// </summary>
public sealed class EventEntitySnapshotConfiguration :
    IEntityTypeConfiguration<EventEntitySnapshot>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<EventEntitySnapshot> builder)
    {
        _ = builder.HasKey(p => p.Id);
        _ = builder.HasIndex(e => new { e.Id, e.OwnerId, e.EventName, e.EventVersion });
        _ = builder.Property(p => p.EventData).IsRequired();
        _ = builder.Property(p => p.EventName).IsRequired();
        _ = builder.Property(p => p.EventFullName).IsRequired();
        _ = builder.Property(p => p.EventVersion).IsRequired();
        _ = builder.Property(e => e.Status).IsRequired().HasMaxLength(50);
        _ = builder.Property(e => e.CreatedOn).IsRequired();
        _ = builder.Property(e => e.UpdatedOn).IsRequired(false);
        _ = builder.Property(e => e.DeletedOn).IsRequired(false);

        _ = builder.HasQueryFilter(e => e.Status != EntityStatus.DELETED);
    }
}
