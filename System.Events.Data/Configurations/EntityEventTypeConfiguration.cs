
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
using System.Diagnostics.CodeAnalysis;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace System.Events.Data.Configurations;

/// <summary>
/// Base class for configuring entity event types in Entity Framework.
/// </summary>
/// <typeparam name="TEntityEvent">The type of the entity event.</typeparam>
public abstract class EntityEventTypeConfiguration<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes)] TEntityEvent> :
    IEntityTypeConfiguration<TEntityEvent>
    where TEntityEvent : EntityEvent
{
    /// <inheritdoc />
    public virtual void Configure(EntityTypeBuilder<TEntityEvent> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        _ = builder.HasKey(e => e.KeyId);
        _ = builder.Property(e => e.KeyId).IsRequired();
        _ = builder.Property(e => e.Sequence).ValueGeneratedOnAdd();
        _ = builder.Property(e => e.EventName).IsRequired().HasMaxLength(byte.MaxValue);
        _ = builder.Property(e => e.EventData).IsRequired();
        _ = builder.Property(e => e.Status).IsRequired().HasMaxLength(byte.MaxValue);
        _ = builder.Property(e => e.CreatedOn).IsRequired();
        _ = builder.Property(e => e.UpdatedOn).IsRequired(false);
        _ = builder.Property(e => e.DeletedOn).IsRequired(false);

        _ = builder.HasQueryFilter(e => e.Status != EventStatus.DELETED);

        // Global ordering/reads
        _ = builder.HasIndex(e => e.Sequence);
    }

    internal const DynamicallyAccessedMemberTypes DynamicallyAccessedMemberTypes =
         System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicConstructors
         | System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.NonPublicConstructors
         | System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicProperties
         | System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicFields
         | System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.NonPublicProperties
         | System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.NonPublicFields
         | System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.Interfaces;
}