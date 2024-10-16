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
using System.ComponentModel.DataAnnotations;

namespace Xpandables.Net.Repositories;

/// <summary>
/// Represents an abstract base class for entities with a specific key type 
/// that are used in relational repositories.
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <typeparam name="TTimeStamp">The type of the timestamp.</typeparam>
public abstract class Entity<TKey, TTimeStamp> : IEntity<TKey, TTimeStamp>
    where TKey : notnull, IComparable
    where TTimeStamp : notnull, IComparable
{
    /// <summary>
    /// Initializes a new instance of <see cref="Entity{TKey, TVersion}"/>.
    /// </summary>
    protected Entity() { }

    /// <inheritdoc/>  
    public string Status { get; protected set; } = EntityStatus.ACTIVE;

    /// <inheritdoc/>  
    public DateTime CreatedOn { get; protected set; } = DateTime.UtcNow;

    /// <inheritdoc/>  
    public DateTime? UpdatedOn { get; protected set; }

    /// <inheritdoc/>  
    public DateTime? DeletedOn { get; protected set; }

    /// <inheritdoc/>  
    [Key]
    public TKey Id { get; protected set; } = default!;

    /// <inheritdoc/>
    [ConcurrencyCheck]
    public TTimeStamp Timestamp { get; protected set; } = default!;

    /// <inheritdoc/>  
    public void SetStatus(string status)
    {
        Status = status;
        DeletedOn = status switch
        {
            EntityStatus.DELETED => (DateTime?)DateTime.UtcNow,
            _ => null,
        };

        UpdatedOn = DateTime.UtcNow;
    }
}