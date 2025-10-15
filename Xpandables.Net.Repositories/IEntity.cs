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
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Xpandables.Net.Repositories;

/// <summary>
/// Represents an entity with a unique identifier and lifecycle metadata.
/// </summary>
/// <remarks>The <c>IEntity</c> interface defines the basic properties and methods for an entity,  including its
/// unique key, status, and timestamps for creation, updates, and deletion.  Implementations of this interface can be
/// used to track the state and history of an entity  within a system.</remarks>
public interface IEntity
{
    /// <summary>
    /// Gets the object unique identity.
    /// </summary>
    object KeyId { get; }

    /// <summary>  
    /// Get a value indicating the state of the underlying instance.  
    /// </summary>  
    string Status { get; set; }

    /// <summary>  
    /// Gets the creation date of the underlying instance.  
    /// </summary>  
    DateTime CreatedOn { get; set; }

    /// <summary>  
    /// Gets the last update date of the underlying instance if exist.  
    /// </summary>  
    DateTime? UpdatedOn { get; set; }

    /// <summary>  
    /// Gets the deletion date of the underlying instance if exist.  
    /// </summary>  
    DateTime? DeletedOn { get; set; }

    /// <summary>
    /// Gets a value indicating whether the entity has been marked as deleted.
    /// </summary>
    bool IsDeleted { get; }

    /// <summary>  
    /// Sets the status of the underlying instance.  
    /// </summary>  
    void SetStatus(string status);
}

/// <summary>
/// Represents an entity with a unique key identifier.
/// </summary>
/// <typeparam name="TKey">The type of the key identifier, 
/// which must be non-nullable and implement <see cref="IComparable"/>.</typeparam>
public interface IEntity<out TKey> : IEntity
    where TKey : notnull, IComparable
{
    /// <summary>
    /// Gets the specific unique identity.
    /// </summary>
    new TKey KeyId { get; }

    [EditorBrowsable(EditorBrowsableState.Never), JsonIgnore]
    object IEntity.KeyId => KeyId;
}
