
/************************************************************************************************************
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
************************************************************************************************************/
using System.ComponentModel.DataAnnotations;
using System.Reflection;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Xpandables.Net.Extensions;

using static Xpandables.Net.Operations.MiddlewareExtensions;

namespace Xpandables.Net.Operations;

/// <summary>
/// Defines the operation result controller middleware.
/// </summary>
public sealed class OperationResultControllerMiddleware : IMiddleware
{
    private readonly bool _bypassResponseHasStarted = true;

    ///<inheritdoc/>
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        try
        {
            await next(context).ConfigureAwait(false);
        }
        catch (Exception exception) when (!context.Response.HasStarted && _bypassResponseHasStarted)
        {
            if (exception is TargetInvocationException targetInvocation)
                exception = targetInvocation.InnerException ?? targetInvocation;

            var logger = context.RequestServices.GetRequiredService<ILogger<OperationResultControllerMiddleware>>();
            logger.ErrorExecutingProcess(nameof(OperationResultControllerMiddleware), exception);

            var controller = GetExceptionController(context);

            Task task = exception switch
            {
                BadHttpRequestException badHttpRequestException => OnBadHttpExceptionAsync(context, badHttpRequestException, controller),
                OperationResultException operationResultException => OnOperationResultExceptionAsync(context, operationResultException, controller),
                ValidationException validationException => OnValidationExceptionAsync(context, validationException, controller),
                _ => OnExceptionAsync(context, exception, controller)
            };

            await task.ConfigureAwait(false);
        }
    }
}
