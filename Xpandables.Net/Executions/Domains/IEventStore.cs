﻿/*******************************************************************************
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
    /// Appends a collection of events to the store.
    /// </summary>
    /// <param name="events">The events to append.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when appending events 
    /// fails.</exception>
    Task AppendAsync(
        IEnumerable<IEvent> events,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches events from the store based on the specified filter.
    /// </summary>
    /// <param name="filter">The filter to apply when fetching events.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An asynchronous enumerable of events.</returns>
    /// <exception cref="InvalidOperationException">Thrown when fetching events
    /// fails.</exception>
    IAsyncEnumerable<IEvent> FetchAsync(
        IEventFilter filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes events from the store based on the specified filter.
    /// </summary>
    /// <param name="filter">The filter to apply when deleting events.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="InvalidOperationException">Thrown when deleting events
    /// fails.</exception>
    Task DeleteAsync(
        IEventFilter filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks the event represented by the publish information as published.
    /// </summary>
    /// <param name="eventPublished">The publish information of the event.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when marking events
    /// fails.</exception>
    Task MarkAsPublishedAsync(
        EventPublished eventPublished,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents an event that has been published.
/// </summary>
public readonly record struct EventPublished
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