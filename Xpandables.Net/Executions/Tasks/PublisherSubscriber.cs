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

using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.Collections;

namespace Xpandables.Net.Executions.Tasks;

/// <summary>
/// Provides a mechanism for publishing and subscribing to events.
/// </summary>
/// <param name="serviceProvider">
/// The service provider to resolve event
/// handlers.
/// </param>
public sealed class PublisherSubscriber(IServiceProvider serviceProvider) :
    Disposable, IPublisher, ISubscriber
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly ConcurrentDictionary<Type, ConcurrentBag<object>> _subscribers = [];

    /// <inheritdoc />
    public async Task PublishAsync<TEvent>(
        TEvent @event,
        CancellationToken cancellationToken = default)
        where TEvent : notnull, IEvent
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
                        IEventHandler<TEvent> eventHandler => eventHandler.HandleAsync(@event, cancellationToken),
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
        where TEvent : notnull, IEvent =>
        GetHandlersOf<TEvent>()
            .Add(subscriber);

    /// <inheritdoc />
    public void Subscribe<TEvent>(Func<TEvent, Task> subscriber)
        where TEvent : notnull, IEvent =>
        GetHandlersOf<TEvent>()
            .Add(subscriber);

    /// <inheritdoc />
    public void Subscribe<TEvent>(IEventHandler<TEvent> subscriber)
        where TEvent : notnull, IEvent =>
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
        where TEvent : notnull, IEvent =>
        GetHandlersOf(typeof(TEvent));

    private ConcurrentBag<object> GetHandlersOf(Type eventType)
    {
        ConcurrentBag<object> handlers = _subscribers.GetOrAdd(eventType, _ => []);

        _serviceProvider
            .GetServices(typeof(IEventHandler<>).MakeGenericType(eventType))
            .Cast<object>()
            .Where(handler => !handlers.Contains(handler))
            .ForEach(handlers.Add);

        return handlers;
    }
}