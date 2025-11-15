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
using Microsoft.Extensions.Hosting;

namespace Xpandables.Net.EventSourcing;

/// <summary>
/// Defines a scheduler capable of performing scheduling operations asynchronously.
/// </summary>
/// <remarks>Implementations of this interface are responsible for scheduling tasks or operations
/// during their lifecycle. The scheduling operation is performed asynchronously to allow for
/// non-blocking execution and improved responsiveness in applications.</remarks>
public interface IScheduler : IDisposable
{
    /// <summary>
    /// Schedules the operation asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the scheduling operation.</param>
    /// <returns>A task that represents the asynchronous scheduling operation.</returns>
    Task ScheduleAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Defines a scheduler service that can be hosted within an application and managed by the application's lifetime.
/// </summary>
/// <remarks>Implementations of this interface combine scheduling capabilities with hosted service lifecycle
/// management, allowing scheduled tasks to be started and stopped in coordination with the application's host. This is
/// typically used in environments such as ASP.NET Core, where background services are managed by the host.</remarks>
public interface IHostedScheduler : IScheduler, IHostedService
{
}