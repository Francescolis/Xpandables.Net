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
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using Xpandables.Net.AsyncPaged.Extensions;

namespace Xpandables.Net.AsyncPaged.Extensions;

/// <summary>
/// Provides extension methods for reading HTTP content as an asynchronous paged enumerable of JSON objects.
/// </summary>
/// <remarks>These extension methods enable efficient, asynchronous processing of large JSON payloads returned
/// from HTTP responses by exposing the items as an <see cref="IAsyncPagedEnumerable{T}"/>. This is particularly useful when working
/// with paged or streaming JSON APIs, as it allows consuming items incrementally without loading the entire response
/// into memory.</remarks>
public static class HttpContentExtensions
{
    /// <summary>
    /// Reads the HTTP content as an asynchronous paged enumerable of JSON objects.
    /// </summary>
    extension(HttpContent content)
    {
        /// <summary>
        /// Reads the HTTP content as a paged asynchronous sequence of objects of type T, deserialized from JSON using
        /// the specified serializer options.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The method supports JSON content in the following formats:
        /// </para>
        /// <list type="number">
        /// <item>
        /// <description>
        /// Structured response with pagination metadata:
        /// <code>
        /// {
        ///   "pagination": { "pageSize": 10, "currentPage": 1, "totalCount": 100, "continuationToken": "..." },
        ///   "items": [ { ... }, { ... } ]
        /// }
        /// </code>
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// Structured response without pagination metadata (TotalCount will be computed during enumeration):
        /// <code>
        /// {
        ///   "items": [ { ... }, { ... } ]
        /// }
        /// </code>
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// Top-level array (TotalCount will be computed during enumeration):
        /// <code>
        /// [ { ... }, { ... } ]
        /// </code>
        /// </description>
        /// </item>
        /// </list>
        /// <para>
        /// This method uses streaming deserialization to minimize memory allocation. Items are deserialized 
        /// on-demand as they are enumerated. Pagination metadata is extracted lazily only when accessed.
        /// </para>
        /// <para>
        /// Performance characteristics:
        /// - Single-pass streaming deserialization with minimal memory overhead
        /// - Items deserialized on-demand during enumeration
        /// - Pagination metadata extracted only when GetPaginationAsync() is called
        /// - Zero-copy for simple array root scenarios
        /// </para>
        /// </remarks>
        /// <typeparam name="T">The type of objects to deserialize from the JSON content.</typeparam>
        /// <param name="options">The options to use for JSON deserialization. Cannot be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>An asynchronous paged enumerable containing the deserialized objects of type T.</returns>
        [RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializer.DeserializeAsyncEnumerable<TValue>(Stream, JsonSerializerOptions, CancellationToken)")]
        [RequiresDynamicCode("Calls System.Text.Json.JsonSerializer.DeserializeAsyncEnumerable<TValue>(Stream, JsonSerializerOptions, CancellationToken)")]
        public IAsyncPagedEnumerable<T> ReadFromJsonAsAsyncPagedEnumerable<T>(
            JsonSerializerOptions? options,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(content);
            ArgumentNullException.ThrowIfNull(options);

            var jsonTypeInfo = (JsonTypeInfo<T>?)options?.GetTypeInfo(typeof(T));
            return ReadFromJsonAsAsyncPagedEnumerable(content, jsonTypeInfo, options, cancellationToken);
        }

        /// <summary>
        /// Reads the HTTP content as a paged asynchronous sequence of JSON values of type T using streaming deserialization.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The method supports JSON content in the following formats:
        /// </para>
        /// <list type="number">
        /// <item>
        /// <description>
        /// Structured response with pagination metadata:
        /// <code>
        /// {
        ///   "pagination": { "pageSize": 10, "currentPage": 1, "totalCount": 100, "continuationToken": "..." },
        ///   "items": [ { ... }, { ... } ]
        /// }
        /// </code>
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// Structured response without pagination metadata (TotalCount will be computed during enumeration):
        /// <code>
        /// {
        ///   "items": [ { ... }, { ... } ]
        /// }
        /// </code>
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// Top-level array (TotalCount will be computed during enumeration):
        /// <code>
        /// [ { ... }, { ... } ]
        /// </code>
        /// </description>
        /// </item>
        /// </list>
        /// <para>
        /// This method uses streaming deserialization to minimize memory allocation. Items are deserialized 
        /// on-demand as they are enumerated. Pagination metadata is extracted lazily only when accessed.
        /// </para>
        /// <para>
        /// Performance characteristics:
        /// - Single-pass streaming deserialization with minimal memory overhead
        /// - Items deserialized on-demand during enumeration
        /// - Pagination metadata extracted only when GetPaginationAsync() is called
        /// - Zero-copy for simple array root scenarios
        /// </para>
        /// </remarks>
        /// <typeparam name="T">The type of elements to deserialize from the JSON content.</typeparam>
        /// <param name="jsonTypeInfo">Metadata used to control the deserialization of JSON values to type T. Cannot be null.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests. The default value is None.</param>
        /// <returns>An asynchronous paged enumerable containing the deserialized objects of type T.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="jsonTypeInfo"/> is null.</exception>
        /// <exception cref="JsonException">Thrown when the JSON content is malformed or cannot be parsed.</exception>
        [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
        [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
        public IAsyncPagedEnumerable<T> ReadFromJsonAsAsyncPagedEnumerable<T>(
            JsonTypeInfo<T> jsonTypeInfo,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(content);
            ArgumentNullException.ThrowIfNull(jsonTypeInfo);

            return ReadFromJsonAsAsyncPagedEnumerable(content, jsonTypeInfo, null, cancellationToken);
        }

        [RequiresUnreferencedCode("Calls Xpandables.Net.AsyncPaged.Extensions.HttpContentExtensions.StreamItemsAsync<T>(StreamState, JsonTypeInfo<T>, JsonSerializerOptions, CancellationToken)")]
        [RequiresDynamicCode("Calls Xpandables.Net.AsyncPaged.Extensions.HttpContentExtensions.StreamItemsAsync<T>(StreamState, JsonTypeInfo<T>, JsonSerializerOptions, CancellationToken)")]
        private static AsyncPagedEnumerable<T> ReadFromJsonAsAsyncPagedEnumerable<T>(
            HttpContent httpContent,
            JsonTypeInfo<T>? jsonTypeInfo,
            JsonSerializerOptions? options,
            CancellationToken cancellationToken)
        {
            // Create shared state for the stream
            var streamState = new StreamState(httpContent, cancellationToken);

            // Create the async enumerable source for items - this will stream directly
            IAsyncEnumerable<T> itemsSource = StreamItemsAsync(streamState, jsonTypeInfo, options, cancellationToken);

            // Create the pagination factory - only executed if GetPaginationAsync is called
            Func<CancellationToken, ValueTask<Pagination>> paginationFactory = ct => ExtractPaginationAsync(streamState, options, ct);

            // Return an AsyncPagedEnumerable that wraps both
            return new AsyncPagedEnumerable<T>(itemsSource, paginationFactory);
        }
    }

    /// <summary>
    /// Shared state for streaming operations.
    /// </summary>
    private sealed class StreamState(HttpContent content, CancellationToken cancellationToken)
    {
        private readonly HttpContent _content = content;
        private readonly CancellationToken _cancellationToken = cancellationToken;
        private byte[]? _buffer;
        private int _bufferLength;
        private int _state; // 0 = not loaded, 1 = loading, 2 = loaded

        public async ValueTask<(byte[] Buffer, int Length)> GetBufferAsync()
        {
            // Fast path - already loaded
            if (Volatile.Read(ref _state) == 2)
            {
                return (_buffer!, _bufferLength);
            }

            // Try to start loading
            if (Interlocked.CompareExchange(ref _state, 1, 0) == 0)
            {
                try
                {
                    Stream contentStream = await GetContentStreamAsync(_content, _cancellationToken).ConfigureAwait(false);

                    // Try to get content length for better buffer sizing
                    int? contentLength = (int?)_content.Headers.ContentLength;

                    if (contentLength.HasValue && contentLength.Value > 0)
                    {
                        // We know the size - allocate exactly
                        _buffer = ArrayPool<byte>.Shared.Rent(contentLength.Value);
                        int totalRead = 0;
                        int read;
                        while (totalRead < contentLength.Value &&
                               (read = await contentStream.ReadAsync(_buffer.AsMemory(totalRead, contentLength.Value - totalRead), _cancellationToken).ConfigureAwait(false)) > 0)
                        {
                            totalRead += read;
                        }
                        _bufferLength = totalRead;
                        await contentStream.DisposeAsync().ConfigureAwait(false);
                    }
                    else
                    {
                        // Unknown size - use MemoryStream
                        using var ms = new MemoryStream();
                        await contentStream.CopyToAsync(ms, _cancellationToken).ConfigureAwait(false);
                        await contentStream.DisposeAsync().ConfigureAwait(false);

                        _bufferLength = (int)ms.Length;
                        _buffer = ArrayPool<byte>.Shared.Rent(_bufferLength);
                        ms.Position = 0;
                        _ = await ms.ReadAsync(_buffer.AsMemory(0, _bufferLength), _cancellationToken).ConfigureAwait(false);
                    }

                    Volatile.Write(ref _state, 2);
                }
                catch
                {
                    Volatile.Write(ref _state, 0); // Reset on error
                    throw;
                }
            }
            else
            {
                // Wait for loading to complete
                SpinWait spinner = new();
                while (Volatile.Read(ref _state) != 2)
                {
                    spinner.SpinOnce();
                }
            }

            return (_buffer!, _bufferLength);
        }

        ~StreamState()
        {
            if (_buffer is not null)
            {
                ArrayPool<byte>.Shared.Return(_buffer);
            }
        }
    }

    /// <summary>
    /// Streams items directly from the content without pre-parsing.
    /// </summary>
    [RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializer.DeserializeAsyncEnumerable<TValue>(Stream, JsonSerializerOptions, CancellationToken)")]
    [RequiresDynamicCode("Calls System.Text.Json.JsonSerializer.DeserializeAsyncEnumerable<TValue>(Stream, JsonSerializerOptions, CancellationToken)")]
    private static async IAsyncEnumerable<T> StreamItemsAsync<T>(
        StreamState streamState,
        JsonTypeInfo<T>? jsonTypeInfo,
        JsonSerializerOptions? options,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var (buffer, length) = await streamState.GetBufferAsync().ConfigureAwait(false);

        if (length == 0)
        {
            yield break;
        }

        // Quick peek to determine structure
        var reader = new Utf8JsonReader(new ReadOnlySpan<byte>(buffer, 0, Math.Min(length, 256)), isFinalBlock: false, default);

        if (!reader.Read())
        {
            yield break;
        }

        if (reader.TokenType == JsonTokenType.StartArray)
        {
            // Top-level array - stream directly
            using var stream = new MemoryStream(buffer, 0, length, writable: false);
            if (jsonTypeInfo is null)
            {
                await foreach (var item in JsonSerializer.DeserializeAsyncEnumerable<T>(stream, options, cancellationToken).ConfigureAwait(false))
                {
                    if (item is not null)
                    {
                        yield return item;
                    }
                }
            }
            else
            {
                await foreach (var item in JsonSerializer.DeserializeAsyncEnumerable(stream, jsonTypeInfo, cancellationToken).ConfigureAwait(false))
                {
                    if (item is not null)
                    {
                        yield return item;
                    }
                }
            }
        }
        else if (reader.TokenType == JsonTokenType.StartObject)
        {
            // Find the "items" array
            int itemsStart = -1;
            int itemsLength = 0;

            reader = new Utf8JsonReader(new ReadOnlySpan<byte>(buffer, 0, length), isFinalBlock: true, default);

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.PropertyName && reader.ValueTextEquals("items"u8))
                {
                    reader.Read(); // Move to value

                    if (reader.TokenType == JsonTokenType.StartArray)
                    {
                        itemsStart = (int)reader.TokenStartIndex;

                        // Skip to end of array
                        reader.Skip();
                        itemsLength = (int)(reader.BytesConsumed - itemsStart);
                        break;
                    }
                }
            }

            if (itemsStart >= 0 && itemsLength > 0)
            {
                using var itemsStream = new MemoryStream(buffer, itemsStart, itemsLength, writable: false);

                if (jsonTypeInfo is null)
                {
                    await foreach (var item in JsonSerializer.DeserializeAsyncEnumerable<T>(itemsStream, cancellationToken: cancellationToken).ConfigureAwait(false))
                    {
                        if (item is not null)
                        {
                            yield return item;
                        }
                    }
                }
                else
                {
                    await foreach (var item in JsonSerializer.DeserializeAsyncEnumerable(itemsStream, jsonTypeInfo, cancellationToken).ConfigureAwait(false))
                    {
                        if (item is not null)
                        {
                            yield return item;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Extracts pagination metadata from the buffered content.
    /// </summary>
    private static async ValueTask<Pagination> ExtractPaginationAsync(
        StreamState streamState,
        JsonSerializerOptions? options,
        CancellationToken cancellationToken)
    {
        var (buffer, length) = await streamState.GetBufferAsync().ConfigureAwait(false);

        if (length == 0)
        {
            return Pagination.FromTotalCount(0);
        }

        try
        {
            var reader = new Utf8JsonReader(new ReadOnlySpan<byte>(buffer, 0, length), isFinalBlock: true, default);

            if (!reader.Read())
            {
                return Pagination.FromTotalCount(0);
            }

            if (reader.TokenType == JsonTokenType.StartArray)
            {
                // Top-level array - count items efficiently
                int itemCount = 0;
                int depth = 1;

                while (depth > 0 && reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.StartArray || reader.TokenType == JsonTokenType.StartObject)
                    {
                        if (depth == 1 && reader.TokenType == JsonTokenType.StartObject)
                        {
                            itemCount++;
                        }
                        depth++;
                    }
                    else if (reader.TokenType == JsonTokenType.EndArray || reader.TokenType == JsonTokenType.EndObject)
                    {
                        depth--;
                    }
                }

                return Pagination.FromTotalCount(itemCount);
            }
            else if (reader.TokenType == JsonTokenType.StartObject)
            {
                Pagination pagination = Pagination.Empty;
                int itemCount = 0;
                bool foundItems = false;

                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.PropertyName)
                    {
                        if (reader.ValueTextEquals("pagination"u8))
                        {
                            reader.Read(); // Move to value

                            if (reader.TokenType == JsonTokenType.StartObject)
                            {
                                long startPos = reader.TokenStartIndex;
                                reader.Skip();
                                long endPos = reader.BytesConsumed;
                                int paginationLength = (int)(endPos - startPos);

                                try
                                {
                                    var paginationSpan = new ReadOnlySpan<byte>(buffer, (int)startPos, paginationLength);

                                    // Try to get JsonTypeInfo for Pagination from options, or use default context
                                    if (options is not null)
                                    {
                                        JsonTypeInfo<Pagination>? paginationTypeInfo = options.TryGetTypeInfo(typeof(Pagination), out JsonTypeInfo? typeInfo)
                                            ? typeInfo as JsonTypeInfo<Pagination>
                                            : null;

                                        if (paginationTypeInfo is not null)
                                        {
                                            pagination = JsonSerializer.Deserialize(paginationSpan, paginationTypeInfo);
                                        }
                                        else
                                        {
                                            // Fallback to source generation context
                                            pagination = JsonSerializer.Deserialize(paginationSpan, PaginationSourceGenerationContext.Default.Pagination);
                                        }
                                    }
                                    else
                                    {
                                        pagination = JsonSerializer.Deserialize(paginationSpan, PaginationSourceGenerationContext.Default.Pagination);
                                    }
                                }
                                catch (JsonException)
                                {
                                    // Continue with empty pagination
                                }
                            }
                        }
                        else if (reader.ValueTextEquals("items"u8))
                        {
                            foundItems = true;
                            reader.Read(); // Move to value

                            if (reader.TokenType == JsonTokenType.StartArray)
                            {
                                // Count items efficiently
                                int depth = 1;
                                while (depth > 0 && reader.Read())
                                {
                                    if (reader.TokenType == JsonTokenType.StartArray || reader.TokenType == JsonTokenType.StartObject)
                                    {
                                        if (depth == 1 && reader.TokenType == JsonTokenType.StartObject)
                                        {
                                            itemCount++;
                                        }
                                        depth++;
                                    }
                                    else if (reader.TokenType == JsonTokenType.EndArray || reader.TokenType == JsonTokenType.EndObject)
                                    {
                                        depth--;
                                    }
                                }
                            }
                        }
                        else
                        {
                            reader.Skip();
                        }
                    }
                }

                // If no explicit pagination, compute from items
                if (pagination.Equals(Pagination.Empty) && foundItems)
                {
                    pagination = Pagination.FromTotalCount(itemCount);
                }

                return pagination;
            }

            return Pagination.FromTotalCount(0);
        }
        catch (JsonException)
        {
            return Pagination.FromTotalCount(0);
        }
    }

    internal static ValueTask<Stream> GetContentStreamAsync(HttpContent content, CancellationToken cancellationToken)
    {
        Task<Stream> task = ReadHttpContentStreamAsync(content, cancellationToken);

        return GetEncoding(content) is Encoding sourceEncoding && sourceEncoding != Encoding.UTF8
            ? GetTranscodingStreamAsync(task, sourceEncoding)
            : new(task);
    }

    private static Task<Stream> ReadHttpContentStreamAsync(HttpContent content, CancellationToken cancellationToken)
    {
        return content.ReadAsStreamAsync(cancellationToken);
    }

    private static async ValueTask<Stream> GetTranscodingStreamAsync(Task<Stream> task, Encoding sourceEncoding)
    {
        Stream contentStream = await task.ConfigureAwait(false);
        return Encoding.CreateTranscodingStream(contentStream, innerStreamEncoding: sourceEncoding, outerStreamEncoding: Encoding.UTF8);
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
    private static Encoding? GetEncoding(HttpContent content)
    {
        string? charset = content.Headers.ContentType?.CharSet;
        if (string.IsNullOrEmpty(charset))
        {
            return null;
        }

        try
        {
            return Encoding.GetEncoding(charset);
        }
        catch
        {
            return null;
        }
    }
}