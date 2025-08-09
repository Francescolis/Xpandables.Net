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
namespace Xpandables.Net.Events;

/// <summary>
/// Represents the configuration options for the scheduler.
/// This configuration controls the behavior of the scheduler,
/// including enabling or disabling the event scheduler, setting the maximum
/// retries for scheduler execution, controlling retry intervals, scheduler frequency,
/// and the maximum number of events processed per thread.
/// </summary>
public sealed record SchedulerOptions
{
    /// <summary>
    /// Gets a value indicating whether the event scheduler is enabled.
    /// </summary>
    public bool IsEventSchedulerEnabled { get; set; } = true;

    /// <summary>
    /// Gets the maximum number of retries for the scheduler.
    /// </summary>
    public uint MaxSchedulerRetries { get; set; } = 5;

    /// <summary>
    /// Gets the interval between scheduler retries in milliseconds.
    /// </summary>
    public uint SchedulerRetryInterval { get; set; } = 500;

    /// <summary>
    /// Gets the frequency of the scheduler in milliseconds.
    /// </summary>
    public uint SchedulerFrequency { get; set; } = 15000;

    /// <summary>
    /// Gets the maximum number of events per thread for the scheduler.
    /// </summary>
    public ushort MaxSchedulerEventPerThread { get; set; } = 100;
}
