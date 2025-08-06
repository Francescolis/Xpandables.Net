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
/// Defines a contract for storing and retrieving aggregates.
/// </summary>
/// <remarks>Implementations of this interface are responsible for persisting aggregates and  providing mechanisms
/// to retrieve them based on their identifiers. The interface  supports asynchronous operations to accommodate
/// I/O-bound tasks.</remarks>
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
/// Defines a contract for storing and retrieving aggregates of type <typeparamref name="TAggregate"/>.
/// </summary>
/// <remarks>This interface provides methods to append and resolve aggregates, ensuring that the operations are
/// asynchronous and can be cancelled.</remarks>
/// <typeparam name="TAggregate">The type of aggregate managed by this store. 
/// Must inherit from <see cref="Aggregate"/> and have a parameterless
/// constructor.</typeparam>
public interface IAggregateStore<TAggregate> : IAggregateStore
    where TAggregate : Aggregate, new()
{
    /// <summary>
    /// Appends the specified aggregate.
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
        Aggregate aggregate,
        CancellationToken cancellationToken) =>
        AppendAsync((TAggregate)aggregate, cancellationToken);

    /// <summary>
    /// Resolves the aggregate that matches the specified keyId.
    /// </summary>
    /// <remarks>This method retrieves an aggregate of type <typeparamref name="TAggregate"/> using
    /// a collection of events that match the specified keyId and implements the <see cref="IDomainEvent{TAggregate}"/> interface.</remarks>
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