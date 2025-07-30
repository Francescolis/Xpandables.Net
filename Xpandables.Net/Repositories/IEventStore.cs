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

using Xpandables.Net.Executions.Tasks;

namespace Xpandables.Net.Repositories;

/// <summary>
/// Defines a contract for an event store that supports asynchronous operations for appending, processing,  and querying
/// events. This interface extends <see cref="IRepository"/> to provide additional event-specific  functionality.
/// </summary>
/// <remarks>Implementations of <see cref="IEventStore"/> are expected to handle event persistence and retrieval 
/// efficiently, supporting scenarios where events need to be appended, marked as processed, or queried  based on
/// specific criteria. The interface provides methods for both single and batch operations,  ensuring flexibility in
/// handling event data.</remarks>
public interface IEventStore : IRepository
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
    /// <param name="cancellationToken">A token to monitor for cancellation requests. 
    /// The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task MarkAsProcessedAsync(IEnumerable<EventProcessedInfo> infos, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously fetches a sequence of event entities based on the specified filter.
    /// </summary>
    /// <remarks>You wil need to call <see cref="RepositoryExtensions.AsEventsAsync(IAsyncEnumerable{IEntityEvent}, CancellationToken)"/>
    /// to convert entities to events if necessary.</remarks>
    /// <typeparam name="TEntity">The type of the entity to query, which must implement <see cref="IEntity"/>.</typeparam>
    /// <typeparam name="TResult">The type of the result to return.</typeparam>
    /// <param name="filter">A function that defines the query to apply to the <typeparamref name="TEntity"/> collection.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. 
    /// The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>An asynchronous sequence of <typeparamref name="TResult"/> that matches the specified filter.</returns>
    new IAsyncEnumerable<TResult> FetchAsync<TEntity, TResult>(
        Func<IQueryable<TEntity>, IQueryable<TResult>> filter,
        CancellationToken cancellationToken = default)
        where TEntity : class, IEntity;

    // Hidden members from IRepository

    /// <summary>
    /// Asynchronously inserts a collection of entities into the repository.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entities to insert. Must implement <see cref="IEntity"/>.</typeparam>
    /// <param name="entities">The collection of entities to be inserted. Cannot be null.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. 
    /// The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous insert operation.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    new Task InsertAsync<TEntity>(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default)
        where TEntity : class, IEntity;

    /// <summary>
    /// Asynchronously updates a collection of entities in the data store.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entities to update. Must implement <see cref="IEntity"/>.</typeparam>
    /// <param name="entities">The collection of entities to be updated. Cannot be null.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. 
    /// The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous update operation.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    new Task UpdateAsync<TEntity>(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default)
        where TEntity : class, IEntity;

    /// <summary>
    /// Asynchronously deletes entities from the data source based on the specified filter.
    /// </summary>
    /// <remarks>This method is intended for internal use and may not be visible in all contexts.</remarks>
    /// <typeparam name="TEntity">The type of the entity to delete. Must implement <see cref="IEntity"/>.</typeparam>
    /// <param name="filter">A function to filter the entities to be deleted. The function should return a filtered <see
    /// cref="IQueryable{TEntity}"/>.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. 
    /// The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    new Task DeleteAsync<TEntity>(
        Func<IQueryable<TEntity>, IQueryable<TEntity>> filter,
        CancellationToken cancellationToken = default)
        where TEntity : class, IEntity;
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