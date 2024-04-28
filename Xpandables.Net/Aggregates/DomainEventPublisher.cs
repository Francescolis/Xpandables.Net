
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
using Microsoft.Extensions.Options;

using Xpandables.Net.Operations;
using Xpandables.Net.Primitives.I18n;
using Xpandables.Net.Primitives.Text;

namespace Xpandables.Net.Aggregates;

internal sealed class DomainEventPublisher<TAggregateId>(
    IServiceProvider serviceProvider,
    IOptions<EventOptions> options)
    : IDomainEventPublisher<TAggregateId>
    where TAggregateId : struct, IAggregateId<TAggregateId>
{
    public async ValueTask<IOperationResult> PublishAsync<TDomainEvent>(
        TDomainEvent @event,
        CancellationToken cancellationToken = default)
        where TDomainEvent : notnull, IEventDomain<TAggregateId>
    {
        ArgumentNullException.ThrowIfNull(@event);

        List<IDomainEventHandler<TDomainEvent, TAggregateId>> handlers =
            serviceProvider
            .GetServices<IDomainEventHandler<TDomainEvent, TAggregateId>>()
            .ToList();

        if (handlers.Count <= 0)
        {
            return options.Value.ConsiderNoDomainEventHandlerAsError
                ? OperationResults
                    .InternalError()
                    .WithException(
                        new InvalidOperationException(
                        I18nXpandables.EventSourcingNoDomainEventHandler
                            .StringFormat(@event.GetTypeName())))
                    .Build()
                : OperationResults.
                    Ok()
                    .Build();
        }

        foreach (IDomainEventHandler<TDomainEvent, TAggregateId>? handler
            in handlers)
        {
            if (await handler
                .HandleAsync(@event, cancellationToken)
                .ConfigureAwait(false)
                is { IsFailure: true } failureOperation)
            {
                return failureOperation;
            }
        }

        return OperationResults
            .Ok()
            .Build();
    }
}