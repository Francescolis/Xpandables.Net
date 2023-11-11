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
    protected Entity()
    {
        OnCreated += (s, e) => CreatedOn = DateTime.UtcNow;
        OnUpdated += (s, e) => UpdatedOn = DateTime.UtcNow;
        OnDeleted += (s, e) => DeletedOn = DateTime.UtcNow;
    }

    /// <summary>
    /// Occurs when the underlying instance is created.
    /// </summary>
    public event EventHandler? OnCreated;

    /// <summary>
    /// Occurs when the underlying instance is deleted.
    /// </summary>
    public event EventHandler? OnDeleted;

    /// <summary>
    /// Occurs when the underlying instance is updated.
    /// </summary>
    public event EventHandler? OnUpdated;

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
    }

    /// <inheritdoc/>
    public void SetStatusInactive()
    {
        Status = EntityStatus.INACTIVE;
        DeletedOn = DateTime.UtcNow;
    }

    /// <inheritdoc/>
    public void SetStatusSuspended()
    {
        Status = EntityStatus.SUSPENDED;
        UpdatedOn = DateTime.UtcNow;
    }

    /// <inheritdoc/>
    public void SetStatusDeleted()
    {
        Status = EntityStatus.DELETED;
        DeletedOn = DateTime.UtcNow;
    }

    void IEntity.OnCreation() => OnCreated?.Invoke(this, EventArgs.Empty);
    void IEntity.OnDeletion() => OnDeleted?.Invoke(this, EventArgs.Empty);
    void IEntity.OnUpdate() => OnUpdated?.Invoke(this, EventArgs.Empty);
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