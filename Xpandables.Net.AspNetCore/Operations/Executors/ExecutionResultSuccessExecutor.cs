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

using Xpandables.Net.Executions;

namespace Xpandables.Net.Operations.Executors;

/// <summary>
/// Executes the execution result when the result indicates a successful execution.
/// </summary>
public sealed class ExecutionResultSuccessExecutor : IExecutionResultExecutor
{
    ///<inheritdoc/>
    public bool CanExecute(IExecutionResult executionResult) =>
        executionResult.IsSuccessStatusCode();

    ///<inheritdoc/>
    public async Task ExecuteAsync(
        HttpContext context,
        IExecutionResult executionResult)
    {
        if (executionResult.IsCreated())
        {
            IResult resultCreated = (executionResult.Result is not null) switch
            {
                true => Results.Created(
                    executionResult.Location,
                    executionResult.Result),
                _ => Results.Created(executionResult.Location, null)
            };

            await resultCreated
                .ExecuteAsync(context)
                .ConfigureAwait(false);

            return;
        }

        if (executionResult.Result is ResultFile resultFile)
        {
            context.Response.Headers
                .Append(
                    "Content-Disposition",
                    $"attachment; filename={resultFile.FileName}");

            IResult result = Results.File(
                [.. resultFile.Content],
                resultFile.ContentType,
                resultFile.FileName);

            await result
                .ExecuteAsync(context)
                .ConfigureAwait(false);

            return;
        }

        if (executionResult.Result is not null)
        {
            await context.Response.WriteAsJsonAsync(
                executionResult.Result,
                executionResult.Result.GetType())
                .ConfigureAwait(false);

            return;
        }

        await context.Response
            .CompleteAsync()
            .ConfigureAwait(false);
    }
}
