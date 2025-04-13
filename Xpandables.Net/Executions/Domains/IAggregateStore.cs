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

namespace Xpandables.Net.Executions.Domains;

/// <summary>
/// Represents a store for aggregates root.
/// </summary>
public interface IAggregateStore
{
    /// <summary>
    /// Appends the specified aggregate root.
    /// </summary>
    /// <param name="aggregateRoot">The aggregate root to append.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The operation result.</returns>
    /// <exception cref="InvalidOperationException">Unable to append the 
    /// aggregate. See inner exception for details.</exception>
    Task AppendAsync(AggregateRoot aggregateRoot, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves the aggregate root that matches the specified keyId.
    /// </summary>
    /// <param name="keyId">The aggregate root identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The operation result containing the aggregate root.</returns>
    /// <exception cref="ValidationException">The aggregate root with the specified 
    /// keyId does not exist.</exception>
    /// <exception cref="InvalidOperationException">Unable to resolve the aggregate.
    /// See inner exception for details.</exception>
    Task<AggregateRoot> ResolveAsync(
        Guid keyId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a store for aggregates root.
/// </summary>
/// <typeparam name="TAggregateRoot">The type of the aggregate root.</typeparam>
public interface IAggregateStore<TAggregateRoot> : IAggregateStore
    where TAggregateRoot : AggregateRoot, new()
{
    /// <summary>
    /// Appends the specified aggregate root.
    /// </summary>
    /// <param name="aggregateRoot">The aggregate to append.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The operation result.</returns>
    /// <exception cref="InvalidOperationException">Unable to append the 
    /// aggregate. See inner exception for details.</exception>
    Task AppendAsync(
        TAggregateRoot aggregateRoot,
        CancellationToken cancellationToken = default);

    [EditorBrowsable(EditorBrowsableState.Never)]
    Task IAggregateStore.AppendAsync(
        AggregateRoot aggregateRoot,
        CancellationToken cancellationToken) =>
        AppendAsync((TAggregateRoot)aggregateRoot, cancellationToken);

    /// <summary>
    /// Resolves the aggregate root that matches the specified keyId.
    /// </summary>
    /// <param name="keyId">The aggregate root identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The operation result containing the aggregate root.</returns>
    /// <exception cref="ValidationException">The aggregate root with the specified 
    /// keyId does not exist.</exception>
    /// <exception cref="InvalidOperationException">Unable to resolve the aggregate.
    /// See inner exception for details.</exception>
    new Task<TAggregateRoot> ResolveAsync(
        Guid keyId,
        CancellationToken cancellationToken = default);

    [EditorBrowsable(EditorBrowsableState.Never)]
    async Task<AggregateRoot> IAggregateStore.ResolveAsync(
        Guid keyId,
        CancellationToken cancellationToken) =>
        await ResolveAsync(keyId, cancellationToken)
        .ConfigureAwait(false);
}