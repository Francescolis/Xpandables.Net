
/*******************************************************************************
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
********************************************************************************/
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Xpandables.Net.Repositories;

namespace Xpandables.Net.Aggregates.Configurations;

/// <summary>
/// Defines the <see cref="EventEntityNotification"/> configuration.
/// </summary>
public sealed class EventEntityNotificationTypeConfiguration
    : IEntityTypeConfiguration<EventEntityNotification>
{
    ///<inheritdoc/>
    public void Configure(EntityTypeBuilder<EventEntityNotification> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        _ = builder.Property(p => p.Id).IsRequired();
        _ = builder.HasKey(p => p.Id);
        _ = builder.HasIndex(p => p.Id).IsUnique();

        _ = builder.Property(p => p.Data);
        _ = builder.Property(p => p.EventTypeFullName);
        _ = builder.Property(p => p.EventTypeName);
        _ = builder.Property(p => p.Version);
        _ = builder.Property(p => p.Status);
        _ = builder.Property(p => p.ErrorMessage);

        _ = builder.HasQueryFilter(f => f.Status == EntityStatus.ACTIVE);
    }
}
