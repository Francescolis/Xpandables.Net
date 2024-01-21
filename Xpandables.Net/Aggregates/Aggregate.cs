
/************************************************************************************************************
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
************************************************************************************************************/
using System.Collections.Immutable;

using Xpandables.Net.Aggregates.DomainEvents;
using Xpandables.Net.Primitives.Collections;
using Xpandables.Net.Primitives.I18n;
using Xpandables.Net.Primitives.Text;

namespace Xpandables.Net.Aggregates;

/// <summary>
/// Represents a helper class that allows implementation of <see cref="IAggregate{TAggregateId}"/>.
/// <para>It contains a collection of <see cref="IDomainEvent{TAggregateId}"/>
/// and a dictionary of event handlers (each handler is registered using the <see cref="Aggregate{TAggregateId}.On{TEvent}(Action{TEvent})"/>
/// method).</para>
/// <para>You may register event handlers using  the <see cref="On{TEvent}(Action{TEvent})"/> or
/// <see cref="RegisterEventHandler{TEvent}(Delegate)"/>. </para>
/// You may use the <see cref="PushEvent{TEvent}(TEvent)"/> method to push the specified event.
/// </summary>
/// <typeparam name="TAggregateId">The type of aggregate Id.</typeparam>
public abstract partial class Aggregate<TAggregateId> : IAggregate<TAggregateId>
    where TAggregateId : struct, IAggregateId<TAggregateId>
{
    private readonly Queue<IDomainEvent<TAggregateId>> _events = new();
    private readonly Dictionary<Type, Delegate> _eventHandlers = [];

    /// <inheritdoc/>
    public ulong Version { get; protected set; }

    ///<inheritdoc/>
    public TAggregateId AggregateId { get; protected set; } = TAggregateId.DefaultInstance();

    /// <summary>
    /// Constructs the default instance of an aggregate root.
    /// </summary>
    protected Aggregate() { }

    /// <inheritdoc/>
    public void MarkEventsAsCommitted() => _events.Clear();

    /// <inheritdoc/>
    public IReadOnlyCollection<IDomainEvent<TAggregateId>> GetUncommittedEvents()
        => _events.OrderBy(o => o.Version).ToImmutableArray();

    /// <inheritdoc/>
    public void LoadFromHistory(IEnumerable<IDomainEvent<TAggregateId>> events)
    {
        ArgumentNullException.ThrowIfNull(events);

        foreach (IDomainEvent<TAggregateId> @event in events)
        {
            (this as IDomainEventSourcing<TAggregateId>).LoadFromHistory(@event);
        }
    }

    void IDomainEventSourcing<TAggregateId>.LoadFromHistory(
        IDomainEvent<TAggregateId> @event)
    {
        Mutate(@event);
        Version = @event.Version;
    }

    private void Apply(IDomainEvent<TAggregateId> @event)
    {
        if (_events.Any(e => Equals(e.Id, @event.Id)))
            return;

        Version++;
        @event = @event.WithVersion(Version);

        Mutate(@event);

        _events.Enqueue(@event);
    }

    private void Mutate(IDomainEvent<TAggregateId> @event)
    {
        if (!_eventHandlers.TryGetValue(@event.GetType(), out Delegate? messageHandler))
            return;

        AggregateId = @event.AggregateId;
        _ = messageHandler.DynamicInvoke(@event);
    }

    /// <summary>
    /// Pushes the specified domain event to the aggregate instance.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    /// <param name="event">The domain event instance to act on.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="event"/> is null.</exception>
    protected void PushEvent<TEvent>(TEvent @event)
        where TEvent : notnull, IDomainEvent<TAggregateId>
    {
        _ = @event ?? throw new ArgumentNullException(nameof(@event));

        Apply(@event);
    }

    /// <summary>
    /// Registers an handler for the <typeparamref name="TEvent"/> domain event type.
    /// </summary>
    /// <typeparam name="TEvent">The type of the domain event.</typeparam>
    /// <param name="eventHandler">the target handler to register.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="eventHandler"/> is null.</exception>
    /// <exception cref="ArgumentException">An element with the same 
    /// key already exist in the collection.</exception>
    protected void RegisterEventHandler<TEvent>(Delegate eventHandler)
        where TEvent : notnull, IDomainEvent<TAggregateId>
    {
        _ = eventHandler ?? throw new ArgumentNullException(nameof(eventHandler));
        RegisterEventHandler(typeof(TEvent), eventHandler);
    }

    /// <summary>
    /// Registers an handler for the specified domain event type.
    /// </summary>
    /// <param name="eventType">The domain event type, must implement 
    /// <see cref="IDomainEvent{TAggregateId}"/>.</param>
    /// <param name="eventHandler">the target handler to register.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="eventHandler"/> 
    /// is null.</exception>
    /// <exception cref="ArgumentException">An element with the same key 
    /// already exist in the collection.</exception>
    /// <exception cref="ArgumentException">The <paramref name="eventType"/> 
    /// does not implement <see cref="IDomainEvent{TAggregateId}"/> interface.</exception>
    protected void RegisterEventHandler(Type eventType, Delegate eventHandler)
    {
        _ = eventHandler ?? throw new ArgumentNullException(nameof(eventHandler));
        _ = eventType ?? throw new ArgumentNullException(nameof(eventType));

        if (!eventType.GetInterfaces().Exists(i => i.IsGenericType && i == typeof(IDomainEvent<TAggregateId>)))
            throw new ArgumentException(
                I18nXpandables.TypeMustImplement
                .StringFormat(
                    eventType.Name,
                    typeof(IDomainEvent<>).Name));

        if (!_eventHandlers.TryAdd(eventType, eventHandler))
            throw new ArgumentException(
                I18nXpandables.EventSourcingDomainEventAlreadyExist
                    .StringFormat(eventType.Name));
    }

    /// <summary>
    /// Registers the handler for the specific <typeparamref name="TEvent"/> domain event type.
    /// </summary>
    /// <typeparam name="TEvent">The type of the domain event to act on, 
    /// must implement <see cref="IDomainEvent{TAggregateId}"/>.</typeparam>
    /// <param name="handler">The target domain event handler to register.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="handler"/> is null.</exception>
    protected void On<TEvent>(Action<TEvent> handler)
        where TEvent : notnull, IDomainEvent<TAggregateId>
    {
        ArgumentNullException.ThrowIfNull(handler);

        RegisterEventHandler<TEvent>(handler);
    }
}