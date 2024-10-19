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
using System.Net;

using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.Collections;
using Xpandables.Net.Operations;

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
    public async Task<IOperationResult> PublishAsync<TEvent>(
        TEvent @event,
        CancellationToken cancellationToken = default)
        where TEvent : notnull, IEvent
    {
        try
        {
            ConcurrentBag<object> handlers = GetHandlersOf<TEvent>();

            Task<IOperationResult>[] tasks = handlers
                .Select(handler => handler switch
                {
                    Action<TEvent> action => Task.FromResult(action.ToOperationResult(@event)),
                    Func<TEvent, Task> func => func(@event).ToOperationResultAsync(),
                    IEventHandler<TEvent> eventHandler => eventHandler.HandleAsync(@event),
                    _ => Task.FromResult(OperationResults.Ok().Build())
                })
                .ToArray();

            IOperationResult[] results = await Task
                .WhenAll(tasks)
                .ConfigureAwait(false);

            IFailureBuilder failureBuilder = OperationResults
                .Failure()
                .WithExtension(nameof(@event.EventId), @event.EventId.ToString());

            // if one of the result is an internal server error, get it.
            if (results.Any(result => result.IsInternalServerError()))
            {
                failureBuilder = failureBuilder
                    .WithStatusCode(HttpStatusCode.InternalServerError);
            }

            // add all errors to the failure result.
            results
                .Where(result => !result.IsSuccessStatusCode)
                .ForEach(result => failureBuilder.WithErrors(result.Errors));

            IOperationResult failure = failureBuilder.Build();

            return failure.Errors.Any()
                ? failure :
                OperationResults
                    .Ok()
                    .WithExtension(nameof(@event.EventId), @event.EventId.ToString())
                    .Build();
        }
        catch (Exception exception)
        {
            return OperationResults
                .InternalServerError()
                .WithExtension(nameof(@event.EventId), @event.EventId.ToString())
                .WithException(exception)
                .Build();
        }
    }
    /// <inheritdoc/>
    public async Task<IOperationResult<IEnumerable<EventPublished>>> PublishAsync<TEvent>(
        IEnumerable<TEvent> events,
        CancellationToken cancellationToken = default)
        where TEvent : notnull, IEvent
    {
        try
        {
            Task<IOperationResult>[] operationResults = events
                .Select(@event => PublishAsync(@event, cancellationToken))
                .ToArray();

            IOperationResult[] results = await Task
                .WhenAll(operationResults)
                .ConfigureAwait(false);

            IEnumerable<EventPublished> eventPublished = results
                .Select(result => new EventPublished
                {
                    EventId = Guid.Parse(
                        result.Extensions[nameof(Event.EventId)]
                        !.Value.Values.First()),
                    PublishedOn = DateTimeOffset.UtcNow,
                    ErrorMessage = !result.IsSuccessStatusCode
                        ? null
                        : string.Join(
                            Environment.NewLine,
                            result.Errors.Select(error => error.Values))
                });

            return OperationResults.Ok(eventPublished).Build();
        }
        catch (Exception exception)
        {
            return OperationResults
                .InternalServerError<IEnumerable<EventPublished>>()
                .WithException(exception)
                .Build();
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

    private ConcurrentBag<object> GetHandlersOf<TEvent>()
        where TEvent : notnull, IEvent
    {
        Type eventType = typeof(TEvent);
        ConcurrentBag<object> handlers = _subscribers.GetOrAdd(eventType, _ => []);

        _serviceProvider
            .GetServices<IEventHandler<TEvent>>()
            .ForEach(handlers.Add);

        return handlers;
    }
}
