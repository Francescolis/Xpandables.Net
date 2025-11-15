/*******************************************************************************
 * Copyright (C) 2025 Kamersoft
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
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization.Metadata;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.ExecutionResults;
using Xpandables.Net.Http;

namespace Xpandables.Net.Minimals;

/// <summary>
/// Represents an ASP.NET Core result that writes an execution result to the HTTP response using a registered response
/// writer or a default JSON format.
/// </summary>
/// <remarks>The response format is determined by the registered IExecutionResultResponseWriter services. If no
/// suitable writer is found, the result is written as JSON if available; otherwise, the response is completed with no
/// content.</remarks>
/// <param name="executionResult">The execution result to be written to the HTTP response. Cannot be null.</param>
public sealed class ExecutionResultMinimalResult(ExecutionResult executionResult) : IResult
{
    private sealed class HeaderOnlyExecutor : ExecutionResultResponseWriter
    {
        public override bool CanWrite(ExecutionResult executionResult) => true;
        // Do not override ExecuteAsync: base implementation sets headers and authentication challenges.
    }

    private static readonly ExecutionResultResponseWriter FallbackExecutor = new HeaderOnlyExecutor();

    /// <summary>
    /// Executes the result operation asynchronously and writes the response to the specified HTTP context.
    /// </summary>
    /// <remarks>The response format is determined by the registered IExecutionResultResponseWriter services.
    /// If no suitable writer is found, the result is written as JSON if available; otherwise, the response is completed
    /// with no content.</remarks>
    /// <param name="httpContext">The HTTP context for the current request. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous execution operation.</returns>
    public async Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        IExecutionResultResponseWriter? responseWriter = httpContext
            .RequestServices
            .GetServices<IExecutionResultResponseWriter>()
            .FirstOrDefault(writer => writer.CanWrite(executionResult));

        if (responseWriter is null)
        {
            if (executionResult.Value is not null)
            {
                var options = httpContext.GetJsonSerializerOptions();

                Type type = executionResult.Value.GetType();
                JsonTypeInfo? jsonTypeInfo = options.GetTypeInfo(type);

                await FallbackExecutor.WriteAsync(httpContext, executionResult).ConfigureAwait(false);

                if (jsonTypeInfo is null)
                    await WriteAsJsonAsync(
                        httpContext,
                        type,
                        executionResult)
                        .ConfigureAwait(false);
                else
                    await httpContext.Response
                        .WriteAsJsonAsync(
                             executionResult.Value,
                            jsonTypeInfo,
                            cancellationToken: httpContext.RequestAborted)
                        .ConfigureAwait(false);
            }
            else
            {
                await httpContext.Response
                    .CompleteAsync()
                    .ConfigureAwait(false);
            }

            return;
        }

        await responseWriter.WriteAsync(httpContext, executionResult).ConfigureAwait(false);
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
    static async Task WriteAsJsonAsync(HttpContext httpContext, Type type, ExecutionResult executionResult)
    {
        await httpContext.Response.WriteAsJsonAsync(
            executionResult.Value,
            type,
            httpContext.RequestAborted).ConfigureAwait(false);
    }
}
