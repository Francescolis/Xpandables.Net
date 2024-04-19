
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
using Microsoft.AspNetCore.Http;

namespace Xpandables.Net.Operations;

/// <summary>
/// Provides a contract for building the response of an operation result for 
/// Asp.Net.
/// </summary>
public interface IOperationResultResponseBuilder
{
    /// <summary>
    /// Writes the operation result to the response.
    /// </summary>
    /// <param name="context">The HTTP context to act on.</param>
    /// <param name="operation">The operation result to act on.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task ExecuteAsync(HttpContext context, IOperationResult operation);

    /// <summary>
    /// Builds the response for the specified exception.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task OnExceptionAsync(HttpContext context, Exception exception);
}
