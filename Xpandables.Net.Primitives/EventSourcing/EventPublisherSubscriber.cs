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
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Xpandables.Net.EventSourcing;

/// <summary>
/// Provides functionalities for event-based publish-subscribe communication.
/// Manages subscriptions for various event types and facilitates publishing events
/// to the subscribed handlers.
/// </summary>
public sealed class EventPublisherSubscriber(
    IEventHandlerRegistry handlerRegistry) : Disposable, IEventPublisher, IEventSubscriber
{
    private readonly ConcurrentDictionary<Type, EventHandlerCollection> _subscribers = new();

    /// <inheritdoc/>
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, IEvent
    {
        ArgumentNullException.ThrowIfNull(@event);

        var eventType = @event.GetType();
        var tasks = new List<Task>();
        var handlers = GetHandlersOf(eventType);

        foreach (var action in handlers.SyncHandlers.Cast<Action<TEvent>>())
        {
            tasks.Add(ExecuteHandlerSafelyAsync(() => { action(@event); return Task.CompletedTask; }));
        }

        foreach (var func in handlers.AsyncHandlers.Cast<Func<TEvent, CancellationToken, Task>>())
        {
            tasks.Add(ExecuteHandlerSafelyAsync(() => func(@event, cancellationToken)));
        }

        foreach (var serviceHandler in handlers.ServiceHandlers.Cast<IEventHandler<TEvent>>())
        {
            tasks.Add(ExecuteHandlerSafelyAsync(() => serviceHandler.HandleAsync(@event, cancellationToken)));
        }

        if (handlerRegistry.TryGetWrapper(eventType, out var wrapper))
        {
            tasks.Add(ExecuteHandlerSafelyAsync(() => wrapper.HandleAsync(@event, cancellationToken)));
        }

        if (tasks.Count > 0)
        {
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
        else
        {
            Trace.WriteLine($"No handlers found for event type {eventType.Name}. Event ID: {@event.EventId}");
        }
    }

    /// <inheritdoc/>
    public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class, IEvent
        => GetOrCreateHandlersOf<TEvent>().AddSyncHandler(handler);

    /// <inheritdoc/>
    public void Subscribe<TEvent>(Func<TEvent, CancellationToken, Task> handler) where TEvent : class, IEvent
        => GetOrCreateHandlersOf<TEvent>().AddAsyncHandler(handler);

    /// <inheritdoc/>
    public void Subscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : class, IEvent
        => GetOrCreateHandlersOf<TEvent>().AddServiceHandler(handler);

    /// <inheritdoc/>
    public IDisposable SubscribeDisposable<TEvent>(Action<TEvent> handler) where TEvent : class, IEvent
    {
        Subscribe(handler);
        return new SubscriptionToken<TEvent>(this, handler, HandlerType.Sync);
    }

    /// <inheritdoc/>
    public IDisposable SubscribeDisposable<TEvent>(Func<TEvent, CancellationToken, Task> handler) where TEvent : class, IEvent
    {
        Subscribe(handler);
        return new SubscriptionToken<TEvent>(this, handler, HandlerType.Async);
    }

    /// <inheritdoc/>
    public IDisposable SubscribeDisposable<TEvent>(IEventHandler<TEvent> handler) where TEvent : class, IEvent
    {
        Subscribe(handler);
        return new SubscriptionToken<TEvent>(this, handler, HandlerType.Service);
    }

    /// <inheritdoc/>
    public bool Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : class, IEvent
        => _subscribers.TryGetValue(typeof(TEvent), out var handlers) && handlers.RemoveSyncHandler(handler);

    /// <inheritdoc/>
    public bool Unsubscribe<TEvent>(Func<TEvent, CancellationToken, Task> handler) where TEvent : class, IEvent
        => _subscribers.TryGetValue(typeof(TEvent), out var handlers) && handlers.RemoveAsyncHandler(handler);

    /// <inheritdoc/>
    public bool Unsubscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : class, IEvent
        => _subscribers.TryGetValue(typeof(TEvent), out var handlers) && handlers.RemoveServiceHandler(handler);

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _subscribers.Clear();
        }

        base.Dispose(disposing);
    }

    private static async Task ExecuteHandlerSafelyAsync(Func<Task> handler)
        => await handler().ConfigureAwait(false);

    private EventHandlerCollection GetOrCreateHandlersOf<TEvent>() where TEvent : class, IEvent
        => _subscribers.GetOrAdd(typeof(TEvent), _ => new EventHandlerCollection());

    private EventHandlerCollection GetHandlersOf(Type eventType)
        => _subscribers.TryGetValue(eventType, out var handlers) ? handlers : EventHandlerCollection.Empty;

    private sealed class EventHandlerCollection
    {
        public static readonly EventHandlerCollection Empty = new();

        private readonly ConcurrentDictionary<object, byte> _syncHandlers = new();
        private readonly ConcurrentDictionary<object, byte> _asyncHandlers = new();
        private readonly ConcurrentDictionary<object, byte> _serviceHandlers = new();

        public IEnumerable<object> SyncHandlers => _syncHandlers.Keys;
        public IEnumerable<object> AsyncHandlers => _asyncHandlers.Keys;
        public IEnumerable<object> ServiceHandlers => _serviceHandlers.Keys;

        public int Count => _syncHandlers.Count + _asyncHandlers.Count + _serviceHandlers.Count;
        public bool IsEmpty => Count == 0;

        public void AddSyncHandler(object handler) => _syncHandlers.TryAdd(handler, 0);
        public void AddAsyncHandler(object handler) => _asyncHandlers.TryAdd(handler, 0);
        public void AddServiceHandler(object handler) => _serviceHandlers.TryAdd(handler, 0);

        public bool RemoveSyncHandler(object handler) => _syncHandlers.TryRemove(handler, out _);
        public bool RemoveAsyncHandler(object handler) => _asyncHandlers.TryRemove(handler, out _);
        public bool RemoveServiceHandler(object handler) => _serviceHandlers.TryRemove(handler, out _);
    }

    private enum HandlerType { Sync, Async, Service }

    private sealed class SubscriptionToken<TEvent>(
        EventPublisherSubscriber subscriber,
        object handler,
        HandlerType handlerType) : IDisposable
        where TEvent : class, IEvent
    {
#pragma warning disable CA2213 // Disposable fields should be disposed
        private readonly EventPublisherSubscriber _subscriber = subscriber;
#pragma warning restore CA2213 // Disposable fields should be disposed
        private readonly object _handler = handler;
        private readonly HandlerType _handlerType = handlerType;
        private volatile bool _disposed;

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            var success = _handlerType switch
            {
                HandlerType.Sync => _subscriber.Unsubscribe((Action<TEvent>)_handler),
                HandlerType.Async => _subscriber.Unsubscribe((Func<TEvent, CancellationToken, Task>)_handler),
                HandlerType.Service => _subscriber.Unsubscribe((IEventHandler<TEvent>)_handler),
                _ => false
            };

            if (!success)
            {
                Trace.WriteLine($"Failed to unsubscribe handler of type {_handlerType} for event {typeof(TEvent).Name}.");
            }
        }
    }
}