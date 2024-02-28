
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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Xpandables.Net.HostedServices;
using Xpandables.Net.Operations;
using Xpandables.Net.Primitives;

namespace Xpandables.Net.Aggregates.Notifications;

internal sealed class NotificationScheduler(
    IServiceScopeFactory scopeFactory,
    IOptions<NotificationOptions> options,
    ILogger<NotificationScheduler> logger)
    : BackgroundServiceBase<NotificationScheduler>, INotificationScheduler
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
                logger.CancelExecutingProcess(nameof(NotificationScheduler), cancelException);
                IsRunning = false;
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.ErrorExecutingProcess(nameof(NotificationScheduler), exception);

                if (++_attempts > options.Value.MaxAttempts)
                {
                    using CancellationTokenSource cancellationSource = CancellationTokenSource
                        .CreateLinkedTokenSource(stoppingToken, new CancellationToken(true));

                    stoppingToken = cancellationSource.Token;
                    _ = await StopServiceAsync(stoppingToken)
                        .ConfigureAwait(false);

                    return;
                }

                logger.RetryExecutingProcess(
                    nameof(NotificationScheduler),
                    _attempts,
                    options.Value.DelayMilliSeconds);
            }
        }
    }

    private async Task DoExecuteAsync(CancellationToken cancellationToken)
    {
        using AsyncServiceScope service = scopeFactory.CreateAsyncScope();

        INotificationPublisher publisher = service.ServiceProvider
            .GetRequiredService<INotificationPublisher>();

        INotificationStore eventStore = service.ServiceProvider
            .GetRequiredService<INotificationStore>();

        INotificationFilter filter = new NotificationFilter
        {
            Pagination = Pagination.With(0, options.Value.TotalPerThread)
        };

        await foreach (INotification @event in eventStore.ReadAsync(filter, cancellationToken))
        {
            try
            {
                IOperationResult operationResult = await publisher
                    .PublishAsync((dynamic)@event, cancellationToken)
                    .ConfigureAwait(false);

                OperationResultException? exception = operationResult.IsFailure
                    ? new OperationResultException(operationResult)
                    : default;

                await eventStore
                    .AppendCloseAsync(@event.Id, exception, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception exception) when (exception is not ArgumentNullException)
            {
                await eventStore
                    .AppendCloseAsync(@event.Id, exception, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
    }
}