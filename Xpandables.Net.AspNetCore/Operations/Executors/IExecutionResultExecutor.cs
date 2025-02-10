﻿
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
using Microsoft.AspNetCore.Http;

using Xpandables.Net.Executions;

namespace Xpandables.Net.Operations.Executors;

/// <summary>
/// Defines a contract for executing execution results.
/// </summary>
public interface IExecutionResultExecutor
{
    /// <summary>
    /// Determines whether the specified execution result can be executed by the
    /// executor.
    /// </summary>
    /// <param name="executionResult">The execution result to check.</param>
    /// <returns><see langword="true"/> if the execution result can be executed
    /// by the executor; otherwise, <see langword="false"/>.</returns>
    bool CanExecute(IExecutionResult executionResult);

    /// <summary>
    /// Executes the execution result asynchronously.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="executionResult">The execution result to execute.</param>
    /// <returns>A task that represents the asynchronous execution.</returns>
    Task ExecuteAsync(
        HttpContext context,
        IExecutionResult executionResult);
}
