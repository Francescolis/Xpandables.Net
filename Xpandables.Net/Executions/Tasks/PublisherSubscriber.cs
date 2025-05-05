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
public sealed class PublisherSubscriber(IServiceProvider serviceProvider) :
    Disposable, IPublisher, ISubscriber
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
                $"Unable to publish the event {@event.EventId}. " +
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

                // ReSharper disable once ConvertToLocalFunction
                DelHandler<IEvent> delHandler = (evt, token) =>
                    (Task)method!.Invoke(handler, [evt, token])!;

                handlers.Add(delHandler);
            });

        return handlers;
    }

    private delegate Task DelHandler<in TEvent>(TEvent @event, CancellationToken cancellationToken)
        where TEvent : class, IEvent;
}