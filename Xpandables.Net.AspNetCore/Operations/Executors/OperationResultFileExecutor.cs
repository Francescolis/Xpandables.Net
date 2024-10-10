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
using Microsoft.Extensions.Primitives;

using Xpandables.Net.Collections;

namespace Xpandables.Net.Operations.Executors;

/// <summary>
/// Executes an operation result that contains a file.
/// </summary>
public sealed class OperationResultFileExecutor : IOperationResultExecutor
{
    ///<inheritdoc/>
    public bool CanExecute(IOperationResult operationResult) =>
        operationResult.Result is ResultFile;

    ///<inheritdoc/>
    public async Task ExecuteAsync(
        HttpContext context,
        IOperationResult operationResult)
    {
        if (operationResult.Result is not ResultFile resultFile)
        {
            throw new InvalidOperationException(
                "The operation result is not a file result.");
        }

        context.Response.StatusCode = (int)operationResult.StatusCode;
        context.Response.Headers
            .Append(
                "Content-Disposition",
                $"attachment; filename={resultFile.FileName}");

        foreach (ElementEntry entry in operationResult.Headers)
        {
            context.Response.Headers
                .Append(entry.Key, new StringValues([.. entry.Values]));
        }

        IResult result = Results.File(
            resultFile.Content,
            resultFile.ContentType,
            resultFile.FileName);

        await result
            .ExecuteAsync(context)
            .ConfigureAwait(false);
    }
}
