
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
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

using Xpandables.Net.Aggregates;

namespace Xpandables.Net.Events;

/// <summary>
/// Helper class used to create a domain event.
/// </summary>
public abstract record class EventDomain : Event, IEventDomain
{
    /// <inheritdoc/>
    public required Guid AggregateId { get; init; }

    /// <inheritdoc/>
    public virtual IEventDomain WithVersion(ulong version)
        => this with { Version = Version + 1 };
}

/// <summary>
/// Helper class used to create a domain event with aggregate.
/// </summary>
/// <typeparam name="TAggregate">The type of aggregate.</typeparam>
/// <remarks>Add a private parameterless constructor and decorate it 
/// with the <see cref="JsonConstructorAttribute"/> attribute.</remarks>
public abstract record EventDomain<TAggregate> : EventDomain
    where TAggregate : class, IAggregate
{
    /// <summary>
    /// Initializes a new instance of 
    /// <see cref="EventDomain{TAggregate}"/>.
    /// </summary>
    /// <remarks>Used for deserialization.</remarks>
    [JsonConstructor]
    protected EventDomain() { }

    /// <summary>
    /// Initializes a new instance of 
    /// <see cref="EventDomain{TAggregate}"/>.
    /// </summary>
    /// <param name="aggregate">The target aggregate instance.</param>
    /// <exception cref="ArgumentNullException">
    /// The <paramref name="aggregate"/> is null.</exception>
    [SetsRequiredMembers]
    protected EventDomain(TAggregate aggregate)
    {
        ArgumentNullException.ThrowIfNull(aggregate);

        AggregateId = aggregate.AggregateId;
        Version = aggregate.Version;
    }
}