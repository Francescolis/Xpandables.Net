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
namespace Xpandables.Net.Repositories;

/// <summary>  
/// Represents an entity that has relational properties and status tracking.  
/// </summary>  
public interface IEntityRelational : IEntity
{
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
    /// Gets the deletion date of the underlying instance if exist.  
    /// </summary>  
    DateTime? DeletedOn { get; }

    /// <summary>  
    /// Sets the status of the underlying instance.  
    /// </summary>  
    void SetStatus(string status);
}

/// <summary>
/// Represents an entity that has relational properties and status tracking 
/// with a specific key type.
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
public interface IEntityRelational<out TKey> : IEntity<TKey>, IEntityRelational
    where TKey : notnull, IComparable
{ }