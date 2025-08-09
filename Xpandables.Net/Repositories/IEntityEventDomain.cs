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

namespace Xpandables.Net.Repositories;

/// <summary>
/// Represents a domain event associated with an entity aggregate.
/// </summary>
/// <remarks>This interface extends <see cref="IEntityEvent"/> to include the unique identifier of the aggregate
/// that the event pertains to. It is used to track changes or actions related to a specific entity within a
/// domain-driven design context.</remarks>
public interface IEntityEventDomain : IEntityEvent
{
    /// <summary>
    /// Gets the identifier of the aggregate.
    /// </summary>
    Guid AggregateId { get; }

    /// <summary>
    /// Gets the stream version of the event.
    /// </summary>
    long StreamVersion { get; }

    /// <summary>
    /// Gets the aggregate type name that this event belongs to.
    /// </summary>
    string AggregateName { get; init; }
}