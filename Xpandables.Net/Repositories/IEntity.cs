﻿/*******************************************************************************
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
/// Represents an entity with a unique identity.
/// </summary>
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
    /// Sets the status of the underlying instance.  
    /// </summary>  
    void SetStatus(string status);
}

/// <summary>
/// Represents an entity with a unique identity of type <typeparamref name="TKey"/>.
/// </summary>
/// <typeparam name="TKey">The type of the unique identity.</typeparam>
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
