
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

using Xpandables.Net.Aggregates.DomainEvents;
using Xpandables.Net.Aggregates.IntegrationEvents;

namespace Xpandables.Net.Operations.Messaging;

internal sealed class TransientPublisher(
    IServiceProvider serviceProvider) : ITransientPublisher
{
    private readonly IServiceProvider _serviceProvider = serviceProvider
        ?? throw new ArgumentNullException(nameof(serviceProvider));

    public async ValueTask<OperationResult> PublishAsync<T>(
        T @event,
        CancellationToken cancellationToken = default)
        where T : notnull
    {
        ArgumentNullException.ThrowIfNull(@event);

        if (@event.GetType().GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDomainEvent<>)))
        {
            var eventHandlers = _serviceProvider.GetServices<DomainEventHandler<T>>();
            foreach (var handler in eventHandlers)
            {
                if (await handler.Invoke(@event, cancellationToken).ConfigureAwait(false) is { IsFailure: true } failureOperation)
                    return failureOperation;
            }
        }
        else if (@event.GetType().GetInterfaces().Any(i => !i.IsGenericType && i == typeof(IIntegrationEvent)))
        {
            var integrationHandlers = _serviceProvider.GetServices<IntegrationEventHandler<T>>();
            foreach (var handler in integrationHandlers)
            {
                if (await handler.Invoke(@event, cancellationToken).ConfigureAwait(false) is { IsFailure: true } failureOperation)
                    return failureOperation;
            }

        }

        return OperationResults
            .Ok()
            .Build();
    }
}