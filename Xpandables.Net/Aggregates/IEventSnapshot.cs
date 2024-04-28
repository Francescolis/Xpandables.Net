/*******************************************************************************
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
********************************************************************************/
using System.ComponentModel.DataAnnotations;

namespace Xpandables.Net.Aggregates;

/// <summary>
/// Defines a marker interface to be used to mark an object to act
/// as a snapshot.
/// </summary>
public interface IEventSnapshot : IEvent
{
    /// <summary>
    /// Gets the memento of the snapshot.
    /// </summary>
    IMemento Memento { get; }

    /// <summary>
    /// Gets the object identifier.
    /// </summary>
    [Key]
    Guid ObjectId { get; }

    /// <summary>
    /// Contains the string representation of the .Net entity type name.
    /// </summary>
    string EntityTypeName { get; }

    /// <summary>
    /// Contains the string representation of the .Net entity 
    /// full assembly qualified type name.
    /// </summary>
    string EntityTypeFullName { get; }
}