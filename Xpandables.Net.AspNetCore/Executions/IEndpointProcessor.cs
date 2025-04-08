
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
/// Executes an execution result asynchronously in a specified HTTP context. 
/// Returns a task representing the asynchronous operation.
/// </summary>
public interface IEndpointProcessor
{
    /// <summary>
    /// Processes an asynchronous operation using the provided context and execution result.
    /// </summary>
    /// <param name="httpContext">Provides the current HTTP context for the operation.</param>
    /// <param name="executionResult">Contains the result of the execution to be processed.</param>
    /// <returns>Returns a task representing the asynchronous operation.</returns>
    Task ProcessAsync(HttpContext httpContext, ExecutionResult executionResult);
}
