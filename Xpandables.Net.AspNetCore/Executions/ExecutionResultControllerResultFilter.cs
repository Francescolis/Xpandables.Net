
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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Xpandables.Net.Executions;

/// <summary>
/// A filter that processes the result of an action execution and ensures that the result is handled appropriately.
/// </summary>
/// <remarks>This filter inspects the result of an action execution to determine if it is an <see
/// cref="ObjectResult"/> containing an <see cref="ExecutionResult"/>. If so, it attempts to find an <see
/// cref="IExecutionResultExecutor"/> service that can handle the <see cref="ExecutionResult"/>. If no such service is
/// found, the filter writes the value of the <see cref="ExecutionResult"/> directly to the HTTP response or completes
/// the response if the value is null.</remarks>
public sealed class ExecutionResultControllerResultFilter : IAsyncAlwaysRunResultFilter
{
    // Reuse the header-setting logic from ExecutionResultExecutor.
    private sealed class HeaderOnlyExecutor : ExecutionResultExecutor
    {
        public override bool CanExecute(ExecutionResult executionResult) => true;
        // Do not override ExecuteAsync: base implementation sets headers and authentication challenges.
    }

    private static readonly ExecutionResultExecutor FallbackExecutor = new HeaderOnlyExecutor();

    /// <inheritdoc/>
    public async Task OnResultExecutionAsync(
        ResultExecutingContext context,
        ResultExecutionDelegate next)
    {
        if (context.Result is ObjectResult objectResult
            && objectResult.Value is ExecutionResult executionResult)
        {
            IExecutionResultExecutor? executionResultExecutor = context
                .HttpContext
                .RequestServices
                    .GetServices<IExecutionResultExecutor>()
                    .FirstOrDefault(execution => execution.CanExecute(executionResult));

            if (executionResultExecutor is null)
            {
                await FallbackExecutor.ExecuteAsync(context.HttpContext, executionResult).ConfigureAwait(false);
                context.HttpContext.Response.StatusCode = (int)executionResult.StatusCode;

                if (executionResult.Value is not null)
                {
                    await context.HttpContext.Response.WriteAsJsonAsync(
                        executionResult.Value,
                        executionResult.Value.GetType())
                        .ConfigureAwait(false);

                }
                return;
            }

            await executionResultExecutor
                .ExecuteAsync(context.HttpContext, executionResult)
                .ConfigureAwait(false);
        }

        await next().ConfigureAwait(false);
    }
}
