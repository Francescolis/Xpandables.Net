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
/// Represents an abstract base class for event entities in a domain context.
/// </summary>
/// <typeparam name="TAggregateId">The type of the aggregate identifier.</typeparam>
/// <typeparam name="TKey">The type of the key.</typeparam>
public abstract class EventEntityDomain<TAggregateId, TKey> :
    EventEntity<TKey>,
    IEventEntityDomain<TAggregateId, TKey>
    where TKey : notnull, IComparable
    where TAggregateId : struct
{
    /// <inheritdoc/>
    public required TAggregateId AggregateId { get; init; }
}