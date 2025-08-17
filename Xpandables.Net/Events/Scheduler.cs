﻿
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
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Xpandables.Net.Repositories;

namespace Xpandables.Net.Events;

/// <summary>
/// High-performance scheduled background service that implements the <see cref="IScheduler"/> interface.
/// Provides optimized event processing with parallel execution, circuit breaker pattern, 
/// and comprehensive error handling for production environments.
/// </summary>
/// <remarks>
/// This implementation features:
/// - Parallel event processing with configurable concurrency
/// - Circuit breaker pattern for fault tolerance
/// - Exponential backoff retry strategy with secure random jitter
/// - Batched event processing for improved throughput
/// - High-performance logging with LoggerMessage delegates
/// - Memory-efficient processing with proper resource management
/// </remarks>
internal sealed class Scheduler : BackgroundService, IScheduler
{
    #region Private Fields

    private readonly ILogger<Scheduler> _logger;
    private readonly IDisposable? _optionsMonitor;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly SemaphoreSlim _concurrencyLimiter;
    private readonly SchedulerMetrics _metrics;

    private volatile SchedulerOptions _options;
    private volatile CircuitBreakerState _circuitBreakerState = CircuitBreakerState.Closed;
    private volatile int _circuitBreakerFailureCount;
    private volatile int _consecutiveFailures;

    private volatile Func<DateTime> _circuitBreakerLastFailureTimeProvider = static () => DateTime.MinValue;
    private volatile Func<TimeSpan> _currentBackoffDelayProvider = static () => TimeSpan.Zero;

    #endregion

    #region LoggerMessage Delegates (High-Performance Logging)

    private static readonly Action<ILogger, int, int, int, Exception?> LogSchedulerInitialized =
        LoggerMessage.Define<int, int, int>(
            LogLevel.Information,
            new EventId(1001, nameof(LogSchedulerInitialized)),
            "Scheduler initialized with MaxConcurrentProcessors: {MaxConcurrentProcessors}, BatchSize: {BatchSize}, Frequency: {Frequency}ms");

    private static readonly Action<ILogger, int, int, int, Exception?> LogOptionsUpdated =
        LoggerMessage.Define<int, int, int>(
            LogLevel.Information,
            new EventId(1002, nameof(LogOptionsUpdated)),
            "Scheduler options updated. MaxConcurrentProcessors: {MaxConcurrentProcessors}, BatchSize: {BatchSize}, CircuitBreakerThreshold: {CircuitBreakerThreshold}");

    private static readonly Action<ILogger, Exception?> LogSchedulerDisabled =
        LoggerMessage.Define(
            LogLevel.Debug,
            new EventId(1003, nameof(LogSchedulerDisabled)),
            "Event scheduler is disabled, skipping execution");

    private static readonly Action<ILogger, Exception?> LogCircuitBreakerOpen =
        LoggerMessage.Define(
            LogLevel.Debug,
            new EventId(1004, nameof(LogCircuitBreakerOpen)),
            "Circuit breaker is open, skipping execution");

    private static readonly Action<ILogger, Exception?> LogCircuitBreakerHalfOpen =
        LoggerMessage.Define(
            LogLevel.Information,
            new EventId(1005, nameof(LogCircuitBreakerHalfOpen)),
            "Circuit breaker transitioning to half-open state");

    private static readonly Action<ILogger, Exception?> LogNoEventsToSchedule =
        LoggerMessage.Define(
            LogLevel.Debug,
            new EventId(1006, nameof(LogNoEventsToSchedule)),
            "No events to schedule");

    private static readonly Action<ILogger, Exception?> LogNoValidEventsCollected =
        LoggerMessage.Define(
            LogLevel.Debug,
            new EventId(1007, nameof(LogNoValidEventsCollected)),
            "No valid events collected for processing");

    private static readonly Action<ILogger, int, int, Exception?> LogProcessingBatches =
        LoggerMessage.Define<int, int>(
            LogLevel.Debug,
            new EventId(1008, nameof(LogProcessingBatches)),
            "Processing {EventCount} events in {BatchCount} batches");

    private static readonly Action<ILogger, int, long, Exception?> LogSuccessfulProcessing =
        LoggerMessage.Define<int, long>(
            LogLevel.Debug,
            new EventId(1009, nameof(LogSuccessfulProcessing)),
            "Successfully processed {ProcessedCount} events in {ElapsedMs}ms");

    private static readonly Action<ILogger, int, int, long, Exception?> LogProcessingWithErrors =
        LoggerMessage.Define<int, int, long>(
            LogLevel.Warning,
            new EventId(1010, nameof(LogProcessingWithErrors)),
            "Processed {ProcessedCount} events with {ErrorCount} errors in {ElapsedMs}ms");

    private static readonly Action<ILogger, int, Exception?> LogSchedulerExecutionFailed =
        LoggerMessage.Define<int>(
            LogLevel.Error,
            new EventId(1011, nameof(LogSchedulerExecutionFailed)),
            "Scheduler execution failed. Consecutive failures: {ConsecutiveFailures}");

    private static readonly Action<ILogger, Exception> LogBackgroundServiceError =
        LoggerMessage.Define(
            LogLevel.Error,
            new EventId(1012, nameof(LogBackgroundServiceError)),
            "Background service execution error. Will retry with backoff");

    private static readonly Action<ILogger, int, Exception?> LogCircuitBreakerOpened =
        LoggerMessage.Define<int>(
            LogLevel.Warning,
            new EventId(1013, nameof(LogCircuitBreakerOpened)),
            "Circuit breaker opened due to {FailureCount} consecutive failures");

    private static readonly Action<ILogger, Exception?> LogCircuitBreakerClosed =
        LoggerMessage.Define(
            LogLevel.Information,
            new EventId(1014, nameof(LogCircuitBreakerClosed)),
            "Circuit breaker closed after successful execution");

    private static readonly Action<ILogger, Exception?> LogCircuitBreakerFailureCountReset =
        LoggerMessage.Define(
            LogLevel.Debug,
            new EventId(1015, nameof(LogCircuitBreakerFailureCountReset)),
            "Reset circuit breaker failure count after successful execution");

    private static readonly Action<ILogger, double, int, Exception?> LogBackoffApplied =
        LoggerMessage.Define<double, int>(
            LogLevel.Debug,
            new EventId(1016, nameof(LogBackoffApplied)),
            "Applied exponential backoff: {BackoffDelay}ms for {ConsecutiveFailures} consecutive failures");

    private static readonly Action<ILogger, double, Exception?> LogApplyingBackoff =
        LoggerMessage.Define<double>(
            LogLevel.Debug,
            new EventId(1017, nameof(LogApplyingBackoff)),
            "Applying backoff delay: {DelayMs}ms");

    private static readonly Action<ILogger, Guid, string, Exception> LogEventProcessingFailed =
        LoggerMessage.Define<Guid, string>(
            LogLevel.Error,
            new EventId(1018, nameof(LogEventProcessingFailed)),
            "Failed to process event {EventId} of type {EventType}");

    private static readonly Action<ILogger, int, Exception> LogEventStoreBatchUpdateFailed =
        LoggerMessage.Define<int>(
            LogLevel.Error,
            new EventId(1019, nameof(LogEventStoreBatchUpdateFailed)),
            "Failed to update event store for batch of {Count} events");

    private static readonly Action<ILogger, Exception?> LogBackgroundServiceStarting =
        LoggerMessage.Define(
            LogLevel.Information,
            new EventId(1020, nameof(LogBackgroundServiceStarting)),
            "Scheduler background service starting");

    private static readonly Action<ILogger, long, long, double, Exception?> LogBackgroundServiceStopping =
        LoggerMessage.Define<long, long, double>(
            LogLevel.Information,
            new EventId(1021, nameof(LogBackgroundServiceStopping)),
            "Scheduler background service stopping. Total processed: {TotalProcessed}, Total errors: {TotalErrors}, Average processing time: {AvgProcessingTime}ms");

    private static readonly Action<ILogger, long, long, double, Exception?> LogSchedulerDisposed =
        LoggerMessage.Define<long, long, double>(
            LogLevel.Information,
            new EventId(1022, nameof(LogSchedulerDisposed)),
            "Scheduler disposed. Final metrics - Total processed: {TotalProcessed}, Total errors: {TotalErrors}, Average processing time: {AvgTime}ms");

    #endregion

    #region Constructor and Initialization

    public Scheduler(
        IServiceScopeFactory serviceScopeFactory,
        IOptionsMonitor<SchedulerOptions> options,
        ILogger<Scheduler> logger)
    {
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.CurrentValue ?? throw new ArgumentNullException(nameof(options));

        _concurrencyLimiter = new SemaphoreSlim(_options.MaxConcurrentProcessors, _options.MaxConcurrentProcessors);
        _metrics = new SchedulerMetrics();

        _optionsMonitor = options.OnChange(newOptions =>
        {
            var oldMaxConcurrency = _options.MaxConcurrentProcessors;
            _options = newOptions;

            AdjustConcurrencyLimiter(oldMaxConcurrency, newOptions.MaxConcurrentProcessors);

            LogOptionsUpdated(_logger, newOptions.MaxConcurrentProcessors, newOptions.BatchSize,
                newOptions.CircuitBreakerFailureThreshold, null);
        });

        LogSchedulerInitialized(_logger, _options.MaxConcurrentProcessors, _options.BatchSize,
            (int)_options.SchedulerFrequency, null);
    }

    #endregion

    #region Public Interface Implementation

    /// <inheritdoc />
    public async Task ScheduleAsync(CancellationToken cancellationToken = default)
    {
        if (!_options.IsEventSchedulerEnabled)
        {
            LogSchedulerDisabled(_logger, null);
            return;
        }

        if (_circuitBreakerState == CircuitBreakerState.Open)
        {
            if (ShouldAttemptCircuitBreakerReset())
            {
                _circuitBreakerState = CircuitBreakerState.HalfOpen;
                LogCircuitBreakerHalfOpen(_logger, null);
            }
            else
            {
                LogCircuitBreakerOpen(_logger, null);
                return;
            }
        }

        var stopwatch = Stopwatch.StartNew();
        var processedCount = 0;
        var errorCount = 0;

        try
        {
            await using var serviceScope = _serviceScopeFactory.CreateAsyncScope();
            var messageQueue = serviceScope.ServiceProvider.GetRequiredService<IMessageQueue>();
            var eventPublisher = serviceScope.ServiceProvider.GetRequiredService<IPublisher>();
            var eventStore = serviceScope.ServiceProvider.GetRequiredService<IEventStore>();

            await messageQueue.DequeueAsync(_options.BatchSize, cancellationToken).ConfigureAwait(false);

            if (messageQueue.Channel.Reader.Count == 0)
            {
                LogNoEventsToSchedule(_logger, null);
                ResetBackoffDelay(); // Reset on successful empty queue
                HandleCircuitBreakerSuccess();
                return;
            }

            var eventBatches = await CollectEventBatchesAsync(messageQueue, cancellationToken).ConfigureAwait(false);

            if (eventBatches.Count == 0)
            {
                LogNoValidEventsCollected(_logger, null);
                return;
            }

            LogProcessingBatches(_logger, eventBatches.Sum(b => b.Count), eventBatches.Count, null);

            var processingTasks = eventBatches.Select(batch =>
                ProcessEventBatchAsync(batch, eventPublisher, eventStore, cancellationToken));

            var batchResults = await Task.WhenAll(processingTasks).ConfigureAwait(false);

            processedCount = batchResults.Sum(r => r.ProcessedCount);
            errorCount = batchResults.Sum(r => r.ErrorCount);

            UpdateMetrics(processedCount, errorCount, stopwatch.Elapsed);

            if (errorCount == 0)
            {
                ResetBackoffDelay();
                HandleCircuitBreakerSuccess();
                LogSuccessfulProcessing(_logger, processedCount, stopwatch.ElapsedMilliseconds, null);
            }
            else
            {
                LogProcessingWithErrors(_logger, processedCount, errorCount, stopwatch.ElapsedMilliseconds, null);
                HandlePartialFailure(errorCount);
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            HandleSchedulerException(exception);
            throw;
        }
    }

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
            try
            {
                var currentBackoffDelay = _currentBackoffDelayProvider();

                var delayTask = currentBackoffDelay > TimeSpan.Zero
                    ? Task.Delay(currentBackoffDelay, stoppingToken)
                    : Task.CompletedTask;

                var timerResult = await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false);

                await delayTask.ConfigureAwait(false);

                if (!timerResult) break; // Timer was cancelled

                await _concurrencyLimiter.WaitAsync(stoppingToken).ConfigureAwait(false);

                try
                {
                    await ScheduleAsync(stoppingToken).ConfigureAwait(false);
                }
                finally
                {
                    _concurrencyLimiter.Release();
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                HandleBackgroundException(exception);
                await ApplyExponentialBackoffAsync(stoppingToken).ConfigureAwait(false);
            }
        }

        LogBackgroundServiceStopping(_logger, _metrics.TotalProcessedEvents, _metrics.TotalErrors,
            _metrics.AverageProcessingTime.TotalMilliseconds, null);
    }

    #endregion

    #region Private Implementation Methods

    /// <summary>
    /// Collects events from the message queue into batches for parallel processing.
    /// </summary>
    private static async Task<List<List<IIntegrationEvent>>> CollectEventBatchesAsync(
        IMessageQueue messageQueue,
        CancellationToken cancellationToken)
    {
        await Task.Yield(); // Ensure we are not blocking the caller

        var allEvents = new List<IIntegrationEvent>();
        var reader = messageQueue.Channel.Reader;

        // Collect all available events
        while (reader.TryRead(out var @event) && @event is not null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            allEvents.Add(@event);
        }

        if (allEvents.Count == 0)
        {
            return [];
        }

        var batchSize = Math.Max(1, allEvents.Count / Environment.ProcessorCount);
        var batches = new List<List<IIntegrationEvent>>();

        for (int i = 0; i < allEvents.Count; i += batchSize)
        {
            var batch = allEvents.Skip(i).Take(batchSize).ToList();
            batches.Add(batch);
        }

        return batches;
    }

    /// <summary>
    /// Processes a batch of events with error isolation and performance tracking.
    /// </summary>
    private async Task<BatchProcessingResult> ProcessEventBatchAsync(
        List<IIntegrationEvent> events,
        IPublisher eventPublisher,
        IEventStore eventStore,
        CancellationToken cancellationToken)
    {
        var processedCount = 0;
        var errorCount = 0;
        var processedInfos = new List<EventProcessedInfo>(events.Count);

        foreach (var @event in events)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            try
            {
                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeoutCts.CancelAfter(TimeSpan.FromMilliseconds(_options.EventProcessingTimeout));

                await eventPublisher.PublishAsync(@event, timeoutCts.Token).ConfigureAwait(false);

                processedInfos.Add(new EventProcessedInfo
                {
                    EventId = @event.Id,
                    ProcessedOn = DateTime.UtcNow,
                    ErrorMessage = null
                });

                processedCount++;
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                errorCount++;

                var errorMessage = exception switch
                {
                    TimeoutException => "Event processing timeout exceeded",
                    _ => exception.ToString()
                };

                processedInfos.Add(new EventProcessedInfo
                {
                    EventId = @event.Id,
                    ProcessedOn = DateTime.UtcNow,
                    ErrorMessage = errorMessage
                });

                LogEventProcessingFailed(_logger, @event.Id, @event.GetType().Name, exception);
            }
        }

        // Batch update event store for better performance
        if (processedInfos.Count > 0)
        {
            try
            {
                await eventStore.MarkAsProcessedAsync(processedInfos, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                LogEventStoreBatchUpdateFailed(_logger, processedInfos.Count, exception);
                errorCount += processedInfos.Count(p => p.ErrorMessage == null);
            }
        }

        return new BatchProcessingResult(processedCount, errorCount);
    }

    /// <summary>
    /// Handles scheduler-level exceptions with circuit breaker logic.
    /// </summary>
    private void HandleSchedulerException(Exception exception)
    {
        _consecutiveFailures++;
        _metrics.IncrementError();

        LogSchedulerExecutionFailed(_logger, _consecutiveFailures, exception);

        if (_circuitBreakerState != CircuitBreakerState.Open)
        {
            Interlocked.Increment(ref _circuitBreakerFailureCount);

            if (_circuitBreakerFailureCount >= _options.CircuitBreakerFailureThreshold)
            {
                _circuitBreakerState = CircuitBreakerState.Open;
                var currentTime = DateTime.UtcNow;
                _circuitBreakerLastFailureTimeProvider = () => currentTime;

                LogCircuitBreakerOpened(_logger, _circuitBreakerFailureCount, null);
            }
        }

        CalculateBackoffDelay();
    }

    /// <summary>
    /// Handles background service exceptions with exponential backoff.
    /// </summary>
    private void HandleBackgroundException(Exception exception)
    {
        LogBackgroundServiceError(_logger, exception);
        HandleSchedulerException(exception);
    }

    /// <summary>
    /// Handles partial failures where some events succeeded.
    /// </summary>
    private void HandlePartialFailure(int errorCount)
    {
        if (errorCount > _options.BatchSize * 0.5) // More than 50% failed
        {
            _consecutiveFailures++;
            CalculateBackoffDelay();
        }
    }

    /// <summary>
    /// Handles successful execution, resetting failure counters.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void HandleCircuitBreakerSuccess()
    {
        if (_circuitBreakerState == CircuitBreakerState.HalfOpen)
        {
            _circuitBreakerState = CircuitBreakerState.Closed;
            _circuitBreakerFailureCount = 0;
            LogCircuitBreakerClosed(_logger, null);
        }
        else if (_circuitBreakerState == CircuitBreakerState.Closed)
        {
            if (Interlocked.Exchange(ref _circuitBreakerFailureCount, 0) > 0)
            {
                LogCircuitBreakerFailureCountReset(_logger, null);
            }
        }
    }

    /// <summary>
    /// Determines if the circuit breaker should attempt to reset.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool ShouldAttemptCircuitBreakerReset() =>
        DateTime.UtcNow - _circuitBreakerLastFailureTimeProvider() >= _options.CircuitBreakerTimeout;

    /// <summary>
    /// Calculates exponential backoff delay using cryptographically secure random jitter.
    /// </summary>
    private void CalculateBackoffDelay()
    {
        if (_consecutiveFailures == 0)
        {
            _currentBackoffDelayProvider = static () => TimeSpan.Zero;
            return;
        }

        var baseDelay = TimeSpan.FromMilliseconds(_options.BackoffBaseDelayMs);
        var maxDelay = TimeSpan.FromMilliseconds(_options.BackoffMaxDelayMs);

        var exponentialDelay = TimeSpan.FromTicks(baseDelay.Ticks * (1L << Math.Min(_consecutiveFailures - 1, 20)));

        var jitterMs = GenerateSecureRandomJitter((int)baseDelay.TotalMilliseconds);
        var jitter = TimeSpan.FromMilliseconds(jitterMs);

        var calculatedDelay = TimeSpan.FromTicks(Math.Min(exponentialDelay.Ticks + jitter.Ticks, maxDelay.Ticks));

        _currentBackoffDelayProvider = () => calculatedDelay;

        LogBackoffApplied(_logger, calculatedDelay.TotalMilliseconds, _consecutiveFailures, null);
    }

    /// <summary>
    /// Generates cryptographically secure random jitter for backoff calculations.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GenerateSecureRandomJitter(int maxValue)
    {
        if (maxValue <= 0) return 0;

        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[4];
        rng.GetBytes(bytes);
        var randomValue = BitConverter.ToUInt32(bytes, 0);

        return (int)(randomValue % (uint)maxValue);
    }

    /// <summary>
    /// Resets backoff delay after successful execution.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ResetBackoffDelay()
    {
        if (_consecutiveFailures > 0)
        {
            _consecutiveFailures = 0;
            _currentBackoffDelayProvider = static () => TimeSpan.Zero;
        }
    }

    /// <summary>
    /// Applies exponential backoff delay.
    /// </summary>
    private async Task ApplyExponentialBackoffAsync(CancellationToken cancellationToken)
    {
        var currentBackoffDelay = _currentBackoffDelayProvider();
        if (currentBackoffDelay > TimeSpan.Zero)
        {
            LogApplyingBackoff(_logger, currentBackoffDelay.TotalMilliseconds, null);
            await Task.Delay(currentBackoffDelay, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Adjusts concurrency limiter when options change.
    /// </summary>
    private void AdjustConcurrencyLimiter(int oldMaxConcurrency, int newMaxConcurrency)
    {
        if (oldMaxConcurrency == newMaxConcurrency) return;

        var difference = newMaxConcurrency - oldMaxConcurrency;

        if (difference > 0)
        {
            _concurrencyLimiter.Release(difference);
        }
        else
        {
            _ = Task.Run(async () =>
            {
                for (int i = 0; i < Math.Abs(difference); i++)
                {
                    await _concurrencyLimiter.WaitAsync().ConfigureAwait(false);
                }
            });
        }
    }

    /// <summary>
    /// Updates performance metrics.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void UpdateMetrics(int processedCount, int errorCount, TimeSpan elapsed) =>
        _metrics.UpdateMetrics(processedCount, errorCount, elapsed);

    #endregion

    #region Disposal

    /// <inheritdoc />
    public override void Dispose()
    {
        _optionsMonitor?.Dispose();
        _concurrencyLimiter?.Dispose();
        base.Dispose();

        LogSchedulerDisposed(_logger, _metrics.TotalProcessedEvents, _metrics.TotalErrors,
            _metrics.AverageProcessingTime.TotalMilliseconds, null);
    }

    #endregion

    #region Supporting Types

    /// <summary>
    /// Represents the state of the circuit breaker.
    /// </summary>
    private enum CircuitBreakerState
    {
        Closed,   // Normal operation
        Open,     // Circuit breaker tripped, blocking requests
        HalfOpen  // Testing if the circuit can be closed
    }

    /// <summary>
    /// Result of batch processing operation.
    /// </summary>
    private readonly record struct BatchProcessingResult(int ProcessedCount, int ErrorCount);

    /// <summary>
    /// Tracks scheduler performance metrics using thread-safe operations.
    /// </summary>
    private sealed class SchedulerMetrics
    {
        private long _totalProcessedEvents;
        private long _totalErrors;
        private long _totalExecutions;
        private long _totalProcessingTimeTicks;

        public long TotalProcessedEvents => Interlocked.Read(ref _totalProcessedEvents);
        public long TotalErrors => Interlocked.Read(ref _totalErrors);
        public TimeSpan AverageProcessingTime
        {
            get
            {
                var executions = Interlocked.Read(ref _totalExecutions);
                return executions > 0
                    ? TimeSpan.FromTicks(Interlocked.Read(ref _totalProcessingTimeTicks) / executions)
                    : TimeSpan.Zero;
            }
        }

        public void UpdateMetrics(int processedCount, int errorCount, TimeSpan elapsed)
        {
            Interlocked.Add(ref _totalProcessedEvents, processedCount);
            Interlocked.Add(ref _totalErrors, errorCount);
            Interlocked.Increment(ref _totalExecutions);
            Interlocked.Add(ref _totalProcessingTimeTicks, elapsed.Ticks);
        }

        public void IncrementError() => Interlocked.Increment(ref _totalErrors);
    }

    #endregion
}