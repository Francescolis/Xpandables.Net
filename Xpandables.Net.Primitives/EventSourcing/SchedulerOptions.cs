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
using System.ComponentModel.DataAnnotations;

namespace Xpandables.Net.EventSourcing;

/// <summary>
/// Represents the configuration options for the high-performance scheduler.
/// This configuration provides comprehensive control over scheduler behavior,
/// including concurrency, batching, circuit breaker, and performance tuning parameters.
/// </summary>
public sealed record SchedulerOptions
{
    /// <summary>
    /// Gets a value indicating whether the event scheduler is enabled.
    /// </summary>
    public bool IsEventSchedulerEnabled { get; set; } = true;

    /// <summary>
    /// Gets the maximum number of retries for the scheduler before giving up.
    /// </summary>
    [Range(1, 100)]
    public uint MaxSchedulerRetries { get; set; } = 5;

    /// <summary>
    /// Gets the interval between scheduler retries in milliseconds.
    /// </summary>
    [Range(100, 60000)]
    public uint SchedulerRetryInterval { get; set; } = 500;

    /// <summary>
    /// Gets the frequency of the scheduler execution in milliseconds.
    /// </summary>
    [Range(1000, 300000)]
    public uint SchedulerFrequency { get; set; } = 15000;

    /// <summary>
    /// Gets the maximum number of events per batch for the scheduler.
    /// This replaces the previous MaxSchedulerEventPerThread for clarity.
    /// </summary>
    [Range(1, 10000)]
    public ushort BatchSize { get; set; } = 100;

    /// <summary>
    /// Gets the maximum number of concurrent processors for parallel event processing.
    /// </summary>
    [Range(1, 50)]
    public int MaxConcurrentProcessors { get; set; } = Environment.ProcessorCount;

    /// <summary>
    /// Gets the timeout for individual event processing in milliseconds.
    /// </summary>
    [Range(1000, 300000)]
    public uint EventProcessingTimeout { get; set; } = 30000;

    /// <summary>
    /// Gets the circuit breaker failure threshold.
    /// When this number of consecutive failures is reached, the circuit breaker opens.
    /// </summary>
    [Range(1, 100)]
    public int CircuitBreakerFailureThreshold { get; set; } = 10;

    /// <summary>
    /// Gets the circuit breaker timeout duration in milliseconds.
    /// After this duration, the circuit breaker will attempt to close.
    /// </summary>
    [Range(5000, 600000)]
    public uint CircuitBreakerTimeoutMs { get; set; } = 60000;

    /// <summary>
    /// Gets the circuit breaker timeout as a TimeSpan.
    /// </summary>
    public TimeSpan CircuitBreakerTimeout => TimeSpan.FromMilliseconds(CircuitBreakerTimeoutMs);

    /// <summary>
    /// Gets the base delay for exponential backoff in milliseconds.
    /// </summary>
    [Range(100, 10000)]
    public uint BackoffBaseDelayMs { get; set; } = 1000;

    /// <summary>
    /// Gets the maximum delay for exponential backoff in milliseconds.
    /// </summary>
    [Range(5000, 600000)]
    public uint BackoffMaxDelayMs { get; set; } = 300000;

    /// <summary>
    /// Gets a value indicating whether to enable detailed performance metrics collection.
    /// </summary>
    public bool EnableMetrics { get; set; } = true;

    /// <summary>
    /// Gets a value indicating whether to enable health checks for the scheduler.
    /// </summary>
    public bool EnableHealthChecks { get; set; } = true;

    // Backward compatibility
    /// <summary>
    /// Gets the maximum number of events per thread for the scheduler.
    /// This property is obsolete. Use <see cref="BatchSize"/> instead.
    /// </summary>
    [Obsolete("Use BatchSize instead", false)]
    public ushort MaxSchedulerEventPerThread
    {
        get => BatchSize;
        set => BatchSize = value;
    }
}