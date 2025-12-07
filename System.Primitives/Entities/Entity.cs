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
using System.ComponentModel.DataAnnotations;

namespace System.Entities;

/// <summary>
/// Represents an abstract base class for entities with a specific key type
/// that are used in relational repositories.
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
public abstract class Entity<TKey> : IEntity<TKey>
    where TKey : notnull, IComparable
{
    /// <summary>
    /// Initializes a new instance of <see cref="Entity{TKey}" />.
    /// </summary>
    protected Entity() { }

    /// <inheritdoc />
    [EntityStatusValidation(allowCustomStatuses: true)]
    public string Status { get; set; } = EntityStatus.ACTIVE;

    /// <inheritdoc />
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    /// <inheritdoc />
    public DateTime? UpdatedOn { get; set; }

    /// <inheritdoc />
    public DateTime? DeletedOn { get; set; }

    /// <inheritdoc/>
    public bool IsDeleted => Status == EntityStatus.DELETED;

    /// <inheritdoc />
    [Key]
    public required TKey KeyId { get; init; }

    /// <inheritdoc />
    public void SetStatus(string status)
    {
        Status = status;
        DeletedOn = status == EntityStatus.DELETED
            ? DateTime.UtcNow
            : null;
    }
}