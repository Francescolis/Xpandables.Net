
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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using Xpandables.Net.Primitives;

namespace Xpandables.Net.Operations;

/// <summary>
/// When used as a filter, it'll automatically convert bad operation result 
/// to MVC Core <see cref="ValidationProblemDetails"/>,
/// and exceptionally, convert <see cref="IOperationResult{TValue}"/> where 
/// TValue is <see cref="BinaryResult"/> to <see cref="FileContentResult"/>.
/// It also add location header.
/// </summary>
public sealed class OperationResultControllerFilter(
    IOperationResultResponseBuilder resultResponseBuilder) :
    IAsyncAlwaysRunResultFilter
{
    private readonly IOperationResultResponseBuilder _resultResponseBuilder =
        resultResponseBuilder
        ?? throw new ArgumentNullException(nameof(resultResponseBuilder));

    ///<inheritdoc/>
    public async Task OnResultExecutionAsync(
        ResultExecutingContext context,
        ResultExecutionDelegate next)
    {
        _ = context ?? throw new ArgumentNullException(nameof(context));
        _ = next ?? throw new ArgumentNullException(nameof(next));

        if (context.Result is ObjectResult objectResult
            && objectResult.Value is IOperationResult operationResult)
        {
            await _resultResponseBuilder
                .ExecuteAsync(context.HttpContext, operationResult)
                .ConfigureAwait(false);
        }
        else
        {
            _ = await next().ConfigureAwait(false);
        }
    }
}
