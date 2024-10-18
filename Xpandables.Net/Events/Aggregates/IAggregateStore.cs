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
using Xpandables.Net.Operations;

namespace Xpandables.Net.Events.Aggregates;

/// <summary>
/// Represents a store for aggregates.
/// </summary>
/// <typeparam name="TAggregate">The type of the aggregate.</typeparam>
/// <typeparam name="TAggregateId">The type of the aggregate identifier.</typeparam>
public interface IAggregateStore<TAggregate, TAggregateId>
    where TAggregateId : struct
    where TAggregate : class, IAggregate<TAggregateId>, new()
{
    /// <summary>
    /// Appends the specified aggregate asynchronously.
    /// </summary>
    /// <param name="aggregate">The aggregate to append.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The operation result.</returns>
    Task<IOperationResult> AppendAsync(
        TAggregate aggregate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Peeks the specified aggregate asynchronously.
    /// </summary>
    /// <param name="aggregateId">The aggregate identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The operation result containing the aggregate.</returns>
    Task<IOperationResult<TAggregate>> PeekAsync(
        TAggregateId aggregateId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a store for aggregates with a GUID identifier.
/// </summary>
/// <typeparam name="TAggregate">The type of the aggregate.</typeparam>
public interface IAggregateStore<TAggregate> : IAggregateStore<TAggregate, Guid>
    where TAggregate : class, IAggregate<Guid>, new()
{
}