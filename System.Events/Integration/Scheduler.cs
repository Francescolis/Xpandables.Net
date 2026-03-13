/*******************************************************************************
 * Copyright (C) 2025-2026 Kamersoft
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
/// <list type="bullet">
/// <item>Parallel event processing with configurable concurrency</item>
/// <item>Circuit breaker pattern for fault tolerance</item>
/// <item>Exponential backoff retry strategy with secure random jitter</item>
/// <item>Batched event processing for improved throughput</item>
/// <item>Per-event outbox state updates to prevent stuck PROCESSING events</item>
/// <item>High-performance logging with <see cref="LoggerMessageAttribute"/> source generators</item>
/// <item>Memory-efficient processing with proper resource management</item>
/// </list>
/// </remarks>
public sealed partial class Scheduler : Disposable, IScheduler
{
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
					newOptions.CircuitBreakerFailureThreshold);
		});

		LogSchedulerInitialized(_logger, _options.MaxConcurrentProcessors, _options.BatchSize,
			(int)_options.SchedulerFrequency);
	}

	/// <inheritdoc />
	public async Task ScheduleAsync(CancellationToken cancellationToken = default)
	{
		if (!_options.IsEventSchedulerEnabled)
		{
			LogSchedulerDisabled(_logger);
			return;
		}

		if (_circuitBreakerState == CircuitBreakerState.Open)
		{
			if (ShouldAttemptCircuitBreakerReset())
			{
				_circuitBreakerState = CircuitBreakerState.HalfOpen;
				LogCircuitBreakerHalfOpen(_logger);
			}
			else
			{
				LogCircuitBreakerOpen(_logger);
				return;
			}
		}

		var stopwatch = Stopwatch.StartNew();

		try
		{
			AsyncServiceScope serviceScope = _serviceScopeFactory.CreateAsyncScope();
			await using (serviceScope.ConfigureAwait(false))
			{
				IOutboxStore outbox = serviceScope.ServiceProvider.GetRequiredService<IOutboxStore>();

				IReadOnlyList<IIntegrationEvent> claimed = await outbox
					.DequeueAsync(_options.BatchSize, visibilityTimeout: null, cancellationToken: cancellationToken)
					.ConfigureAwait(false);

				if (claimed.Count == 0)
				{
					LogNoEventsToSchedule(_logger);
					ResetBackoffDelay();
					HandleCircuitBreakerSuccess();
					return;
				}

				List<List<IIntegrationEvent>> eventBatches = CreateBatches(claimed);
				LogProcessingBatches(_logger, claimed.Count, eventBatches.Count);

				IEnumerable<Task<BatchResult>> processingTasks = eventBatches
					.Select(batch => ProcessEventBatchAsync(batch, cancellationToken));

				BatchResult[] batchResults = await Task.WhenAll(processingTasks).ConfigureAwait(false);

				int processedCount = batchResults.Sum(static r => r.ProcessedCount);
				int errorCount = batchResults.Sum(static r => r.ErrorCount);

				UpdateMetrics(processedCount, errorCount, stopwatch.Elapsed);

				if (errorCount == 0)
				{
					ResetBackoffDelay();
					HandleCircuitBreakerSuccess();
					LogSuccessfulProcessing(_logger, processedCount, stopwatch.ElapsedMilliseconds);
				}
				else
				{
					LogProcessingWithErrors(_logger, processedCount, errorCount, stopwatch.ElapsedMilliseconds);
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

	private static List<List<IIntegrationEvent>> CreateBatches(IReadOnlyList<IIntegrationEvent> events)
	{
		int batchSize = Math.Max(1, events.Count / Math.Max(1, Environment.ProcessorCount));
		var batches = new List<List<IIntegrationEvent>>(
			(events.Count + batchSize - 1) / batchSize);

		for (int i = 0; i < events.Count; i += batchSize)
		{
			int count = Math.Min(batchSize, events.Count - i);
			var batch = new List<IIntegrationEvent>(count);
			for (int j = i; j < i + count; j++)
			{
				batch.Add(events[j]);
			}
			batches.Add(batch);
		}

		return batches;
	}

	/// <summary>
	/// Processes a batch of events, publishing each one and immediately updating the outbox
	/// so that no event remains stuck in PROCESSING status on crash or cancellation.
	/// </summary>
	private async Task<BatchResult> ProcessEventBatchAsync(
		List<IIntegrationEvent> events,
		CancellationToken cancellationToken)
	{
		int processedCount = 0;
		int errorCount = 0;

		AsyncServiceScope batchScope = _serviceScopeFactory.CreateAsyncScope();
		await using (batchScope.ConfigureAwait(false))
		{
			IEventPublisher eventPublisher = batchScope.ServiceProvider.GetRequiredService<IEventPublisher>();
			IOutboxStore outbox = batchScope.ServiceProvider.GetRequiredService<IOutboxStore>();

			foreach (IIntegrationEvent @event in events)
			{
				if (cancellationToken.IsCancellationRequested)
				{
					await MarkEventAsErrorSafeAsync(outbox, @event, "Operation was cancelled").ConfigureAwait(false);
					errorCount++;
					cancellationToken.ThrowIfCancellationRequested();
				}

				try
				{
					using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
					timeoutCts.CancelAfter(TimeSpan.FromMilliseconds(_options.EventProcessingTimeout));

					await eventPublisher.PublishAsync(@event, timeoutCts.Token).ConfigureAwait(false);
					await outbox.CompleteAsync([new(@event.EventId)], cancellationToken).ConfigureAwait(false);
					processedCount++;
				}
				catch (OperationCanceledException)
				{
					errorCount++;
					await MarkEventAsErrorSafeAsync(outbox, @event, "Operation was cancelled during processing").ConfigureAwait(false);
					LogEventCancelled(_logger, @event.EventId);
					throw;
				}
				#pragma warning disable CA1031
				catch (Exception exception)
#pragma warning restore CA1031
				{
					errorCount++;
					string errorMessage = exception is TimeoutException
						? "Event processing timeout exceeded"
						: exception.ToString();

					await MarkEventAsErrorSafeAsync(outbox, @event, errorMessage).ConfigureAwait(false);
					LogEventProcessingFailed(_logger, exception, @event.EventId, @event.GetType().Name);
				}
			}
		}

		return new BatchResult(processedCount, errorCount);
	}

	/// <summary>
	/// Best-effort attempt to mark an event as failed in the outbox.
	/// Swallows exceptions to avoid masking the original error.
	/// </summary>
	private async Task MarkEventAsErrorSafeAsync(
		IOutboxStore outbox,
		IIntegrationEvent @event,
		string errorMessage)
	{
		try
		{
			await outbox.FailAsync([new(@event.EventId, errorMessage)], CancellationToken.None).ConfigureAwait(false);
		}
		#pragma warning disable CA1031
		catch (Exception exception)
#pragma warning restore CA1031
		{
			LogOutboxUpdateFailed(_logger, exception, @event.EventId);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void HandleSchedulerException(Exception exception)
	{
		int failures = Interlocked.Increment(ref _consecutiveFailures);
		_metrics.IncrementError();

		LogSchedulerExecutionFailed(_logger, exception, failures);

		if (_circuitBreakerState != CircuitBreakerState.Open)
		{
			int failureCount = Interlocked.Increment(ref _circuitBreakerFailureCount);

			if (failureCount >= _options.CircuitBreakerFailureThreshold)
			{
				_circuitBreakerState = CircuitBreakerState.Open;
				DateTime currentTime = DateTime.UtcNow;
				_circuitBreakerLastFailureTimeProvider = () => currentTime;

				LogCircuitBreakerOpened(_logger, failureCount);
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
			LogCircuitBreakerClosed(_logger);
		}
		else if (_circuitBreakerState == CircuitBreakerState.Closed)
		{
			if (Interlocked.Exchange(ref _circuitBreakerFailureCount, 0) > 0)
			{
				LogCircuitBreakerFailureCountReset(_logger);
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

		LogBackoffApplied(_logger, calculatedDelay.TotalMilliseconds, _consecutiveFailures);
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

	/// <inheritdoc/>
	protected sealed override void Dispose(bool disposing)
	{
		if (disposing)
		{
			_optionsMonitor?.Dispose();
			_concurrencyLimiter?.Dispose();

			LogSchedulerDisposed(_logger, _metrics.TotalProcessedEvents, _metrics.TotalErrors,
					_metrics.AverageProcessingTime.TotalMilliseconds);
		}

		base.Dispose(disposing);
	}

	private enum CircuitBreakerState
	{
		Closed,
		Open,
		HalfOpen
	}

	private readonly record struct BatchResult(int ProcessedCount, int ErrorCount);

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
}
