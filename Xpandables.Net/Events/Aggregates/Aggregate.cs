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

namespace Xpandables.Net.Events.Aggregates;

/// <summary>
/// Represents an abstract base class for aggregates that handle domain events.
/// </summary>
public abstract class Aggregate : IAggregate
{
    private readonly ConcurrentQueue<IEventDomain> _uncommittedEvents = new();
    private readonly Dictionary<Type, Delegate> _eventHandlers = [];

    /// <inheritdoc/>
    public Guid KeyId { get; protected set; } = Guid.Empty;

    /// <inheritdoc/>
    public ulong Version { get; protected set; }

    /// <inheritdoc/>
    public bool IsEmpty => KeyId == Guid.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="Aggregate"/> class.
    /// </summary>
    protected Aggregate() { }

    /// <inheritdoc/>
    public IReadOnlyCollection<IEventDomain> GetUncommittedEvents() =>
        [.. _uncommittedEvents];

    /// <inheritdoc/>
    public void LoadFromHistory(IEnumerable<IEventDomain> events) =>
        events.ForEach(LoadFromHistory);

    /// <inheritdoc/>
    public void LoadFromHistory(IEventDomain @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        Mutate(@event);

        Version = @event.EventVersion;
    }

    /// <inheritdoc/>
    public void MarkEventsAsCommitted() => _uncommittedEvents.Clear();

    /// <inheritdoc/>
    public void ClearHandlers() => _eventHandlers.Clear();

    /// <inheritdoc/>
    public void PushEvent(IEventDomain @event)
    {
        ArgumentNullException.ThrowIfNull(@event);
        Apply(@event);
    }

    /// <summary>
    /// Registers an event handler for a specific event type.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    /// <param name="handler">The event handler.</param>
    public void On<TEvent>(Action<TEvent> handler)
        where TEvent : notnull, IEventDomain => On(typeof(TEvent), handler);

    /// <summary>
    /// Registers an event handler for a specific event type.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    /// <param name="handler">The event handler.</param>
    public void On<TEvent>(Delegate handler)
        where TEvent : notnull, IEventDomain => On(typeof(TEvent), handler);

    /// <summary>
    /// Registers an event handler for a specific event type.
    /// </summary>
    /// <param name="eventType">The type of the event.</param>
    /// <param name="handler">The event handler.</param>
    /// <exception cref="ArgumentException">Thrown when the event type is 
    /// not an event domain.</exception>
    public void On(Type eventType, Delegate handler)
    {
        ArgumentNullException.ThrowIfNull(eventType);
        ArgumentNullException.ThrowIfNull(handler);

        if (typeof(IEventDomain).IsAssignableFrom(eventType))
        {
            _ = _eventHandlers.TryAdd(eventType, handler);
            return;
        }

        throw new ArgumentException(
            $"The type {eventType.Name} is not an event domain.");
    }

    private void Apply(IEventDomain @event)
    {
        if (_uncommittedEvents.Any(e => e.EventId == @event.EventId))
        {
            return;
        }

        Version++;
        @event = @event.WithVersion(Version);

        Mutate(@event);

        _uncommittedEvents.Enqueue(@event);
    }
    private void Mutate(IEventDomain @event)
    {
        if (_eventHandlers.TryGetValue(@event.GetType(), out Delegate? handler))
        {
            KeyId = @event.AggregateId;
            _ = handler.DynamicInvoke(@event);
        }
        else
        {
            throw new UnauthorizedAccessException(
                $"The submitted action {@event.GetType().Name} is not authorized.");
        }
    }
}
