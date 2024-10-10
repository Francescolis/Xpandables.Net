
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

namespace Xpandables.Net.Operations;

/// <summary>
/// Represents a minimal operation result that implements the 
/// <see cref="IResult"/> interface.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="OperationResultMinimal"/> class.
/// </remarks>
/// <param name="operationResult">The operation result to be executed.</param>
/// <exception cref="ArgumentNullException">Thrown when the operation result 
/// is null.</exception>
public sealed class OperationResultMinimal(IOperationResult operationResult) : IResult
{
    private readonly IOperationResult _operationResult = operationResult
        ?? throw new ArgumentNullException(nameof(operationResult));

    /// <summary>
    /// Executes the operation result asynchronously.
    /// </summary>
    /// <param name="httpContext">The HTTP context.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task ExecuteAsync(HttpContext httpContext)
    {
        IOperationResultExecute execute = httpContext
            .RequestServices
            .GetRequiredService<IOperationResultExecute>();

        return execute.ExecuteAsync(httpContext, _operationResult);
    }
}
