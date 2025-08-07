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

using System.Data.Common;

namespace Xpandables.Net.Repositories.Sql;

/// <summary>
/// Defines retry policy for database operations with configurable retry logic.
/// </summary>
internal sealed class RetryPolicy
{
    /// <summary>
    /// Gets the maximum number of retry attempts.
    /// </summary>
    public int MaxRetries { get; init; }

    /// <summary>
    /// Gets the delay between retry attempts.
    /// </summary>
    public TimeSpan Delay { get; init; }

    /// <summary>
    /// Gets the function that determines whether an exception should trigger a retry.
    /// </summary>
    public Func<Exception, bool> ShouldRetry { get; init; } = DefaultShouldRetry;

    /// <summary>
    /// Creates a new retry policy with the specified configuration.
    /// </summary>
    /// <param name="maxRetries">The maximum number of retries.</param>
    /// <param name="delay">The delay between retries.</param>
    /// <param name="shouldRetry">Optional custom retry logic.</param>
    public RetryPolicy(int maxRetries, TimeSpan delay, Func<Exception, bool>? shouldRetry = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maxRetries);
        ArgumentOutOfRangeException.ThrowIfNegative(delay.TotalMilliseconds);

        MaxRetries = maxRetries;
        Delay = delay;
        ShouldRetry = shouldRetry ?? DefaultShouldRetry;
    }

    /// <summary>
    /// Executes a function with retry logic.
    /// </summary>
    /// <typeparam name="T">The return type of the function.</typeparam>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the operation.</returns>
    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        int attempt = 0;
        while (true)
        {
            try
            {
                return await operation().ConfigureAwait(false);
            }
            catch (Exception ex) when (attempt < MaxRetries && ShouldRetry(ex))
            {
                attempt++;
                await Task.Delay(Delay, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// Default logic to determine if an exception should trigger a retry.
    /// Handles common transient SQL Server errors.
    /// </summary>
    /// <param name="exception">The exception to evaluate.</param>
    /// <returns>True if the operation should be retried, false otherwise.</returns>
    private static bool DefaultShouldRetry(Exception exception)
    {
        return exception switch
        {
            DbException dbEx => IsTransientError(dbEx),
            TimeoutException => true,
            InvalidOperationException invalidOpEx when invalidOpEx.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase) => true,
            _ => false
        };
    }

    /// <summary>
    /// Determines if a database exception represents a transient error.
    /// </summary>
    /// <param name="exception">The database exception.</param>
    /// <returns>True if the error is transient, false otherwise.</returns>
    private static bool IsTransientError(DbException exception)
    {
        // Check for common transient error patterns
        string message = exception.Message.ToLowerInvariant();
        
        return message.Contains("timeout") ||
               message.Contains("connection") ||
               message.Contains("network") ||
               message.Contains("deadlock") ||
               message.Contains("memory") ||
               exception.GetType().Name.Contains("Timeout", StringComparison.OrdinalIgnoreCase);
    }
}