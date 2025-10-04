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
/// Represents an aggregate root in a domain-driven design (DDD) context,  providing event sourcing capabilities and
/// managing the state of a domain entity.
/// </summary>
/// <remarks>An aggregate is the central concept in event sourcing and domain-driven design,  encapsulating the
/// state and behavior of a domain entity. This interface defines  the contract for aggregates that support event
/// sourcing, including replaying  historical events, managing uncommitted events, and handling optimistic concurrency 
/// through stream versioning.</remarks>
public interface IAggregate : IEventSourcing
{
    /// <summary>
    /// Gets the unique identifier for the stream.
    /// </summary>
    Guid StreamId { get; }

    /// <summary>
    /// Current persisted version of the stream (-1 if new stream is also acceptable, here 0-based).
    /// </summary>
    long StreamVersion { get; }

    /// <summary>
    /// Optional business versioning (separate from stream version).
    /// </summary>
    int BusinessVersion { get; }

    /// <summary>
    /// True if the aggregate has no identity yet.
    /// </summary>
    bool IsEmpty { get; }

    /// <summary>
    /// Expected version for optimistic concurrency.
    /// </summary>
    long ExpectedStreamVersion { get; }
}
