
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

namespace Xpandables.Net.Aggregates;

/// <summary>
/// Represents a set of methods to append/read domain events to/from store.
/// </summary>
public interface IDomainEventStore : IDisposable
{
    /// <summary>
    /// Asynchronously appends the specified domain event to the store.
    /// </summary>
    /// <typeparam name="TAggregateId">The type of aggregate Id.</typeparam>
    /// <param name="event">The domain event to act on.</param>
    /// <param name="cancellationToken">A CancellationToken to observe 
    /// while waiting for the task to complete.</param>
    /// <returns>A value that represents an asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="event"/> is null.</exception>   
    /// <exception cref="InvalidOperationException">The operation failed. 
    /// See inner exception.</exception>
    ValueTask AppendAsync<TAggregateId>(
        IEventDomain<TAggregateId> @event,
        CancellationToken cancellationToken = default)
        where TAggregateId : struct, IAggregateId<TAggregateId>;

    /// <summary>
    /// Asynchronously returns a collection of domain events matching 
    /// the aggregate identifier.
    /// if not found, returns an empty collection.
    /// </summary>
    /// <typeparam name="TAggregateId">The type of aggregate Id.</typeparam>
    /// <param name="aggregateId">The aggregate identifier to search events for.</param>
    /// <param name="cancellationToken">A CancellationToken to observe 
    /// while waiting for the task to complete.</param>
    /// <returns>An enumerator of <see cref="IEventDomain{TAggregateId}"/> 
    /// that can be asynchronously enumerated.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="aggregateId"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The operation failed.
    /// See inner exception.</exception>
    /// <remarks>For performance, use the method with filters.</remarks>
    IAsyncEnumerable<IEventDomain<TAggregateId>> ReadAsync<TAggregateId>(
        TAggregateId aggregateId,
        CancellationToken cancellationToken = default)
        where TAggregateId : struct, IAggregateId<TAggregateId>;

    /// <summary>
    /// Asynchronously returns a collection of results from 
    /// domain events matching the filter.
    /// if not found, returns an empty collection.
    /// </summary>
    /// <typeparam name="TAggregateId">The type of aggregate Id.</typeparam>
    /// <param name="eventFilter">The filter to search domain events for.</param>
    /// <param name="cancellationToken">A CancellationToken to observe 
    /// while waiting for the task to complete.</param>
    /// <returns>An enumerator of <see cref="IEventDomain{TAggregateId}"/> 
    /// type that can be asynchronously enumerated.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="eventFilter"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The operation failed. 
    /// See inner exception.</exception>
    IAsyncEnumerable<IEventDomain<TAggregateId>> ReadAsync<TAggregateId>(
        IEventFilter eventFilter,
        CancellationToken cancellationToken = default)
        where TAggregateId : struct, IAggregateId<TAggregateId>;
}
