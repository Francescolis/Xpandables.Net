
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
using Microsoft.AspNetCore.Http;

namespace Xpandables.Net.Operations;
#pragma warning disable CA1812 // Avoid uninstantiated internal classes
internal sealed class OperationResultResponseBuilder : IOperationResultResponseBuilder
#pragma warning restore CA1812 // Avoid uninstantiated internal classes
{
    public async Task ExecuteAsync(HttpContext httpContext, IOperationResult operationResult)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(operationResult);

        httpContext.Response.StatusCode = (int)operationResult.StatusCode;

        httpContext.AddLocationUrlIfAvailable(operationResult);
        httpContext.AddHeadersIfAvailable(operationResult);

        await httpContext.AddAuthenticationSchemeIfUnauthorizedAsync(operationResult)
            .ConfigureAwait(false);

        if (operationResult.GetCreatedResultIfAvailable() is { } createdResult)
        {
            await createdResult.ExecuteAsync(httpContext).ConfigureAwait(false);
            return;
        }

        if (operationResult.GetFileResult() is { } fileResult)
        {
            await fileResult.ExecuteAsync(httpContext).ConfigureAwait(false);
            return;
        }

        if (operationResult.IsSuccess && operationResult.Result.IsNotEmpty)
        {
            await httpContext.Response.WriteAsJsonAsync(
                   operationResult.Result.Value,
                   operationResult.Result.Value.GetType())
                   .ConfigureAwait(false);
            return;
        }

        if (httpContext.GetValidationProblemDetails(operationResult) is { } validationProblem)
        {
            await validationProblem.ExecuteAsync(httpContext).ConfigureAwait(false);
            return;
        }
    }

    public async Task OnExceptionAsync(HttpContext context, Exception exception)
    {
        if (exception is BadHttpRequestException badHttpRequestException)
        {
            IResult resultBad = context.GetResultFromBadHttpException(badHttpRequestException);
            await resultBad.ExecuteAsync(context).ConfigureAwait(false);
            return;
        }

        IResult resultProblem = await context
           .GetProblemDetailsAsync(exception)
           .ConfigureAwait(false);

        await resultProblem
            .ExecuteAsync(context)
            .ConfigureAwait(false);
    }
}
