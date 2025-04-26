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

using Xpandables.Net.Executions.Domains;

namespace Xpandables.Net.Repositories;

/// <summary>
/// Represents the data context for handling event entities.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="DataContextEvent" /> class.
/// </remarks>
/// <param name="options">The options to be used by a <see cref="DbContext" />.</param>
public sealed class DataContextEvent(
    DbContextOptions<DataContextEvent> options) :
    DataContext(options)
{
    /// <summary>
    /// Gets or sets the DbSet for EventEntityDomain.
    /// </summary>
    public DbSet<EntityDomainEvent> Domains { get; set; } = default!;

    /// <summary>
    /// Gets or sets the DbSet for EventEntityIntegration.
    /// </summary>
    public DbSet<EntityIntegrationEvent> Integrations { get; set; } = default!;

    /// <summary>
    /// Gets or sets the DbSet for EventEntitySnapshot.
    /// </summary>
    public DbSet<EntitySnapshotEvent> Snapshots { get; set; } = default!;

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        _ = modelBuilder.ApplyConfiguration(new EventEntityDomainConfiguration());
        _ = modelBuilder.ApplyConfiguration(new EventEntityIntegrationConfiguration());
        _ = modelBuilder.ApplyConfiguration(new EventEntitySnapshotConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}