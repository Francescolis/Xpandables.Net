
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

using Xpandables.Net.Aggregates;
using Xpandables.Net.Aggregates.IntegrationEvents;
using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Extensions;
using Xpandables.Net.Primitives;

namespace Xpandables.Net.Operations.Messaging;

internal sealed class TransientScheduler(
    IServiceScopeFactory scopeFactory,
    IOptions<SchedulerOptions> options,
    ILogger<TransientScheduler> logger)
    : BackgroundServiceBase<TransientScheduler>, ITransientScheduler
{
    private int _attempts;
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory
        ?? throw new ArgumentNullException(nameof(scopeFactory));
    private readonly SchedulerOptions _options = options.Value
        ?? throw new ArgumentNullException(nameof(options));
    private readonly ILogger<TransientScheduler> _logger = logger
        ?? throw new ArgumentNullException(nameof(logger));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        IsRunning = true;
        _attempts = 0;

        await Task.Yield();

        while (!stoppingToken.IsCancellationRequested)
        {
#pragma warning disable CA1031 // Do not catch general exception types
            try
            {
                await Task.Delay(
                    TimeSpan.FromMilliseconds(_options.DelayMilliSeconds), stoppingToken)
                    .ConfigureAwait(false);

                await DoExecuteAsync(stoppingToken)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException cancelException)
            {
                _logger.CancelExecutingProcess(nameof(TransientScheduler), cancelException);
                IsRunning = false;
            }
            catch (Exception exception)
            {
                _logger.ErrorExecutingProcess(nameof(TransientScheduler), exception);

                if (++_attempts <= _options.MaxAttempts)
                {
                    try
                    {
                        await Task.Delay(
                            _options.DelayBetweenAttempts, stoppingToken)
                            .ConfigureAwait(false);
                    }
                    catch (OperationCanceledException cancelException)
                    {
                        // if stoppingToken is canceled, the process will return and terminate itself.
                        _logger.CancelExecutingProcess(nameof(TransientScheduler), cancelException);

                        IsRunning = false;
                    }

                    return;
                }

                IsRunning = false;
                stoppingToken = CancellationTokenSource.CreateLinkedTokenSource(
                    stoppingToken, new CancellationToken(true))
                    .Token;
            }
#pragma warning restore CA1031 // Do not catch general exception types
        }
    }

    private async Task DoExecuteAsync(CancellationToken cancellationToken)
    {
        using var service = _scopeFactory.CreateAsyncScope();

        ITransientPublisher publisher = service.ServiceProvider
            .GetRequiredService<ITransientPublisher>();

        IIntegrationEventStore eventStore = service.ServiceProvider
            .GetRequiredService<IIntegrationEventStore>();

        Pagination pagination = Pagination.With(0, _options.TotalPerThread);

        await foreach (IIntegrationEvent @event in eventStore.ReadAsync(pagination, cancellationToken))
        {
            OperationResult operationResult = await publisher
                .PublishAsync((dynamic)@event, cancellationToken)
                .ConfigureAwait(false);

            if (operationResult.IsFailure)
                _logger.ErrorExecutingProcess(
                    nameof(TransientScheduler),
                    new OperationResultException(operationResult));
            else
                await eventStore
                    .DeleteAsync(@event.Id, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
        }
    }
}