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
using System.ExecutionResults;
using System.IO.Pipelines;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Net;

/// <summary>
/// Provides an ASP.NET Core result filter that processes controller action results, handling execution result headers
/// and serializing paged results for HTTP responses.
/// </summary>
/// <remarks>This filter is intended for use in scenarios where controller actions may return execution results or
/// asynchronous paged enumerables. It automatically writes execution result headers and serializes paged data to the
/// response stream in JSON format. The filter always runs asynchronously and is typically registered as part of the
/// application's filter pipeline.</remarks>
public sealed class ControllerResultFilter : IAsyncAlwaysRunResultFilter
{
    private IOperationResultHeaderWriter? headerWriter;

    /// <inheritdoc/>
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        if (context.Result is ObjectResult objectResult)
        {
            if (objectResult.Value is OperationResult execution)
            {
                headerWriter ??= context.HttpContext
                .RequestServices
                .GetRequiredService<IOperationResultHeaderWriter>();

                await headerWriter
                    .WriteAsync(context.HttpContext, execution)
                    .ConfigureAwait(false);

                if (execution.Value is not null)
                {
                    objectResult.Value = execution.Value;
                }
            }

            if (objectResult.Value is IAsyncPagedEnumerable paged)
            {
                PipeWriter pipeWriter = context.HttpContext.Response.BodyWriter;
                context.HttpContext.Response.ContentType ??= context.HttpContext.GetContentType("application/json; charset=utf-8");
                var cancellationToken = context.HttpContext.RequestAborted;
                Type itemType = paged.GetArgumentType();

                var options = context.HttpContext.GetJsonSerializerOptions();
                JsonTypeInfo? jsonTypeInfo = options.GetTypeInfo(itemType);

                if (jsonTypeInfo is not null)
                {
                    await JsonSerializer
                        .SerializeAsyncPaged(pipeWriter, paged, jsonTypeInfo, cancellationToken)
                        .ConfigureAwait(false);
                }
                else
                {
                    await JsonSerializer
                        .SerializeAsyncPaged(pipeWriter, paged, options, cancellationToken)
                        .ConfigureAwait(false);
                }

                return;
            }
        }

        await next().ConfigureAwait(false);
    }
}
