
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
using System.Text.Json.Serialization;

namespace Xpandables.Net.Aggregates;

/// <summary>
/// Helper class used to create a domain event with aggregate.
/// </summary>
/// <typeparam name="TAggregateId">The type of aggregate.</typeparam>
/// <remarks>Initializes a new instance of 
/// <see cref="DomainEvent{TAggregateId}"/>.</remarks>
public abstract record class DomainEvent<TAggregateId>
    : Event, IEventDomain<TAggregateId>
    where TAggregateId : struct, IAggregateId<TAggregateId>
{
    /// <inheritdoc/>
    public IEventDomain<TAggregateId> WithVersion(ulong version)
        => this with { Version = Version + 1 };

    /// <inheritdoc/>
    public TAggregateId AggregateId { get; init; }
}

/// <summary>
/// Helper class used to create a domain event with aggregate.
/// </summary>
/// <typeparam name="TAggregate">The type of aggregate.</typeparam>
/// <typeparam name="TAggregateId">The type of aggregate.</typeparam>
/// <remarks>Add a private parameterless constructor and decorate it 
/// with the <see cref="JsonConstructorAttribute"/> attribute.</remarks>
public abstract record DomainEvent<TAggregate, TAggregateId>
    : DomainEvent<TAggregateId>
    where TAggregateId : struct, IAggregateId<TAggregateId>
    where TAggregate : class, IAggregate<TAggregateId>
{
    /// <summary>
    /// Initializes a new instance of 
    /// <see cref="DomainEvent{TAggregate, TAggregateId}"/>.
    /// </summary>
    /// <remarks>Used for deserialization.</remarks>
    [JsonConstructor]
    protected DomainEvent() { }

    /// <summary>
    /// Initializes a new instance of 
    /// <see cref="DomainEvent{TAggregate, TAggregateId}"/>.
    /// </summary>
    /// <param name="aggregate">The target aggregate instance.</param>
    /// <exception cref="ArgumentNullException">
    /// The <paramref name="aggregate"/> is null.</exception>
    protected DomainEvent(TAggregate aggregate)
    {
        ArgumentNullException.ThrowIfNull(aggregate);

        AggregateId = aggregate.AggregateId;
        Version = aggregate.Version;
    }
}