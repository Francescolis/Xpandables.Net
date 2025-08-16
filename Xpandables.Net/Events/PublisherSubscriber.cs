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
using System.Diagnostics;
using System.Runtime.CompilerServices;

using Microsoft.Extensions.DependencyInjection;

namespace Xpandables.Net.Events;

/// <summary>
/// Provides functionalities for event-based publish-subscribe communication.
/// Manages subscriptions for various event types and facilitates publishing events
/// to the subscribed handlers.
/// </summary>
public sealed class PublisherSubscriber(IServiceProvider serviceProvider) : Disposable, IPublisher, ISubscriber
{
    private readonly ConcurrentDictionary<Type, EventHandlerCollection> _subscribers = new();
    private readonly ConcurrentDictionary<Type, object[]> _serviceHandlerCache = new();

    /// <inheritdoc />
    public async Task PublishAsync<TEvent>(
        TEvent @event,
        CancellationToken cancellationToken = default)
        where TEvent : class, IEvent
    {
        ArgumentNullException.ThrowIfNull(@event);

        try
        {
            var eventType = @event.GetType();
            var handlers = GetHandlersOf(eventType);

            var tasks = new List<Task>(handlers.Count);

            if (!handlers.IsEmpty)
            {
                // Process synchronous action handlers
                foreach (var action in handlers.SyncHandlers)
                {
                    tasks.Add(ExecuteHandlerSafelyAsync(() =>
                    {
                        InvokeAction(action, @event, eventType);
                        return Task.CompletedTask;
                    }));
                }

                // Process asynchronous function handlers
                foreach (var func in handlers.AsyncHandlers)
                {
                    tasks.Add(ExecuteHandlerSafelyAsync(() =>
                        InvokeAsyncFunc(func, @event, eventType, cancellationToken)));
                }

                // Process service handlers
                foreach (var serviceHandler in handlers.ServiceHandlers)
                {
                    tasks.Add(ExecuteHandlerSafelyAsync(() =>
                        InvokeServiceHandler(serviceHandler, @event, eventType, cancellationToken)));
                }
            }

            // Process dependency injection handlers
            var diHandlers = GetServiceHandlers(eventType);
            foreach (var diHandler in diHandlers)
            {
                tasks.Add(ExecuteHandlerSafelyAsync(() =>
                    InvokeServiceHandler(diHandler, @event, eventType, cancellationToken)));
            }

            if (tasks.Count > 0)
            {
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            else
            {
                Trace.WriteLine($"No handlers found for event type {eventType.Name}. Event ID: {@event.Id}");
            }
        }
        catch (Exception exception) when (exception is not InvalidOperationException)
        {
            throw new InvalidOperationException(
                $"Unable to publish the event {@event.Id}. See inner exception for details.",
                exception);
        }
    }

    /// <inheritdoc />
    public void Subscribe<TEvent>(Action<TEvent> subscriber)
        where TEvent : class, IEvent
    {
        ArgumentNullException.ThrowIfNull(subscriber);
        GetOrCreateHandlersOf<TEvent>().AddSyncHandler(subscriber);
    }

    /// <inheritdoc />
    public void Subscribe<TEvent>(Func<TEvent, CancellationToken, Task> subscriber)
        where TEvent : class, IEvent
    {
        ArgumentNullException.ThrowIfNull(subscriber);
        GetOrCreateHandlersOf<TEvent>().AddAsyncHandler(subscriber);
    }

    /// <inheritdoc />
    public void Subscribe<TEvent>(IEventHandler<TEvent> subscriber)
        where TEvent : class, IEvent
    {
        ArgumentNullException.ThrowIfNull(subscriber);
        GetOrCreateHandlersOf<TEvent>().AddServiceHandler(subscriber);
    }

    /// <inheritdoc />
    public IDisposable SubscribeDisposable<TEvent>(Action<TEvent> subscriber)
        where TEvent : class, IEvent
    {
        Subscribe(subscriber);
        return new SubscriptionToken<TEvent>(this, subscriber, HandlerType.Sync);
    }

    /// <inheritdoc />
    public IDisposable SubscribeDisposable<TEvent>(Func<TEvent, CancellationToken, Task> subscriber)
        where TEvent : class, IEvent
    {
        Subscribe(subscriber);
        return new SubscriptionToken<TEvent>(this, subscriber, HandlerType.Async);
    }

    /// <inheritdoc />
    public IDisposable SubscribeDisposable<TEvent>(IEventHandler<TEvent> subscriber)
        where TEvent : class, IEvent
    {
        Subscribe(subscriber);
        return new SubscriptionToken<TEvent>(this, subscriber, HandlerType.Service);
    }

    /// <inheritdoc />
    public bool Unsubscribe<TEvent>(Action<TEvent> subscriber)
        where TEvent : class, IEvent =>
        _subscribers.TryGetValue(typeof(TEvent), out var handlers) &&
        handlers.RemoveSyncHandler(subscriber);

    /// <inheritdoc />
    public bool Unsubscribe<TEvent>(Func<TEvent, CancellationToken, Task> subscriber)
        where TEvent : class, IEvent =>
        _subscribers.TryGetValue(typeof(TEvent), out var handlers) &&
        handlers.RemoveAsyncHandler(subscriber);

    /// <inheritdoc />
    public bool Unsubscribe<TEvent>(IEventHandler<TEvent> subscriber)
        where TEvent : class, IEvent =>
        _subscribers.TryGetValue(typeof(TEvent), out var handlers) &&
        handlers.RemoveServiceHandler(subscriber);

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _subscribers.Clear();
            _serviceHandlerCache.Clear();
        }

        base.Dispose(disposing);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static async Task ExecuteHandlerSafelyAsync(Func<Task> handler) =>
        await handler().ConfigureAwait(false);

    private EventHandlerCollection GetOrCreateHandlersOf<TEvent>()
        where TEvent : class, IEvent =>
        _subscribers.GetOrAdd(typeof(TEvent), _ => new EventHandlerCollection());

    private EventHandlerCollection GetHandlersOf(Type eventType) =>
        _subscribers.TryGetValue(eventType, out var handlers) ? handlers : EventHandlerCollection.Empty;

    private object[] GetServiceHandlers(Type eventType) =>
        _serviceHandlerCache.GetOrAdd(eventType, type =>
        {
            var handlerType = typeof(IEventHandler<>).MakeGenericType(type);
            return [.. serviceProvider.GetServices(handlerType).OfType<object>()];
        });

    private static void InvokeAction(object action, object eventObj, Type eventType)
    {
        var actionType = typeof(Action<>).MakeGenericType(eventType);
        var method = actionType.GetMethod("Invoke")!;
        method.Invoke(action, [eventObj]);
    }

    private static Task InvokeAsyncFunc(object func, object eventObj, Type eventType, CancellationToken cancellationToken)
    {
        var funcType = typeof(Func<,,>).MakeGenericType(eventType, typeof(CancellationToken), typeof(Task));
        var method = funcType.GetMethod("Invoke")!;
        return (Task)method.Invoke(func, [eventObj, cancellationToken])!;
    }

    private static Task InvokeServiceHandler(object serviceHandler, object eventObj, Type eventType, CancellationToken cancellationToken)
    {
        var handlerType = typeof(IEventHandler<>).MakeGenericType(eventType);
        var method = handlerType.GetMethod(nameof(IEventHandler<IEvent>.HandleAsync))!;
        return (Task)method.Invoke(serviceHandler, [eventObj, cancellationToken])!;
    }

    /// <summary>
    /// Thread-safe collection for managing different types of event handlers.
    /// </summary>
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

    private enum HandlerType
    {
        Sync,
        Async,
        Service
    }

    /// <summary>
    /// Represents a subscription token that can be disposed to unsubscribe from events.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    /// <param name="subscriber">The publisher-subscriber instance.</param>
    /// <param name="handler">The event handler to unsubscribe.</param>
    /// <param name="handlerType">The type of handler being tracked.</param>
    private sealed class SubscriptionToken<TEvent>(
        PublisherSubscriber subscriber,
        object handler,
        HandlerType handlerType) : IDisposable
        where TEvent : class, IEvent
    {
        private readonly PublisherSubscriber _subscriber = subscriber;
        private readonly object _handler = handler;
        private readonly HandlerType _handlerType = handlerType;
        private volatile bool _disposed;

        public void Dispose()
        {
            if (_disposed)
                return;

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
                Trace.WriteLine($"Failed to unsubscribe the handler of type {_handlerType} for event type {typeof(TEvent).Name}.");
            }
        }
    }
}