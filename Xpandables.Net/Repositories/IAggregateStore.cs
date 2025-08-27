
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

using Xpandables.Net.Events;

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
    /// Persists pending domain events of the aggregate with optimistic concurrency.
    /// </summary>
    /// <param name="aggregate">The aggregate to append.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The operation result.</returns>
    /// <exception cref="InvalidOperationException">Unable to append the 
    /// aggregate. See inner exception for details.</exception>
    Task SaveAsync(
        IAggregate aggregate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads an aggregate by id by replaying its event stream.
    /// </summary>
    /// <param name="aggregateId">The aggregate identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The operation result containing the aggregate.</returns>
    /// <exception cref="InvalidOperationException">Unable to resolve the aggregate.
    /// See inner exception for details.</exception>
    Task<IAggregate> LoadAsync(
        Guid aggregateId,
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
    where TAggregate : class, IAggregate, new()
{
    /// <summary>
    /// Persists pending domain events of the aggregate with optimistic concurrency.
    /// </summary>
    /// <param name="aggregate">The aggregate to persist.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The operation result.</returns>
    /// <exception cref="InvalidOperationException">Unable to append the 
    /// aggregate. See inner exception for details.</exception>
    Task SaveAsync(
        TAggregate aggregate,
        CancellationToken cancellationToken = default);

    [EditorBrowsable(EditorBrowsableState.Never)]
    Task IAggregateStore.SaveAsync(
        IAggregate aggregate,
        CancellationToken cancellationToken) =>
        SaveAsync((TAggregate)aggregate, cancellationToken);

    /// <summary>
    /// Loads an aggregate by id by replaying its event stream.
    /// </summary>
    /// <remarks>This method retrieves an aggregate of type <typeparamref name="TAggregate"/> using
    /// a collection of events that match the specified keyId and implements the <see cref="IDomainEvent{TAggregate}"/> interface.</remarks>
    /// <param name="aggregateId">The aggregate identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The operation result containing the aggregate.</returns>
    /// <exception cref="ValidationException">The aggregate with the specified 
    /// keyId does not exist.</exception>
    /// <exception cref="InvalidOperationException">Unable to resolve the aggregate.
    /// See inner exception for details.</exception>
    new Task<TAggregate> LoadAsync(
        Guid aggregateId,
        CancellationToken cancellationToken = default);

    [EditorBrowsable(EditorBrowsableState.Never)]
    async Task<IAggregate> IAggregateStore.LoadAsync(
        Guid aggregateId,
        CancellationToken cancellationToken) =>
        await LoadAsync(aggregateId, cancellationToken)
        .ConfigureAwait(false);
}

/// <summary>
/// Provides extension methods for the <see cref="IAggregateStore"/> interface to simplify loading and saving
/// aggregates.
/// </summary>
/// <remarks>This static class includes methods for loading aggregates by replaying their event streams and saving
/// aggregates to the underlying store. These methods are designed to work with aggregates that implement the <see
/// cref="IAggregate"/> interface and have a parameterless constructor.</remarks>
public static class IAggregateStoreExtensions
{
    /// <summary>
    /// Loads an aggregate by id by replaying its event stream.
    /// </summary>
    /// <remarks>This method retrieves an aggregate of type <typeparamref name="TAggregate"/> using
    /// a collection of events that match the specified keyId and implements the <see cref="IDomainEvent{TAggregate}"/> interface.</remarks>
    /// <typeparam name="TAggregate">The type of aggregate managed by this store. 
    /// Must inherit from <see cref="Aggregate"/> and have a parameterless
    /// constructor.</typeparam>
    /// <param name="store">The aggregate store.</param>
    /// <param name="aggregateId">The aggregate identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The operation result containing the aggregate.</returns>
    /// <exception cref="ValidationException">The aggregate with the specified 
    /// keyId does not exist.</exception>
    /// <exception cref="InvalidOperationException">Unable to resolve the aggregate.
    /// See inner exception for details.</exception>
    public static async Task<TAggregate> LoadAsync<TAggregate>(
        this IAggregateStore store,
        Guid aggregateId,
        CancellationToken cancellationToken = default)
        where TAggregate : class, IAggregate, new() =>
        (TAggregate)(await store.LoadAsync(aggregateId, cancellationToken).ConfigureAwait(false));

    /// <summary>
    /// Saves the specified aggregate to the underlying aggregate store asynchronously.
    /// </summary>
    /// <typeparam name="TAggregate">The type of the aggregate to save. Must implement <see cref="IAggregate"/> and have a parameterless constructor.</typeparam>
    /// <param name="store">The aggregate store instance where the aggregate will be saved.</param>
    /// <param name="aggregate">The aggregate instance to save. Cannot be <see langword="null"/>.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. 
    /// The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous save operation.</returns>
    public static async Task SaveAsync<TAggregate>(
        this IAggregateStore store,
        TAggregate aggregate,
        CancellationToken cancellationToken = default)
        where TAggregate : class, IAggregate, new() =>
        await store.SaveAsync(aggregate, cancellationToken).ConfigureAwait(false);
}