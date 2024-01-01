
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

namespace Xpandables.Net.IntegrationEvents;

internal sealed class IntegrationEventTransientScheduler(
    IServiceScopeFactory scopeFactory,
    IOptions<IntegrationEventSchedulerOptions> options,
    ILogger<IntegrationEventTransientScheduler> logger)
    : BackgroundServiceBase<IntegrationEventTransientScheduler>, IIntegrationEventTransientScheduler
{
    private int _attempts;
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory
        ?? throw new ArgumentNullException(nameof(scopeFactory));
    private readonly IntegrationEventSchedulerOptions _options = options.Value
        ?? throw new ArgumentNullException(nameof(options));
    private readonly ILogger<IntegrationEventTransientScheduler> _logger = logger
        ?? throw new ArgumentNullException(nameof(logger));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        IsRunning = true;
        _attempts = 0;

        using PeriodicTimer periodicTimer = new(TimeSpan.FromMilliseconds(_options.DelayMilliSeconds));

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
                _logger.CancelExecutingProcess(nameof(IntegrationEventTransientScheduler), cancelException);
                IsRunning = false;
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                _logger.ErrorExecutingProcess(nameof(IntegrationEventTransientScheduler), exception);

                if (++_attempts > _options.MaxAttempts)
                {
                    using var cancellationSource = CancellationTokenSource
                        .CreateLinkedTokenSource(stoppingToken, new CancellationToken(true));

                    stoppingToken = cancellationSource.Token;
                    await StopServiceAsync(stoppingToken)
                        .ConfigureAwait(false);

                    return;
                }

                _logger.RetryExecutingProcess(nameof(IntegrationEventTransientScheduler), _attempts, _options.DelayMilliSeconds);
            }
        }
    }

    private async Task DoExecuteAsync(CancellationToken cancellationToken)
    {
        using var service = _scopeFactory.CreateAsyncScope();

        IIntegrationEventPublisher publisher = service.ServiceProvider
            .GetRequiredService<IIntegrationEventPublisher>();

        IIntegrationEventStore eventStore = service.ServiceProvider
            .GetRequiredService<IIntegrationEventStore>();

        Pagination pagination = Pagination.With(0, _options.TotalPerThread);

        await foreach (IIntegrationEvent @event in eventStore.ReadAsync(pagination, cancellationToken))
        {
            try
            {
                OperationResult operationResult = await publisher
                    .PublishAsync((dynamic)@event, cancellationToken)
                    .ConfigureAwait(false);

                if (operationResult.IsFailure)
                    await eventStore
                        .SetErrorAsync(@event.Id, new OperationResultException(operationResult), cancellationToken)
                        .ConfigureAwait(false);
                else
                    await eventStore
                        .MarkAsProcessedAsync(@event.Id, cancellationToken)
                        .ConfigureAwait(false);
            }
            catch (Exception exception) when (exception is not ArgumentNullException)
            {
                await eventStore
                    .SetErrorAsync(@event.Id, exception, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
    }
}