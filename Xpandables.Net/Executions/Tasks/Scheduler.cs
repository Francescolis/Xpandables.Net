/*******************************************************************************
 * Copyright (C) 2024 Francis-Black EWANE
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
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Xpandables.Net.Executions.Domains;

namespace Xpandables.Net.Executions.Tasks;

/// <summary>
/// Represents a background service that schedules and publishes events.
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class Scheduler : BackgroundService, IScheduler
{
    private readonly ILogger<Scheduler> _logger;
    private readonly IMessageQueue _messageQueue;
    private readonly IDisposable? _optionsMonitor;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private SchedulerOptions _options;

    private uint _retryCount;
#pragma warning disable CA1848 // Use the LoggerMessage delegates
#pragma warning disable CA1031 // Do not catch general exception types

    /// <summary>
    /// Initializes a new instance of the <see cref="Scheduler" /> class.
    /// </summary>
    /// <param name="serviceScopeFactory"> The service scope factory to create service scopes. </param>
    /// <param name="messageQueue">The message queue to enqueue and dequeue events.</param>
    /// <param name="options"> The options monitor to track changes in event options. </param>
    /// <param name="logger">The logger to log information and errors.</param>
    /// <remarks>This code is subject to change in future versions.</remarks>
    public Scheduler(
        IServiceScopeFactory serviceScopeFactory,
        IMessageQueue messageQueue,
        IOptionsMonitor<SchedulerOptions> options,
        ILogger<Scheduler> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _messageQueue = messageQueue;
        _options = options.CurrentValue;
        _logger = logger;

        _optionsMonitor = options.OnChange(updatedOptions => _options = updatedOptions);
    }

    /// <inheritdoc />
    public async Task ScheduleAsync(CancellationToken cancellationToken = default)
    {
        if (!_options.IsEventSchedulerEnabled)
        {
            _logger.LogWarning("Event scheduler is disabled.");
            return;
        }

        // ReSharper disable once UseAwaitUsing
        using AsyncServiceScope serviceScope = _serviceScopeFactory.CreateAsyncScope();

        await _messageQueue.DequeueAsync(_options.MaxSchedulerEventPerThread, cancellationToken).ConfigureAwait(false);
        if (_messageQueue.Channel.Reader.Count == 0)
        {
            _logger.LogInformation("No events to schedule.");
            return;
        }

        IPublisher eventPublisher = serviceScope.ServiceProvider.GetRequiredService<IPublisher>();
        IEventStore eventStore = serviceScope.ServiceProvider.GetRequiredService<IEventStore>();

        while (_messageQueue.Channel.Reader.TryRead(out IIntegrationEvent? @event))
        {
            try
            {
                await eventPublisher.PublishAsync(@event, cancellationToken).ConfigureAwait(false);

                EventProcessed eventPublished = new()
                {
                    EventId = @event.EventId, PublishedOn = DateTime.UtcNow, ErrorMessage = null
                };

                await eventStore.MarkAsProcessedAsync(eventPublished, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                EventProcessed eventPublished = new()
                {
                    EventId = @event.EventId, PublishedOn = DateTime.UtcNow, ErrorMessage = exception.ToString()
                };

                await eventStore
                    .MarkAsProcessedAsync(eventPublished, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _retryCount = 0;

        TimeSpan period = TimeSpan.FromMilliseconds(_options.SchedulerFrequency);
        using PeriodicTimer timer = new(period);

        while (!stoppingToken.IsCancellationRequested
               && await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
        {
            if (!_options.IsEventSchedulerEnabled)
            {
                break;
            }

            try
            {
                await ScheduleAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                _retryCount++;
                _logger.LogError(exception,
                    "An error occurred while scheduling events. " +
                    "Retry count: {RetryCount}", _retryCount);

                if (_retryCount < _options.MaxSchedulerRetries)
                {
                    continue;
                }

                _logger.LogError("Maximum retry count reached. " +
                                 "Stopping the event scheduler.");

                using CancellationTokenSource cts =
                    CancellationTokenSource.CreateLinkedTokenSource(
                        stoppingToken,
                        new CancellationToken(true));

                stoppingToken = cts.Token;

                await StopAsync(stoppingToken).ConfigureAwait(false);

                break;
            }
        }
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        _optionsMonitor?.Dispose();
        base.Dispose();
    }
}