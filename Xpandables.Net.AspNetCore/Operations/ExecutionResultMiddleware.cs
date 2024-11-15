
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
using System.Reflection;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Xpandables.Net.Operations;

/// <summary>  
/// Middleware to handle execution results and convert exceptions to 
/// execution results.  
/// </summary>  
public sealed class ExecutionResultMiddleware : IMiddleware
{
    /// <inheritdoc/>  
    public async Task InvokeAsync(
        HttpContext context,
        RequestDelegate next)
    {
        try
        {
            await next(context).ConfigureAwait(false);
        }
        catch (Exception exception)
            when (!context.Response.HasStarted)
        {
            if (exception is TargetInvocationException targetInvocation)
            {
                exception = targetInvocation.InnerException ?? targetInvocation;
            }

            IExecutionResultExecute execute = context
                .RequestServices
                .GetRequiredService<IExecutionResultExecute>();

            IExecutionResult executionResult =
                exception.ToExecutionResultForProblemDetails();

            await execute
                .ExecuteAsync(context, executionResult)
                .ConfigureAwait(false);
        }
    }
}
