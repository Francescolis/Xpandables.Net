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

using Xpandables.Net.Collections;

namespace Xpandables.Net.Events;

/// <summary>
/// Represents the base class for an aggregate root in a domain-driven design.
/// </summary>
public abstract class Aggregate : IEventSourcing
{
    private readonly Dictionary<Type, Delegate> _eventHandlers = [];
    private readonly ConcurrentQueue<IDomainEvent> _uncommittedEvents = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="Aggregate" /> class.
    /// </summary>
    protected Aggregate() { }

    /// <summary>
    /// Gets the unique identifier of the aggregate root.
    /// </summary>
    public Guid KeyId { get; protected set; } = Guid.Empty;

    /// <summary>
    /// Gets the version of the aggregate root.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    public long StreamVersion { get; protected set; }

    /// <summary>
    /// Gets the business version of the aggregate (optional, for business logic).
    /// This is separate from the stream version and can be used for business versioning.
    /// </summary>
    public int BusinessVersion { get; protected set; } = 1;

    /// <summary>
    /// Gets a value indicating whether the aggregate root is empty.
    /// </summary>
    public bool IsEmpty => KeyId == Guid.Empty;

    /// <summary>
    /// Gets the expected stream version for optimistic concurrency control.
    /// </summary>
    public long ExpectedStreamVersion => StreamVersion;

    /// <inheritdoc />
    public IReadOnlyCollection<IDomainEvent> GetUncommittedEvents() =>
        [.. _uncommittedEvents];

    /// <inheritdoc />
    public void LoadFromHistory(IEnumerable<IDomainEvent> events) =>
        events.OrderBy(e => e.StreamVersion).ForEach(LoadFromHistory);

    /// <inheritdoc />
    public void LoadFromHistory(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        Mutate(domainEvent);

        StreamVersion = domainEvent.StreamVersion;

        UpdateBusinessVersionFromEvent(domainEvent);
    }

    /// <inheritdoc />
    public void MarkEventsAsCommitted() => _uncommittedEvents.Clear();

    /// <inheritdoc />
    public void PushEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        Apply(domainEvent);
    }

    /// <inheritdoc />
    public void PushVersioningEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        long nextStreamVersion = StreamVersion + 1;
        domainEvent = domainEvent
            .WithStreamVersion(nextStreamVersion)
            .WithAggregateId(KeyId);

        Apply(domainEvent);
    }

    /// <inheritdoc />
    public void PushVersioningEvent<TEvent>(Func<long, TEvent> eventFactory)
        where TEvent : notnull, IDomainEvent
    {
        ArgumentNullException.ThrowIfNull(eventFactory);

        long nextStreamVersion = StreamVersion + 1;
        TEvent domainEvent = eventFactory(nextStreamVersion);

        // Ensure the event has the correct stream version and aggregate information
        domainEvent = (TEvent)domainEvent
            .WithStreamVersion(nextStreamVersion)
            .WithAggregateId(KeyId);

        Apply(domainEvent);
    }

    /// <summary>
    /// Registers an event handler for a specific event type.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    /// <param name="handler">The event handler.</param>
    protected void On<TEvent>(Action<TEvent> handler)
        where TEvent : notnull, IDomainEvent => On(typeof(TEvent), handler);

    /// <summary>
    /// Registers the delegate for the specific event type.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    /// <param name="handler">The event handler.</param>
    protected void On<TEvent>(Delegate handler)
        where TEvent : notnull, IDomainEvent => On(typeof(TEvent), handler);

    /// <summary>
    /// Registers the delegate for the specified event type.
    /// </summary>
    /// <param name="eventType">The type of the event.</param>
    /// <param name="handler">The event handler.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when the event type is
    /// not an event domain.
    /// </exception>
    // ReSharper disable once MemberCanBePrivate.Global
    protected void On(Type eventType, Delegate handler)
    {
        ArgumentNullException.ThrowIfNull(eventType);
        ArgumentNullException.ThrowIfNull(handler);

        if (!typeof(IDomainEvent).IsAssignableFrom(eventType))
        {
            throw new ArgumentException(
                $"The type {eventType.Name} is not an event domain.");
        }

        _ = _eventHandlers.TryAdd(eventType, handler);
        return;
    }

    /// <summary>
    /// Override this method to implement custom business version logic based on events.
    /// </summary>
    /// <param name="domainEvent">The domain event being applied.</param>
    protected virtual void UpdateBusinessVersionFromEvent(IDomainEvent domainEvent)
    {
        // Default implementation: increment business version for certain event types
        // Override in derived classes for custom business versioning logic

        // Example: increment business version only for significant business events
        if (IsSignificantBusinessEvent(domainEvent))
        {
            BusinessVersion++;
        }
    }

    /// <summary>
    /// Determines if an event represents a significant business change.
    /// Override in derived classes to implement custom logic.
    /// </summary>
    /// <param name="domainEvent">The domain event to evaluate.</param>
    /// <returns>True if the event represents a significant business change.</returns>
    protected virtual bool IsSignificantBusinessEvent(IDomainEvent domainEvent) => false;

    private void Apply(IDomainEvent domainEvent)
    {
        if (_uncommittedEvents.Any(e => e.Id == domainEvent.Id))
        {
            return;
        }

        if (domainEvent.StreamVersion <= StreamVersion)
        {
            long nextStreamVersion = StreamVersion + 1;
            domainEvent = domainEvent.WithStreamVersion(nextStreamVersion);
        }

        Mutate(domainEvent);

        StreamVersion = domainEvent.StreamVersion;

        _uncommittedEvents.Enqueue(domainEvent);
    }

    private void Mutate(IDomainEvent domainEvent)
    {
        if (_eventHandlers.TryGetValue(domainEvent.GetType(), out Delegate? handler))
        {
            KeyId = domainEvent.AggregateId;
            _ = handler.DynamicInvoke(domainEvent);
        }
        else
        {
            throw new UnauthorizedAccessException(
                $"The submitted event {domainEvent.GetType().Name} is not authorized.");
        }
    }
}