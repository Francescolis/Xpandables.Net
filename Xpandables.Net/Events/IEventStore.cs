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

namespace Xpandables.Net.Events;

/// <summary>
/// Represents methods to append, fetch, mark, and persist events in the store.
/// </summary>
public interface IEventStore
{
    /// <summary>
    /// Asynchronously appends the specified event to the store.
    /// </summary>
    /// <param name="event">The event to append.</param>
    /// <param name="cancellationToken">A CancellationToken to observe 
    /// while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the 
    /// <paramref name="event"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the operation 
    /// fails. See inner exception for details.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation 
    /// is canceled.</exception>
    Task AppendEventAsync(
        IEvent @event,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously fetches a collection of events matching the filter. 
    /// Returns an empty collection if no events are found.
    /// </summary>
    /// <param name="eventFilter">The filter to search events for.</param>
    /// <param name="cancellationToken">A CancellationToken to observe 
    /// while waiting for the task to complete.</param>
    /// <returns>An enumerator of <see cref="IEvent"/> 
    /// type that can be asynchronously enumerated.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the 
    /// <paramref name="eventFilter"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the operation 
    /// fails. See inner exception for details.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation 
    /// is canceled.</exception>
    IAsyncEnumerable<IEvent> FetchEventsAsync(
        IEventFilter eventFilter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously marks an event as published and optionally records 
    /// an exception.
    /// </summary>
    /// <param name="eventId">The ID of the event to mark as published.</param>
    /// <param name="exception">The optional exception that occurred during 
    /// publishing.</param>
    /// <param name="cancellationToken">A CancellationToken to observe 
    /// while waiting for the task to complete.</param>
    /// <returns>A value that represents an asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the 
    /// <paramref name="eventId"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the operation 
    /// fails. See inner exception for details.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation 
    /// is canceled.</exception>
    Task MarkEventAsPublishedAsync(
        Guid eventId,
        Exception? exception = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously persists all pending events to the data storage.
    /// </summary>
    /// <param name="cancellationToken">A CancellationToken 
    /// to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the number of persisted objects.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the operation 
    /// fails. See inner exception for details.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation 
    /// is canceled.</exception>
    Task<int> PersistEventsAsync(CancellationToken cancellationToken = default);
}
