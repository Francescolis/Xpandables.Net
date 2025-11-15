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
using Microsoft.AspNetCore.Http;

using Xpandables.Net.ExecutionResults;

namespace Xpandables.Net;

/// <summary>
/// Defines a contract for writing an HTTP response based on an execution result.
/// </summary>
/// <remarks>Implementations of this interface determine whether they can handle a given execution result and, if
/// so, write the appropriate response to the HTTP context. This is typically used in scenarios where different result
/// types or formats require custom response handling.</remarks>
public interface IExecutionResultResponseWriter
{
    /// <summary>
    /// Determines whether the specified execution result can be written by this instance.
    /// </summary>
    /// <param name="executionResult">The execution result to evaluate for writability. Cannot be null.</param>
    /// <returns>true if the execution result can be written; otherwise, false.</returns>
    bool CanWrite(ExecutionResult executionResult);

    /// <summary>
    /// Asynchronously writes the specified execution result to the HTTP response.
    /// </summary>
    /// <param name="context">The HTTP context for the current request. Provides access to the response where the execution result will be
    /// written. Cannot be null.</param>
    /// <param name="executionResult">The result of the execution to be written to the response. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    Task WriteAsync(HttpContext context, ExecutionResult executionResult);
}
