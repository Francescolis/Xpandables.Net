
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
using Xpandables.Net.Collections;
using Xpandables.Net.I18n;
using Xpandables.Net.Operations;

namespace Xpandables.Net.Aggregates.IntegrationEvents;

internal sealed class IntegrationEventOutbox(
    IIntegrationEventSourcing eventSourcing,
    IIntegrationEventStore eventStore) : Disposable, IIntegrationEventOutbox
{
    private readonly IIntegrationEventSourcing _eventSourcing = eventSourcing
        ?? throw new ArgumentNullException(nameof(eventSourcing));
    private readonly IIntegrationEventStore _eventStore = eventStore
        ?? throw new ArgumentNullException(nameof(eventStore));

    public async ValueTask<OperationResult> AppendAsync(CancellationToken cancellationToken = default)
    {
        await foreach (IIntegrationEvent @event in _eventSourcing
            .GetIntegrationEvents()
            .ToAsyncEnumerable())
        {
            if (await DoAppendAsync(@event, cancellationToken)
                .ConfigureAwait(false) is { IsFailure: true } failureOperation)
                return failureOperation;
        }

        _eventSourcing.MarkIntegrationEventsAsCommitted();

        return OperationResults
               .Ok()
               .Build();
    }

    public async ValueTask<OperationResult> AppendAsync(
        IIntegrationEvent @event,
        CancellationToken cancellationToken = default)
        => await DoAppendAsync(@event, cancellationToken)
            .ConfigureAwait(false);

    private async ValueTask<OperationResult> DoAppendAsync(
        IIntegrationEvent @event,
        CancellationToken cancellationToken)
    {
        try
        {
            await _eventStore.AppendAsync(@event, cancellationToken)
                .ConfigureAwait(false);

            return OperationResults
                   .Ok()
                   .Build();
        }
        catch (Exception exception) when (exception is not ArgumentNullException)
        {
            return OperationResults
                .InternalError()
                .WithDetail(I18nXpandables.OutboxFailedToAppendNotification)
                .WithError(nameof(IIntegrationEventOutbox), exception)
                .Build();
        }
    }
}