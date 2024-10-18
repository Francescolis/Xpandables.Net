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

using Xpandables.Net.Collections;
using Xpandables.Net.States;

namespace Xpandables.Net.Events.Aggregates;

/// <summary>
/// Represents an abstract base class for aggregates that handle domain events.
/// </summary>
public abstract class Aggregate : IAggregate
{
    private readonly Queue<IEventDomain> _uncommittedEvents = new();
    private readonly Dictionary<Type, Delegate> _eventHandlers = [];

    /// <inheritdoc/>
    public object AggregateId { get; protected set; } = default!;

    /// <inheritdoc/>
    public ulong Version { get; protected set; }

    /// <inheritdoc/>
    public bool IsEmpty => AggregateId is null;

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
        Mutate(@event);

        Version = @event.EventVersion;
    }

    /// <inheritdoc/>
    public void MarkEventsAsCommitted() => _uncommittedEvents.Clear();

    /// <inheritdoc/>
    public void PushEvent(IEventDomain @event) => Apply(@event);

    /// <summary>
    /// Registers an event handler for a specific event type.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    /// <param name="handler">The event handler.</param>
    protected void On<TEvent>(Action<TEvent> handler)
        where TEvent : notnull, IEventDomain => On(typeof(TEvent), handler);

    /// <summary>
    /// Registers an event handler for a specific event type.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    /// <param name="handler">The event handler.</param>
    protected void On<TEvent>(Delegate handler)
        where TEvent : notnull, IEventDomain => On(typeof(TEvent), handler);

    /// <summary>
    /// Registers an event handler for a specific event type.
    /// </summary>
    /// <param name="eventType">The type of the event.</param>
    /// <param name="handler">The event handler.</param>
    /// <exception cref="ArgumentException">Thrown when the event type is 
    /// not an event domain.</exception>
    protected void On(Type eventType, Delegate handler)
    {
        if (typeof(IEventDomain).IsAssignableFrom(eventType))
        {
            _ = _eventHandlers.TryAdd(eventType, handler);
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
            AggregateId = @event.AggregateId;
            _ = handler.DynamicInvoke(@event);
        }
    }
}

/// <summary>
/// Represents an abstract base class for aggregates with a specific aggregate ID type.
/// </summary>
/// <typeparam name="TAggregateId">The type of the aggregate ID.</typeparam>
public abstract class Aggregate<TAggregateId> : Aggregate, IAggregate<TAggregateId>
    where TAggregateId : struct
{
    /// <inheritdoc/>
    public new TAggregateId AggregateId
    {
        get => (TAggregateId)base.AggregateId;
        protected set => base.AggregateId = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Aggregate{TAggregateId}"/> class.
    /// </summary>
    protected Aggregate() { }
}

/// <summary>
/// Represents an abstract base class for aggregates with a specific state context.
/// </summary>
/// <typeparam name="TAggregate">The type of the aggregate.</typeparam>
/// <typeparam name="TAggregateId">The type of the aggregate ID.</typeparam>
/// <typeparam name="TAggregateState">The type of the aggregate state.</typeparam>
public abstract class AggregateStateContext<TAggregate, TAggregateState, TAggregateId> :
    Aggregate<TAggregateId>,
    IStateContext<TAggregateState>
    where TAggregate : AggregateStateContext<TAggregate, TAggregateState, TAggregateId>
    where TAggregateId : struct
    where TAggregateState : class, IState
{
    /// <inheritdoc/>
    public TAggregateState CurrentState { get; protected set; } = default!;

    /// <summary>
    /// Initializes a new instance of the 
    /// <see cref="AggregateStateContext{TAggregate, TAggregateId, TAggregateState}"/> class.
    /// </summary>
    /// <param name="startState">The initial state of the aggregate.</param>
    protected AggregateStateContext(TAggregateState startState) =>
        TransitionToState(startState);

    /// <inheritdoc/>
    public void TransitionToState(TAggregateState state)
    {
        CurrentState = state ?? throw new ArgumentNullException(nameof(state));
        CurrentState.EnterStateContext(this);
    }
}