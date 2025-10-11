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
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Xpandables.Net.Async;

/// <summary>
/// Provides extension methods for reading HTTP content as an asynchronous paged enumerable of JSON objects.
/// </summary>
/// <remarks>These extension methods enable efficient, asynchronous processing of large JSON payloads returned
/// from HTTP responses by exposing the items as an <see cref="IAsyncPagedEnumerable{T}"/>. This is particularly useful when working
/// with paged or streaming JSON APIs, as it allows consuming items incrementally without loading the entire response
/// into memory.</remarks>
[SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]
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
        /// <typeparam name="T">The type of objects to deserialize from the JSON content.</typeparam>
        /// <param name="options">The options to use for JSON deserialization. Cannot be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>An asynchronous paged enumerable containing the deserialized objects of type T.</returns>
        public IAsyncPagedEnumerable<T> ReadFromJsonAsAsyncPagedEnumerable<T>(
            JsonSerializerOptions options,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(content);
            ArgumentNullException.ThrowIfNull(options);

            var jsonTypeInfo = (JsonTypeInfo<T>)options.GetTypeInfo(typeof(T));
            return ReadFromJsonAsAsyncPagedEnumerable(content, jsonTypeInfo, cancellationToken);
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
        /// This method uses streaming deserialization via <see langword="JsonSerializer.DeserializeAsyncEnumerable{T}"/>
        /// to minimize memory allocation. Items are deserialized on-demand as they are enumerated. Pagination metadata
        /// is extracted from the JSON structure and cached for efficient access.
        /// </para>
        /// <para>
        /// Performance characteristics:
        /// - Streaming deserialization with minimal memory overhead
        /// - Items deserialized on-demand during enumeration
        /// - Pagination metadata extracted eagerly and cached
        /// - Comparable performance to System.Net.Http.Json.HttpContentJsonExtensions.ReadFromJsonAsAsyncEnumerable
        /// </para>
        /// </remarks>
        /// <typeparam name="T">The type of elements to deserialize from the JSON content.</typeparam>
        /// <param name="jsonTypeInfo">Metadata used to control the deserialization of JSON values to type T. Cannot be null.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests. The default value is None.</param>
        /// <returns>An asynchronous paged enumerable containing the deserialized objects of type T.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="jsonTypeInfo"/> is null.</exception>
        /// <exception cref="JsonException">Thrown when the JSON content is malformed or cannot be parsed.</exception>
        public IAsyncPagedEnumerable<T> ReadFromJsonAsAsyncPagedEnumerable<T>(
            JsonTypeInfo<T> jsonTypeInfo,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(content);
            ArgumentNullException.ThrowIfNull(jsonTypeInfo);

            // Create a lazy task that buffers the content once
            var bufferTask = new Lazy<Task<byte[]>>(async () =>
            {
                Stream contentStream = await GetContentStreamAsync(content, cancellationToken).ConfigureAwait(false);
                using var bufferedStream = new MemoryStream();
                await contentStream.CopyToAsync(bufferedStream, cancellationToken).ConfigureAwait(false);
                await contentStream.DisposeAsync().ConfigureAwait(false);
                return bufferedStream.ToArray();
            });

            // Create the async enumerable source for items
            IAsyncEnumerable<T> itemsSource = CreateItemsEnumerableAsync(bufferTask, jsonTypeInfo, cancellationToken);

            // Create the pagination factory
            Func<CancellationToken, ValueTask<Pagination>> paginationFactory = ct =>
                ExtractPaginationAsync(bufferTask, ct);

            // Return an AsyncPagedEnumerable that wraps both
            return new AsyncPagedEnumerable<T>(itemsSource, paginationFactory);
        }
    }

    /// <summary>
    /// Creates an async enumerable that streams items from the buffered content.
    /// </summary>
    private static async IAsyncEnumerable<T> CreateItemsEnumerableAsync<T>(
        Lazy<Task<byte[]>> bufferTask,
        JsonTypeInfo<T> jsonTypeInfo,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // Get the buffered content
        byte[] buffer = await bufferTask.Value.ConfigureAwait(false);

        // Handle empty buffer
        if (buffer.Length == 0)
        {
            yield break;
        }

        // Parse structure and extract items stream
        using var itemsStream = await ExtractItemsStreamAsync(buffer, cancellationToken).ConfigureAwait(false);

        // Don't try to deserialize empty streams
        if (itemsStream.Length == 0)
        {
            yield break;
        }

        // Stream deserialize the items
        await foreach (var item in JsonSerializer.DeserializeAsyncEnumerable(itemsStream, jsonTypeInfo, cancellationToken).ConfigureAwait(false))
        {
            if (item is not null)
            {
                yield return item;
            }
        }
    }

    /// <summary>
    /// Extracts the items stream from the buffered content.
    /// </summary>
    private static async ValueTask<Stream> ExtractItemsStreamAsync(
        byte[] buffer,
        CancellationToken cancellationToken)
    {
        JsonDocument? document;
        try
        {
            document = await JsonDocument.ParseAsync(new MemoryStream(buffer), default, cancellationToken).ConfigureAwait(false);
        }
        catch (JsonException)
        {
            return new MemoryStream();
        }

        using (document)
        {
            var root = document.RootElement;

            if (root.ValueKind == JsonValueKind.Array)
            {
                // Top-level array - return the whole buffer
                return new MemoryStream(buffer, writable: false);
            }
            else if (root.ValueKind == JsonValueKind.Object)
            {
                // Extract items array if exists
                if (root.TryGetProperty("items", out var itemsElement) &&
                    itemsElement.ValueKind == JsonValueKind.Array)
                {
                    // Create a new stream with just the items array
                    var itemsJson = itemsElement.GetRawText();
                    return new MemoryStream(Encoding.UTF8.GetBytes(itemsJson));
                }
            }

            // No items found
            return new MemoryStream();
        }
    }

    /// <summary>
    /// Extracts pagination metadata from the buffered content.
    /// </summary>
    private static async ValueTask<Pagination> ExtractPaginationAsync(
        Lazy<Task<byte[]>> bufferTask,
        CancellationToken cancellationToken)
    {
        // Get the buffered content
        byte[] buffer = await bufferTask.Value.ConfigureAwait(false);

        // Handle empty buffer
        if (buffer.Length == 0)
        {
            return Pagination.FromTotalCount(0);
        }

        JsonDocument? document;
        try
        {
            document = await JsonDocument.ParseAsync(new MemoryStream(buffer), default, cancellationToken).ConfigureAwait(false);
        }
        catch (JsonException)
        {
            return Pagination.FromTotalCount(0);
        }

        using (document)
        {
            var root = document.RootElement;

            if (root.ValueKind == JsonValueKind.Array)
            {
                // Top-level array
                return Pagination.FromTotalCount(root.GetArrayLength());
            }
            else if (root.ValueKind == JsonValueKind.Object)
            {
                // Try to extract pagination metadata
                if (root.TryGetProperty("pagination", out var paginationElement))
                {
                    try
                    {
                        return JsonSerializer.Deserialize(
                            paginationElement.GetRawText(),
                            PaginationSourceGenerationContext.Default.Pagination);
                    }
                    catch (JsonException)
                    {
                        // Fall through to compute from items
                    }
                }

                // Try to get items array to compute total count
                if (root.TryGetProperty("items", out var itemsElement) &&
                    itemsElement.ValueKind == JsonValueKind.Array)
                {
                    return Pagination.FromTotalCount(itemsElement.GetArrayLength());
                }

                // Object without items
                return Pagination.Empty;
            }

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