
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

using Xpandables.Net.Aggregates.Configurations;
using Xpandables.Net.Repositories;

namespace Xpandables.Net.Aggregates;

/// <summary>
/// Provides the base events db context.
/// </summary>
/// <remarks>
/// Initializes a new instance of <see cref="EventDataContext"/> 
/// with the specified configuration.
/// </remarks>
/// <param name="contextOptions">The configuration to be applied.</param>
/// <exception cref="ArgumentNullException">The 
/// <paramref name="contextOptions"/> is null.</exception>
public sealed class EventDataContext(
    DbContextOptions<EventDataContext> contextOptions)
    : DataContext(contextOptions)
{
    ///<inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        _ = modelBuilder
            .ApplyConfiguration(new EventEntityDomainTypeConfiguration());
        _ = modelBuilder
            .ApplyConfiguration(new EventEntityIntegrationTypeConfiguration());
        _ = modelBuilder
            .ApplyConfiguration(new EventEntitySnapShotTypeConfiguration());

        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// A collection of <see cref="EventEntityDomain"/> entities.
    /// </summary>
    public DbSet<EventEntityDomain> Domains { get; set; }

    /// <summary>
    /// A collection of <see cref="EventEntityIntegration"/> entities.
    /// </summary>
    public DbSet<EventEntityIntegration> Integrations { get; set; }

    /// <summary>
    /// A collection of <see cref="EventEntitySnapshot"/> entities.
    /// </summary>
    public DbSet<EventEntitySnapshot> Snapshots { get; set; }
}
