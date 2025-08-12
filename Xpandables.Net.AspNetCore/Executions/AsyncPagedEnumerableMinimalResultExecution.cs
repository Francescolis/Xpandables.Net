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
using System.Text.Json;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;

using Xpandables.Net.Collections;

namespace Xpandables.Net.Executions;

/// <summary>
/// This class handles the execution of minimal results for asynchronous paged enumerables.
/// </summary>
public sealed class AsyncPagedEnumerableMinimalResultExecution : MinimalResultExecution
{
    readonly static MethodInfo writeMethod = typeof(AsyncPagedEnumerableMinimalResultExecution)
        .GetMethod(nameof(WritePagedEnumerableAsJsonAsync), BindingFlags.NonPublic | BindingFlags.Static)!;

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
        if (!CanExecute(executionResult))
        {
            throw new InvalidOperationException(
                "Execution result must be an IAsyncPagedEnumerable<T> with a status code of OK.");
        }

        await base
            .ExecuteAsync(context, executionResult)
            .ConfigureAwait(false);

        Type itemType = executionResult.Value!.GetType().GetGenericArguments()[0];

        MethodInfo genericWriteMethod = writeMethod.MakeGenericMethod(itemType);

        await ((Task)genericWriteMethod.Invoke(null, [context, executionResult.Value!])!)
            .ConfigureAwait(false);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability",
        "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "<Pending>")]
    private static async Task WritePagedEnumerableAsJsonAsync<T>(
        HttpContext context, IAsyncPagedEnumerable<T> pagedEnumerable)
    {
        var pagination = await pagedEnumerable.GetPaginationAsync().ConfigureAwait(false);

        context.Response.ContentType = GetContentTypeFromEndpoint(context) ?? "application/json; charset=utf-8";

        await using var writer = new Utf8JsonWriter(context.Response.BodyWriter.AsStream(), new JsonWriterOptions
        {
            Indented = false
        });

        writer.WriteStartObject();

        // Write pagination metadata first
        writer.WritePropertyName("pagination");
        JsonSerializer.Serialize(writer, pagination);

        // Write data array start
        writer.WritePropertyName("data");
        writer.WriteStartArray();

        await foreach (T item in pagedEnumerable.WithCancellation(context.RequestAborted))
        {
            JsonSerializer.Serialize(writer, item);

            // Flush periodically to send data to client immediately
            await writer.FlushAsync(context.RequestAborted).ConfigureAwait(false);
        }

        writer.WriteEndArray();
        writer.WriteEndObject();

        // Final flush
        await writer.FlushAsync(context.RequestAborted).ConfigureAwait(false);
    }

    private static string? GetContentTypeFromEndpoint(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint == null) return null;

        // Look for ProducesResponseTypeMetadata in endpoint metadata
        var producesMetadata = endpoint.Metadata
            .OfType<ProducesResponseTypeMetadata>()
            .FirstOrDefault();

        if (producesMetadata?.ContentTypes != null && producesMetadata.ContentTypes.Any())
        {
            return producesMetadata.ContentTypes.First();
        }

        // Alternative: Look for IProducesResponseTypeMetadata
        var iProducesMetadata = endpoint.Metadata
            .OfType<IProducesResponseTypeMetadata>()
            .FirstOrDefault();

        if (iProducesMetadata?.ContentTypes != null && iProducesMetadata.ContentTypes.Any())
        {
            return iProducesMetadata.ContentTypes.First();
        }

        return null;
    }
}
