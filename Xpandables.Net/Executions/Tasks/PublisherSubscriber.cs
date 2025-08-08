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
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.Collections;

namespace Xpandables.Net.Executions.Tasks;

/// <summary>
/// Provides functionalities for event-based publish-subscribe communication.
/// Manages subscriptions for various event types and facilitates publishing events
/// to the subscribed handlers.
/// </summary>
public sealed class PublisherSubscriber(IServiceProvider serviceProvider) : Disposable, IPublisher, ISubscriber
{
    private readonly ConcurrentDictionary<Type, ConcurrentBag<object>> _subscribers = [];

    /// <inheritdoc />
    public async Task PublishAsync<TEvent>(
        TEvent @event,
        CancellationToken cancellationToken = default)
        where TEvent : class, IEvent
    {
        try
        {
            ConcurrentBag<object> handlers = GetHandlersOf(@event.GetType());

            Task[] tasks =
            [
                .. handlers
                    .Select(handler => handler switch
                    {
                        Action<TEvent> action => Task.Run(() => action(@event), cancellationToken),
                        Func<TEvent, Task> func => func(@event),
                        DelHandler<IEvent> eventHandler => eventHandler.Invoke(@event, cancellationToken),
                        _ => Task.CompletedTask
                    })
            ];

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
        catch (Exception exception)
            when (exception is not InvalidOperationException)
        {
            throw new InvalidOperationException(
                $"Unable to publish the event {@event.Id}. " +
                $"See inner exception for details.",
                exception);
        }
    }

    /// <inheritdoc />
    public void Subscribe<TEvent>(Action<TEvent> subscriber)
        where TEvent : class, IEvent =>
        GetHandlersOf<TEvent>()
            .Add(subscriber);

    /// <inheritdoc />
    public void Subscribe<TEvent>(Func<TEvent, Task> subscriber)
        where TEvent : class, IEvent =>
        GetHandlersOf<TEvent>()
            .Add(subscriber);

    /// <inheritdoc />
    public void Subscribe<TEvent>(IEventHandler<TEvent> subscriber)
        where TEvent : class, IEvent =>
        GetHandlersOf<TEvent>()
            .Add(subscriber);

    /// <inheritdoc />
    public IDisposable SubscribeDisposable<TEvent>(Action<TEvent> subscriber)
        where TEvent : class, IEvent
    {
        Subscribe(subscriber);
        return new SubscriptionToken<TEvent>(this, subscriber);
    }

    /// <inheritdoc />
    public IDisposable SubscribeDisposable<TEvent>(Func<TEvent, Task> subscriber)
        where TEvent : class, IEvent
    {
        Subscribe(subscriber);
        return new SubscriptionToken<TEvent>(this, subscriber);
    }

    /// <inheritdoc />
    public IDisposable SubscribeDisposable<TEvent>(IEventHandler<TEvent> subscriber)
        where TEvent : class, IEvent
    {
        Subscribe(subscriber);
        return new SubscriptionToken<TEvent>(this, subscriber);
    }

    /// <inheritdoc />
    public bool Unsubscribe<TEvent>(Action<TEvent> subscriber)
        where TEvent : class, IEvent =>
        RemoveSubscriber<TEvent>(subscriber);

    /// <inheritdoc />
    public bool Unsubscribe<TEvent>(Func<TEvent, Task> subscriber)
        where TEvent : class, IEvent =>
        RemoveSubscriber<TEvent>(subscriber);

    /// <inheritdoc />
    public bool Unsubscribe<TEvent>(IEventHandler<TEvent> subscriber)
        where TEvent : class, IEvent =>
        RemoveSubscriber<TEvent>(subscriber);

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (ConcurrentBag<object> handlers in _subscribers.Values)
            {
                handlers.Clear();
            }

            _subscribers.Clear();
        }

        base.Dispose(disposing);
    }

    private bool RemoveSubscriber<TEvent>(object subscriber)
        where TEvent : class, IEvent
    {
        if (!_subscribers.TryGetValue(typeof(TEvent), out ConcurrentBag<object>? handlers))
        {
            return false;
        }

        // Since ConcurrentBag doesn't support direct removal, we need to recreate it
        List<object> existingHandlers = [.. handlers];
        bool removed = existingHandlers.Remove(subscriber);

        if (removed)
        {
            // Replace the bag with a new one containing the remaining handlers
            _subscribers.TryUpdate(typeof(TEvent), [.. existingHandlers], handlers);
        }

        return removed;
    }

    private ConcurrentBag<object> GetHandlersOf<TEvent>()
        where TEvent : class, IEvent =>
        GetHandlersOf(typeof(TEvent));

    private ConcurrentBag<object> GetHandlersOf(Type eventType)
    {
        Type handlerType = typeof(IEventHandler<>).MakeGenericType(eventType);
        ConcurrentBag<object> handlers = _subscribers.GetOrAdd(eventType, _ => []);

        serviceProvider
            .GetServices(handlerType)
            .Cast<object>()
            .Where(handler => !handlers.Contains(handler))
            .ForEach(handler =>
            {
                MethodInfo? method = handlerType.GetMethod(
                    "HandleAsync",
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic,
                    [eventType, typeof(CancellationToken)]);

                DelHandler<IEvent> delHandler = (evt, token) =>
                    (Task)method!.Invoke(handler, [evt, token])!;

                handlers.Add(delHandler);
            });

        return handlers;
    }

    private delegate Task DelHandler<in TEvent>(TEvent @event, CancellationToken cancellationToken)
        where TEvent : class, IEvent;

    /// <summary>
    /// Represents a subscription token that can be disposed to unsubscribe from events.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    /// <param name="subscriber">The publisher-subscriber instance.</param>
    /// <param name="handler">The event handler to unsubscribe.</param>
    private sealed class SubscriptionToken<TEvent>(PublisherSubscriber subscriber, object handler) : IDisposable
        where TEvent : class, IEvent
    {
#pragma warning disable CA2213 // Disposable fields should be disposed
        private readonly PublisherSubscriber _subscriber = subscriber;
#pragma warning restore CA2213 // Disposable fields should be disposed
        private readonly object _handler = handler;
        private bool _disposed;

        public void Dispose()
        {
            if (!_disposed)
            {
                _subscriber.RemoveSubscriber<TEvent>(_handler);
                _disposed = true;
            }
        }
    }
}