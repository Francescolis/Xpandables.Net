
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
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace Xpandables.Net.Repositories;

/// <summary>
/// The object entity base implementation that provides an identifier for derived class.
/// </summary>
[DebuggerDisplay("Id = {" + nameof(Id) + "}")]
public abstract class Entity : IEntity
{
    /// <summary>
    /// Initializes a new instance of <see cref="Entity"/>.
    /// </summary>
    protected Entity() { }

    ///<inheritdoc/>
    [Key]
    public object Id { get; protected set; } = default!;

    ///<inheritdoc/>
    public string Status { get; protected set; } = EntityStatus.ACTIVE;

    ///<inheritdoc/>
    ///<remarks>This property is automatically set by the constructor.</remarks>
    public DateTime CreatedOn { get; protected set; } = DateTime.UtcNow;

    ///<inheritdoc/>
    ///<remarks>This property is automatically 
    ///set by the <see cref="IEntity.SetStatusActive"/> method.</remarks>
    public DateTime? UpdatedOn { get; protected set; }

    ///<inheritdoc/>
    /// <remarks>This property is automatically 
    /// set by the <see cref="IEntity.SetStatusInactive"/> method.</remarks>
    public DateTime? DeletedOn { get; protected set; }

    /// <inheritdoc/>
    public void SetStatusActive()
    {
        Status = EntityStatus.ACTIVE;
        UpdatedOn = DateTime.UtcNow;
        DeletedOn = null;
    }

    /// <inheritdoc/>
    public void SetStatusInactive()
    {
        Status = EntityStatus.INACTIVE;
        UpdatedOn = DateTime.UtcNow;
        DeletedOn = null;
    }

    /// <inheritdoc/>
    public void SetStatusSuspended()
    {
        Status = EntityStatus.SUSPENDED;
        UpdatedOn = DateTime.UtcNow;
        DeletedOn = null;
    }

    /// <inheritdoc/>
    public void SetStatusDeleted()
    {
        Status = EntityStatus.DELETED;
        UpdatedOn = DateTime.UtcNow;
        DeletedOn = DateTime.UtcNow;
    }
}

/// <summary>
/// The generic entity object base implementation that provides a specific key for derived class.
/// </summary>
/// <typeparam name="TId">The type of the key for the entity.</typeparam>
[DebuggerDisplay("Id = {" + nameof(Id) + "}")]
public abstract class Entity<TId> : Entity, IEntity<TId>
    where TId : notnull, IComparable
{
    /// <summary>
    /// Initializes a new instance of <see cref="Entity{TKey}"/>.
    /// </summary>
    protected Entity() { }

    ///<inheritdoc/>
    [Key]
    public new TId Id { get; protected set; } = default!;
}