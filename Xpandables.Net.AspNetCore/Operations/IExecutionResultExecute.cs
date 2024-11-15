
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

namespace Xpandables.Net.Operations;

/// <summary>
/// Defines a method to execute an execution result within the given HTTP context.
/// </summary>
public interface IExecutionResultExecute
{
    /// <summary>
    /// Executes the execution result asynchronously within the given HTTP context.
    /// </summary>
    /// <param name="httpContext">The HTTP context in which to execute the 
    /// execution result.</param>
    /// <param name="executionResult">The execution result to execute.</param>
    /// <returns>A task that represents the asynchronous execution.</returns>
    Task ExecuteAsync(
        HttpContext httpContext,
        IExecutionResult executionResult);
}
