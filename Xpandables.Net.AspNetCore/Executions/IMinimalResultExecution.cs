
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

namespace Xpandables.Net.Executions;

/// <summary>
/// Defines an interface for executing minimal results based on an execution result.
/// </summary>
public interface IMinimalResultExecution
{
    /// <summary>
    /// Determines whether the execution can be performed based on the provided execution result.
    /// </summary>
    /// <param name="executionResult">The execution result that contains the status code and other details.</param>
    /// <returns>True if the execution can be performed; otherwise, false.</returns>
    bool CanExecute(ExecutionResult executionResult);

    /// <summary>
    /// Writes the execution result to the HTTP response asynchronously.
    /// </summary>
    /// <param name="context">The HTTP context that contains the request and response information.</param>
    /// <param name="executionResult">The execution result that will be written to the response.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task ExecuteAsync(
        HttpContext context,
        ExecutionResult executionResult);
}
