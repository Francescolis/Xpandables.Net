
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
using System.Reflection;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Xpandables.Net.Operations;

/// <summary>
/// Transforms the operation result to a minimal response.
/// </summary>
public sealed class OperationResultMiddleware(
    IOperationResultResponseBuilder resultResponseBuilder) : IMiddleware
{
    private readonly bool _bypassResponseHasStarted = true;
    private readonly IOperationResultResponseBuilder _resultResponseBuilder =
        resultResponseBuilder
        ?? throw new ArgumentNullException(nameof(resultResponseBuilder));

    ///<inheritdoc/>
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        try
        {
            await next(context).ConfigureAwait(false);
        }
        catch (Exception exception)
            when (!context.Response.HasStarted && _bypassResponseHasStarted)
        {
            if (exception is TargetInvocationException targetInvocation)
            {
                exception = targetInvocation.InnerException ?? targetInvocation;
            }

            ILogger<OperationResultMiddleware> logger = context
                .RequestServices
                .GetRequiredService<ILogger<OperationResultMiddleware>>();

            logger.ErrorExecutingProcess(
                nameof(OperationResultMiddleware),
                exception);

            await _resultResponseBuilder
                .OnExceptionAsync(context, exception)
                .ConfigureAwait(false);
        }
    }
}
