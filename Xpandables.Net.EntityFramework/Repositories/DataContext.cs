
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

namespace Xpandables.Net.Repositories;

/// <summary>  
/// Represents the base class for the Entity Framework Core data context.  
/// </summary>  
public abstract class DataContext : DbContext
{
    /// <summary>  
    /// Initializes a new instance of the <see cref="DataContext"/> class 
    /// using the specified options.  
    /// </summary>  
    /// <param name="options">The options to be used by a 
    /// <see cref="DbContext"/>.</param>  
    protected DataContext(DbContextOptions options) : base(options)
    {
        ChangeTracker.Tracked += static (sender, e) =>
        {
            if (!e.FromQuery
                && e.Entry.State == EntityState.Added
                && e.Entry.Entity is IEntity entity)
            {
                if (entity.Status is null)
                {
                    entity.SetStatus(EntityStatus.ACTIVE);
                }
            }
        };

        ChangeTracker.StateChanged += static (sender, e) =>
        {
            if (e.NewState == EntityState.Modified
                && e.Entry.Entity is IEntity entity)
            {
                entity.SetUpdatedOn();
            }
        };
    }
}
