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

namespace Xpandables.Net.Events;

/// <summary>
/// Represents a domain event that is associated with an aggregate.
/// </summary>
public record EventDomain : Event, IEventDomain
{
    /// <inheritdoc/>
    public required Guid AggregateId { get; init; }

    /// <inheritdoc/>
    public virtual IEventDomain WithVersion(ulong version) =>
        this with { EventVersion = version };
}

/// <summary>
/// Represents a domain event that is associated with an aggregate.
/// </summary>
/// <remarks>Add a private parameterless constructor and decorate it 
/// with the <see cref="JsonConstructorAttribute"/> attribute when you
/// are using the base constructor with <typeparamref name="TAggregate"/>.</remarks>
public record EventDomain<TAggregate> : EventDomain
    where TAggregate : class, IAggregate
{
    /// <summary>
    /// Initializes a new instance of the 
    /// <see cref="EventDomain"/> class.
    /// </summary>
    [JsonConstructor]
    protected EventDomain() { }

    /// <summary>
    /// Initializes a new instance of the 
    /// <see cref="EventDomain"/> class.
    /// </summary>
    /// <param name="aggregate">The aggregate associated with the event.</param>
    [SetsRequiredMembers]
    protected EventDomain(TAggregate aggregate)
    {
        AggregateId = aggregate.KeyId;
        EventVersion = aggregate.Version;
    }
}
