
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
using System.Reflection;
using System.Text;
using System.Text.Json;

using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

using Xpandables.Net.Collections;

namespace Xpandables.Net.Executions;

/// <summary>
/// Provides functionality to format and serialize objects implementing <see cref="IAsyncPagedEnumerable{T}"/>  to JSON
/// in an asynchronous manner. This formatter is specifically designed for paginated data streams.
/// </summary>
/// <remarks>This formatter supports the "application/json" and "application/json; charset=utf-8" media types, 
/// and encodings such as UTF-8 and Unicode. It is intended for use in scenarios where paginated data  needs to be
/// serialized and streamed efficiently to the response body.  The formatter writes the JSON output in the following
/// structure: <code> {   "pagination": { ... },   "data": [ ... ] } </code> The "pagination" property contains metadata
/// about the paginated data, while the "data" property  contains the serialized items in the paginated
/// collection.</remarks>
public sealed class AsyncPagedEnumerableJsonOutputFormatter : TextOutputFormatter
{
    readonly static MethodInfo _method = typeof(AsyncPagedEnumerableJsonOutputFormatter)
     .GetMethod(nameof(WriteCoreAsync),
        BindingFlags.NonPublic | BindingFlags.Static)!;

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncPagedEnumerableJsonOutputFormatter"/> class.
    /// </summary>
    /// <remarks>This formatter supports the "application/json" and "application/json; charset=utf-8" media
    /// types, and the UTF-8 and Unicode encodings. It is designed to handle JSON output for asynchronous paged
    /// enumerable data.</remarks>
    public AsyncPagedEnumerableJsonOutputFormatter()
    {
        SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/json"));
        SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/json; charset=utf-8"));
        SupportedEncodings.Add(Encoding.UTF8);
        SupportedEncodings.Add(Encoding.Unicode);
    }

    /// <inheritdoc/>
    protected override bool CanWriteType(Type? type)
    {
        if (type is null) return false;

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IAsyncPagedEnumerable<>))
            return true;

        return type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IAsyncPagedEnumerable<>));
    }

    /// <inheritdoc/>
    public override async Task WriteResponseBodyAsync(
        OutputFormatterWriteContext context,
        Encoding selectedEncoding)
    {
        var httpContext = context.HttpContext;
        var ct = httpContext.RequestAborted;

        var jsonOptions = httpContext.RequestServices
            .GetService<IOptions<JsonOptions>>()?.Value?.SerializerOptions;

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
        await using var writer = new Utf8JsonWriter(
            httpContext.Response.BodyWriter.AsStream(),
            new JsonWriterOptions
            {
                Indented = false
            });
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

        var instance = context.Object!;
        var objectType = instance.GetType();
        var asyncPagedType = objectType.IsGenericType
            && objectType.GetGenericTypeDefinition() == typeof(IAsyncPagedEnumerable<>)
            ? objectType
            : objectType.GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IAsyncPagedEnumerable<>));

        var itemType = asyncPagedType.GetGenericArguments()[0];

        var method = _method.MakeGenericMethod(itemType);

        await ((Task)method.Invoke(null, [writer, instance, jsonOptions!, ct])!).ConfigureAwait(false);
    }

    private static async Task WriteCoreAsync<T>(
        Utf8JsonWriter writer,
        object instance,
        JsonSerializerOptions options,
        CancellationToken ct)
    {
        var paged = (IAsyncPagedEnumerable<T>)instance;

        // Ensure pagination first; in prime mode this primes and prepares the underlying stream.
        var pagination = await paged.GetPaginationAsync(ct).ConfigureAwait(false);

        writer.WriteStartObject();

        writer.WritePropertyName("pagination");
        JsonSerializer.Serialize(writer, pagination, options);

        writer.WritePropertyName("data");
        writer.WriteStartArray();

        await foreach (var item in paged.WithCancellation(ct).ConfigureAwait(false))
        {
            JsonSerializer.Serialize(writer, item, options);
            await writer.FlushAsync(ct).ConfigureAwait(false);
        }

        writer.WriteEndArray();
        writer.WriteEndObject();

        await writer.FlushAsync(ct).ConfigureAwait(false);
    }
}