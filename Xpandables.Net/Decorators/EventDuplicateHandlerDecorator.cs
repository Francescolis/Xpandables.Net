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

using Xpandables.Net.Aggregates;
using Xpandables.Net.Aggregates.Events;
using Xpandables.Net.Distribution;
using Xpandables.Net.Operations;
using Xpandables.Net.Primitives;

namespace Xpandables.Net.Decorators;

/// <summary>
/// Defines an event handler that checks for duplicate domain events.
/// </summary>
/// <typeparam name="TEvent"></typeparam>
/// <param name="serviceProvider"></param>
/// <param name="decoratee"></param>
public sealed class EventDuplicateHandlerDecorator<TEvent>(
    IServiceProvider serviceProvider,
    IEventHandler<TEvent> decoratee) :
    IEventHandler<TEvent>, IDecorator
    where TEvent : notnull, IEventDomain, IEventDuplicateDecorator
{
    /// <summary>
    /// Checks for duplicate events before handling the event.
    /// </summary>
    /// <param name="event">The event to handle.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task<IOperationResult> HandleAsync(
        TEvent @event,
        CancellationToken cancellationToken = default)
    {
        IEventFilter filter = @event.Filter();

        try
        {
            IEventStore eventStore = serviceProvider
                .GetRequiredService<IEventStore>();

            return eventStore
                .FetchAsync(filter, cancellationToken)
                .ToBlockingEnumerable(cancellationToken)
                .Any() switch
            {
                true => @event.OnFailure(),
                _ => await decoratee
                    .HandleAsync(@event, cancellationToken)
                    .ConfigureAwait(false)
            };

        }
        catch (Exception exception)
            when (exception is not OperationResultException)
        {
            return OperationResults
                .InternalError()
                .WithException(exception)
                .Build();
        }
    }
}
