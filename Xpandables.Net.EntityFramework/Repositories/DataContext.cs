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
using Microsoft.EntityFrameworkCore.ChangeTracking;

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
        // ReSharper disable once VirtualMemberCallInConstructor
        ChangeTracker.Tracked += static (sender, e) => OnEntityTracked(e);
        // ReSharper disable once VirtualMemberCallInConstructor
        ChangeTracker.StateChanged += static (sender, e) => OnEntityStateChanged(e);
    }

    private static void OnEntityTracked(EntityTrackedEventArgs e)
    {
        if (e is { FromQuery: false, Entry: { State: EntityState.Added, Entity: IEntity entity } }
            && entity.Status != EntityStatus.ACTIVE)
        {
            entity.SetStatus(entity.Status);
        }
    }

    private static void OnEntityStateChanged(EntityStateChangedEventArgs e)
    {
        if (e is { NewState: EntityState.Modified, Entry.Entity: IEntity entity })
        {
            entity.SetUpdatedOn();
        }
    }

    /// <summary>
    /// Releases the resources used by the current instance of <see cref="DataContext"/>
    /// and unsubscribes from the tracked and state changed events to prevent memory leaks.
    /// </summary>
    public override void Dispose()
    {
        // Unsubscribe from events to prevent memory leaks
        // ReSharper disable once EventUnsubscriptionViaAnonymousDelegate
        ChangeTracker.Tracked -= static (sender, e) => OnEntityTracked(e);
        // ReSharper disable once EventUnsubscriptionViaAnonymousDelegate
        ChangeTracker.StateChanged -= static (sender, e) => OnEntityStateChanged(e);

        base.Dispose();
        
        GC.SuppressFinalize(this);
    }
}