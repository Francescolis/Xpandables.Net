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
using System.ComponentModel;

using Xpandables.Net.Events.Aggregates;

namespace Xpandables.Net.Events;
/// <summary>
/// Represents a domain event that includes versioning and an aggregate identifier.
/// </summary>
public interface IEventDomain : IEvent
{
    /// <summary>
    /// Sets the version of the event.
    /// </summary>
    /// <param name="version">The version to set.</param>
    /// <returns>The event domain with the specified version.</returns>
    IEventDomain WithVersion(ushort version);

    /// <summary>
    /// Gets the aggregate identifier associated with the event.
    /// </summary>
    object AggregateId { get; }
}

/// <summary>
/// Represents a domain event that includes versioning, a strongly-typed 
/// aggregate identifier, and an aggregate.
/// </summary>
/// <typeparam name="TAggregateId">The type of the aggregate identifier.</typeparam>
/// <typeparam name="TAggregate">The type of the aggregate.</typeparam>
public interface IEventDomain<out TAggregate, out TAggregateId> : IEventDomain
    where TAggregateId : struct
    where TAggregate : class, IAggregate<TAggregateId>
{
    /// <summary>
    /// Gets the aggregate identifier associated with the event.
    /// </summary>
    new TAggregateId AggregateId { get; }

    [EditorBrowsable(EditorBrowsableState.Never)]
    object IEventDomain.AggregateId => AggregateId;
}