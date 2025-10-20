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
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization.Metadata;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Xpandables.Net.ExecutionResults;

/// <summary>
/// Implements an ASP.NET Core result filter that processes controller results containing an ExecutionResult, formatting
/// and writing the response using a registered IExecutionResultResponseWriter implementation.
/// </summary>
/// <remarks>This filter enables custom formatting of ExecutionResult objects returned from controller actions. If
/// no suitable IExecutionResultResponseWriter is registered, a fallback writer sets response headers and authentication
/// challenges, and the ExecutionResult's value is serialized to JSON using the configured JsonSerializerOptions. The
/// filter always runs, regardless of the result type, but only processes results of type ObjectResult containing an
/// ExecutionResult. For all other results, the filter delegates to the next result filter in the pipeline.</remarks>
public sealed class ExecutionResultControllerResultFilter : IAsyncAlwaysRunResultFilter
{
    private sealed class HeaderOnlyExecutor : ExecutionResultResponseWriter
    {
        public override bool CanWrite(ExecutionResult executionResult) => true;
        // Do not override ExecuteAsync: base implementation sets headers and authentication challenges.
    }

    private static readonly ExecutionResultResponseWriter FallbackExecutor = new HeaderOnlyExecutor();

    /// <inheritdoc/>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "AOT already apply on the JsonSerializer method")]
    [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "AOT already apply on the JsonSerializer method")]
    public async Task OnResultExecutionAsync(
        ResultExecutingContext context,
        ResultExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        if (context.Result is ObjectResult objectResult
            && objectResult.Value is ExecutionResult executionResult)
        {
            IExecutionResultResponseWriter? executionResultExecutor = context
                .HttpContext
                .RequestServices
                    .GetServices<IExecutionResultResponseWriter>()
                    .FirstOrDefault(execution => execution.CanWrite(executionResult));

            if (executionResultExecutor is null)
            {
                await FallbackExecutor.WriteAsync(context.HttpContext, executionResult).ConfigureAwait(false);

                if (executionResult.Value is not null)
                {
                    JsonTypeInfo? jsonTypeInfo = context.HttpContext.RequestServices
                        .GetService<IOptions<JsonOptions>>()?.Value?.JsonSerializerOptions?
                        .GetTypeInfo(executionResult.Value.GetType());

                    if (jsonTypeInfo is not null)
                    {
                        await context.HttpContext.Response.WriteAsJsonAsync(
                            executionResult.Value,
                            jsonTypeInfo)
                            .ConfigureAwait(false);
                    }
                    else
                    {
                        await context.HttpContext.Response.WriteAsJsonAsync(
                            executionResult.Value,
                            executionResult.Value.GetType())
                            .ConfigureAwait(false);
                    }
                }

                return;
            }

            await executionResultExecutor
                .WriteAsync(context.HttpContext, executionResult)
                .ConfigureAwait(false);
        }

        await next().ConfigureAwait(false);
    }
}
