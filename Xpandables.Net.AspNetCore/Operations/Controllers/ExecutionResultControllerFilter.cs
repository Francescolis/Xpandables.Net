
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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.Executions;

namespace Xpandables.Net.Operations.Controllers;

/// <summary>
/// A filter that executes an <see cref="IExecutionResult"/> if the result 
/// is of type <see cref="ObjectResult"/>.
/// </summary>
public sealed class ExecutionResultControllerFilter : IAsyncAlwaysRunResultFilter
{
    /// <inheritdoc/>
    public Task OnResultExecutionAsync(
        ResultExecutingContext context,
        ResultExecutionDelegate next)
    {
        if (context.Result is ObjectResult objectResult
            && objectResult.Value is IExecutionResult executionResult)
        {
            IExecutionResultExecute execute = context
                .HttpContext
                .RequestServices
                .GetRequiredService<IExecutionResultExecute>();

            return execute.ExecuteAsync(context.HttpContext, executionResult);
        }

        return next();
    }
}
