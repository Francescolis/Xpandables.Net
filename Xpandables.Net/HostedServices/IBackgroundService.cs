﻿/************************************************************************************************************
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
using Microsoft.Extensions.Hosting;

using Xpandables.Net.Operations;

namespace Xpandables.Net.DependencyInjection;

/// <summary>
/// Provides with method to manage background service.
/// </summary>
public interface IBackgroundService : IHostedService, IDisposable
{
    /// <summary>
    /// Determines whether the service is running.
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// tries to stop the service.
    /// </summary>
    /// <param name="cancellationToken">A CancellationToken to observe 
    /// while waiting for the task to complete.</param>
    /// <returns>A task that represents an object of <see cref="OperationResult"/>.</returns>
    Task<OperationResult> StopServiceAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Tries to start the service.
    /// </summary>
    /// <param name="cancellationToken">A CancellationToken to observe 
    /// while waiting for the task to complete.</param>
    /// <returns>A task that represents an object of <see cref="OperationResult"/>.</returns>
    Task<OperationResult> StartServiceAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the status of the service.
    /// </summary>
    /// <param name="cancellationToken">A cancellationToken to observer 
    /// while waiting for the tack to complete.</param>
    /// <returns>A task that represents an object of <see cref="OperationResult{TValue}"/>.</returns>
    Task<OperationResult<string>> StatusServiceAsync(CancellationToken cancellationToken = default);
}