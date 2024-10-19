
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
using Xpandables.Net.Operations;

namespace Xpandables.Net.Responsibilities;

/// <summary>
/// Defines a handler for command aggregates.
/// </summary>
/// <remarks>It's used for implementing the Decider pattern. </remarks>
/// <typeparam name="TCommand">The type of the command.</typeparam>
/// <typeparam name="TAggregate">The type of the aggregate.</typeparam>
/// <typeparam name="TKey">The type of the key.</typeparam>
public interface ICommandAggregateHandler<TCommand, TAggregate, TKey>
    where TCommand : ICommandAggregate<TAggregate, TKey>
    where TAggregate : class, IAggregate<TKey>, new()
    where TKey : struct
{
    /// <summary>
    /// Handles the specified command asynchronously.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the operation.</returns>
    Task<IOperationResult> HandleAsync(
        TCommand command,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Defines a handler for command aggregates with a GUID key.
/// </summary>
/// <remarks>It's used for implementing the Decider pattern. </remarks>
/// <typeparam name="TCommand">The type of the command.</typeparam>
/// <typeparam name="TAggregate">The type of the aggregate.</typeparam>
public interface ICommandAggregateHandler<TCommand, TAggregate> :
    ICommandAggregateHandler<TCommand, TAggregate, Guid>
    where TCommand : ICommandAggregate<TAggregate, Guid>
    where TAggregate : class, IAggregate<Guid>, new()
{
}
