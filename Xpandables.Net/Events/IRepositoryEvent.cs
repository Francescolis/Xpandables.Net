
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
/// Represents the the repository for events.
/// </summary>
public interface IRepositoryEvent
{
    /// <summary>
    /// Marks the specified entity event to be inserted to the data storage 
    /// on persistence according to the database provider/ORM.
    /// </summary>
    /// <param name="entity">The entity event to be added.</param>
    /// <param name="cancellationToken">A CancellationToken to observe while 
    /// waiting for the task to complete.</param>
    /// <returns>A task that represents an  asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="entity"/> is null.</exception>
    /// <exception cref="OperationCanceledException">If 
    /// the <see cref="CancellationToken" /> is canceled.</exception>
    /// <exception cref="InvalidOperationException"> The operation failed.
    /// See inner exception.</exception>
    Task InsertAsync(
        IEntityEvent entity,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns an enumerable of <see cref="IEntityEvent"/> type that match 
    /// the criteria and that can be asynchronously enumerated.
    /// If no result found, returns an empty enumerable.
    /// </summary>
    /// <param name="eventFilter">A function to test each element for a 
    /// condition.</param>
    /// <param name="cancellationToken">A CancellationToken 
    /// to observe while waiting for the task to complete.</param>
    /// <returns>A collection of <see cref="IEntityEvent"/>
    /// that can be asynchronously enumerated.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="eventFilter"/> is null.</exception>
    /// <exception cref="OperationCanceledException">
    /// If the <see cref="CancellationToken" /> is canceled.</exception>
    /// <exception cref="InvalidOperationException"> The operation failed.
    /// See inner exception.</exception>
    IAsyncEnumerable<IEntityEvent> FetchAsync(
        IEventFilter eventFilter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an event as published and add the exception if defined.
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
    Task MarkEventsAsPublishedAsync(
        Guid eventId,
        Exception? exception = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists all pending events to the data storage according 
    /// to the database provider/ORM.
    /// </summary>
    /// <param name="cancellationToken">A CancellationToken 
    /// to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the number of persisted objects
    /// .</returns>
    /// <exception cref="InvalidOperationException">All exceptions 
    /// related to the operation.</exception>
    /// <exception cref="OperationCanceledException">The 
    /// operation has been canceled.</exception>
    Task PersistAsync(CancellationToken cancellationToken = default);
}
