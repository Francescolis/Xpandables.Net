
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

using Xpandables.Net.Collections;

namespace Xpandables.Net.Executions;

/// <summary>
/// Represents an execution strategy for handling successful results with minimal processing.
/// </summary>
/// <remarks>This class is designed to handle execution results with an HTTP status code of <see
/// cref="HttpStatusCode.OK"/>  and a non-stream value. It writes the result value as JSON to the HTTP response if
/// available, or completes the  response if no value is present.</remarks>
public sealed class SuccessMinimalResultExecution : MinimalResultExecution
{
    /// <inheritdoc/>
    public sealed override bool CanExecute(ExecutionResult executionResult) =>
        executionResult.StatusCode == HttpStatusCode.OK
            && ((executionResult.Value is null) || (executionResult.Value.GetType()
                    .GetInterfaces()
                    .All(i => i.IsGenericType && i.GetGenericTypeDefinition() != typeof(IAsyncPagedEnumerable<>))));

    /// <inheritdoc/>
    public sealed override async Task ExecuteAsync(HttpContext context, ExecutionResult executionResult)
    {
        await base
            .ExecuteAsync(context, executionResult)
            .ConfigureAwait(false);

        if (executionResult.Value is not null)
        {
            await context
                .Response
                .WriteAsJsonAsync(
                    executionResult.Value,
                    executionResult.Value.GetType(),
                    context.RequestAborted)
                .ConfigureAwait(false);

            return;
        }

        await context
            .Response
            .CompleteAsync()
            .ConfigureAwait(false);
    }
}
