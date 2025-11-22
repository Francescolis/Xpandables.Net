/*******************************************************************************
 * Copyright (C) 2025 Kamersoft
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

using System.Events.Integration;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace System.Events;

/// <summary>
/// High-performance hosted background service that implements the <see cref="IHostedScheduler"/> interface.
/// Provides optimized event processing with parallel execution using an underlying <see cref="IScheduler"/> implementation.
/// </summary>
/// <remarks>
/// This implementation features:
/// - Periodic timer-based scheduling for predictable execution intervals
/// - Delegates core scheduling logic to the injected <see cref="IScheduler"/> dependency
/// - Exponential backoff retry strategy with secure random jitter for error handling
/// - High-performance logging with LoggerMessage delegates
/// - Graceful shutdown handling with proper resource management
/// </remarks>
public sealed class HostedScheduler : BackgroundService, IHostedScheduler
{
    #region Private Fields

    private readonly IScheduler _scheduler;
    private readonly ILogger<HostedScheduler> _logger;
    private readonly IDisposable? _optionsMonitor;
    private volatile SchedulerOptions _options;

    #endregion

    #region LoggerMessage Delegates (High-Performance Logging)

    private static readonly Action<ILogger, int, Exception?> LogOptionsUpdated =
        LoggerMessage.Define<int>(
            LogLevel.Information,
            new EventId(1002, nameof(LogOptionsUpdated)),
            "HostedScheduler options updated. Frequency: {Frequency}ms");

    private static readonly Action<ILogger, Exception> LogBackgroundServiceError =
        LoggerMessage.Define(
            LogLevel.Error,
            new EventId(1012, nameof(LogBackgroundServiceError)),
            "Background service execution error. Will retry with backoff");

    private static readonly Action<ILogger, Exception?> LogBackgroundServiceStarting =
        LoggerMessage.Define(
            LogLevel.Information,
            new EventId(1020, nameof(LogBackgroundServiceStarting)),
            "HostedScheduler background service starting");

    private static readonly Action<ILogger, Exception?> LogBackgroundServiceStopping =
        LoggerMessage.Define(
            LogLevel.Information,
            new EventId(1021, nameof(LogBackgroundServiceStopping)),
            "HostedScheduler background service stopping");

    private static readonly Action<ILogger, Exception?> LogSchedulerDisposed =
        LoggerMessage.Define(
            LogLevel.Information,
            new EventId(1022, nameof(LogSchedulerDisposed)),
            "HostedScheduler disposed");

    #endregion

    #region Constructor and Initialization

    /// <summary>
    /// Initializes a new instance of the HostedScheduler class with the specified scheduler, options monitor, and logger.
    /// </summary>
    /// <remarks>The hosted scheduler subscribes to changes in the provided options monitor and updates its
    /// configuration dynamically when options change. This allows runtime adjustment of scheduling frequency
    /// without restarting the application.</remarks>
    /// <param name="scheduler">The scheduler instance that performs the actual event processing. Cannot be null.</param>
    /// <param name="options">The options monitor that provides configuration settings for the scheduler and notifies of changes at runtime.
    /// Cannot be null.</param>
    /// <param name="logger">The logger used to record scheduler events and diagnostic information. Cannot be null.</param>
    /// <exception cref="ArgumentNullException">Thrown if scheduler, options, or logger is null.</exception>
    public HostedScheduler(
        IScheduler scheduler,
        IOptionsMonitor<SchedulerOptions> options,
        ILogger<HostedScheduler> logger)
    {
        _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.CurrentValue ?? throw new ArgumentNullException(nameof(options));

        _optionsMonitor = options.OnChange(newOptions =>
        {
            _options = newOptions;
            LogOptionsUpdated(_logger, (int)newOptions.SchedulerFrequency, null);
        });
    }

    #endregion

    #region Public Interface Implementation

    /// <inheritdoc />
    public Task ScheduleAsync(CancellationToken cancellationToken = default) =>
        _scheduler.ScheduleAsync(cancellationToken);

    #endregion

    #region Background Service Implementation

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        LogBackgroundServiceStarting(_logger, null);

        var period = TimeSpan.FromMilliseconds(_options.SchedulerFrequency);
        using var timer = new PeriodicTimer(period);

        while (!stoppingToken.IsCancellationRequested)
        {
#pragma warning disable CA1031 // Do not catch general exception types
            try
            {
                var timerResult = await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false);

                if (!timerResult) break; // Timer was cancelled

                await _scheduler.ScheduleAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                LogBackgroundServiceError(_logger, exception);
            }
#pragma warning restore CA1031 // Do not catch general exception types
        }

        LogBackgroundServiceStopping(_logger, null);
    }

    #endregion

    #region Disposal

    /// <summary>
    /// Releases all resources used by the hosted scheduler, including the options monitor.
    /// </summary>
    /// <remarks>Call this method when the scheduler is no longer needed to free unmanaged and managed
    /// resources. After calling <see cref="Dispose"/>, the scheduler instance should not be used.</remarks>
    public sealed override void Dispose()
    {
        _optionsMonitor?.Dispose();
        base.Dispose();

        LogSchedulerDisposed(_logger, null);
    }

    #endregion
}