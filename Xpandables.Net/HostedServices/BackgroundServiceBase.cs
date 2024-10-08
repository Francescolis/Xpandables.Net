﻿
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
using Microsoft.Extensions.Hosting;

using Xpandables.Net.Operations;

namespace Xpandables.Net.HostedServices;

/// <summary>
/// Represents a helper class that allows 
/// implementation of <see cref="IBackgroundService"/>.
/// </summary>
/// <typeparam name="TBackgroundService">The type of 
/// target background service.</typeparam>
public abstract class BackgroundServiceBase<TBackgroundService>
    : BackgroundService, IBackgroundService
    where TBackgroundService : IBackgroundService
{
    ///<inheritdoc/>
    public bool IsRunning { get; protected set; }

    ///<inheritdoc/>
    public virtual async Task<IOperationResult> StartServiceAsync(
        CancellationToken cancellationToken = default)
    {
        if (IsRunning)
        {
            return OperationResults
                .BadRequest()
                .WithError(
                    "status",
                    $"{typeof(TBackgroundService).Name} is already up.")
                .Build();
        }

        IsRunning = true;
        await StartAsync(cancellationToken).ConfigureAwait(false);
        return OperationResults.Ok().Build();
    }

    ///<inheritdoc/>
    public virtual async Task<IOperationResult> StopServiceAsync(
        CancellationToken cancellationToken = default)
    {
        if (!IsRunning)
        {
            return OperationResults
                .BadRequest()
                .WithError(
                    "status",
                    $"{typeof(TBackgroundService).Name} is already down.")
                .Build();
        }

        IsRunning = false;
        await StopAsync(cancellationToken).ConfigureAwait(false);
        return OperationResults.Ok().Build();
    }

    ///<inheritdoc/>
    public virtual async Task<IOperationResult<string>> StatusServiceAsync(
        CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        string response = $"{typeof(TBackgroundService).Name}" +
            $" {(IsRunning ? "Is Up" : "Is Down")}";

        return OperationResults
            .Ok(response)
            .Build();
    }
}