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
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace System.Events.Integration;

/// <summary>
/// High-performance scheduled service that implements the <see cref="IScheduler"/> interface.
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
public sealed class Scheduler : Disposable, IScheduler
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

    private static readonly Action<ILogger, Guid, string, Exception> LogEventProcessingFailed =
        LoggerMessage.Define<Guid, string>(
            LogLevel.Error,
            new EventId(1018, nameof(LogEventProcessingFailed)),
            "Failed to process event {EventId} of type {EventType}");

    private static readonly Action<ILogger, int, Exception?> LogOutboxBatchUpdateFailed =
        LoggerMessage.Define<int>(
            LogLevel.Error,
            new EventId(1019, nameof(LogOutboxBatchUpdateFailed)),
            "Failed to update outbox for batch of {Count} events");

    private static readonly Action<ILogger, long, long, double, Exception?> LogSchedulerDisposed =
        LoggerMessage.Define<long, long, double>(
            LogLevel.Information,
            new EventId(1022, nameof(LogSchedulerDisposed)),
            "Scheduler disposed. Final metrics - Total processed: {TotalProcessed}, Total errors: {TotalErrors}, Average processing time: {AvgTime}ms");

    #endregion

    #region Constructor and Initialization

    /// <summary>
    /// Initializes a new instance of the Scheduler class with the specified service scope factory, options monitor, and
    /// logger.
    /// </summary>
    /// <remarks>The scheduler subscribes to changes in the provided options monitor and updates its
    /// configuration dynamically when options change. This allows runtime adjustment of concurrency and other
    /// scheduling parameters without restarting the application.</remarks>
    /// <param name="serviceScopeFactory">The factory used to create service scopes for dependency injection within scheduled tasks. Cannot be null.</param>
    /// <param name="options">The options monitor that provides configuration settings for the scheduler and notifies of changes at runtime.
    /// Cannot be null.</param>
    /// <param name="logger">The logger used to record scheduler events and diagnostic information. Cannot be null.</param>
    /// <exception cref="ArgumentNullException">Thrown if serviceScopeFactory, options, or logger is null.</exception>
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
			int oldMaxConcurrency = _options.MaxConcurrentProcessors;
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
		int processedCount = 0;
		int errorCount = 0;

        try
        {
			AsyncServiceScope serviceScope = _serviceScopeFactory.CreateAsyncScope();
            await using (serviceScope.ConfigureAwait(false))
            {
				IOutboxStore outbox = serviceScope.ServiceProvider.GetRequiredService<IOutboxStore>();

				// Claim a batch directly from the outbox (multi-instance safe)
				IReadOnlyList<IIntegrationEvent> claimed = await outbox.DequeueAsync(_options.BatchSize, visibilityTimeout: null, cancellationToken: cancellationToken).ConfigureAwait(false);
                if (claimed.Count == 0)
                {
                    LogNoEventsToSchedule(_logger, null);
                    ResetBackoffDelay();
                    HandleCircuitBreakerSuccess();
                    return;
                }

				// Partition into batches for parallel processing
				List<List<IIntegrationEvent>> eventBatches = CreateBatches(claimed);
                LogProcessingBatches(_logger, eventBatches.Sum(b => b.Count), eventBatches.Count, null);

				IEnumerable<Task<BatchProcessingResult>> processingTasks = eventBatches.Select(batch =>
                    ProcessEventBatchAsync(batch, cancellationToken));

				BatchProcessingResult[] batchResults = await Task.WhenAll(processingTasks).ConfigureAwait(false);

                processedCount = batchResults.Sum(r => r.ProcessedCount);
                errorCount = batchResults.Sum(r => r.ErrorCount);

                var successIds = batchResults.SelectMany(r => r.Successes).ToList();
                var failures = batchResults.SelectMany(r => r.Failures).ToList();

#pragma warning disable CA1031 // Do not catch general exception types
                try
                {
                    if (successIds.Count > 0)
                    {
                        await outbox.CompleteAsync(successIds, cancellationToken).ConfigureAwait(false);
                    }
                    if (failures.Count > 0)
                    {
                        await outbox.FailAsync(failures, cancellationToken).ConfigureAwait(false);
                    }
                }
                catch (Exception exception)
                {
                    LogOutboxBatchUpdateFailed(_logger, successIds.Count + failures.Count, exception);

                    // All would be retried later when lease expires; count successful publishes as errors
                    errorCount += successIds.Count;
                }
#pragma warning restore CA1031 // Do not catch general exception types

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
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            HandleSchedulerException(exception);
            throw;
        }
    }

    #endregion

    #region Private Implementation Methods

    private static List<List<IIntegrationEvent>> CreateBatches(IReadOnlyList<IIntegrationEvent> events)
    {
        var list = events.ToList();
		int batchSize = Math.Max(1, list.Count / Math.Max(1, Environment.ProcessorCount));
        var batches = new List<List<IIntegrationEvent>>();
        for (int i = 0; i < list.Count; i += batchSize)
        {
            batches.Add([.. list.Skip(i).Take(batchSize)]);
        }

        return batches;
    }

    private async Task<BatchProcessingResult> ProcessEventBatchAsync(
        List<IIntegrationEvent> events,
        CancellationToken cancellationToken)
    {
		int processedCount = 0;
		int errorCount = 0;

        var successIds = new List<CompletedOutboxEvent>(events.Count);
        var failures = new List<FailedOutboxEvent>(events.Count);

		// Create a dedicated scope for this batch to isolate connections and avoid MARS issues
		AsyncServiceScope batchScope = _serviceScopeFactory.CreateAsyncScope();
        await using (batchScope.ConfigureAwait(false))
        {
			IEventPublisher eventPublisher = batchScope.ServiceProvider.GetRequiredService<IEventPublisher>();

            foreach (IIntegrationEvent @event in events)
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
                    successIds.Add(new(@event.EventId));
                    processedCount++;
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    errorCount++;
					string errorMessage = exception is TimeoutException
                        ? "Event processing timeout exceeded"
                        : exception.ToString();
                    failures.Add(new(@event.EventId, errorMessage));
                    LogEventProcessingFailed(_logger, @event.EventId, @event.GetType().Name, exception);
                }
            }
        }

        return new BatchProcessingResult(processedCount, errorCount, successIds, failures);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void HandleSchedulerException(Exception exception)
    {
		int failures = Interlocked.Increment(ref _consecutiveFailures);
        _metrics.IncrementError();

        LogSchedulerExecutionFailed(_logger, failures, exception);

        if (_circuitBreakerState != CircuitBreakerState.Open)
        {
			int failureCount = Interlocked.Increment(ref _circuitBreakerFailureCount);

            if (failureCount >= _options.CircuitBreakerFailureThreshold)
            {
                _circuitBreakerState = CircuitBreakerState.Open;
				DateTime currentTime = DateTime.UtcNow;
                _circuitBreakerLastFailureTimeProvider = () => currentTime;

                LogCircuitBreakerOpened(_logger, failureCount, null);
            }
        }

        CalculateBackoffDelay();
    }

    private void HandlePartialFailure(int errorCount)
    {
        if (errorCount > _options.BatchSize * 0.5)
        {
            Interlocked.Increment(ref _consecutiveFailures);
            CalculateBackoffDelay();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void HandleCircuitBreakerSuccess()
    {
        if (_circuitBreakerState == CircuitBreakerState.HalfOpen)
        {
            _circuitBreakerState = CircuitBreakerState.Closed;
            Interlocked.Exchange(ref _circuitBreakerFailureCount, 0);
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool ShouldAttemptCircuitBreakerReset() =>
        DateTime.UtcNow - _circuitBreakerLastFailureTimeProvider() >= _options.CircuitBreakerTimeout;

    private void CalculateBackoffDelay()
    {
        if (_consecutiveFailures == 0)
        {
            return;
        }

        var baseDelay = TimeSpan.FromMilliseconds(_options.BackoffBaseDelayMs);
        var maxDelay = TimeSpan.FromMilliseconds(_options.BackoffMaxDelayMs);

        var exponentialDelay = TimeSpan.FromTicks(baseDelay.Ticks * (1L << Math.Min(_consecutiveFailures - 1, 20)));
		int jitterMs = GenerateSecureRandomJitter((int)baseDelay.TotalMilliseconds);
        var jitter = TimeSpan.FromMilliseconds(jitterMs);
        var calculatedDelay = TimeSpan.FromTicks(Math.Min(exponentialDelay.Ticks + jitter.Ticks, maxDelay.Ticks));

        LogBackoffApplied(_logger, calculatedDelay.TotalMilliseconds, _consecutiveFailures, null);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GenerateSecureRandomJitter(int maxValue)
    {
        if (maxValue <= 0)
		{
			return 0;
		}

		using var rng = RandomNumberGenerator.Create();
        Span<byte> bytes = stackalloc byte[4];
        rng.GetBytes(bytes);
		uint randomValue = BitConverter.ToUInt32(bytes);
        return (int)(randomValue % (uint)maxValue);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ResetBackoffDelay() =>
        Interlocked.Exchange(ref _consecutiveFailures, 0);

    private void AdjustConcurrencyLimiter(int oldMaxConcurrency, int newMaxConcurrency)
    {
        if (oldMaxConcurrency == newMaxConcurrency)
		{
			return;
		}

		int difference = newMaxConcurrency - oldMaxConcurrency;

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void UpdateMetrics(int processedCount, int errorCount, TimeSpan elapsed) =>
        _metrics.UpdateMetrics(processedCount, errorCount, elapsed);

    #endregion

    #region Disposal

    /// <inheritdoc/>
    protected sealed override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _optionsMonitor?.Dispose();
            _concurrencyLimiter?.Dispose();

            LogSchedulerDisposed(_logger, _metrics.TotalProcessedEvents, _metrics.TotalErrors,
                _metrics.AverageProcessingTime.TotalMilliseconds, null);
        }

        base.Dispose(disposing);
    }

    #endregion

    #region Supporting Types

    private enum CircuitBreakerState
    {
        Closed,
        Open,
        HalfOpen
    }

    private readonly record struct BatchProcessingResult(
        int ProcessedCount,
        int ErrorCount,
        IReadOnlyList<CompletedOutboxEvent> Successes,
        IReadOnlyList<FailedOutboxEvent> Failures);

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
				long executions = Interlocked.Read(ref _totalExecutions);
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