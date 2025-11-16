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
using System.Text.Json;

namespace Xpandables.Net.EventSourcing.Repositories;

/// <summary>
/// Represents an event associated with an entity, providing access to event metadata and data payload.
/// </summary>
/// <remarks>Implementations of this interface expose information about an entity event, including its name, fully
/// qualified name, sequence number, and associated data. The interface inherits from <see cref="IDisposable"/>,
/// indicating that resources associated with the event may need to be released when the event is no longer
/// needed.</remarks>
public interface IEntityEvent : IDisposable
{
    /// <summary>
    /// Gets the object unique identity.
    /// </summary>
    Guid KeyId { get; }

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

    /// <summary>
    /// Gets the name of the event associated with the current instance.
    /// </summary>
    string EventName { get; }

    /// <summary>
    /// Gets the current sequence number associated with the instance, which is used to track the order of events.
    /// </summary>
    long Sequence { get; }

    /// <summary>
    /// Gets the JSON data associated with this instance.
    /// </summary>
    JsonDocument EventData { get; }
}
