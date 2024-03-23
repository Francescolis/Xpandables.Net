
/*******************************************************************************
 * Copyright (C) 2023 Francis-Black EWANE
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

namespace Xpandables.Net;

/// <summary>
/// Provides with logger extensions.
/// </summary>
public static partial class LogExtensions
{
    /// <summary>
    /// Logs a warning message when there is no event handler implementation found.
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="eventName"></param>
    [LoggerMessage(EventId = 0, Level = LogLevel.Warning,
        Message = "There is no event handler implementation found for : {eventName}")]
    public static partial void CouldNotFindEventHandler(
        this ILogger logger,
        string eventName);

    /// <summary>
    /// Logs an error message when an error occurred when executing a process.
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="processName"></param>
    /// <param name="exception"></param>
    [LoggerMessage(EventId = 1, Level = LogLevel.Error,
        Message = "Error occurred when executing {processName}")]
    public static partial void ErrorExecutingProcess(
        this ILogger logger,
        string processName,
        Exception exception);

    /// <summary>
    /// Logs a warning message when retrying to execute a process.
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="processName"></param>
    /// <param name="retryCount"></param>
    /// <param name="delay"></param>
    [LoggerMessage(EventId = 3, Level = LogLevel.Warning,
               Message = "Retrying to execute {processName} ({retryCount}) in {delay} ms")]
    public static partial void RetryExecutingProcess(
        this ILogger logger,
        string processName,
        int retryCount,
        int delay);

    /// <summary>
    /// Logs a warning message when a cancellation occurred when executing a process.
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="processName"></param>
    /// <param name="exception"></param>
    [LoggerMessage(EventId = 2, Level = LogLevel.Warning,
        Message = "Execution has been canceled {processName}")]
    public static partial void CancelExecutingProcess(
        this ILogger logger,
        string processName,
        Exception exception);
}
