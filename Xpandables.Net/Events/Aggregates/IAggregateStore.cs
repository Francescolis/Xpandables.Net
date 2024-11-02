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
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Xpandables.Net.Events.Aggregates;

/// <summary>
/// Represents a store for aggregates.
/// </summary>
public interface IAggregateStore
{
    /// <summary>
    /// Appends the specified aggregate asynchronously.
    /// </summary>
    /// <param name="aggregate">The aggregate to append.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The operation result.</returns>
    /// <exception cref="InvalidOperationException">Unable to append the 
    /// aggregate. See inner exception for details.</exception>
    Task AppendAsync(
        IAggregate aggregate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Peeks the specified aggregate asynchronously.
    /// </summary>
    /// <param name="keyId">The aggregate identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The operation result containing the aggregate.</returns>
    /// <exception cref="ValidationException">The aggregate with the specified 
    /// keyId does not exist.</exception>
    /// <exception cref="InvalidOperationException">Unable to peek the aggregate.
    /// See inner exception for details.</exception>
    Task<IAggregate> PeekAsync(
        Guid keyId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a store for aggregates.
/// </summary>
/// <typeparam name="TAggregate">The type of the aggregate.</typeparam>
public interface IAggregateStore<TAggregate> : IAggregateStore
    where TAggregate : class, IAggregate, new()
{
    /// <summary>
    /// Appends the specified aggregate asynchronously.
    /// </summary>
    /// <param name="aggregate">The aggregate to append.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The operation result.</returns>
    /// <exception cref="InvalidOperationException">Unable to append the 
    /// aggregate. See inner exception for details.</exception>
    Task AppendAsync(
        TAggregate aggregate,
        CancellationToken cancellationToken = default);

    [EditorBrowsable(EditorBrowsableState.Never)]
    Task IAggregateStore.AppendAsync(
        IAggregate aggregate,
        CancellationToken cancellationToken) =>
        AppendAsync((TAggregate)aggregate, cancellationToken);

    /// <summary>
    /// Peeks the specified aggregate asynchronously.
    /// </summary>
    /// <param name="keyId">The aggregate identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The operation result containing the aggregate.</returns>
    /// <exception cref="ValidationException">The aggregate with the specified 
    /// keyId does not exist.</exception>
    /// <exception cref="InvalidOperationException">Unable to peek the aggregate.
    /// See inner exception for details.</exception>
    new Task<TAggregate> PeekAsync(
        Guid keyId,
        CancellationToken cancellationToken = default);

    [EditorBrowsable(EditorBrowsableState.Never)]
    async Task<IAggregate> IAggregateStore.PeekAsync(
        Guid keyId,
        CancellationToken cancellationToken) =>
        await PeekAsync(keyId, cancellationToken)
        .ConfigureAwait(false);
}