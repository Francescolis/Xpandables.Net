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
using Microsoft.Extensions.Logging;

namespace System.Events.Integration;

public sealed partial class Scheduler
{
    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Information,
        Message = "Scheduler initialized with MaxConcurrentProcessors: {MaxConcurrentProcessors}, BatchSize: {BatchSize}, Frequency: {Frequency}ms")]
    private static partial void LogSchedulerInitialized(ILogger logger, int maxConcurrentProcessors, int batchSize, int frequency);

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Information,
        Message = "Scheduler options updated. MaxConcurrentProcessors: {MaxConcurrentProcessors}, BatchSize: {BatchSize}, CircuitBreakerThreshold: {CircuitBreakerThreshold}")]
    private static partial void LogOptionsUpdated(ILogger logger, int maxConcurrentProcessors, int batchSize, int circuitBreakerThreshold);

    [LoggerMessage(
        EventId = 1003,
        Level = LogLevel.Debug,
        Message = "Event scheduler is disabled, skipping execution")]
    private static partial void LogSchedulerDisabled(ILogger logger);

    [LoggerMessage(
        EventId = 1004,
        Level = LogLevel.Debug,
        Message = "Circuit breaker is open, skipping execution")]
    private static partial void LogCircuitBreakerOpen(ILogger logger);

    [LoggerMessage(
        EventId = 1005,
        Level = LogLevel.Information,
        Message = "Circuit breaker transitioning to half-open state")]
    private static partial void LogCircuitBreakerHalfOpen(ILogger logger);

    [LoggerMessage(
        EventId = 1006,
        Level = LogLevel.Debug,
        Message = "No events to schedule")]
    private static partial void LogNoEventsToSchedule(ILogger logger);

    [LoggerMessage(
        EventId = 1008,
        Level = LogLevel.Debug,
        Message = "Processing {EventCount} events in {BatchCount} batches")]
    private static partial void LogProcessingBatches(ILogger logger, int eventCount, int batchCount);

    [LoggerMessage(
        EventId = 1009,
        Level = LogLevel.Debug,
        Message = "Successfully processed {ProcessedCount} events in {ElapsedMs}ms")]
    private static partial void LogSuccessfulProcessing(ILogger logger, int processedCount, long elapsedMs);

    [LoggerMessage(
        EventId = 1010,
        Level = LogLevel.Warning,
        Message = "Processed {ProcessedCount} events with {ErrorCount} errors in {ElapsedMs}ms")]
    private static partial void LogProcessingWithErrors(ILogger logger, int processedCount, int errorCount, long elapsedMs);

    [LoggerMessage(
        EventId = 1011,
        Level = LogLevel.Error,
        Message = "Scheduler execution failed. Consecutive failures: {ConsecutiveFailures}")]
    private static partial void LogSchedulerExecutionFailed(ILogger logger, Exception exception, int consecutiveFailures);

    [LoggerMessage(
        EventId = 1013,
        Level = LogLevel.Warning,
        Message = "Circuit breaker opened due to {FailureCount} consecutive failures")]
    private static partial void LogCircuitBreakerOpened(ILogger logger, int failureCount);

    [LoggerMessage(
        EventId = 1014,
        Level = LogLevel.Information,
        Message = "Circuit breaker closed after successful execution")]
    private static partial void LogCircuitBreakerClosed(ILogger logger);

    [LoggerMessage(
        EventId = 1015,
        Level = LogLevel.Debug,
        Message = "Reset circuit breaker failure count after successful execution")]
    private static partial void LogCircuitBreakerFailureCountReset(ILogger logger);

    [LoggerMessage(
        EventId = 1016,
        Level = LogLevel.Debug,
        Message = "Applied exponential backoff: {BackoffDelay}ms for {ConsecutiveFailures} consecutive failures")]
    private static partial void LogBackoffApplied(ILogger logger, double backoffDelay, int consecutiveFailures);

    [LoggerMessage(
        EventId = 1018,
        Level = LogLevel.Error,
        Message = "Failed to process event {EventId} of type {EventType}")]
    private static partial void LogEventProcessingFailed(ILogger logger, Exception exception, Guid eventId, string eventType);

    [LoggerMessage(
        EventId = 1019,
        Level = LogLevel.Error,
        Message = "Failed to update outbox status for event {EventId}")]
    private static partial void LogOutboxUpdateFailed(ILogger logger, Exception exception, Guid eventId);

    [LoggerMessage(
        EventId = 1020,
        Level = LogLevel.Warning,
        Message = "Event {EventId} cancelled, marked as error before propagating cancellation")]
    private static partial void LogEventCancelled(ILogger logger, Guid eventId);

    [LoggerMessage(
        EventId = 1022,
        Level = LogLevel.Information,
        Message = "Scheduler disposed. Final metrics - Total processed: {TotalProcessed}, Total errors: {TotalErrors}, Average processing time: {AvgTime}ms")]
    private static partial void LogSchedulerDisposed(ILogger logger, long totalProcessed, long totalErrors, double avgTime);
}
