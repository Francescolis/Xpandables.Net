
/************************************************************************************************************
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
************************************************************************************************************/
namespace Xpandables.Net.HostedServices;

/// <summary>
/// Defines the <see cref="ITransientScheduler"/> options.
/// </summary>
public sealed record class SchedulerOptions
{
    /// <summary>
    /// The delay between two executions.
    /// </summary>
    /// <remarks>The default value is 15000.</remarks>
    public int DelayMilliSeconds { get; init; } = 15000;

    /// <summary>
    /// The delay between two  attempts.
    /// </summary>
    /// <remarks>The default value is 15000.</remarks>
    public int DelayBetweenAttempts { get; init; } = 15000;

    /// <summary>
    /// The total number of events to load for each thread.
    /// </summary>
    /// <remarks>The default value is 100.</remarks>
    public int TotalPerThread { get; init; } = 100;

    /// <summary>
    /// The total number of attempts i case of exception.
    /// </summary>
    /// <remarks>The default value is 5.</remarks>
    public int MaxAttempts { get; init; } = 5;
}

/// <summary>
/// Provides with a method to schedule events when requested.
/// </summary>
public interface ITransientScheduler : IBackgroundService { }