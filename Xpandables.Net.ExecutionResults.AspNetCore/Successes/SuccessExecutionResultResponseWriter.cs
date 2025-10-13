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
using System.Text.Json.Serialization.Metadata;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Xpandables.Net.Async;

namespace Xpandables.Net.ExecutionResults.Successes;

/// <summary>
/// Provides an HTTP response writer that serializes successful GraphQL execution results to JSON and writes them to the
/// response body.
/// </summary>
/// <remarks>This writer handles execution results with successful HTTP status codes, excluding those with no
/// content. It does not support results whose value implements <see cref="IAsyncEnumerable{T}"/>. The response is
/// completed without a body if the execution result value is <see langword="null"/>.</remarks>
public sealed class SuccessExecutionResultResponseWriter : ExecutionResultResponseWriter
{
    /// <inheritdoc/>
    public override bool CanWrite(ExecutionResult executionResult)
    {
        ArgumentNullException.ThrowIfNull(executionResult);

        return executionResult.StatusCode.IsSuccess
            && executionResult.StatusCode != System.Net.HttpStatusCode.NoContent
            && (executionResult.Value is null || executionResult.Value is not IAsyncPagedEnumerable);
    }

    /// <summary>
    /// Asynchronously writes the GraphQL execution result to the HTTP response in JSON format.
    /// </summary>
    /// <param name="context">The HTTP context for the current request. Provides access to the request and response objects.</param>
    /// <param name="executionResult">The result of the GraphQL execution to be written to the response. If the value is null, the response is
    /// completed without a body.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    public override async Task WriteAsync(HttpContext context, ExecutionResult executionResult)
    {
        await base.WriteAsync(context, executionResult).ConfigureAwait(false);

        if (executionResult.Value is null)
        {
            await context.Response.CompleteAsync().ConfigureAwait(false);
            return;
        }

        if (executionResult.StatusCode.IsCreated)
        {
            IResult createdResult = (executionResult.Value, executionResult.Location) switch
            {
                (not null, not null) => Results.Created(executionResult.Location, executionResult.Value),
                (null, not null) => Results.Created(executionResult.Location, null),
                (not null, null) => Results.Created((Uri?)null, executionResult.Value),
                _ => Results.Created((Uri?)null, value: null)
            };

            await createdResult.ExecuteAsync(context).ConfigureAwait(false);
            return;
        }

        Type type = executionResult.Value.GetType();
        JsonTypeInfo jsonTypeInfo = context.RequestServices
            .GetRequiredService<IOptions<JsonOptions>>().Value.SerializerOptions
            .GetTypeInfo(type)
            ?? throw new InvalidOperationException(
                $"JsonTypeInfo for type '{type.Name}' " +
                $"could not be obtained from the provided JsonSerializerOptions.");

        await context.Response.WriteAsJsonAsync(
            executionResult.Value,
            jsonTypeInfo,
            cancellationToken: context.RequestAborted).ConfigureAwait(false);
    }
}
