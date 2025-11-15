/*******************************************************************************
 * Copyright (C) 2025 Kamersoft
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
namespace Xpandables.Net.EventSourcing;

/// <summary>
/// Defines the contract for subscribing to and unsubscribing from event notifications within an event-driven system.
/// </summary>
/// <remarks>Implementations of this interface allow consumers to register event handlers for specific event types
/// and to remove those handlers when they are no longer needed. Handlers can be registered as delegates or as event
/// handler objects, and may be disposed of via IDisposable for scoped subscriptions. This interface is typically used
/// to decouple event publishers from subscribers, enabling flexible and testable event handling
/// architectures.</remarks>
public interface IEventSubscriber
{
    /// <summary>
    /// Registers a handler to be invoked when an event of type <typeparamref name="TEvent"/> is published.
    /// </summary>
    /// <remarks>Multiple handlers can be registered for the same event type. Handlers are called in the order
    /// they were subscribed.</remarks>
    /// <typeparam name="TEvent">The type of event to subscribe to. Must be a reference type that implements <see cref="IEvent"/>.</typeparam>
    /// <param name="handler">The delegate to invoke when an event of type <typeparamref name="TEvent"/> is received. Cannot be null.</param>
    void Subscribe<TEvent>(Action<TEvent> handler)
        where TEvent : class, IEvent;

    /// <summary>
    /// Registers an asynchronous handler to be invoked when events of the specified type are published.
    /// </summary>
    /// <remarks>Multiple handlers can be registered for the same event type. Handlers are invoked
    /// asynchronously when an event is published. The order in which handlers are invoked is not guaranteed.</remarks>
    /// <typeparam name="TEvent">The type of event to subscribe to. Must implement <see cref="IEvent"/> and be a reference type.</typeparam>
    /// <param name="handler">A function that processes the event. The function receives the event instance and a <see
    /// cref="CancellationToken"/> for cooperative cancellation. Cannot be null.</param>
    void Subscribe<TEvent>(Func<TEvent, CancellationToken, Task> handler)
        where TEvent : class, IEvent;

    /// <summary>
    /// Registers an event handler to receive notifications for events of the specified type.
    /// </summary>
    /// <remarks>Multiple handlers can be registered for the same event type. Handlers are invoked in the
    /// order they are subscribed. This method is not thread-safe; external synchronization may be required when
    /// subscribing from multiple threads.</remarks>
    /// <typeparam name="TEvent">The type of event to subscribe to. Must be a reference type that implements <see cref="IEvent"/>.</typeparam>
    /// <param name="handler">The event handler instance that will be invoked when an event of type <typeparamref name="TEvent"/> is
    /// published. Cannot be null.</param>
    void Subscribe<TEvent>(IEventHandler<TEvent> handler)
        where TEvent : class, IEvent;

    /// <summary>
    /// Unsubscribes the specified event handler from receiving notifications for events of type <typeparamref
    /// name="TEvent"/>.
    /// </summary>
    /// <remarks>If the specified handler was not previously subscribed, the method returns false and no
    /// action is taken. This method is thread-safe.</remarks>
    /// <typeparam name="TEvent">The type of event to unsubscribe from. Must be a reference type that implements <see cref="IEvent"/>.</typeparam>
    /// <param name="handler">The delegate to remove from the event subscription. Cannot be null.</param>
    /// <returns>true if the handler was successfully unsubscribed; otherwise, false.</returns>
    bool Unsubscribe<TEvent>(Action<TEvent> handler)
        where TEvent : class, IEvent;

    /// <summary>
    /// Unsubscribes the specified event handler from receiving events of type <typeparamref name="TEvent"/>.
    /// </summary>
    /// <typeparam name="TEvent">The type of event to unsubscribe from. Must be a reference type that implements <see cref="IEvent"/>.</typeparam>
    /// <param name="handler">The event handler to remove from the subscription. The handler must match the signature <c>Func&lt;TEvent,
    /// CancellationToken, Task&gt;</c>.</param>
    /// <returns>true if the handler was successfully unsubscribed; otherwise, false.</returns>
    bool Unsubscribe<TEvent>(Func<TEvent, CancellationToken, Task> handler)
        where TEvent : class, IEvent;

    /// <summary>
    /// Removes the specified event handler from the subscription list for the given event type.
    /// </summary>
    /// <remarks>If the handler was not previously subscribed, the method returns false and no action is
    /// taken. This method is thread-safe.</remarks>
    /// <typeparam name="TEvent">The type of event to unsubscribe from. Must implement <see cref="IEvent"/>.</typeparam>
    /// <param name="handler">The event handler to remove from the subscription list. Cannot be null.</param>
    /// <returns>true if the handler was successfully removed; otherwise, false.</returns>
    bool Unsubscribe<TEvent>(IEventHandler<TEvent> handler)
        where TEvent : class, IEvent;

    /// <summary>
    /// Subscribes the specified event handler to receive notifications for events of type <typeparamref
    /// name="TEvent"/>.
    /// </summary>
    /// <typeparam name="TEvent">The type of event to subscribe to. Must implement <see cref="IEvent"/> and be a reference type.</typeparam>
    /// <param name="handler">The action to invoke when an event of type <typeparamref name="TEvent"/> is published. Cannot be null.</param>
    /// <returns>An <see cref="IDisposable"/> that can be used to unsubscribe the handler from future event notifications.</returns>
    IDisposable SubscribeDisposable<TEvent>(Action<TEvent> handler)
        where TEvent : class, IEvent;

    /// <summary>
    /// Subscribes to events of type <typeparamref name="TEvent"/> using the specified asynchronous handler, and returns
    /// a disposable object that can be used to unsubscribe.
    /// </summary>
    /// <remarks>The returned <see cref="IDisposable"/> should be disposed when event handling is no longer
    /// required to prevent resource leaks. The handler may be invoked concurrently for different events; ensure thread
    /// safety if necessary.</remarks>
    /// <typeparam name="TEvent">The type of event to subscribe to. Must implement <see cref="IEvent"/> and be a reference type.</typeparam>
    /// <param name="handler">An asynchronous delegate that handles events of type <typeparamref name="TEvent"/>. The delegate receives the
    /// event instance and a <see cref="CancellationToken"/> for cooperative cancellation.</param>
    /// <returns>An <see cref="IDisposable"/> that unsubscribes the handler when disposed.</returns>
    IDisposable SubscribeDisposable<TEvent>(Func<TEvent, CancellationToken, Task> handler)
        where TEvent : class, IEvent;

    /// <summary>
    /// Subscribes the specified event handler to receive events of type TEvent and returns a disposable object that can
    /// be used to unsubscribe.
    /// </summary>
    /// <typeparam name="TEvent">The type of event to subscribe to. Must implement IEvent and be a reference type.</typeparam>
    /// <param name="handler">The event handler that will be invoked when an event of type TEvent is published. Cannot be null.</param>
    /// <returns>An IDisposable that, when disposed, unsubscribes the handler from receiving further events of type TEvent.</returns>
    IDisposable SubscribeDisposable<TEvent>(IEventHandler<TEvent> handler)
        where TEvent : class, IEvent;
}
