
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
using Xpandables.Net.Events.Aggregates;
using Xpandables.Net.Optionals;

namespace Xpandables.Net.Responsibilities;

/// <summary>
/// Represents a command aggregate interface that defines the structure 
/// for command aggregates.
/// </summary>
/// <remarks>It's used for implementing the Decider pattern. </remarks>
/// <typeparam name="TAggregate">The type of the aggregate.</typeparam>
/// <typeparam name="TKey">The type of the key identifier.</typeparam>
public interface ICommandAggregate<TAggregate, TKey>
    where TAggregate : class, IAggregate<TKey>, new()
    where TKey : struct
{
    /// <summary>
    /// Gets or sets the aggregate.
    /// </summary>
    Optional<TAggregate> Aggregate { get; set; }

    /// <summary>
    /// Gets the key identifier.
    /// </summary>
    TKey KeyId { get; }
}


/// <summary>
/// Represents a command aggregate interface that defines the structure 
/// for command aggregates using GUID identifier.
/// </summary>
/// <remarks>It's used for implementing the Decider pattern. </remarks>
/// <typeparam name="TAggregate">The type of the aggregate.</typeparam>
public interface ICommandAggregate<TAggregate> :
    ICommandAggregate<TAggregate, Guid>
    where TAggregate : class, IAggregate<Guid>, new()
{
}
