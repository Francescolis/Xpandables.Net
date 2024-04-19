
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
using Microsoft.AspNetCore.Http;

namespace Xpandables.Net.Operations;
#pragma warning disable CA1812 // Avoid uninstantiated internal classes
internal sealed class OperationResultResponseBuilder :
    IOperationResultResponseBuilder
#pragma warning restore CA1812 // Avoid uninstantiated internal classes
{
    public async Task ExecuteAsync(
        HttpContext context,
        IOperationResult operation)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(operation);

        context.Response.StatusCode = (int)operation.StatusCode;

        await context
            .BuildMetaDataContextAsync(operation)
            .ConfigureAwait(false);

        if (operation.StatusCode == System.Net.HttpStatusCode.Created)
        {
            await context
                .BuildCreatedResponseAsync(operation)
                .ConfigureAwait(false);

            return;
        }

        if (operation.IsOperationResultFile())
        {
            await context
                .BuildFileResponseAsync(operation)
                .ConfigureAwait(false);

            return;
        }

        if (operation.IsSuccess && operation.Result is not null)
        {
            await context.Response.WriteAsJsonAsync(
                   operation.Result,
                   operation.Result.GetType())
                   .ConfigureAwait(false);
            return;
        }

        if (operation.IsFailure)
        {
            await context
                .GetProblemDetailsAsync(operation)
                .ConfigureAwait(false);
        }
    }

    public async Task OnExceptionAsync(HttpContext context, Exception exception)
    {
        await context
           .GetProblemDetailsAsync(exception)
           .ConfigureAwait(false);
    }
}
