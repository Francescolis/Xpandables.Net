
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

namespace Xpandables.Net.Repositories;

/// <summary>
/// This is the <see langword="abstract"/> db context class that inherits from <see cref="DbContext"/>.
/// </summary>
public abstract class DataContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataContext"/> class
    /// using the specified options. The <see cref="DbContext.OnConfiguring(DbContextOptionsBuilder)"/>
    /// method will still be called to allow further configuration of the options.
    /// </summary>
    /// <param name="contextOptions">The options for this context.</param>
    protected DataContext(DbContextOptions contextOptions)
        : base(contextOptions)
    {
        ChangeTracker.Tracked += (sender, e) =>
        {
            if (e.FromQuery || e.Entry.State != EntityState.Added || e.Entry.Entity is not IEntity entity) return;

            entity.OnCreation();
        };

        ChangeTracker.StateChanged += (sender, e) =>
        {
            if (e.NewState != EntityState.Modified || e.Entry.Entity is not IEntity entity) return;

            entity.OnUpdate();

            if (entity.IsInactive)
                entity.OnDeletion();
        };
    }
}
