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

namespace Xpandables.Net.Executions.Domains;

/// <summary>
/// Represents a store for events that can be appended, fetched,
/// and marked as published.
/// </summary>
public interface IEventStore
{
    /// <summary>
    /// Appends a single event to the store.
    /// </summary>
    /// <param name="event">The events to be appended to the data source.</param>
    /// <param name="cancellationToken">Allows the operation to be canceled if needed.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AppendAsync(
        IEvent @event,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Appends a collection of events to the store.
    /// </summary>
    /// <param name="events">The events to append.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task AppendAsync(
        IEnumerable<IEvent> events,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches events from the store based on the specified filter.
    /// </summary>
    /// <param name="filter">The filter to apply when fetching events.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An asynchronous enumerable of events.</returns>
    IAsyncEnumerable<IEvent> FetchAsync(
        IEventFilter filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks the event as processed.
    /// </summary>
    /// <param name="eventProcessed">The processed information of the event.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task MarkAsProcessedAsync(
        EventProcessed eventProcessed,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents an event that has been processed.
/// </summary>
public readonly record struct EventProcessed
{
    /// <summary>
    /// Gets the unique identifier of the event.
    /// </summary>
    public required Guid EventId { get; init; }

    /// <summary>
    /// Gets the date and time when the event was published.
    /// </summary>
    public required DateTimeOffset PublishedOn { get; init; }

    /// <summary>
    /// Gets the error message if the event failed to publish.
    /// </summary>
    public required string? ErrorMessage { get; init; }
}