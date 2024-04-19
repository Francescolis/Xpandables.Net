
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
using Xpandables.Net.Operations;

namespace Xpandables.Net.Aggregates;

/// <summary>
/// Implements <see cref="ITransientPublisher"/> 
/// and <see cref="ITransientSubscriber"/> interfaces.
/// </summary>
public sealed class TransientPublisherSubscriber
    : Disposable, ITransientPublisher, ITransientSubscriber
{
    private readonly AsyncLocal<Dictionary<Type, List<object>>> _subscribers
        = new();

    /// <summary>
    /// Initializes a new instance 
    /// of the <see cref="TransientPublisherSubscriber"/> class.
    /// </summary>
    public TransientPublisherSubscriber() => _subscribers.Value = [];

    /// <inheritdoc/>
    public async ValueTask<IOperationResult> PublishAsync<T>(
        T @event, CancellationToken cancellationToken = default)
        where T : notnull
    {
        try
        {
            IOperationResult result = OperationResults.Ok().Build();

            foreach (object subscriber in GetHandlersOf<T>())
            {
                switch (subscriber)
                {
                    case Action<T> action:
                        action(@event);
                        break;
                    case Func<T, ValueTask> action:
                        await action(@event).ConfigureAwait(false);
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
    public void Subscribe<T>(Action<T> subscriber)
       where T : notnull
       => GetHandlersOf<T>().Add(subscriber);

    /// <inheritdoc/>
    public void Subscribe<T>(Func<T, ValueTask> subscriber)
        where T : notnull
        => GetHandlersOf<T>().Add(subscriber);

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (List<object> value in _subscribers.Value!.Values)
                value.Clear();

            _subscribers.Value!.Clear();
        }

        base.Dispose(disposing);
    }

    private List<object> GetHandlersOf<T>()
        where T : notnull
    {
        List<object>? result = _subscribers.Value!.GetValueOrDefault(typeof(T));
        if (result is null)
        {
            result = [];
            _subscribers.Value![typeof(T)] = result;
        }

        return result;
    }
}
