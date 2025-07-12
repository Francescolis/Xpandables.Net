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

using Xpandables.Net.Executions.Tasks;
using Xpandables.Net.Repositories.Filters;

namespace Xpandables.Net.Repositories;

/// <summary>
/// Represents a store for managing events, providing methods to append, fetch, and mark events as processed.
/// </summary>
public interface IEventStore
{
    /// <summary>
    /// Appends the specified event to the event stream asynchronously.
    /// </summary>
    /// <param name="event">The event to append. Cannot be null.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous append operation.</returns>
    Task AppendAsync(IEvent @event, CancellationToken cancellationToken = default);

    /// <summary>
    /// Appends a collection of events asynchronously to the event store.
    /// </summary>
    /// <param name="events">The collection of events to append. Cannot be null or contain null elements.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. 
    /// The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous append operation.</returns>
    Task AppendAsync(IEnumerable<IEvent> events, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously fetches a stream of events that match the specified filter criteria.
    /// </summary>
    /// <remarks>The method returns an <see cref="IAsyncEnumerable{T}"/> that allows for asynchronous
    /// iteration over the events. The operation can be cancelled by passing a <see cref="CancellationToken"/> to the
    /// method.</remarks>
    /// <typeparam name="TEntityEvent">The type of the entity event to filter.</typeparam>
    /// <typeparam name="TEvent">The type of the event to return.</typeparam>
    /// <param name="filter">The filter criteria used to select events.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An asynchronous stream of events of type <typeparamref name="TEvent"/> that match the filter criteria.</returns>
    IAsyncEnumerable<TEvent> FetchAsync<TEntityEvent, TEvent>(
        IEventFilter<TEntityEvent, TEvent> filter,
        CancellationToken cancellationToken = default)
        where TEntityEvent : class, IEntityEvent
        where TEvent : class, IEvent;

    /// <summary>
    /// Marks the specified event as processed asynchronously.
    /// </summary>
    /// <param name="info">The information about the event to be marked as processed.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task MarkAsProcessedAsync(EventProcessedInfo info, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks the specified events as processed asynchronously.
    /// </summary>
    /// <remarks>This method updates the state of the specified events to indicate they have been processed.
    /// It is designed to be used in scenarios where event processing is tracked asynchronously.</remarks>
    /// <param name="infos">A collection of <see cref="EventProcessedInfo"/> objects representing the events to be marked as processed.
    /// Cannot be null or contain null elements.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task MarkAsProcessedAsync(IEnumerable<EventProcessedInfo> infos, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents information about a processed event, including its unique identifier,  the completion time of processing,
/// and any associated error message.
/// </summary>
public readonly record struct EventProcessedInfo
{
    /// <summary>
    /// Gets the unique identifier of the event.
    /// </summary>
    public readonly required Guid EventId { get; init; }

    /// <summary>
    /// Gets the date and time when the processing was completed.
    /// </summary>
    public readonly required DateTimeOffset ProcessedOn { get; init; }

    /// <summary>
    /// Gets the error message associated with the current operation.
    /// </summary>
    public readonly required string? ErrorMessage { get; init; }
}