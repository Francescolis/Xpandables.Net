
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

using Xpandables.Net.Events.Configurations;
using Xpandables.Net.Events.Repositories;

namespace Xpandables.Net.Events;

/// <summary>
/// Represents the data context for handling event entities.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="EventStoreDataContext" /> class.
/// </remarks>
/// <param name="options">The options to be used by a <see cref="DbContext" />.</param>
[RequiresUnreferencedCode("This context may be used with unreferenced code.")]
[RequiresDynamicCode("This context may be used with dynamic code.")]
public sealed class EventStoreDataContext(DbContextOptions<EventStoreDataContext> options) : EventDataContext(options)
{
    /// <summary>
    /// Gets or sets the DbSet for EventEntityDomain.
    /// </summary>
    public DbSet<EntityDomainEvent> Domains { get; set; } = null!;

    /// <summary>
    /// Gets or sets the DbSet for EventEntitySnapshot.
    /// </summary>
    public DbSet<EntitySnapshotEvent> Snapshots { get; set; } = null!;

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema("Events");

        _ = modelBuilder.ApplyConfiguration(new EntityDomainEventTypeConfiguration());
        _ = modelBuilder.ApplyConfiguration(new EntitySnapShotEventTypeConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}