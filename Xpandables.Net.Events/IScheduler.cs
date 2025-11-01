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
using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.Hosting;

namespace Xpandables.Net.Events;

/// <summary>
/// Defines a contract for scheduling operations that can be started, stopped, and disposed, typically as part of a
/// hosted service lifecycle.
/// </summary>
/// <remarks>Implementations of this interface are intended to be managed by the host and may perform background
/// scheduling tasks. The interface combines hosted service management with resource cleanup via IDisposable. Thread
/// safety and scheduling guarantees depend on the specific implementation.</remarks>
public interface IScheduler : IHostedService, IDisposable
{
    /// <summary>
    /// Schedules the operation asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the scheduling operation.</param>
    /// <returns>A task that represents the asynchronous scheduling operation.</returns>
    [RequiresDynamicCode("The implementation may use reflection or dynamic code generation.")]
    [RequiresUnreferencedCode("The implementation may access members that are not statically referenced.")]
    Task ScheduleAsync(CancellationToken cancellationToken = default);
}
