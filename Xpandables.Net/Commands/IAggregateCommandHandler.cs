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
using Xpandables.Net.Aggregates;
using Xpandables.Net.Operations;

namespace Xpandables.Net.Commands;

/// <summary>
/// This interface is used as a marker for commands targeting aggregate. 
/// It's used for implementing the Decider pattern. 
/// </summary>
public interface IAggregateCommand : ICommand
{
    /// <summary>
    /// Gets the aggretate identitifer
    /// </summary>
    Guid AggregateId { get; }
}

/// <summary>
/// Provides with a method to handle commands that are associated with an 
/// aggregate using the Decider asynchronous command pattern.
/// </summary>
/// <typeparam name="TAggregate">The type of the aggregate.</typeparam>
/// <typeparam name="TAggregateCommand">The type of the aggregate 
/// command.</typeparam>
public interface IAggregateCommandHandler<TAggregate, TAggregateCommand>
    where TAggregate : class, IAggregate
    where TAggregateCommand : notnull, IAggregateCommand
{
    /// <summary>
    /// Handles the specified command for the specified aggregate.
    /// </summary>
    /// <param name="aggregate">The aggregate instance to act on.</param>
    /// <param name="command">The command instance.</param>
    /// <param name="cancellationToken">A CancellationToken 
    /// to observe while waiting for the task to complete.</param>
    ValueTask<IOperationResult> HandleAsync(
        TAggregate aggregate,
        TAggregateCommand command,
        CancellationToken cancellationToken = default);
}
