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
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

using Xpandables.Net.Async;

namespace Xpandables.Net.Async;

/// <summary>
/// Provides functionality to format and serialize objects implementing <see cref="IAsyncPagedEnumerable{T}"/>  to JSON
/// in an asynchronous manner. This formatter is specifically designed for paginated data streams.
/// </summary>
/// <remarks>This formatter supports the "application/json", "text/json;" and "application/*+json" media types, 
/// and encodings such as UTF-8 and Unicode. It is intended for use in scenarios where paginated data  needs to be
/// serialized and streamed efficiently to the response body.  The formatter writes the JSON output in the following
/// structure: <code> {   "pageContext": { ... },   "data": [ ... ] } </code> The "pageContext" property contains metadata
/// about the paginated data, while the "data" property  contains the serialized items in the paginated
/// collection.</remarks>
public sealed class AsyncPagedEnumerableJsonOutputFormatter : TextOutputFormatter
{
    static readonly MethodInfo WriteCoreAsyncOpenGeneric =
        typeof(AsyncPagedEnumerableJsonOutputFormatter)
            .GetMethod(nameof(WriteCoreAsync), BindingFlags.NonPublic | BindingFlags.Static)!
            .GetGenericMethodDefinition();

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncPagedEnumerableJsonOutputFormatter"/> class.
    /// </summary>
    /// <remarks>This formatter supports the "application/json" and "application/json; charset=utf-8" media
    /// types, and the UTF-8 and Unicode encodings. It is designed to handle JSON output for asynchronous paged
    /// enumerable data.</remarks>
    public AsyncPagedEnumerableJsonOutputFormatter(JsonSerializerOptions jsonSerializerOptions)
    {
        SerializerOptions = jsonSerializerOptions ?? throw new ArgumentNullException(nameof(jsonSerializerOptions));
        jsonSerializerOptions.MakeReadOnly();

        SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/json"));
        SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/json;"));
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
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
    [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
    [SuppressMessage("Trimming", "IL2060:Call to 'System.Reflection.MethodInfo.MakeGenericMethod' can not be statically analyzed. It's not possible to guarantee the availability of requirements of the generic method.", Justification = "<Pending>")]
    public sealed override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(selectedEncoding);
        ArgumentNullException.ThrowIfNull(context.Object);

        var httpContext = context.HttpContext;
        var ct = httpContext.RequestAborted;

        var instance = context.Object;
        var itemType = ((IAsyncPagedEnumerable)instance).Type;

        var method = WriteCoreAsyncOpenGeneric.MakeGenericMethod(itemType);

        if (selectedEncoding.CodePage == Encoding.UTF8.CodePage)
        {
            try
            {
                using var writer = new Utf8JsonWriter(httpContext.Response.BodyWriter.AsStream(),
                    new JsonWriterOptions
                    {
                        Indented = SerializerOptions.WriteIndented,
                        Encoder = SerializerOptions.Encoder
                    });

                await ((Task)method.Invoke(null, [writer, instance, SerializerOptions, ct])!).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (context.HttpContext.RequestAborted.IsCancellationRequested) { }
        }
        else
        {
            var transcodingStream = Encoding.CreateTranscodingStream(httpContext.Response.Body, selectedEncoding, Encoding.UTF8, leaveOpen: true);

            ExceptionDispatchInfo? exceptionDispatchInfo = null;

            try
            {
                using var writer = new Utf8JsonWriter(transcodingStream,
                    new JsonWriterOptions
                    {
                        Indented = SerializerOptions.WriteIndented,
                        Encoder = SerializerOptions.Encoder
                    });

                await ((Task)method.Invoke(null, [writer, instance, SerializerOptions, ct])!).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                exceptionDispatchInfo = ExceptionDispatchInfo.Capture(exception);
            }
            finally
            {
                try
                {
                    await transcodingStream.DisposeAsync().ConfigureAwait(false);
                }
                catch when (exceptionDispatchInfo != null)
                {
                }

                exceptionDispatchInfo?.Throw();
            }
        }
    }

    internal static async Task WriteCoreAsync<T>(
        Utf8JsonWriter writer,
        object instance,
        JsonSerializerOptions options,
        CancellationToken ct)
    {
        var paged = (IAsyncPagedEnumerable<T>)instance;
        Delegate @delegate = paged.GetPaginationAsync;
        var v = @delegate.Method.Invoke(paged, [ct]) as Task<Pagination>;
        var pagination = await paged.GetPaginationAsync(ct).ConfigureAwait(false);

        writer.WriteStartObject();

        writer.WritePropertyName("pagination");
        JsonSerializer.Serialize(writer, pagination, PaginationSourceGenerationContext.Default.Pagination);

        writer.WritePropertyName("items");
        writer.WriteStartArray();

        JsonTypeInfo jsonTypeInfo = options.GetTypeInfo(typeof(T))
            ?? throw new InvalidOperationException($"Cannot get the {typeof(T)} type information.");
        await foreach (var item in paged.WithCancellation(ct).ConfigureAwait(false))
        {
            JsonSerializer.Serialize(writer, item, jsonTypeInfo);
            await writer.FlushAsync(ct).ConfigureAwait(false);
        }

        writer.WriteEndArray();
        writer.WriteEndObject();

        await writer.FlushAsync(ct).ConfigureAwait(false);
    }
}