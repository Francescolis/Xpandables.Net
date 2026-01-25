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
using System.Collections.Immutable;

namespace System.Rests.Abstractions;

/// <summary>
/// Configuration options for the REST client including timeout, retry, and circuit breaker policies.
/// </summary>
public sealed class RestClientOptions
{
    /// <summary>
    /// Gets or sets the default timeout for HTTP requests.
    /// Default is 30 seconds.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the retry policy options.
    /// Set to <c>null</c> to disable retry behavior.
    /// </summary>
    public RestRetryOptions? Retry { get; set; }

    /// <summary>
    /// Gets or sets the circuit breaker policy options.
    /// Set to <c>null</c> to disable circuit breaker behavior.
    /// </summary>
    public RestCircuitBreakerOptions? CircuitBreaker { get; set; }

    /// <summary>
    /// Gets or sets whether to enable request/response logging.
    /// Default is <c>false</c>.
    /// </summary>
    public bool EnableLogging { get; set; }

    /// <summary>
    /// Gets or sets the log level for request/response logging.
    /// Default is <see cref="RestLogLevel.Information"/>.
    /// </summary>
    public RestLogLevel LogLevel { get; set; } = RestLogLevel.Information;

    /// <summary>
    /// Gets or sets whether to log request/response bodies.
    /// Default is <c>false</c> for security and performance reasons.
    /// </summary>
    public bool LogRequestBody { get; set; }

    /// <summary>
    /// Gets or sets whether to log response bodies.
    /// Default is <c>false</c> for security and performance reasons.
    /// </summary>
    public bool LogResponseBody { get; set; }
}

/// <summary>
/// Configuration options for retry behavior.
/// </summary>
public sealed class RestRetryOptions
{
    /// <summary>
    /// Gets or sets the maximum number of retry attempts.
    /// Default is 3.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Gets or sets the delay between retry attempts.
    /// Default is 1 second.
    /// </summary>
    public TimeSpan Delay { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Gets or sets the maximum delay between retry attempts when using exponential backoff.
    /// Default is 30 seconds.
    /// </summary>
    public TimeSpan MaxDelay { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets whether to use exponential backoff for delays.
    /// Default is <c>true</c>.
    /// </summary>
    public bool UseExponentialBackoff { get; set; } = true;

    /// <summary>
    /// Gets or sets the jitter factor for randomizing delays (0.0 to 1.0).
    /// Default is 0.2 (20% jitter).
    /// </summary>
    public double JitterFactor { get; set; } = 0.2;

    /// <summary>
    /// Gets the HTTP status codes that should trigger a retry.
    /// Default includes 408 (Request Timeout), 429 (Too Many Requests), 500, 502, 503, 504.
    /// </summary>
    public ImmutableArray<int> RetryableStatusCodes { get; init; } =
    [
        408, // Request Timeout
        429, // Too Many Requests
        500, // Internal Server Error
        502, // Bad Gateway
        503, // Service Unavailable
        504  // Gateway Timeout
    ];
}

/// <summary>
/// Configuration options for circuit breaker behavior.
/// </summary>
public sealed class RestCircuitBreakerOptions
{
    /// <summary>
    /// Gets or sets the number of consecutive failures before opening the circuit.
    /// Default is 5.
    /// </summary>
    public int FailureThreshold { get; set; } = 5;

    /// <summary>
    /// Gets or sets the duration the circuit stays open before transitioning to half-open.
    /// Default is 30 seconds.
    /// </summary>
    public TimeSpan BreakDuration { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the sampling duration for calculating failure rate.
    /// Default is 30 seconds.
    /// </summary>
    public TimeSpan SamplingDuration { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the minimum throughput required in the sampling duration
    /// before the circuit breaker can open.
    /// Default is 10.
    /// </summary>
    public int MinimumThroughput { get; set; } = 10;
}

/// <summary>
/// Log levels for REST client logging.
/// </summary>
public enum RestLogLevel
{
    /// <summary>
    /// Logs that track the general flow of the application.
    /// </summary>
    Trace,

    /// <summary>
    /// Logs that are useful for debugging.
    /// </summary>
    Debug,

    /// <summary>
    /// Logs that track the general flow of the application.
    /// </summary>
    Information,

    /// <summary>
    /// Logs that highlight an abnormal or unexpected event.
    /// </summary>
    Warning,

    /// <summary>
    /// Logs that highlight when the current flow of execution is stopped due to a failure.
    /// </summary>
    Error,

    /// <summary>
    /// Logs that describe an unrecoverable application or system crash.
    /// </summary>
    Critical,

    /// <summary>
    /// Specifies that logging should not write any messages.
    /// </summary>
    None
}
