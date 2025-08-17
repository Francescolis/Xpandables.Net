
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

namespace Xpandables.Net.Executions;

/// <summary>
/// This class represents a minimal result execution that handles streaming responses.
/// </summary>
public sealed class StreamMinimalResultExecution : MinimalResultExecution
{
    /// <inheritdoc/>
    public sealed override bool CanExecute(ExecutionResult executionResult) =>
        executionResult is { StatusCode: HttpStatusCode.OK, Value: Stream };

    /// <inheritdoc/>
    public sealed override async Task ExecuteAsync(HttpContext context, ExecutionResult executionResult)
    {
        await base
            .ExecuteAsync(context, executionResult)
            .ConfigureAwait(false);

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
