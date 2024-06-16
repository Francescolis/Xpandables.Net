
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

using Xpandables.Net.Operations;

namespace Xpandables.Net.Aggregates;

/// <summary>
/// Implements <see cref="IEventPublisher"/> 
/// and <see cref="IEventSubscriber"/> interfaces.
/// </summary>
/// <remarks>
/// Initializes a new instance 
/// of the <see cref="EventPublisherSubscriber"/> class.
/// </remarks>
public sealed class EventPublisherSubscriber(IServiceProvider serviceProvider)
        : Disposable, IEventPublisher, IEventSubscriber
{
    private readonly Dictionary<Type, List<object>> _subscribers = [];
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    /// <inheritdoc/>
    public async ValueTask<IOperationResult> PublishAsync<TEvent>(
        TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : notnull, IEvent
    {
        try
        {
            IOperationResult result = OperationResults.Ok().Build();

            foreach (object subscriber in GetHandlersOf<TEvent>())
            {
                switch (subscriber)
                {
                    case Action<TEvent> action:
                        action(@event);
                        break;
                    case Func<TEvent, ValueTask> action:
                        await action(@event).ConfigureAwait(false);
                        break;
                    case IEventHandler<TEvent> handler:
                        result = await handler
                            .HandleAsync(@event, cancellationToken)
                            .ConfigureAwait(false);
                        break;
                    default: break;
                }
            }

            return result;
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
    public void Subscribe<TEvent>(Func<TEvent, ValueTask> subscriber)
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
