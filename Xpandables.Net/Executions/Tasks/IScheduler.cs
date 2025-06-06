﻿/*******************************************************************************
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
using Microsoft.Extensions.Hosting;

namespace Xpandables.Net.Executions.Tasks;

/// <summary>
/// Provides an abstraction for a scheduler responsible for managing and executing scheduled events.
/// </summary>
/// <remarks>
/// Implementing this interface allows a class to handle scheduling of tasks or events. It extends <see cref="IHostedService"/>
/// to integrate with the application's hosted service infrastructure and <see cref="IDisposable"/> for resource cleanup.
/// </remarks>
public interface IScheduler : IHostedService, IDisposable
{
    /// <summary>
    /// Schedules events asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task ScheduleAsync(CancellationToken cancellationToken = default);
}
