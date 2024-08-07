﻿/*******************************************************************************
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
using Xpandables.Net.Events;
using Xpandables.Net.Operations;

namespace Xpandables.Net.Aggregates;

/// <summary>
/// Represents commands to manage an aggregate.
/// </summary>
/// <typeparam name="TAggregate">The type of aggregate.</typeparam>
public interface IAggregateAccessor<TAggregate>
    where TAggregate : class, IAggregate
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
    Task<IOperationResult> AppendAsync(
        TAggregate aggregate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously returns the <typeparamref name="TAggregate"/>
    /// aggregate that matches the specified aggregate identifier.
    /// </summary>
    /// <param name="keyId">The aggregate identifier to search for.</param>
    /// <param name="cancellationToken">A CancellationToken to observe 
    /// while waiting for the task to complete.</param>
    /// <returns>A task that represents an <see cref="IOperationResult{TResult}"/>>
    /// where the result is <typeparamref name="TAggregate"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="keyId"/> 
    /// is null.</exception>
    /// <remarks>You can also apply snapshot pattern for performance
    /// enabling the <see cref="EventOptions.SnapshotOptions"/>.</remarks>
    Task<IOperationResult<TAggregate>> PeekAsync(
        Guid keyId,
        CancellationToken cancellationToken = default);
}
