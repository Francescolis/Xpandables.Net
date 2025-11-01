/*******************************************************************************
 * Copyright (C) 2025 Kamersoft
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
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

using Xpandables.Net.Events.Repositories;

namespace Xpandables.Net.Events;

/// <summary>
/// Represents a custom database context that extends <see cref="DbContext"/> to manage entity tracking and state
/// changes.
/// </summary>
/// <remarks>The <see cref="EventDataContext"/> class provides functionality to initialize a database context with
/// default or specified options. It includes event handling for entity tracking and state changes, ensuring that
/// entities are correctly managed when added or modified. This class is suitable for scenarios where custom behavior is
/// needed during entity lifecycle events.</remarks>
[RequiresUnreferencedCode("This context may be used with unreferenced code.")]
[RequiresDynamicCode("This context may be used with dynamic code.")]
[EditorBrowsable(EditorBrowsableState.Never)]
public class EventDataContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EventDataContext"/> class with default options.
    /// </summary>
    /// <remarks>This constructor is useful for scenarios where no specific <see cref="DbContextOptions"/> are
    /// provided. It initializes the context with default settings, which may be suitable for simple use cases or
    /// testing. The <see cref="DbContext.OnConfiguring(DbContextOptionsBuilder)"/> method will be called to configure 
    /// the database (and other options) to be used for this context.</remarks>
    [RequiresDynamicCode("The context requires dynamic code generation.")]
    [RequiresUnreferencedCode("The context may be trimmed.")]
    protected EventDataContext() : this(new DbContextOptions<EventDataContext>())
    {
        // Default constructor initializes the context with default options.
        // This is useful for scenarios where no specific options are provided.
    }

    /// <summary>  
    /// Initializes a new instance of the <see cref="EventDataContext"/> class 
    /// using the specified options.  
    /// </summary>  
    /// <param name="options">The options to be used by a 
    /// <see cref="DbContext"/>.</param>  
    [RequiresDynamicCode("The context requires dynamic code generation.")]
    [RequiresUnreferencedCode("The context may be trimmed.")]
    protected EventDataContext(DbContextOptions options) : base(options)
    {
        ChangeTracker.Tracked += static (sender, e) => OnEntityTracked(e);
        ChangeTracker.StateChanged += static (sender, e) => OnEntityStateChanged(e);
    }

    private static void OnEntityTracked(EntityTrackedEventArgs e)
    {
        if (e is { FromQuery: false, Entry: { State: EntityState.Added, Entity: IEntityEvent addedEntity } })
        {
            addedEntity.CreatedOn = DateTime.UtcNow;
        }
    }

    private static void OnEntityStateChanged(EntityStateChangedEventArgs e)
    {
        if (e is { NewState: EntityState.Modified, Entry.Entity: IEntityEvent entity })
        {
            entity.UpdatedOn = DateTime.UtcNow;
        }

        if (e is { NewState: EntityState.Deleted, Entry.Entity: IEntityEvent deletedEntity })
        {
            deletedEntity.DeletedOn = DateTime.UtcNow;
            deletedEntity.Status = EventStatus.DELETED;
        }
    }

    /// <summary>
    /// Releases the resources used by the current instance of <see cref="EventDataContext"/>
    /// and unsubscribes from the tracked and state changed events to prevent memory leaks.
    /// </summary>
    public override void Dispose()
    {
        // Unsubscribe from events to prevent memory leaks
        ChangeTracker.Tracked -= static (sender, e) => OnEntityTracked(e);
        ChangeTracker.StateChanged -= static (sender, e) => OnEntityStateChanged(e);

        base.Dispose();

        GC.SuppressFinalize(this);
    }
}