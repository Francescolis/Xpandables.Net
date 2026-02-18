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
using System.Results;

namespace Microsoft.AspNetCore.Http;

/// <summary>
/// Provides middleware for handling exceptions in minimal API endpoints by converting them into standardized HTTP
/// problem details responses.
/// </summary>
/// <remarks>This middleware should be registered in the application's request pipeline to ensure that unhandled
/// exceptions are consistently translated into problem details responses, provided the HTTP response has not already
/// started. It is intended for use with minimal API scenarios and implements the IMiddleware interface for integration
/// with ASP.NET Core's middleware infrastructure.</remarks>
public sealed class ResultMiddleware : IMiddleware
{
    /// <inheritdoc/>
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        try
        {
            await next(context).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
            when (!context.Response.HasStarted)
        {
            if (exception is TargetInvocationException targetInvocation)
            {
                exception = targetInvocation.InnerException ?? targetInvocation;
            }

            Result result = exception switch
            {
                BadHttpRequestException badHttpRequestException => badHttpRequestException.ToResult(context),
                _ => exception.ToResult()
            };

            await ResultEndpointFilter.WriteProblemDetailsAsync(context, result).ConfigureAwait(false);
        }
    }
}
