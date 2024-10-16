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
using System.Text.Json;

using Xpandables.Net.Text;

namespace Xpandables.Net.Events.Providers.MSSql;

/// <summary>
/// Represents an abstract base class for MSSql event entities in a domain context.
/// </summary>
/// <typeparam name="TAggregateId">The type of the aggregate identifier.</typeparam>
public abstract class EventEntityDomain<TAggregateId> :
    EventEntity<Guid, byte[]>,
    IEventEntityDomain<TAggregateId, Guid, byte[]>
    where TAggregateId : struct, IPrimitive
{
    /// <inheritdoc/>
    public TAggregateId AggregateId { get; }

    /// <inheritdoc/>
    public string AggregateName { get; }

    /// <summary>
    /// Initializes a new instance of the 
    /// <see cref="EventEntityDomain{TAggregateId}"/> class.
    /// </summary>
    /// <param name="aggregateId">The aggregate identifier.</param>
    /// <param name="aggregateName">The name of the aggregate.</param>
    /// <param name="key">The key of the event entity.</param>
    /// <param name="eventName">The name of the event.</param>
    /// <param name="eventFullName">The full name of the event.</param>
    /// <param name="version">The version of the event.</param>
    /// <param name="eventData">The data of the event.</param>
#pragma warning disable IDE0290 // Use primary constructor
    protected EventEntityDomain(
#pragma warning restore IDE0290 // Use primary constructor
        TAggregateId aggregateId,
        string aggregateName,
        Guid key,
        string eventName,
        string eventFullName,
        ulong version,
        JsonDocument eventData) :
        base(key, eventName, eventFullName, version, eventData)
    {
        AggregateId = aggregateId;
        AggregateName = aggregateName;
    }
}
