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
namespace Xpandables.Net.Events.Aggregates;

/// <summary>
/// Represents an aggregate with an identifier and version.
/// </summary>
public interface IAggregate : IEventSourcing
{
    /// <summary>
    /// Gets the unique identifier of the aggregate.
    /// </summary>
    Guid AggregateId { get; }

    /// <summary>
    /// Gets the version of the aggregate.
    /// </summary>
    ulong Version { get; }

    /// <summary>
    /// Gets a value indicating whether the aggregate is empty.
    /// </summary>
    bool IsEmpty { get; }
}