
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
using Microsoft.Extensions.DependencyInjection;

namespace Xpandables.Net.Executions.Minimals;

/// <summary>
/// Represents an execution result that implements the 
/// <see cref="IResult"/> interface.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="MinimalResult"/> class.
/// </remarks>
/// <param name="executionResult">The execution result to be executed.</param>
/// <exception cref="ArgumentNullException">Thrown when the execution result 
/// is null.</exception>
public sealed class MinimalResult(ExecutionResult executionResult) : IResult
{
    private readonly ExecutionResult _executionResult = executionResult
        ?? throw new ArgumentNullException(nameof(executionResult));

    /// <summary>
    /// Executes the execution result asynchronously.
    /// </summary>
    /// <param name="httpContext">The HTTP context.</param>
    /// <returns>A task that represents the asynchronous execution.</returns>
    public Task ExecuteAsync(HttpContext httpContext)
    {
        IEndpointProcessor execute = httpContext
            .RequestServices
            .GetRequiredService<IEndpointProcessor>();

        return execute.ProcessAsync(httpContext, _executionResult);
    }
}
