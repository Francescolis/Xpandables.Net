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
using Microsoft.AspNetCore.Http;

using Xpandables.Net.AsyncPaged;
using Xpandables.Net.ExecutionResults;

namespace Xpandables.Net.ExecutionResults.ResponseWriters;

/// <summary>
/// Provides an execution result response writer that streams file content to the HTTP response when the execution
/// result contains a file stream.
/// </summary>
/// <remarks>This class enables returning files as part of an execution result by writing the stream to the HTTP
/// response with appropriate headers, such as content disposition and content type. It is typically used in scenarios
/// where an API endpoint needs to return downloadable files or streamed content to the client.</remarks>
public sealed class FileExecutionResultResponseWriter : ExecutionResultResponseWriter
{
    /// <inheritdoc/>
    public override bool CanWrite(ExecutionResult executionResult)
    {
        ArgumentNullException.ThrowIfNull(executionResult);
        return executionResult.StatusCode.IsOk
            && executionResult.Value is Stream;
    }

    /// <inheritdoc/>
    public override async Task WriteAsync(HttpContext context, ExecutionResult executionResult)
    {
        ArgumentNullException.ThrowIfNull(context);

        await base.WriteAsync(context, executionResult).ConfigureAwait(false);

        context.Response.ContentType ??= context.GetContentType("application/json; charset=utf-8");

        Stream stream = executionResult.Value as Stream
            ?? throw new InvalidOperationException("Execution result value must be a Stream.");

        string fileName = executionResult.Headers
            .FirstOrDefault(h => h.Key.Equals("FileName", StringComparison.OrdinalIgnoreCase))
            .Values.FirstOrDefault() ?? "download";

        string contentType = executionResult.Headers
            .FirstOrDefault(h => h.Key.Equals("ContentType", StringComparison.OrdinalIgnoreCase))
            .Values.FirstOrDefault() ?? "application/octet-stream";

        bool inline = executionResult.Headers
            .FirstOrDefault(h => h.Key.Equals("Inline", StringComparison.OrdinalIgnoreCase))
            .Values.FirstOrDefault()?.Equals("true", StringComparison.OrdinalIgnoreCase) ?? false;

        string disposition = inline ? "inline" : "attachment";
        context.Response.Headers
            .Append(
                "Content-Disposition",
                $"{disposition}; filename={fileName}");

        IResult streamResult = Results.Stream(
            stream,
            contentType,
            fileName);

        await streamResult
            .ExecuteAsync(context)
            .ConfigureAwait(false);
    }
}
