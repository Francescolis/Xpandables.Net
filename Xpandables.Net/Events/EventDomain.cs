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
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

using Xpandables.Net.Events.Aggregates;
using Xpandables.Net.Text;

namespace Xpandables.Net.Events;

/// <summary>
/// Represents a domain event that is associated with an aggregate.
/// </summary>
/// <typeparam name="TAggregate">The type of the aggregate.</typeparam>
/// <typeparam name="TAggregateId">The type of the aggregate identifier.</typeparam>
public abstract record EventDomain<TAggregate, TAggregateId> :
    Event, IEventDomain<TAggregate, TAggregateId>
    where TAggregate : class, IAggregate<TAggregateId>
    where TAggregateId : struct, IPrimitive
{
    /// <summary>
    /// Initializes a new instance of the 
    /// <see cref="EventDomain{TAggregate, TAggregateId}"/> class.
    /// </summary>
    [JsonConstructor]
    protected EventDomain() { }

    /// <summary>
    /// Initializes a new instance of the 
    /// <see cref="EventDomain{TAggregate, TAggregateId}"/> class.
    /// </summary>
    /// <param name="aggregate">The aggregate associated with the event.</param>
    [SetsRequiredMembers]
    protected EventDomain(TAggregate aggregate)
    {
        AggregateId = aggregate.AggregateId;
        EventVersion = aggregate.Version;
    }

    /// <inheritdoc/>
    public required TAggregateId AggregateId { get; init; }

    /// <inheritdoc/>
    public virtual IEventDomain WithVersion(ushort version) =>
        this with { EventVersion = version };
}
