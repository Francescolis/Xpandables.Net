
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

namespace Xpandables.Net.Executions;

/// <summary>
/// Middleware that processes exceptions and converts them into minimal execution results for HTTP responses when the
/// response has not already started.
/// </summary>
/// <remarks>This middleware intercepts exceptions thrown during the execution of the request pipeline. If an
/// exception occurs and the HTTP response has not started, the middleware converts the exception into an <see
/// cref="ExecutionResult"/> and generates a minimal HTTP response using the appropriate problem details.</remarks>
public sealed class ExecutionResultMinimalMiddleware : IMiddleware
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

            ExecutionResult executionResult =
                exception.ToExecutionResultForProblemDetails();

            var minimalResult = executionResult.ToMinimalResult();

            await minimalResult
                .ExecuteAsync(context)
                .ConfigureAwait(false);
        }
    }
}
