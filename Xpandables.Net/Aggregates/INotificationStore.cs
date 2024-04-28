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
/// Represents a set of methods to append/read notifications to/from store.
/// </summary>
public interface INotificationStore : IDisposable
{
    /// <summary>
    /// Asynchronously appends the specified notification to the store.
    /// </summary>
    /// <param name="event">The notification to act on.</param>
    /// <param name="cancellationToken">A CancellationToken to observe 
    /// while waiting for the task to complete.</param>
    /// <returns>A value that represents an asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="event"/> is null.</exception>   
    /// <exception cref="InvalidOperationException">The operation failed. 
    /// See inner exception.</exception>
    ValueTask AppendAsync(
        IEventNotification @event,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously sets the exception that occurred while 
    /// processing the specified notification,
    /// and closes the entity notification.
    /// </summary>
    /// <param name="eventId">The notification id to act on.</param>
    /// <param name="exception">The handled exception.</param>
    /// <param name="cancellationToken">A CancellationToken to observe 
    /// while waiting for the task to complete.</param>
    /// <returns>A value that represents an asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="eventId"/> is null.</exception>   
    /// <exception cref="InvalidOperationException">The operation failed. 
    /// See inner exception.</exception>
    ValueTask AppendCloseAsync(
        Guid eventId,
        Exception? exception = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously returns a collection of notifications matching the filter.
    /// if not found, returns an empty collection.
    /// </summary>
    /// <param name="eventFilter">The filter to search notification for.</param>
    /// <param name="cancellationToken">A CancellationToken to 
    /// observe while waiting for the task to complete.</param>
    /// <returns>An enumerator of <see cref="IEventNotification"/> 
    /// type that can be asynchronously enumerated.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="eventFilter"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The operation failed. 
    /// See inner exception.</exception>
    IAsyncEnumerable<IEventNotification> ReadAsync(
        IEventFilter eventFilter,
        CancellationToken cancellationToken = default);
}
