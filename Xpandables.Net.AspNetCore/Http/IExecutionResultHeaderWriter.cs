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

namespace Xpandables.Net.Http;

/// <summary>
/// Defines a contract for writing execution result headers to an HTTP response asynchronously.
/// </summary>
/// <remarks>Implementations of this interface are responsible for serializing and writing header information
/// based on the provided execution result. This is typically used in middleware or endpoint logic to customize response
/// headers according to the outcome of an operation.</remarks>
public interface IExecutionResultHeaderWriter
{
    /// <summary>
    /// Asynchronously writes the specified execution result to the HTTP response for the given context.
    /// </summary>
    /// <param name="context">The HTTP context representing the current request and response. Cannot be null.</param>
    /// <param name="execution">The execution result to be written to the response. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    Task WriteAsync(HttpContext context, ExecutionResult execution);
}
