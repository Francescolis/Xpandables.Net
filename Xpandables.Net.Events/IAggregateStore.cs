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

namespace Xpandables.Net.Events;

/// <summary>
/// Defines methods for persisting and retrieving aggregate roots in an event-sourced system.
/// </summary>
/// <remarks>Implementations of this interface are responsible for storing and loading aggregates, typically using
/// an event store or similar persistence mechanism. The interface is intended for use in domain-driven design and event
/// sourcing scenarios, where aggregates represent the consistency boundaries of the domain model.
/// <para>If you need more fine-grained control over the persistence of aggregates, consider using the <see cref="IEventStore"/> implementation.</para>
/// </remarks>
public interface IAggregateStore
{
    /// <summary>
    /// Asynchronously persists the specified aggregate to the underlying data store.
    /// </summary>
    /// <param name="aggregate">The aggregate instance to be saved. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the save operation.</param>
    /// <returns>A task that represents the asynchronous save operation.</returns>
    Task SaveAsync(IAggregate aggregate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously loads the aggregate associated with the specified stream identifier.
    /// </summary>
    /// <param name="streamId">The unique identifier of the stream from which to load the aggregate.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation. The default value is <see
    /// cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous load operation. The task result contains the loaded aggregate.</returns>
    Task<IAggregate> LoadAsync(Guid streamId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Defines a contract for storing and retrieving aggregates of a specific type from a persistence mechanism.
/// </summary>
/// <remarks>This interface provides type-safe methods for working with aggregates, enabling implementations to
/// persist and rehydrate domain objects. It is typically used in event sourcing or domain-driven design scenarios to
/// abstract the underlying storage details.
/// <para>If you need more fine-grained control over the persistence of aggregates, consider using the <see cref="IEventStore"/> implementation.</para>
/// </remarks>
/// <typeparam name="TAggregate">The type of aggregate managed by the store. Must be a class that implements the <see cref="IAggregateFactory{TAggregate}"/> 
/// interface.</typeparam>
public interface IAggregateStore<TAggregate> : IAggregateStore
    where TAggregate : class, IAggregate, IAggregateFactory<TAggregate>
{
    /// <summary>
    /// Asynchronously saves the specified aggregate to the underlying data store.
    /// </summary>
    /// <param name="aggregate">The aggregate instance to be saved. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the save operation.</param>
    /// <returns>A task that represents the asynchronous save operation.</returns>
    Task SaveAsync(TAggregate aggregate, CancellationToken cancellationToken = default);

    [EditorBrowsable(EditorBrowsableState.Never)]
    Task IAggregateStore.SaveAsync(IAggregate aggregate, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        if (aggregate is not TAggregate typedAggregate)
            throw new InvalidOperationException($"The aggregate must be of type '{typeof(TAggregate)}'.");

        return SaveAsync(typedAggregate, cancellationToken);
    }

    /// <summary>
    /// Asynchronously loads the aggregate of type TAggregate associated with the specified stream identifier.
    /// </summary>
    /// <param name="streamId">The unique identifier of the stream from which to load the aggregate.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation. The default value is None.</param>
    /// <returns>A task that represents the asynchronous load operation. The task result contains the loaded aggregate of type
    /// TAggregate.</returns>
    new Task<TAggregate> LoadAsync(Guid streamId, CancellationToken cancellationToken = default);

    [EditorBrowsable(EditorBrowsableState.Never)]
    async Task<IAggregate> IAggregateStore.LoadAsync(Guid streamId, CancellationToken cancellationToken)
        => await LoadAsync(streamId, cancellationToken).ConfigureAwait(false);
}
