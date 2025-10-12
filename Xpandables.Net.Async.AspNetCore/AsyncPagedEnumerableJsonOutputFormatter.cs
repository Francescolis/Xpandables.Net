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
using System.Runtime.ExceptionServices;
using System.Text;
using System.Text.Json;

using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace Xpandables.Net.Async;

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
    /// <param name="jsonSerializerOptions">The <see cref="JsonSerializerOptions"/> to use for serialization.</param>
    /// <remarks>
    /// This formatter supports the "application/json", "text/json" and "application/*+json" media types,
    /// and the UTF-8 and Unicode encodings. It is designed to handle JSON output for asynchronous paged
    /// enumerable data.
    /// </remarks>
    public AsyncPagedEnumerableJsonOutputFormatter(JsonSerializerOptions jsonSerializerOptions)
    {
        SerializerOptions = jsonSerializerOptions ?? throw new ArgumentNullException(nameof(jsonSerializerOptions));
        jsonSerializerOptions.MakeReadOnly();

        SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/json"));
        SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/json"));
        SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/*+json"));
        SupportedEncodings.Add(Encoding.UTF8);
        SupportedEncodings.Add(Encoding.Unicode);
    }

    /// <summary>
    /// Gets the <see cref="JsonSerializerOptions"/> used to configure the <see cref="JsonSerializer"/>.
    /// </summary>
    /// <remarks>
    /// A single instance of this formatter is used for all JSON formatting. Any
    /// changes to the options will affect all output formatting.
    /// </remarks>
    public JsonSerializerOptions SerializerOptions { get; }

    /// <inheritdoc/>
    protected override bool CanWriteType(Type? type)
    {
        if (type is null) return false;

        return (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IAsyncPagedEnumerable<>)
            || typeof(IAsyncPagedEnumerable).IsAssignableFrom(type));
    }

    /// <inheritdoc/>
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "")]
    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Dispose on finally")]
    [UnconditionalSuppressMessage("Trimming",
        "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
        Justification = "AOT already apply on the JsonSerializer mehtod")]
    [UnconditionalSuppressMessage("AOT",
        "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.",
        Justification = "AOT already apply on the JsonSerializer mehtod")]
    public sealed override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(selectedEncoding);
        ArgumentNullException.ThrowIfNull(context.Object);

        var httpContext = context.HttpContext;
        var pagedEnumerable = (IAsyncPagedEnumerable)context.Object;

        if (selectedEncoding.CodePage == Encoding.UTF8.CodePage)
        {
            try
            {
                // Use Stream with leaveOpen to avoid disposal issues
                Stream responseStream = httpContext.Response.BodyWriter.AsStream(leaveOpen: true);

                await JsonSerializer.SerializeAsyncPaged(
                    responseStream,
                    pagedEnumerable,
                    SerializerOptions,
                    httpContext.RequestAborted).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (httpContext.RequestAborted.IsCancellationRequested)
            {
                // Client disconnected
            }
        }
        else
        {
            // JsonSerializer only emits UTF8 encoded output, but we need to write the response in the encoding specified by selectedEncoding
            Stream? transcodingStream = null;
            ExceptionDispatchInfo? exceptionDispatchInfo = null;

            try
            {
                transcodingStream = Encoding.CreateTranscodingStream(
                    httpContext.Response.Body,
                    selectedEncoding,
                    Encoding.UTF8,
                    leaveOpen: true);

                // Use the extension method with transcoding stream
                await JsonSerializer.SerializeAsyncPaged(
                    transcodingStream,
                    pagedEnumerable,
                    SerializerOptions,
                    httpContext.RequestAborted).ConfigureAwait(false);

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
                    try
                    {
                        await transcodingStream.DisposeAsync().ConfigureAwait(false);
                    }
                    catch when (exceptionDispatchInfo is not null)
                    {
                        // Suppress disposal exceptions if we already captured a more important exception
                    }
                }

                exceptionDispatchInfo?.Throw();
            }
        }
    }
}