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
using System.IO.Pipelines;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

using Xpandables.Net.Collections.Extensions;
using Xpandables.Net.Collections.Generic;
using Xpandables.Net.Http;

namespace Xpandables.Net.Collections.Http;

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
/// </remarks>
public sealed class AsyncPagedEnumerableJsonOutputFormatter : TextOutputFormatter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncPagedEnumerableJsonOutputFormatter"/> class.
    /// </summary>
    /// <param name="jsonSerializerOptions">The <see cref="JsonSerializerOptions"/> used for serialization.</param>
    /// <remarks>
    /// This formatter supports the "application/json", "text/json" and "application/*+json" media types,
    /// and the UTF-8 and Unicode encodings. It is designed to handle JSON output for asynchronous paged
    /// enumerable data.
    /// </remarks>
    public AsyncPagedEnumerableJsonOutputFormatter(JsonSerializerOptions jsonSerializerOptions)
    {
        SerializerOptions = jsonSerializerOptions ?? throw new ArgumentNullException(nameof(jsonSerializerOptions));
        jsonSerializerOptions.MakeReadOnly();

        SupportedEncodings.Add(Encoding.UTF8);
        SupportedEncodings.Add(Encoding.Unicode);

        SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/json"));
        SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/json"));
        SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/*+json"));
    }

    internal static AsyncPagedEnumerableJsonOutputFormatter CreateFormatter(JsonOptions jsonOptions)
    {
        var jsonSerializerOptions = jsonOptions.JsonSerializerOptions;

        if (jsonSerializerOptions.Encoder is null)
        {
            // If the user hasn't explicitly configured the encoder, use the less strict encoder that does not encode all non-ASCII characters.
            jsonSerializerOptions = new JsonSerializerOptions(jsonSerializerOptions)
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            };
        }

        return new AsyncPagedEnumerableJsonOutputFormatter(jsonSerializerOptions);
    }

    /// <summary>
    /// Gets the <see cref="JsonSerializerOptions"/> used to configure the <see cref="JsonSerializer"/>.
    /// </summary>
    /// <remarks>
    /// A single instance of <see cref="SystemTextJsonOutputFormatter"/> is used for all JSON formatting. Any
    /// changes to the options will affect all output formatting.
    /// </remarks>
    public JsonSerializerOptions SerializerOptions { get; }

    /// <inheritdoc/>
    protected override bool CanWriteType(Type? type)
    {
        if (type is null) return false;

        return type.IsGenericType
            && type.GetGenericTypeDefinition() == typeof(IAsyncPagedEnumerable<>);
    }

    /// <inheritdoc/>
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "<Pending>")]
    public sealed override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(selectedEncoding);
        ArgumentNullException.ThrowIfNull(context.Object);

        var httpContext = context.HttpContext;
        var pagedEnumerable = (IAsyncPagedEnumerable)context.Object;
        Type itemType = pagedEnumerable.GetArgumentType();

        JsonTypeInfo? jsonTypeInfo = null;
        var declaredTypeInfo = SerializerOptions.GetTypeInfo(itemType);
        var runtimeType = context.ObjectType?.GetGenericArguments()[0];
        if (declaredTypeInfo.ShouldUseWith(runtimeType))
        {
            jsonTypeInfo = declaredTypeInfo;
        }

        if (selectedEncoding.CodePage == Encoding.UTF8.CodePage)
        {
            try
            {
                PipeWriter pipeWriter = httpContext.Response.BodyWriter;

                if (jsonTypeInfo is not null)
                {
                    await JsonSerializer.SerializeAsyncPaged(
                        pipeWriter,
                        pagedEnumerable,
                        jsonTypeInfo,
                        httpContext.RequestAborted).ConfigureAwait(false);
                }
                else
                {
                    await WriteAsJsonOptionsDirectAsync(
                        pipeWriter,
                        pagedEnumerable,
                        SerializerOptions,
                        httpContext.RequestAborted).ConfigureAwait(false);
                }
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
            Stream transcodingStream = Encoding.CreateTranscodingStream(
                    httpContext.Response.Body,
                    selectedEncoding,
                    Encoding.UTF8,
                    leaveOpen: true);

            ExceptionDispatchInfo? exceptionDispatchInfo = null;

            try
            {
                if (jsonTypeInfo is not null)
                {
                    await JsonSerializer.SerializeAsyncPaged(
                        transcodingStream,
                        pagedEnumerable,
                        jsonTypeInfo).ConfigureAwait(false);
                }
                else
                {
                    await WriteAsJsonOptionsAsync(
                        transcodingStream,
                        pagedEnumerable,
                        SerializerOptions).ConfigureAwait(false);
                }

                await transcodingStream.FlushAsync(httpContext.RequestAborted).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // TranscodingStream may write to the inner stream as part of its disposal.
                // We do not want this exception "ex" to be eclipsed by any exception encountered during the write.
                // We will stash it and explicitly rethrow it during the finally block.
                exceptionDispatchInfo = ExceptionDispatchInfo.Capture(ex);
            }
            finally
            {
                try
                {
                    await transcodingStream.DisposeAsync().ConfigureAwait(false);
                }
                catch when (exceptionDispatchInfo is not null)
                {
                    // Suppress disposal exceptions if we already captured a more important exception
                }

                exceptionDispatchInfo?.Throw();
            }
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

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
    private static Task WriteAsJsonOptionsAsync(
        Stream stream,
        IAsyncPagedEnumerable pagedEnumerable,
        JsonSerializerOptions options,
        CancellationToken cancellationToken = default)
    {
        return JsonSerializer.SerializeAsyncPaged(
            stream,
            pagedEnumerable,
            options,
            cancellationToken);
    }
}