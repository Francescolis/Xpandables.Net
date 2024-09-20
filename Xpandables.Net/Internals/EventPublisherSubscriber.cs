
/*******************************************************************************
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
********************************************************************************/
using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.Events;
using Xpandables.Net.Operations;

namespace Xpandables.Net.Internals;

/// <summary>
/// Implements <see cref="IEventPublisher"/> 
/// and <see cref="IEventSubscriber"/> interfaces.
/// </summary>
/// <remarks>
/// Initializes a new instance 
/// of the <see cref="EventPublisherSubscriber"/> class.
/// </remarks>
internal sealed class EventPublisherSubscriber(IServiceProvider serviceProvider)
        : Disposable, IEventPublisher, IEventSubscriber
{
    private readonly Dictionary<Type, List<object>> _subscribers = [];
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    /// <inheritdoc/>
    public async Task<IOperationResult> PublishAsync<TEvent>(
        TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : notnull, IEvent
    {
        try
        {
            IOperationResult.IFailureBuilder failureBuilder =
                OperationResults.Failure();

            int subscriberCount = 0;
            foreach (object subscriber in GetHandlersOf<TEvent>())
            {
                subscriberCount++;
                switch (subscriber)
                {
                    case Action<TEvent> action:
                        if (action.ToOperationResult(@event) is
                            { IsFailure: true } actionFailure)
                        {
                            failureBuilder = failureBuilder.Merge(actionFailure);
                        }

                        break;
                    case Func<TEvent, Task> action:
                        if (await action(@event)
                            .ToOperationResultAsync()
                            .ConfigureAwait(false) is
                            { IsFailure: true } taskFailure)
                        {
                            failureBuilder = failureBuilder.Merge(taskFailure);
                        }

                        break;
                    case IEventHandler<TEvent> handler:
                        if (await handler
                            .HandleAsync(@event, cancellationToken)
                            .ConfigureAwait(false)
                            is { IsFailure: true } handlerFailure)
                        {
                            failureBuilder = failureBuilder.Merge(handlerFailure);
                        }

                        break;
                    default: break;
                }
            }

            if (subscriberCount == 0)
            {
                return OperationResults
                    .NotFound()
                    .WithDetail("No subscriber found for the event !")
                    .WithError(nameof(Event), @event.GetTypeName())
                    .Build();
            }

            IOperationResult failure = failureBuilder.Build();
            IOperationResult success = OperationResults.Ok().Build();

            return failure.Errors.Any()
                ? failure
                : success;
        }
        catch (Exception exception)
            when (exception is not ArgumentNullException)
        {
            return OperationResults
                .InternalError()
                .WithDetail("Publishing event failed !")
                .WithException(exception)
                .Build();
        }
    }

    /// <inheritdoc/>
    public void Subscribe<TEvent>(Action<TEvent> subscriber)
       where TEvent : notnull, IEvent
       => GetHandlersOf<TEvent>().Add(subscriber);

    /// <inheritdoc/>
    public void Subscribe<TEvent>(Func<TEvent, Task> subscriber)
        where TEvent : notnull, IEvent
        => GetHandlersOf<TEvent>().Add(subscriber);

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (List<object> value in _subscribers.Values)
            {
                value.Clear();
            }

            _subscribers.Clear();
        }

        base.Dispose(disposing);
    }

    /// <inheritdoc/>
    protected override ValueTask DisposeAsync(bool disposing)
        => base.DisposeAsync(disposing);

    private List<object> GetHandlersOf<T>()
        where T : notnull, IEvent
    {
        List<object>? result = _subscribers.GetValueOrDefault(typeof(T));
        if (result is null)
        {
            result = [];
            _subscribers[typeof(T)] = result;
        }

        foreach (object handler in _serviceProvider.GetServices<IEventHandler<T>>())
        {
            result.Add(handler);
        }

        return result;
    }
}
