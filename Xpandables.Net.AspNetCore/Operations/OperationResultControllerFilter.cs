
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
using System.Net;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Xpandables.Net.Operations;

/// <summary>
/// When used as a filter, it'll automatically convert bad operation result to MVC Core <see cref="ValidationProblemDetails"/>,
/// and exceptionally, convert <see cref="IOperationResult{TValue}"/> where 
/// TValue is <see cref="BinaryEntry"/> to <see cref="FileContentResult"/>.
/// It also add location header.
/// </summary>
internal sealed class OperationResultControllerFilter : IAsyncAlwaysRunResultFilter
{
    private static async Task OnFailureExecutionAsync(ResultExecutingContext context, IOperationResult operation)
    {
        var controller = BuildExceptionController(context);

        var statusCode = operation.StatusCode;
        var modelStateDictionary = operation.Errors.ToModelStateDictionary();

        context.Result = controller.ValidationProblem(
            statusCode.GetProblemDetail(),
            context.HttpContext.Request.Path,
            (int)statusCode,
            statusCode.GetProblemTitle(),
            modelStateDictionary: operation.Errors.Any() ? modelStateDictionary : null);

        await context.HttpContext.AddHeaderIfUnauthorized(operation).ConfigureAwait(false);
    }

    private static async Task OnSuccessExecutionAsync(ResultExecutingContext context, IOperationResult operation)
    {
        context.HttpContext.AddLocationUrlIfAvailable(operation);

        await context.HttpContext.WriteBodyIfAvailableAsync(operation).ConfigureAwait(false);

        if (operation is IOperationResult<BinaryEntry> fileResult)
        {
            await context.HttpContext.WriteFileBodyAsync(fileResult.Result).ConfigureAwait(false);
            return;
        }

        if (operation.StatusCode == HttpStatusCode.Created && operation.LocationUrl.IsNotEmpty)
        {
            await context.HttpContext.ResultCreatedAsync(operation).ConfigureAwait(false);
        }
    }

    ///<inheritdoc/>
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is ObjectResult objectResult && objectResult.Value is IOperationResult operationResult)
        {
            context.HttpContext.AddLocationUrlIfAvailable(operationResult);

            if (operationResult.IsFailure)
            {
                await OnFailureExecutionAsync(context, operationResult).ConfigureAwait(false);
            }
            else
            {
                await OnSuccessExecutionAsync(context, operationResult).ConfigureAwait(false);
            }
        }

        await next().ConfigureAwait(false);
    }


    private static ControllerBase BuildExceptionController(ResultExecutingContext context)
    {
        var controller = (ControllerBase)context.Controller;
        if (controller is null)
        {
            controller = context.HttpContext.RequestServices.GetRequiredService<OperationResultController>();
            controller.ControllerContext = new ControllerContext(
                new ActionContext(
                    context.HttpContext,
                    context.HttpContext.GetRouteData(),
                    new ControllerActionDescriptor()));
        }

        return controller;
    }
}
