﻿/*******************************************************************************
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

using Xpandables.Net.Executions.Tasks;

namespace Xpandables.Net.Executions.Domains;

/// <summary>
/// Represents a domain event that includes versioning and an aggregate identifier.
/// </summary>
public interface IDomainEvent : IEvent
{
    /// <summary>
    /// Gets the aggregate identifier associated with the event.
    /// </summary>
    /// <remarks>It's based on the <see cref="Guid.NewGuid" />.</remarks>
    Guid AggregateId { get; init; }

    /// <summary>
    /// Sets the version of the event.
    /// </summary>
    /// <param name="version">The version to set.</param>
    /// <returns>The event domain with the specified version.</returns>
    IDomainEvent WithVersion(ulong version);
}

/// <summary>
/// Represents a domain event that is associated with an aggregate.
/// </summary>
public record DomainEvent : Event, IDomainEvent
{
    /// <inheritdoc />
    public required Guid AggregateId { get; init; }

    /// <inheritdoc />
    public virtual IDomainEvent WithVersion(ulong version) =>
        this with { EventVersion = version };
}

/// <summary>
/// Represents a domain event that is associated with an aggregate root.
/// </summary>
/// <remarks>
/// Add a private parameterless constructor and decorate it
/// with the <see cref="JsonConstructorAttribute" /> attribute when you
/// are using the base constructor with <typeparamref name="TAggregateRoot" />.
/// </remarks>
public record DomainEvent<TAggregateRoot> : DomainEvent
    where TAggregateRoot : Aggregate
{
    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="DomainEvent" /> class.
    /// </summary>
    [JsonConstructor]
    protected DomainEvent() { }

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="DomainEvent" /> class.
    /// </summary>
    /// <param name="aggregateRoot">The aggregate root associated with the event.</param>
    [SetsRequiredMembers]
    protected DomainEvent(TAggregateRoot aggregateRoot)
    {
        AggregateId = aggregateRoot.KeyId;
        EventVersion = aggregateRoot.Version;
    }
}