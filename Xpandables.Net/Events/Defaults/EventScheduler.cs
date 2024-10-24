
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

using Xpandables.Net.Events.Filters;
using Xpandables.Net.Repositories;

namespace Xpandables.Net.Events.Defaults;

/// <summary>
/// Represents a background service that schedules and publishes events.
/// </summary>
public sealed class EventScheduler : BackgroundService, IEventScheduler
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IDisposable? _optionsMonitor;
    private EventOptions _options;
    private readonly ILogger<EventScheduler> _logger;

    private uint _retryCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventScheduler"/> class.
    /// </summary>
    /// <param name="serviceScopeFactory">The service scope factory to create 
    /// service scopes.</param>
    /// <param name="options">The options monitor to track changes in 
    /// event options.</param>
    /// <param name="logger">The logger to log information and errors.</param>
    public EventScheduler(
        IServiceScopeFactory serviceScopeFactory,
        IOptionsMonitor<EventOptions> options,
        ILogger<EventScheduler> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _options = options.CurrentValue;
        _logger = logger;

        _optionsMonitor = options
            .OnChange(updatedOptions => _options = updatedOptions);
    }

    /// <inheritdoc/>
    public async Task ScheduleAsync(CancellationToken cancellationToken = default)
    {
        if (!_options.IsEventSchedulerEnabled)
        {
            _logger.LogWarning("Event scheduler is disabled.");
            return;
        }

        using AsyncServiceScope serviceScope =
            _serviceScopeFactory.CreateAsyncScope();

        IEventPublisher eventPublisher = serviceScope
            .ServiceProvider
            .GetRequiredService<IEventPublisher>();

        IEventStore eventStore = serviceScope
            .ServiceProvider
            .GetRequiredService<IEventStore>();

        IEventFilter eventFilter = new EventEntityFilterIntegration
        {
            Predicate = x => x.Status == EntityStatus.ACTIVE,
            PageIndex = 0,
            PageSize = _options.MaxSchedulerEventPerThread,
            OrderBy = x => x.OrderBy(x => x.CreatedOn)
        };

        IEnumerable<IEventIntegration> events = await eventStore
            .FetchAsync(eventFilter, cancellationToken)
            .OfType<IEventIntegration>()
            .ToListAsync(cancellationToken);

        if (!events.Any())
        {
            _logger.LogInformation("No events to schedule.");
            return;
        }

        IEnumerable<EventPublished> results =
            await eventPublisher
                .PublishAsync(events, cancellationToken)
                .ConfigureAwait(false);

        await eventStore
            .MarkAsPublishedAsync(results, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _retryCount = 0;

        TimeSpan period = TimeSpan.FromMilliseconds(_options.SchedulerRetryInterval);
        using PeriodicTimer timer = new(period);

        while (!stoppingToken.IsCancellationRequested
            && await timer.WaitForNextTickAsync(stoppingToken)
                .ConfigureAwait(false))
        {
            try
            {
                await ScheduleAsync(stoppingToken);
            }
            catch (Exception exception)
            {
                _retryCount++;
                _logger.LogError(exception,
                    "An error occurred while scheduling events. " +
                    "Retry count: {RetryCount}", _retryCount);

                if (_retryCount >= _options.MaxSchedulerRetries)
                {
                    _logger.LogError("Maximum retry count reached. " +
                        "Stopping the event scheduler.");

                    using CancellationTokenSource cts =
                        CancellationTokenSource.CreateLinkedTokenSource(
                            stoppingToken,
                            new CancellationToken(true));

                    stoppingToken = cts.Token;

                    await StopAsync(stoppingToken)
                        .ConfigureAwait(false);

                    break;
                }
            }
        }
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
        _optionsMonitor?.Dispose();
        base.Dispose();
    }
}
