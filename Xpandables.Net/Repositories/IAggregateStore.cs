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

using Xpandables.Net.Executions.Domains;

namespace Xpandables.Net.Repositories;

/// <summary>
/// Defines an abstraction for managing aggregate instances in a store.
/// </summary>
public interface IAggregateStore
{
    /// <summary>
    /// Appends the specified aggregate.
    /// </summary>
    /// <param name="aggregate">The aggregate to append.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The operation result.</returns>
    /// <exception cref="InvalidOperationException">Unable to append the 
    /// aggregate. See inner exception for details.</exception>
    Task AppendAsync(Aggregate aggregate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves the aggregate that matches the specified keyId.
    /// </summary>
    /// <param name="keyId">The aggregate identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The operation result containing the aggregate.</returns>
    /// <exception cref="ValidationException">The aggregate with the specified 
    /// keyId does not exist.</exception>
    /// <exception cref="InvalidOperationException">Unable to resolve the aggregate.
    /// See inner exception for details.</exception>
    Task<Aggregate> ResolveAsync(
        Guid keyId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents an abstraction for managing aggregate instances in a store.
/// </summary>
public interface IAggregateStore<TAggregate> : IAggregateStore
    where TAggregate : Aggregate, new()
{
    /// <summary>
    /// Appends the specified aggregate.
    /// </summary>
    /// <param name="aggregateRoot">The aggregate to append.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The operation result.</returns>
    /// <exception cref="InvalidOperationException">Unable to append the 
    /// aggregate. See inner exception for details.</exception>
    Task AppendAsync(
        TAggregate aggregateRoot,
        CancellationToken cancellationToken = default);

    [EditorBrowsable(EditorBrowsableState.Never)]
    Task IAggregateStore.AppendAsync(
        Aggregate aggregate,
        CancellationToken cancellationToken) =>
        AppendAsync((TAggregate)aggregate, cancellationToken);

    /// <summary>
    /// Resolves the aggregate that matches the specified keyId.
    /// </summary>
    /// <param name="keyId">The aggregate identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The operation result containing the aggregate.</returns>
    /// <exception cref="ValidationException">The aggregate with the specified 
    /// keyId does not exist.</exception>
    /// <exception cref="InvalidOperationException">Unable to resolve the aggregate.
    /// See inner exception for details.</exception>
    new Task<TAggregate> ResolveAsync(
        Guid keyId,
        CancellationToken cancellationToken = default);

    [EditorBrowsable(EditorBrowsableState.Never)]
    async Task<Aggregate> IAggregateStore.ResolveAsync(
        Guid keyId,
        CancellationToken cancellationToken) =>
        await ResolveAsync(keyId, cancellationToken)
        .ConfigureAwait(false);
}