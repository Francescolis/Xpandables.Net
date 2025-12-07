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
using System.Results;
using System.Text;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Mvc.Filters;

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
    private IResultHeaderWriter? headerWriter;

    /// <inheritdoc/>
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        if (context.Result is ObjectResult objectResult)
        {
            if (objectResult.Value is Result result)
            {
                headerWriter ??= context.HttpContext
                .RequestServices
                .GetRequiredService<IResultHeaderWriter>();

                await headerWriter
                    .WriteAsync(context.HttpContext, result)
                    .ConfigureAwait(false);

                if (result.Value is not null)
                {
                    objectResult.Value = result.Value;
                }
            }

            if (objectResult.Value is IAsyncPagedEnumerable paged)
            {
                context.HttpContext.Response.ContentType ??= context.HttpContext.GetContentType("application/json; charset=utf-8");
                Encoding encoding = context.HttpContext.GetEncoding();
                var jsonOptions = context.HttpContext.RequestServices.GetRequiredService<IOptions<JsonOptions>>().Value;
                var formatter = ControllerResultAsyncPagedOutputFormatter.CreateFormatter(jsonOptions);

                var formatterContext = new OutputFormatterWriteContext(
                    context.HttpContext,
                    (stream, encoding) => new StreamWriter(stream, encoding),
                    paged.GetType(),
                    paged);

                await formatter
                    .WriteResponseBodyAsync(formatterContext, encoding)
                    .ConfigureAwait(false);

                return;
            }
        }

        await next().ConfigureAwait(false);
    }
}
