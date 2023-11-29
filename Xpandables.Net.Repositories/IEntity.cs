
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
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Xpandables.Net.Repositories;

/// <summary>
/// The object base entity interface that provides a state for implementation classes.
/// </summary>
public interface IEntity
{
    /// <summary>
    /// Gets the object unique identity.
    /// </summary>
    object Id { get; }

    /// <summary>
    /// Get a value indicating the state of the underlying instance.
    /// </summary>
    string Status { get; }

    /// <summary>
    /// Gets the creation date of the underlying instance.
    /// </summary>
    DateTime CreatedOn { get; }

    /// <summary>
    /// Gets the last update date of the underlying instance if exist.
    /// </summary>
    DateTime? UpdatedOn { get; }

    /// <summary>
    /// Gets the inactive date of the underlying instance if exist.
    /// </summary>
    DateTime? DeletedOn { get; }

    /// <summary>
    /// Gets a value indicating whether or not the 
    /// underlying instance is marked as <see cref="EntityStatus.ACTIVE"/>.
    /// </summary>
    public bool IsActive => Status == EntityStatus.ACTIVE;

    /// <summary>
    /// Gets a value indicating whether or not the 
    /// underlying instance is marked as <see cref="EntityStatus.INACTIVE"/>.
    /// </summary>
    public bool IsInactive => Status == EntityStatus.INACTIVE;

    /// <summary>
    /// Gets a value indicating whether or not the 
    /// underlying instance is marked as <see cref="EntityStatus.SUSPENDED"/>.
    /// </summary>
    public bool IsSuspended => Status == EntityStatus.SUSPENDED;

    /// <summary>
    /// Gets a value indicating whether or not the 
    /// underlying instance is marked as <see cref="EntityStatus.DELETED"/>.
    /// </summary>
    public bool IsDeleted => Status == EntityStatus.DELETED;

    /// <summary>
    /// Sets the creation date time.
    /// </summary>
    void SetCreatedOn();

    /// <summary>
    /// Sets the update date time.
    /// </summary>
    void SetUpdatedOn();

    /// <summary>
    /// Sets the deleted date time.
    /// </summary>
    void SetDeletedOn();

    /// <summary>
    /// Marks the underlying instance as active and sets the update date time.
    /// </summary>
    void SetStatusActive();

    /// <summary>
    /// Marks the underlying instance as inactive and sets the update date time.
    /// </summary>
    void SetStatusInactive();

    /// <summary>
    /// Marks the underlying instance as suspended and sets the update date time.
    /// </summary>
    void SetStatusSuspended();

    /// <summary>
    /// Marks the underlying instance as deleted and sets the deleted date time.
    /// </summary>
    void SetStatusDeleted();
}

/// <summary>
/// The generic object entity base interface that provides a specific state for implementation classes.
/// </summary>
/// <typeparam name="TId">The type of the id for the entity.</typeparam>
public interface IEntity<TId> : IEntity
    where TId : notnull, IComparable
{
    /// <summary>
    /// Gets the object unique identity.
    /// </summary>
    new TId Id { get; }

    [JsonIgnore, EditorBrowsable(EditorBrowsableState.Never)]
    object IEntity.Id => Id;
}

