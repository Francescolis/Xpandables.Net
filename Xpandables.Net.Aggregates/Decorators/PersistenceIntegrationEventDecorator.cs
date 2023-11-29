/************************************************************************************************************
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
************************************************************************************************************/
using Xpandables.Net.Aggregates.IntegrationEvents;
using Xpandables.Net.Extensions;
using Xpandables.Net.I18n;

namespace Xpandables.Net.Decorators;

/// <summary>
/// Represents a method signature to be used to apply persistence behavior to a notification task.
/// </summary>
/// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
/// <returns>A task that represents an <see cref="OperationResult"/>.</returns>
/// <exception cref="InvalidOperationException">The persistence operation failed to execute.</exception>
public delegate ValueTask<OperationResult> PersistenceIntegrationEventHandler(CancellationToken cancellationToken);

/// <summary>
/// This class allows the application author to add persistence support to notification control flow.
/// The target notification should implement the <see cref="IPersistenceDecorator"/> interface in order to activate the behavior.
/// The class decorates the target notification handler with an definition of <see cref="PersistenceIntegrationEventHandler"/> 
/// that get called after the main one in the same control flow only.
/// </summary>
/// <typeparam name="TIntegrationEvent">Type of notification.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="PersistenceIntegrationEventDecorator{TIntegrationEvent}"/> class with
/// the decorated handler and the unit of work to act on.
/// </remarks>
/// <param name="persistenceIntegrationEventHandler">The persistence delegate to apply persistence.</param>
/// <param name="decoratee">The decorated notification handler.</param>
/// <exception cref="ArgumentNullException">The <paramref name="decoratee"/> or <paramref name="persistenceIntegrationEventHandler"/>
/// is null.</exception>
public sealed class PersistenceIntegrationEventDecorator<TIntegrationEvent>(
    IIntegrationEventHandler<TIntegrationEvent> decoratee,
    PersistenceIntegrationEventHandler persistenceIntegrationEventHandler) :
    IIntegrationEventHandler<TIntegrationEvent>
    where TIntegrationEvent : notnull, IIntegrationEvent, IPersistenceDecorator
{
    private readonly IIntegrationEventHandler<TIntegrationEvent> _decoratee = decoratee
        ?? throw new ArgumentNullException(nameof(decoratee));
    private readonly PersistenceIntegrationEventHandler _persistenceIntegrationEventHandler =
        persistenceIntegrationEventHandler
        ?? throw new ArgumentNullException(nameof(persistenceIntegrationEventHandler));

    /// <summary>
    /// Asynchronously handles the specified notification and persists changes to store if there is no exception or error.
    /// </summary>
    /// <param name="event">The notification instance to act on.</param>
    /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="event"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The operation failed. See inner exception.</exception>
    /// <returns>A task that represents an object of <see cref="IOperationResult"/>.</returns>
    public async ValueTask<OperationResult> HandleAsync(
        TIntegrationEvent @event,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (await _decoratee.HandleAsync(@event, cancellationToken)
                .ConfigureAwait(false) is { IsFailure: true } failedOperation)
                return failedOperation;

            return await _persistenceIntegrationEventHandler(cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is not ArgumentNullException)
        {
            return OperationResults
                .InternalError()
                .WithDetail(I18nXpandables.ActionSpecifiedFailedSeeException
                    .StringFormat(nameof(PersistenceIntegrationEventDecorator<TIntegrationEvent>)))
                .WithError(nameof(PersistenceIntegrationEventDecorator<TIntegrationEvent>), exception)
                .Build();
        }
    }
}
