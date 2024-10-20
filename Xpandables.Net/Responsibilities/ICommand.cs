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
/// This interface is used as a marker for command.
/// Class implementation is used with the <see cref="ICommandHandler{TCommand}"/> 
/// where "TCommand" is a record that implements <see cref="ICommand"/>.
/// This can also be enhanced with some useful decorators.
/// </summary>
public interface ICommand
{
}

/// <summary>
/// Represents a command aggregate interface that defines the structure 
/// for command aggregates.
/// </summary>
/// <remarks>It's used for implementing the Decider pattern. </remarks>
/// <typeparam name="TAggregate">The type of the aggregate.</typeparam>
public interface ICommandAggregate<TAggregate> : ICommand
    where TAggregate : class, IAggregate, new()
{
    /// <summary>
    /// Gets or sets the aggregate.
    /// </summary>
    Optional<TAggregate> Aggregate { get; set; }

    /// <summary>
    /// Gets the key identifier.
    /// </summary>
    Guid KeyId { get; }

    /// <summary>
    /// Gets a value indicating whether to continue when the aggregate is not found.
    /// </summary>
    bool ContinueWhenNotFound { get; }
}