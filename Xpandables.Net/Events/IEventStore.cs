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
/// Represents methods to append/fetch events to/from the store.
/// </summary>
public interface IEventStore
{
    /// <summary>
    /// Asynchronously appends the specified event to the store.
    /// </summary>
    /// <param name="event">The event to act on.</param>
    /// <param name="cancellationToken">A CancellationToken to observe 
    /// while waiting for the task to complete.</param>
    /// <returns>A value that represents an asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="event"/> is null.</exception>   
    /// <exception cref="InvalidOperationException">The operation failed. 
    /// See inner exception.</exception>
    /// <exception cref="OperationCanceledException">The operation was 
    /// canceled.</exception>
    Task AppendAsync(
        IEvent @event,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously fetches a collection of events matching the filter.
    /// if not found, returns an empty collection.
    /// </summary>
    /// <param name="eventFilter">The filter to search events for.</param>
    /// <param name="cancellationToken">A CancellationToken to observe 
    /// while waiting for the task to complete.</param>
    /// <returns>An enumerator of <see cref="IEventDomain"/> 
    /// type that can be asynchronously enumerated.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="eventFilter"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The operation failed. 
    /// See inner exception.</exception>
    /// <exception cref="OperationCanceledException">The operation was
    /// canceled.</exception>
    IAsyncEnumerable<IEvent> FetchAsync(
        IEventFilter eventFilter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously marks event as published and add the exception if defined.
    /// </summary>
    /// <param name="eventId">The event id to act on.</param>
    /// <param name="exception">The optional handled exception during publishing
    /// .</param>
    /// <param name="cancellationToken">A CancellationToken to observe 
    /// while waiting for the task to complete.</param>
    /// <returns>A value that represents an asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="eventId"/> is null.</exception>   
    /// <exception cref="InvalidOperationException">The operation failed. 
    /// See inner exception.</exception>
    /// <exception cref="OperationCanceledException">The operation was 
    /// canceled.</exception>
    Task MarkEventAsPublished(
        Guid eventId,
        Exception? exception = default,
        CancellationToken cancellationToken = default);
}
