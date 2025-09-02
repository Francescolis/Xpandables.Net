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
using System.Reflection;

using Microsoft.AspNetCore.Http;

using Xpandables.Net.Collections;

namespace Xpandables.Net.Executions;

/// <summary>
/// This class handles the execution of minimal results for asynchronous paged enumerables.
/// </summary>
public sealed class AsyncPagedExecutionResultExecutor : ExecutionResultExecutor
{
    readonly static MethodInfo createMethod = typeof(AsyncPagedEnumerableResult)
        .GetMethod(nameof(AsyncPagedEnumerableResult.Create), BindingFlags.NonPublic | BindingFlags.Static)!;

    /// <inheritdoc/>
    public override bool CanExecute(ExecutionResult executionResult) =>
        executionResult is not null &&
            executionResult.Value is not null &&
            executionResult.StatusCode == HttpStatusCode.OK &&
            executionResult.Value.GetType()
            .GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IAsyncPagedEnumerable<>));

    /// <inheritdoc/>
    public override async Task ExecuteAsync(HttpContext context, ExecutionResult executionResult)
    {
        await base
            .ExecuteAsync(context, executionResult)
            .ConfigureAwait(false);

        var value = executionResult.Value!;
        var valueType = value.GetType();

        var asyncPagedType = valueType
            .GetInterfaces()
            .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IAsyncPagedEnumerable<>));

        Type itemType = asyncPagedType.GetGenericArguments()[0];

        MethodInfo genericCreateMethod = createMethod.MakeGenericMethod(itemType);

        IResult result = (IResult)genericCreateMethod.Invoke(null, [executionResult.Value!])!;

        await result.ExecuteAsync(context).ConfigureAwait(false);
    }
}
