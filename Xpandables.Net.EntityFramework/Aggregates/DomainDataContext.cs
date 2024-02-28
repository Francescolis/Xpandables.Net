
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

using Xpandables.Net.Aggregates.Configurations;
using Xpandables.Net.Aggregates.DomainEvents;
using Xpandables.Net.Aggregates.Notifications;
using Xpandables.Net.Repositories;
using Xpandables.Net.SnapShots;

namespace Xpandables.Net.Aggregates;

/// <summary>
/// Provides the base domain db context.
/// </summary>
/// <remarks>
/// Initializes a new instance of <see cref="DomainDataContext"/> with the specified configuration.
/// </remarks>
/// <param name="contextOptions">The configuration to be applied.</param>
/// <exception cref="ArgumentNullException">The <paramref name="contextOptions"/> is null.</exception>
public sealed class DomainDataContext(DbContextOptions<DomainDataContext> contextOptions)
    : DataContext(contextOptions)
{
    ///<inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        _ = modelBuilder.ApplyConfiguration(new EntityDomainEventTypeConfiguration());
        _ = modelBuilder.ApplyConfiguration(new EntityNotificationTypeConfiguration());
        _ = modelBuilder.ApplyConfiguration(new EntitySnapShotTypeConfiguration());

        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// A collection of <see cref="EntityDomainEvent"/> entities.
    /// </summary>
    public DbSet<EntityDomainEvent> Events { get; set; }

    /// <summary>
    /// A collection of <see cref="EntityNotification"/> entities.
    /// </summary>
    public DbSet<EntityNotification> Notifications { get; set; }

    /// <summary>
    /// A collection of <see cref="EntitySnapShot"/> entities.
    /// </summary>
    public DbSet<EntitySnapShot> SnapShots { get; set; }
}
