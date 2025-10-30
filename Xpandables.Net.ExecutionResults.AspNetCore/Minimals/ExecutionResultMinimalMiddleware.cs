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
using System.Reflection;

using Microsoft.AspNetCore.Http;

namespace Xpandables.Net.ExecutionResults.Minimals;

/// <summary>
/// Middleware that processes HTTP requests by invoking the next component in the pipeline and handles exceptions by
/// generating appropriate HTTP responses when possible.
/// </summary>
/// <remarks>If an exception occurs and the HTTP response has not started, this middleware converts the exception
/// into an HTTP response and writes it to the client. If the response has already started, exceptions are not handled
/// by this middleware.</remarks>
public sealed class ExecutionResultMinimalMiddleware : IMiddleware
{
    /// <summary>
    /// Processes an HTTP request by invoking the next middleware in the pipeline and handles exceptions that occur
    /// during execution.
    /// </summary>
    /// <remarks>If an exception is thrown and the response has not started, the exception is converted to an
    /// appropriate HTTP response and written to the client. If the response has already started, the exception is not
    /// handled by this method.</remarks>
    /// <param name="context">The HTTP context for the current request. Cannot be null.</param>
    /// <param name="next">The delegate representing the next middleware component in the request pipeline. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous operation of processing the HTTP request.</returns>
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

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

            ExecutionResult executionResult = exception switch
            {
                BadHttpRequestException badHttpRequestException => badHttpRequestException.ToExecutionResult(),
                _ => exception.ToExecutionResult()
            };

            var minimalResult = executionResult.ToMinimalResult();

            await minimalResult
                .ExecuteAsync(context)
                .ConfigureAwait(false);
        }
    }
}
