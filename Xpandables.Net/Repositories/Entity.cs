
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
    public DateTime CreatedOn { get; protected internal set; } = DateTime.UtcNow;

    ///<inheritdoc/>
    public DateTime? UpdatedOn { get; protected set; }

    ///<inheritdoc/>
    public DateTime? DeletedOn { get; protected set; }

    /// <inheritdoc/>
    public virtual void SetStatus(string status)
    {
        _ = status ?? throw new ArgumentNullException(nameof(status));

        // if the status is updated, we just update the updated on date
        if (status.Equals(EntityStatus.UPDATED, StringComparison.OrdinalIgnoreCase))
        {
            UpdatedOn = DateTime.UtcNow;
            return;
        }

        Status = status;

        if (status.Equals(EntityStatus.ACTIVE, StringComparison.OrdinalIgnoreCase))
        {
            if (UpdatedOn is not null)
                UpdatedOn = DateTime.UtcNow;
        }
        else
        {
            UpdatedOn = DateTime.UtcNow;
        }

        if (!status.Equals(EntityStatus.DELETED, StringComparison.OrdinalIgnoreCase))
        {
            if (DeletedOn is not null)
                DeletedOn = null;
        }
        else
        {
            DeletedOn = DateTime.UtcNow;
        }
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