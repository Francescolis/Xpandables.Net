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
/// Used to persist and read an object to/from an snapshot.
/// </summary>
public interface ISnapshotStore
{
    /// <summary>
    /// Asynchronously appends the specified snapshot to the store.
    /// </summary>
    /// <param name="event">The snapshot to act on.</param>
    /// <param name="cancellationToken">A CancellationToken to observe 
    /// while waiting for the task to complete.</param>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="event"/> is null.</exception>   
    /// <returns>A task that represents an 
    /// <see cref="IOperationResult"/>.</returns>
    ValueTask<IOperationResult> AppendAsync(
       IEventSnapshot @event,
       CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously returns a snapshot from the last snapshot 
    /// matching the specified identifier.
    /// </summary>
    /// <param name="objectId">The expected object identifier to search
    /// for.</param>
    /// <param name="cancellationToken">A CancellationToken to observe 
    /// while waiting for the task to complete.</param>
    /// <returns>A task that represents an 
    /// <see cref="IOperationResult{TResult}"/>.</returns>
    ValueTask<IOperationResult<IEventSnapshot>> ReadAsync(
        Guid objectId,
        CancellationToken cancellationToken = default);
}
