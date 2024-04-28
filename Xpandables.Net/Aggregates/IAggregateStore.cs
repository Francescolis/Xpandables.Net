
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
using Xpandables.Net.Operations;

namespace Xpandables.Net.Aggregates;

/// <summary>
/// Represents a store for aggregates.
/// </summary>
/// <typeparam name="TAggregate">The type of aggregate.</typeparam>
/// <typeparam name="TAggregateId">The type of aggregate Id type.</typeparam>
public interface IAggregateStore<TAggregate, TAggregateId>
    where TAggregateId : struct, IAggregateId<TAggregateId>
    where TAggregate : class, IAggregate<TAggregateId>
{
    /// <summary>
    /// Asynchronously appends the specified aggregate to the store.
    /// </summary>
    /// <param name="aggregate">The aggregate to act on.</param>
    /// <param name="cancellationToken">A CancellationToken to observe 
    /// while waiting for the task to complete.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="aggregate"/>
    /// is null.</exception>
    /// <returns>A task that represents an <see cref="IOperationResult"/>
    /// .</returns>
    ValueTask<IOperationResult> AppendAsync(
        TAggregate aggregate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously returns the <typeparamref name="TAggregate"/>
    /// aggregate that matches the specified aggregate identifier.
    /// </summary>
    /// <param name="aggregateId">The aggregate identifier to search for.</param>
    /// <param name="cancellationToken">A CancellationToken to observe 
    /// while waiting for the task to complete.</param>
    /// <returns>A task that represents an <see cref="IOperationResult{TResult}"/>>
    /// where the result is <typeparamref name="TAggregate"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="aggregateId"/> 
    /// is null.</exception>
    /// <remarks>You can also apply snapshot pattern for performance
    /// using the <see cref="ISnapshotStore"/>.</remarks>
    ValueTask<IOperationResult<TAggregate>> ReadAsync(
        TAggregateId aggregateId,
        CancellationToken cancellationToken = default);
}