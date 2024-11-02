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

namespace Xpandables.Net.Events.Defaults;

/// <summary>
/// Provides a mechanism for publishing and subscribing to events.
/// </summary>
/// <param name="serviceProvider">The service provider to resolve event 
/// handlers.</param>
public sealed class EventPublisherSubscriber(
    IServiceProvider serviceProvider) :
    Disposable, IEventPublisher, IEventSubscriber
{
    private readonly ConcurrentDictionary<Type, ConcurrentBag<object>> _subscribers = [];
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    /// <inheritdoc/>
    public async Task PublishAsync<TEvent>(
        TEvent @event,
        CancellationToken cancellationToken = default)
        where TEvent : notnull, IEvent
    {
        try
        {
            ConcurrentBag<object> handlers = GetHandlersOf<TEvent>();

            Task[] tasks = handlers
                .Select(handler => handler switch
                {
                    Action<TEvent> action => Task.Run(() => action(@event)),
                    Func<TEvent, Task> func => func(@event),
                    IEventHandler<TEvent> eventHandler => eventHandler.HandleAsync(@event),
                    _ => Task.CompletedTask
                })
                .ToArray();

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
    /// <inheritdoc/>
    public async Task<IEnumerable<EventPublished>> PublishAsync(
        IEnumerable<IEvent> events,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (events is null || !events.Any())
            {
                return [];
            }

            ConcurrentBag<EventPublished> eventPublished = [];

            Task[] tasks = events
                .Select(@event => ((Task)PublishAsync((dynamic)@event, cancellationToken))
                    .ContinueWith(t => eventPublished.Add(new EventPublished
                    {
                        EventId = @event.EventId,
                        PublishedOn = DateTimeOffset.UtcNow,
                        ErrorMessage = t.IsFaulted ? t.Exception?.ToString() : null
                    })))
                .ToArray();

            await Task.WhenAll(tasks).ConfigureAwait(false);

            return eventPublished;
        }
        catch (Exception exception)
            when (exception is not InvalidOperationException)
        {
            throw new InvalidOperationException(
                "Unable to publish the events. See inner exception for details.",
                exception);
        }
    }

    /// <inheritdoc/>
    public void Subscribe<TEvent>(
        Action<TEvent> subscriber)
        where TEvent : notnull, IEvent =>
        GetHandlersOf<TEvent>()
        .Add(subscriber);

    /// <inheritdoc/>
    public void Subscribe<TEvent>(
        Func<TEvent, Task> subscriber)
        where TEvent : notnull, IEvent =>
        GetHandlersOf<TEvent>()
        .Add(subscriber);

    /// <inheritdoc/>
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

    /// <summary>
    /// Gets the handlers of the specified event type.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    /// <returns>A concurrent bag of event handlers.</returns>
    public ConcurrentBag<object> GetHandlersOf<TEvent>()
        where TEvent : notnull, IEvent
    {
        Type eventType = typeof(TEvent);
        ConcurrentBag<object> handlers = _subscribers.GetOrAdd(eventType, _ => []);

        _serviceProvider
            .GetServices<IEventHandler<TEvent>>()
            .Where(handler => !handlers.Contains(handler))
            .ForEach(handlers.Add);

        return handlers;
    }
}
