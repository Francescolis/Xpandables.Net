
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
/// Represents the base class for an aggregate in a domain-driven design (DDD) context.
/// </summary>
/// <remarks>The <see cref="Aggregate"/> class provides a foundation for implementing aggregates, which are the 
/// central concept in DDD for encapsulating domain logic and ensuring consistency within a bounded context.  It manages
/// the state of the aggregate, tracks uncommitted domain events, and provides mechanisms for  replaying and applying
/// events to maintain the aggregate's state. <para> This class is designed to be extended by specific aggregate
/// implementations, which define the domain-specific  behavior and event handling logic. It enforces key invariants
/// such as monotonic stream versioning and ensures  that the aggregate's identity is properly initialized.
/// </para></remarks>
public abstract class Aggregate : IAggregate
{
    private readonly Dictionary<Type, Delegate> _eventHandlers = [];
    private readonly ConcurrentQueue<IDomainEvent> _uncommittedEvents = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="Aggregate" /> class.
    /// </summary>
    protected Aggregate() { }

    /// <inheritdoc />
    public Guid StreamId { get; protected set; } = Guid.Empty;

    /// <inheritdoc />
    public virtual string StreamName => GetType().Name;

    /// <inheritdoc />
    public long StreamVersion { get; protected set; } = -1;

    /// <inheritdoc />
    public int BusinessVersion { get; protected set; } = 1;

    /// <inheritdoc />
    public bool IsEmpty => StreamId == Guid.Empty;

    /// <inheritdoc />
    public long ExpectedStreamVersion => StreamVersion;

    /// <inheritdoc />
    public IReadOnlyCollection<IDomainEvent> GetUncommittedEvents() =>
        [.. _uncommittedEvents];

    /// <inheritdoc />
    public IReadOnlyCollection<IDomainEvent> DequeueUncommittedEvents()
    {
        var events = GetUncommittedEvents();
        if (events.Count > 0)
        {
            MarkEventsAsCommitted();
        }

        return events;
    }

    /// <inheritdoc />
    public void Replay(IEnumerable<IDomainEvent> history)
    {
        ArgumentNullException.ThrowIfNull(history);
        LoadFromHistory(history);
    }

    /// <inheritdoc />
    public void LoadFromHistory(IEnumerable<IDomainEvent> events)
    {
        ArgumentNullException.ThrowIfNull(events);

        var eventsArray = events as IDomainEvent[] ?? [.. events];
        if (eventsArray.Length == 0) return;

        var span = eventsArray.AsSpan();
        span.Sort(static (a, b) => a.StreamVersion.CompareTo(b.StreamVersion));

        foreach (ref readonly var domainEvent in span)
        {
            LoadFromHistory(domainEvent);
        }
    }

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
        PushVersioningEvent(domainEvent);
    }

    /// <inheritdoc />
    public void PushVersioningEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        long nextStreamVersion = StreamVersion + 1;

        // Enforce identity for the very first event.
        if (domainEvent.StreamId == Guid.Empty && StreamId == Guid.Empty)
        {
            throw new InvalidOperationException(
                "Aggregate is not initialized. The first event must carry a non-empty StreamId.");
        }

        // Propagate existing KeyId when event didn't carry one.
        if (domainEvent.StreamId == Guid.Empty && StreamId != Guid.Empty)
        {
            domainEvent = domainEvent.WithStreamId(StreamId);
        }

        if (string.IsNullOrWhiteSpace(domainEvent.StreamName))
        {
            domainEvent = domainEvent.WithStreamName(StreamName);
        }

        domainEvent = domainEvent.WithStreamVersion(nextStreamVersion);
        Apply(domainEvent);
    }

    /// <inheritdoc />
    public void PushVersioningEvent<TEvent>(Func<long, TEvent> eventFactory)
        where TEvent : notnull, IDomainEvent
    {
        ArgumentNullException.ThrowIfNull(eventFactory);

        long nextStreamVersion = StreamVersion + 1;
        TEvent domainEvent = eventFactory(nextStreamVersion);

        if (domainEvent.StreamId == Guid.Empty && StreamId == Guid.Empty)
        {
            throw new InvalidOperationException(
                "Aggregate is not initialized. The first event must carry a non-empty StreamId.");
        }

        if (domainEvent.StreamId == Guid.Empty && StreamId != Guid.Empty)
        {
            domainEvent = (TEvent)domainEvent.WithStreamId(StreamId);
        }

        if (string.IsNullOrWhiteSpace(domainEvent.StreamName))
        {
            domainEvent = (TEvent)domainEvent.WithStreamName(StreamName);
        }

        domainEvent = (TEvent)domainEvent.WithStreamVersion(nextStreamVersion);
        Apply(domainEvent);
    }

    /// <summary>
    /// When receiving an event of type <typeparamref name="TEvent"/>, invokes the specified delegate.
    /// </summary>
    /// <typeparam name="TEvent">The type of domain event to handle. Must implement IDomainEvent and cannot be null.</typeparam>
    /// <param name="handler">The delegate to invoke when an event of type TEvent is raised. Cannot be null.</param>
    protected void On<TEvent>(Action<TEvent> handler)
        where TEvent : notnull, IDomainEvent
    {
        ArgumentNullException.ThrowIfNull(handler);
        On(typeof(TEvent), handler);
    }

    /// <summary>
    /// When receiving an event of type <typeparamref name="TEvent"/>, invokes the specified delegate.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    /// <param name="handler">The event handler.</param>
    protected void On<TEvent>(Delegate handler)
        where TEvent : notnull, IDomainEvent
    {
        ArgumentNullException.ThrowIfNull(handler);
        On(typeof(TEvent), handler);
    }

    /// <summary>
    /// when receiving an event of the specified <paramref name="eventType"/>, invokes the specified <paramref name="handler"/>.
    /// </summary>
    /// <param name="eventType">The type of the event.</param>
    /// <param name="handler">The event handler.</param>
    /// <exception cref="ArgumentException">Thrown when the type is not an <see cref="IDomainEvent"/>.</exception>
    protected void On(Type eventType, Delegate handler)
    {
        ArgumentNullException.ThrowIfNull(eventType);
        ArgumentNullException.ThrowIfNull(handler);

        if (!typeof(IDomainEvent).IsAssignableFrom(eventType))
        {
            throw new ArgumentException($"The type {eventType.Name} is not an event domain.");
        }

        _ = _eventHandlers.TryAdd(eventType, handler);
    }

    /// <summary>
    /// Override to implement custom business version logic based on events.
    /// </summary>
    /// <param name="domainEvent">The domain event being applied.</param>
    protected virtual void UpdateBusinessVersionFromEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        if (IsSignificantBusinessEvent(domainEvent))
        {
            BusinessVersion++;
        }
    }

    /// <summary>
    /// Determines if an event represents a significant business change.
    /// </summary>
    /// <param name="domainEvent">The domain event to evaluate.</param>
    protected virtual bool IsSignificantBusinessEvent(IDomainEvent domainEvent) => false;

    private void Apply(IDomainEvent domainEvent)
    {
        // Idempotency for current unit: ignore duplicate event Ids.
        if (_uncommittedEvents.Any(e => e.EventId == domainEvent.EventId))
        {
            return;
        }

        // Ensure monotonic versioning for newly raised events.
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
            // Capture identity at first application.
            if (StreamId == Guid.Empty && domainEvent.StreamId != Guid.Empty)
            {
                StreamId = domainEvent.StreamId;
            }

            _ = handler.DynamicInvoke(domainEvent);
        }
        else
        {
            throw new UnauthorizedAccessException(
                $"The submitted event {domainEvent.GetType().Name} is not authorized.");
        }
    }
}