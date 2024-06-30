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
using Xpandables.Net.Primitives;

namespace Xpandables.Net.Aggregates;

/// <summary>
/// A marker interface that defines an event that cannot be duplicated.
/// </summary>
public interface IEventDuplicate
{
    /// <summary>
    /// Gets the filter to check for duplicate events.
    /// </summary>
    IEventFilter? Filter { get; }

    /// <summary>
    /// Gets the operation result to return when the event is duplicated.
    /// </summary>
    IOperationResult OnFailure { get; }
}

/// <summary>
/// Defines an event handler that checks for duplicate events.
/// </summary>
/// <typeparam name="TEvent"></typeparam>
/// <param name="serviceProvider"></param>
/// <param name="decoratee"></param>
public sealed class EventDuplicateDecorator<TEvent>(
    IServiceProvider serviceProvider,
    IEventHandler<TEvent> decoratee) :
    IEventHandler<TEvent>, IDecorator
    where TEvent : notnull, IEvent, IEventDuplicate
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
        if (@event.Filter is null)
        {
            return await decoratee
                .HandleAsync(@event, cancellationToken)
                .ConfigureAwait(false);
        }

        IEventFilter eventFilter = new EventFilter()
        {
            AggregateName = @event.Filter.AggregateName,
            EventTypeName = @event.Filter.EventTypeName,
            Pagination = @event.Filter.Pagination ?? Pagination.With(0, 1),
            DataCriteria = @event.Filter.DataCriteria,
            AggregateId = @event.Filter.AggregateId.HasValue
                ? @event.Filter.AggregateId.Value : null,
            FromCreatedOn = @event.Filter.FromCreatedOn.HasValue
                ? @event.Filter.FromCreatedOn.Value : null,
            ToCreatedOn = @event.Filter.ToCreatedOn.HasValue
                ? @event.Filter.ToCreatedOn.Value : null,
            Id = @event.Filter.Id.HasValue ? @event.Filter.Id.Value : null,
            OnError = @event.Filter.OnError,
            Status = @event.Filter.Status,
            Version = @event.Filter.Version.HasValue
                ? @event.Filter.Version.Value : null
        };

        try
        {
            IAsyncEnumerable<IEvent> events = @event switch
            {
                IEventDomain => serviceProvider
                    .GetRequiredService<IEventDomainStore>()
                    .ReadAsync(eventFilter, cancellationToken),
                IEventIntegration => serviceProvider
                    .GetRequiredService<IEventIntegrationStore>()
                    .ReadAsync(eventFilter, cancellationToken),
                _ => throw new InvalidOperationException(
                    $"The event type {@event.GetType().Name} is not supported.")
            };

            return events
            .ToBlockingEnumerable(cancellationToken)
            .Any() switch
            {
                true => @event.OnFailure,
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
