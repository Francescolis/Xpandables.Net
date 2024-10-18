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
namespace Xpandables.Net.Events;

/// <summary>
/// Represents a snapshot of an event entity with a specified key type.
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
public abstract class EventEntitySnapshot<TKey> :
    EventEntity<TKey>,
    IEventEntitySnapshot<TKey>
    where TKey : notnull, IComparable
{
    /// <summary>
    /// Gets the owner of the event entity snapshot.
    /// </summary>
    public required string OwnerId { get; init; }
}
