
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

using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.I18n;
using Xpandables.Net.Operations;

namespace Xpandables.Net.Aggregates.DomainEvents;

internal sealed class DomainEventPublisher<TAggregateId>(
    IServiceProvider serviceProvider) : IDomainEventPublisher<TAggregateId>
    where TAggregateId : struct, IAggregateId<TAggregateId>
{
    private readonly IServiceProvider _serviceProvider = serviceProvider
        ?? throw new ArgumentNullException(nameof(serviceProvider));

    public async ValueTask<OperationResult> PublishAsync<TDomainEvent>(
        TDomainEvent @event,
        CancellationToken cancellationToken = default)
        where TDomainEvent : notnull, IDomainEvent<TAggregateId>
    {
        ArgumentNullException.ThrowIfNull(@event);

        var handlers = _serviceProvider
            .GetServices<IDomainEventHandler<TDomainEvent, TAggregateId>>()
            .ToList();

        if (handlers.Count <= 0)
            return OperationResults
                .BadRequest()
                .WithError(
                    ElementEntry.UndefinedKey,
                    I18nXpandables.EventSourcingNoIntegrationEventHandler)
                .Build();

        foreach (var handler in handlers)
        {
            if (await handler.HandleAsync(@event, cancellationToken).ConfigureAwait(false)
                is { IsFailure: true } failureOperation)
                return failureOperation;
        }

        return OperationResults.Ok().Build();
    }
}