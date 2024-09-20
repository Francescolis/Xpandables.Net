
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
using System.Net;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Xpandables.Net.Events;
using Xpandables.Net.HostedServices;
using Xpandables.Net.Operations;
using Xpandables.Net.Primitives;
using Xpandables.Net.Repositories;

namespace Xpandables.Net.Internals;

internal sealed class EventScheduler(
    IServiceScopeFactory scopeFactory,
    IOptions<SchedulerOptions> options,
    IOptions<EventOptions> eventOptions,
    ILogger<EventScheduler> logger)
    : BackgroundServiceBase<EventScheduler>,
    IEventScheduler
{
    private int _attempts;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        IsRunning = true;
        _attempts = 0;

        using PeriodicTimer periodicTimer = new(
            TimeSpan.FromMilliseconds(options.Value.DelayMilliSeconds));

        while (!stoppingToken.IsCancellationRequested
            && await periodicTimer.WaitForNextTickAsync(stoppingToken)
                                    .ConfigureAwait(false))
        {
            try
            {
                await DoExecuteAsync(stoppingToken)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException cancelException)
            {
                logger.CancelExecutingProcess(
                    nameof(EventScheduler),
                    cancelException);

                IsRunning = false;
            }
            catch (Exception exception)
                when (exception is not OperationCanceledException)
            {
                logger.ErrorExecutingProcess(
                    nameof(EventScheduler),
                    exception);

                if (++_attempts > options.Value.MaxAttempts)
                {
                    using CancellationTokenSource cancellationSource
                        = CancellationTokenSource
                        .CreateLinkedTokenSource(
                            stoppingToken,
                            new CancellationToken(true));

                    stoppingToken = cancellationSource.Token;
                    _ = await StopServiceAsync(stoppingToken)
                        .ConfigureAwait(false);

                    return;
                }

                logger.RetryExecutingProcess(
                    nameof(EventScheduler),
                    _attempts,
                    options.Value.DelayMilliSeconds);
            }
        }
    }

    private async Task DoExecuteAsync(CancellationToken cancellationToken)
    {
        using AsyncServiceScope service = scopeFactory.CreateAsyncScope();

        IEventPublisher publisher = service.ServiceProvider
            .GetRequiredService<IEventPublisher>();

        IEventStore eventStore = service.ServiceProvider
            .GetRequiredService<IEventStore>();

        IEventFilter filter = eventOptions.Value
            .GetEventFilterFor<IEventIntegration>();

        filter.Paging = Pagination.With(0, options.Value.TotalPerThread);
        filter.Status = EntityStatus.ACTIVE;

        await foreach (IEvent @event in eventStore
            .FetchEventsAsync(filter, cancellationToken))
        {
            try
            {
                IOperationResult operationResult = await publisher
                    .PublishAsync((dynamic)@event, cancellationToken)
                    .ConfigureAwait(false);

                OperationResultException? exception = operationResult.IsFailure
                    ? new OperationResultException(operationResult)
                    : default;

                if (operationResult.StatusCode == HttpStatusCode.NotFound)
                {
                    continue;
                }

                await eventStore
                    .MarkEventAsPublishedAsync(@event.Id, exception, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception exception)
                when (exception is not ArgumentNullException)
            {
                await eventStore
                    .MarkEventAsPublishedAsync(@event.Id, exception, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
    }
}