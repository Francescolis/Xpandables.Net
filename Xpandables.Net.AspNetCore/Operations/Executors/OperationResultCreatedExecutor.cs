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

using System.Net;

using Microsoft.AspNetCore.Http;

namespace Xpandables.Net.Operations.Executors;

/// <summary>
/// Executor for handling operation results with a status code of Created (201).
/// </summary>
public sealed class OperationResultCreatedExecutor : IOperationResultExecutor
{
    ///<inheritdoc/>
    public bool CanExecute(IOperationResult operationResult) =>
        operationResult.StatusCode == HttpStatusCode.Created
        && operationResult.Location is not null;

    ///<inheritdoc/>
    public async Task ExecuteAsync(
        HttpContext context,
        IOperationResult operationResult)
    {
        IResult resultCreated = (operationResult.Result is not null) switch
        {
            true => Results.Created(operationResult.Location, operationResult.Result),
            _ => Results.Created(operationResult.Location, null)
        };

        await resultCreated
            .ExecuteAsync(context)
            .ConfigureAwait(false);
    }
}
