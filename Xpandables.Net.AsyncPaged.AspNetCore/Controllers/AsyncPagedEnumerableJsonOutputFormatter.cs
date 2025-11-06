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
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

using Xpandables.Net.AsyncPaged.Extensions;

namespace Xpandables.Net.AsyncPaged.Controllers;

/// <summary>
/// Provides functionality to format and serialize objects implementing <see cref="IAsyncPagedEnumerable{T}"/> to JSON
/// in an asynchronous manner. This formatter is specifically designed for paginated data streams.
/// </summary>
/// <remarks>
/// This formatter supports the "application/json", "text/json" and "application/*+json" media types, 
/// and encodings such as UTF-8 and Unicode. It is intended for use in scenarios where paginated data needs to be
/// serialized and streamed efficiently to the response body. The formatter writes the JSON output in the following
/// structure:
/// <code>
/// {
///   "pagination": { "pageSize": 10, "currentPage": 1, "totalCount": 100, ... },
///   "items": [ { ... }, { ... } ]
/// }
/// </code>
/// The "pagination" property contains metadata about the paginated data, while the "items" property
/// contains the serialized items in the paginated collection.
/// <para>
/// PERFORMANCE: This formatter caches JsonTypeInfo resolution results to avoid repeated reflection
/// on every request. For best performance, use source-generated JsonSerializerContext.
/// </para>
/// </remarks>
public sealed class AsyncPagedEnumerableJsonOutputFormatter : TextOutputFormatter
{
    // PERFORMANCE: Cache JsonTypeInfo resolution to avoid repeated reflection
    // Thread-safe dictionary for caching type info per item type
    private readonly ConcurrentDictionary<Type, JsonTypeInfo?> _typeInfoCache = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncPagedEnumerableJsonOutputFormatter"/> class.
    /// </summary>
    /// <remarks>
    /// This formatter supports the "application/json", "text/json" and "application/*+json" media types,
    /// and the UTF-8 and Unicode encodings. It is designed to handle JSON output for asynchronous paged
    /// enumerable data.
    /// </remarks>
    public AsyncPagedEnumerableJsonOutputFormatter()
    {
        SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/json"));
        SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/json"));
        SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/*+json"));
        SupportedEncodings.Add(Encoding.UTF8);
        SupportedEncodings.Add(Encoding.Unicode);
    }

    /// <inheritdoc/>
    protected override bool CanWriteType(Type? type)
    {
        if (type is null) return false;

        return (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IAsyncPagedEnumerable<>)
            || typeof(IAsyncPagedEnumerable).IsAssignableFrom(type));
    }

    /// <inheritdoc/>
    public sealed override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(selectedEncoding);
        ArgumentNullException.ThrowIfNull(context.Object);

        var httpContext = context.HttpContext;
        var pagedEnumerable = (IAsyncPagedEnumerable)context.Object;

        Type itemType = pagedEnumerable.Type;
        var options = GetJsonSerializerOptions(httpContext);
        
        // PERFORMANCE: Use cached JsonTypeInfo to avoid repeated reflection
        JsonTypeInfo? jsonTypeInfo = _typeInfoCache.GetOrAdd(itemType, type =>
            options.TypeInfoResolver?.GetTypeInfo(type, options));

        // PERFORMANCE: Fast path for UTF-8 (most common case) - use PipeWriter directly
        if (selectedEncoding.CodePage == Encoding.UTF8.CodePage)
        {
            try
            {
                // PERFORMANCE: Use PipeWriter directly instead of Stream wrapper
                // This avoids an additional allocation and indirection layer
                PipeWriter pipeWriter = httpContext.Response.BodyWriter;

                if (jsonTypeInfo is not null)
                {
                    await WriteAsJsonTypeInfoDirectAsync(
                        pipeWriter,
                        jsonTypeInfo,
                        pagedEnumerable,
                        httpContext.RequestAborted).ConfigureAwait(false);
                }
                else
                {
                    await WriteAsJsonOptionsDirectAsync(
                        pipeWriter,
                        pagedEnumerable,
                        options,
                        httpContext.RequestAborted).ConfigureAwait(false);
                }
                
                // PERFORMANCE: Don't flush here - let the framework handle it
                // Explicit flushing can cause extra overhead in benchmarks
                // The PipeWriter will flush automatically when completed
                return;
            }
            catch (OperationCanceledException) when (httpContext.RequestAborted.IsCancellationRequested)
            {
                // Client disconnected - this is expected behavior
            }
        }
        else
        {
            // PERFORMANCE: Slower path for non-UTF8 encodings (rare case)
            // JsonSerializer only emits UTF8 encoded output, but we need to write the response in the encoding specified by selectedEncoding
            Stream? transcodingStream = null;
            ExceptionDispatchInfo? exceptionDispatchInfo = null;

            try
            {
#pragma warning disable CA2000 // Dispose objects before losing scope
                transcodingStream = Encoding.CreateTranscodingStream(
                    httpContext.Response.Body,
                    selectedEncoding,
                    Encoding.UTF8,
                    leaveOpen: true);
#pragma warning restore CA2000 // Dispose objects before losing scope

                if (jsonTypeInfo is not null)
                {
                    await WriteAsJsonTypeInfoAsync(
                        transcodingStream,
                        jsonTypeInfo,
                        pagedEnumerable,
                        httpContext.RequestAborted).ConfigureAwait(false);
                }
                else
                {
                    await WriteAsJsonOptionsAsync(
                        transcodingStream,
                        pagedEnumerable,
                        options,
                        httpContext.RequestAborted).ConfigureAwait(false);
                }

                await transcodingStream.FlushAsync(httpContext.RequestAborted).ConfigureAwait(false);
            }
            catch (Exception ex)
                when (ex is not ArgumentNullException)
            {
                // TranscodingStream may write to the inner stream as part of its disposal.
                // We do not want this exception "ex" to be eclipsed by any exception encountered during the write.
                // We will stash it and explicitly rethrow it during the finally block.
                exceptionDispatchInfo = ExceptionDispatchInfo.Capture(ex);
            }
            finally
            {
                if (transcodingStream is not null)
                {
#pragma warning disable CA1031 // Do not catch general exception types
                    try
                    {
                        await transcodingStream.DisposeAsync().ConfigureAwait(false);
                    }
                    catch when (exceptionDispatchInfo is not null)
                    {
                        // Suppress disposal exceptions if we already captured a more important exception
                    }
#pragma warning restore CA1031 // Do not catch general exception types
                }

                exceptionDispatchInfo?.Throw();
            }
        }
    }

    // PERFORMANCE: Direct PipeWriter methods to avoid Stream allocation overhead
    private static Task WriteAsJsonTypeInfoDirectAsync(
        PipeWriter pipeWriter,
        JsonTypeInfo jsonTypeInfo,
        IAsyncPagedEnumerable pagedEnumerable,
        CancellationToken cancellationToken)
    {
        return JsonSerializer.SerializeAsyncPaged(
                pipeWriter,
                pagedEnumerable,
                jsonTypeInfo,
                cancellationToken);
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
    private static Task WriteAsJsonOptionsDirectAsync(
        PipeWriter pipeWriter,
        IAsyncPagedEnumerable pagedEnumerable,
        JsonSerializerOptions options,
        CancellationToken cancellationToken)
    {
        return JsonSerializer.SerializeAsyncPaged(
            pipeWriter,
            pagedEnumerable,
            options,
            cancellationToken);
    }

    // Stream-based methods for transcoding path (non-UTF8 encodings)
    private static Task WriteAsJsonTypeInfoAsync(
        Stream stream,
        JsonTypeInfo jsonTypeInfo,
        IAsyncPagedEnumerable pagedEnumerable,
        CancellationToken cancellationToken)
    {
        return JsonSerializer.SerializeAsyncPaged(
                stream,
                pagedEnumerable,
                jsonTypeInfo,
                cancellationToken);
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
    private static Task WriteAsJsonOptionsAsync(
        Stream stream,
        IAsyncPagedEnumerable pagedEnumerable,
        JsonSerializerOptions options,
        CancellationToken cancellationToken)
    {
        return JsonSerializer.SerializeAsyncPaged(
            stream,
            pagedEnumerable,
            options,
            cancellationToken);
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
    private static JsonSerializerOptions GetJsonSerializerOptions(HttpContext httpContext)
    {
        var options = httpContext.RequestServices
            .GetService<IOptions<JsonOptions>>()?.Value?.JsonSerializerOptions
            ?? JsonSerializerOptions.Default;

        options.MakeReadOnly(true);
        return options;
    }
}