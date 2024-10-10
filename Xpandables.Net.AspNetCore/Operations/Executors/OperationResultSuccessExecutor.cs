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
using Microsoft.AspNetCore.Http;

namespace Xpandables.Net.Operations.Executors;

/// <summary>
/// Executes the operation result when the result indicates a successful operation.
/// </summary>
public sealed class OperationResultSuccessExecutor : IOperationResultExecutor
{
    ///<inheritdoc/>
    public bool CanExecute(IOperationResult operationResult) =>
        operationResult.IsSuccessStatusCode()
        && !operationResult.IsResultFile()
        && !operationResult.IsCreated();

    ///<inheritdoc/>
    public async Task ExecuteAsync(
        HttpContext context,
        IOperationResult operationResult)
    {
        if (operationResult.Result is not null)
        {
            await context.Response.WriteAsJsonAsync(
                operationResult.Result,
                operationResult.Result.GetType())
                .ConfigureAwait(false);

            return;
        }

        await context.Response
            .CompleteAsync()
            .ConfigureAwait(false);
    }
}
