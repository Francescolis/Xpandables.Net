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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using Microsoft.AspNetCore.Http;

using Xpandables.Net.AsyncPaged;
using Xpandables.Net.AsyncPaged.Extensions;

namespace Xpandables.Net.ExecutionResults.ResponseWriters;

/// <summary>
/// Provides an HTTP response writer for streaming execution results using asynchronous paging. This class enables
/// efficient serialization of large result sets to the response stream in JSON format.
/// </summary>
/// <remarks>Use this writer when the execution result contains an asynchronous paged enumerable, allowing clients
/// to consume data as it is streamed. This approach is suitable for scenarios where the result set may be large or when
/// incremental delivery is desired. The response is serialized as JSON and written directly to the HTTP response
/// body.</remarks>
public sealed class StreamExecutionResultResponseWriter : ExecutionResultResponseWriter
{
    /// <inheritdoc/>
    public override bool CanWrite(ExecutionResult executionResult)
    {
        ArgumentNullException.ThrowIfNull(executionResult);
        return executionResult.IsSuccess
            && executionResult.Value is IAsyncPagedEnumerable;
    }

    /// <inheritdoc/>
    public override async Task WriteAsync(HttpContext context, ExecutionResult executionResult)
    {
        await base.WriteAsync(context, executionResult).ConfigureAwait(false);

        Debug.Assert(executionResult.Value is not null, "Value cannot be null here.");

        var asyncPaged = (IAsyncPagedEnumerable)executionResult.Value;
        Type type = asyncPaged.Type;
        var options = context.GetJsonSerializerOptions();
        JsonTypeInfo? jsonTypeInfo = options.GetTypeInfo(type);
        PipeWriter pipeWriter = context.Response.BodyWriter;

        if (jsonTypeInfo is not null)
        {
            await JsonSerializer.SerializeAsyncPaged(
                pipeWriter,
                asyncPaged,
                jsonTypeInfo,
                context.RequestAborted).ConfigureAwait(false);
        }
        else
        {
            await WriteAsJsonOptionsDirectAsync(
                pipeWriter,
                asyncPaged,
                options,
                context.RequestAborted).ConfigureAwait(false);
        }
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
    private static Task WriteAsJsonOptionsDirectAsync(
        PipeWriter pipeWriter,
        IAsyncPagedEnumerable pagedEnumerable,
        JsonSerializerOptions options,
        CancellationToken cancellationToken = default)
    {
        return JsonSerializer.SerializeAsyncPaged(
            pipeWriter,
            pagedEnumerable,
            options,
            cancellationToken);
    }
}
