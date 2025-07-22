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

using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Text.Json;

using Xpandables.Net.Executions.Tasks;
using Xpandables.Net.Repositories.Converters;
using Xpandables.Net.Text;

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
    /// <code>
    /// // Example usage:
    /// var results = await repository.FetchAsync&lt;User, object&gt;(
    ///     query => query
    ///         .Where(u => u.IsActive)
    ///         .Include(u => u.Profile)
    ///         .OrderBy(u => u.LastName)
    ///         .Skip(10)
    ///         .Take(20)
    ///         .Select(u => new { u.Id, u.FirstName, u.LastName, u.Email })
    /// ).ToListAsync();
    /// </code>
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
        Func<IQueryable<TEntityEvent>, IAsyncQueryable<TEvent>> filter,
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

/// <summary>
/// Provides extension methods for querying event data from entities implementing <see cref="IEntityEvent"/>.
/// </summary>
/// <remarks>This static class contains methods that facilitate the projection of event data from a sequence of
/// entities into a sequence of events, using default serialization options.</remarks>
public static class IEventStoreExtensions
{
    /// <summary>
    /// Thread-safe collection to track active event stores and their associated caches.
    /// </summary>
    private static readonly ConcurrentDictionary<WeakReference, ConcurrentDictionary<string, Type>> s_eventStoreCaches = new();

    /// <summary>
    /// Projects each element of the queryable sequence into an event using a custom query provider.
    /// </summary>
    /// <remarks>This method creates a custom expression tree that can be properly translated by LINQ providers,
    /// enabling better query optimization and database-level operations when supported.</remarks>
    /// <typeparam name="TEntity">The type of the entity containing event data, which must implement <see cref="IEntityEvent"/>.</typeparam>
    /// <param name="source">The queryable sequence of entities from which to select events.</param>
    /// <returns>An <see cref="IQueryable{T}"/> of events.</returns>
    public static IAsyncQueryable<IEvent> SelectEvent<TEntity>(this IQueryable<TEntity> source)
        where TEntity : class, IEntityEvent
    {
        ArgumentNullException.ThrowIfNull(source);

        // Create a simple projection expression that only accesses entity properties
        // This can be translated to SQL by Entity Framework
        var entityParameter = Expression.Parameter(typeof(TEntity), "entity");
        var projectionExpression = Expression.New(
            typeof(EventEntityProjection).GetConstructor([typeof(string), typeof(JsonDocument)])!,
            Expression.Property(entityParameter, nameof(IEntityEvent.EventFullName)),
            Expression.Property(entityParameter, nameof(IEntityEvent.EventData)));

        var projectionLambda = Expression.Lambda<Func<TEntity, EventEntityProjection>>(
            projectionExpression, entityParameter);

        // Use the provider to create a query that projects to our simple type
        var selectMethodInfo = typeof(Queryable).GetMethods()
            .First(m => m.Name == "Select" && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(TEntity), typeof(EventEntityProjection));

        var selectExpression = Expression.Call(
            null,
            selectMethodInfo,
            source.Expression,
            Expression.Quote(projectionLambda));

        var projectionQuery = source.Provider.CreateQuery<EventEntityProjection>(selectExpression);

        // Convert to events on the client side using AsEnumerable()
        return projectionQuery.ToAsyncEnumerable()
            .Select(projection =>
            {
                Type concreteEventType = GetOrResolveEventType(projection.EventFullName);

                return EventConverter.DeserializeEvent(
                    projection.EventData,
                    concreteEventType,
                    DefaultSerializerOptions.Defaults);
            }).AsAsyncQueryable();
    }

    /// <summary>
    /// Gets the event type from cache or resolves it and adds to cache for future use.
    /// </summary>
    /// <param name="eventFullName">The full name of the event type.</param>
    /// <returns>The resolved event type.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the event type cannot be found.</exception>
    private static Type GetOrResolveEventType(string eventFullName) =>
        // Use a simple static cache since we don't have access to EventStore instance here
        // This will be managed through the cleanup methods
        EventTypeCache.GetOrAdd(eventFullName, fullName =>
        {
            Type? eventType = Type.GetType(fullName);
            return eventType ?? throw new InvalidOperationException(
                $"The event type '{fullName}' could not be found. " +
                $"Ensure it is referenced and available at runtime.");
        });

    /// <summary>
    /// Registers an event store instance to enable automatic cache cleanup on disposal.
    /// This method should be called when an EventStore is created.
    /// </summary>
    /// <param name="eventStore">The event store instance to register.</param>
    internal static void RegisterEventStore(IEventStore eventStore)
    {
        ArgumentNullException.ThrowIfNull(eventStore);

        var weakRef = new WeakReference(eventStore);
        var cache = new ConcurrentDictionary<string, Type>();
        s_eventStoreCaches.TryAdd(weakRef, cache);

        // Clean up any dead references
        CleanupDeadReferences();
    }

    /// <summary>
    /// Unregisters an event store instance and clears its associated cache.
    /// This method should be called when an EventStore is disposed.
    /// </summary>
    /// <param name="eventStore">The event store instance to unregister.</param>
    internal static void UnregisterEventStore(IEventStore eventStore)
    {
        ArgumentNullException.ThrowIfNull(eventStore);

        var keysToRemove = s_eventStoreCaches.Keys
            .Where(key => ReferenceEquals(key.Target, eventStore))
            .ToList();

        foreach (var key in keysToRemove)
        {
            s_eventStoreCaches.TryRemove(key, out _);
        }

        // If no more event stores are registered, clear the main cache
        if (s_eventStoreCaches.IsEmpty)
        {
            EventTypeCache.Clear();
        }
    }

    /// <summary>
    /// Removes weak references that point to garbage collected objects.
    /// </summary>
    private static void CleanupDeadReferences()
    {
        var deadKeys = s_eventStoreCaches.Keys
            .Where(key => !key.IsAlive)
            .ToList();

        foreach (var deadKey in deadKeys)
        {
            s_eventStoreCaches.TryRemove(deadKey, out _);
        }
    }

    /// <summary>
    /// Gets the current count of cached event types.
    /// </summary>
    /// <returns>The number of event types currently cached.</returns>
    public static int CachedEventTypesCount => EventTypeCache.Count;

    /// <summary>
    /// Gets the current count of registered event stores.
    /// </summary>
    /// <returns>The number of event stores currently registered.</returns>
    internal static int GetRegisteredEventStoresCount()
    {
        CleanupDeadReferences();
        return s_eventStoreCaches.Count;
    }

    /// <summary>
    /// Central cache for event types with automatic cleanup capabilities.
    /// </summary>
    private static class EventTypeCache
    {
        private static readonly ConcurrentDictionary<string, Type> s_cache = new();

        public static Type GetOrAdd(string key, Func<string, Type> valueFactory) =>
            s_cache.GetOrAdd(key, valueFactory);

        public static void Clear() => s_cache.Clear();

        public static int Count => s_cache.Count;
    }

    /// <summary>
    /// Represents a simple projection of entity event data that can be translated by LINQ providers.
    /// </summary>
    /// <param name="EventFullName">The full name of the event type.</param>
    /// <param name="EventData">The serialized event data.</param>
    private readonly record struct EventEntityProjection(string EventFullName, JsonDocument EventData);
}