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
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using Xpandables.Net.Async;
using Xpandables.Net.Text;

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
        /// Reads the HTTP content as a paged asynchronous sequence of JSON values of type T.
        /// </summary>
        /// <remarks>The method supports JSON content that is either a top-level array or an object
        /// containing a items array. The returned enumerable allows for efficient, asynchronous iteration over large or
        /// paged JSON responses. The method does not advance the HTTP content stream; it reads the entire content into
        /// memory before deserialization.</remarks>
        /// <typeparam name="T">The type of elements to deserialize from the JSON content.</typeparam>
        /// <param name="jsonTypeInfo">Metadata used to control the deserialization of JSON values to type T. Cannot be null.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests. The default value is None.</param>
        /// <returns>An asynchronous paged enumerable that yields deserialized values of type T from the JSON content. The
        /// sequence will be empty if the content is empty or does not contain any matching items.</returns>
        public IAsyncPagedEnumerable<T> ReadFromJsonAsAsyncPagedEnumerable<T>(
            JsonTypeInfo<T> jsonTypeInfo,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(content);
            ArgumentNullException.ThrowIfNull(jsonTypeInfo);

            return new AsyncPagedEnumerable<T>(Iterator(cancellationToken), PaginationFactory);

            async ValueTask<byte[]> ReadBufferAsync(CancellationToken ct)
            {
                var bytes = await content.ReadAsByteArrayAsync(ct).ConfigureAwait(false);
                return bytes ?? [];
            }

            async ValueTask<Pagination> PaginationFactory(CancellationToken ct)
            {
                var buffer = await ReadBufferAsync(ct).ConfigureAwait(false);
                if (buffer.Length == 0)
                    return Pagination.FromTotalCount(0);

                using var doc = JsonDocument.Parse(buffer);
                var root = doc.RootElement;
                bool caseInsensitive = jsonTypeInfo.Options.PropertyNameCaseInsensitive;

                if (root.ValueKind == JsonValueKind.Object)
                {
                    var maybe = TryReadPagination(root, caseInsensitive);
                    if (maybe.HasValue)
                        return maybe.Value;

                    var items = TryGetDataArray(root, caseInsensitive);
                    int total = items?.GetArrayLength() ?? 0;
                    return Pagination.FromTotalCount(total);
                }

                if (root.ValueKind == JsonValueKind.Array)
                {
                    return Pagination.FromTotalCount(root.GetArrayLength());
                }

                return Pagination.FromTotalCount(0);
            }

            async IAsyncEnumerable<T> Iterator([EnumeratorCancellation] CancellationToken ct = default)
            {
                var buffer = await ReadBufferAsync(ct).ConfigureAwait(false);
                if (buffer.Length == 0)
                    yield break;

                using var doc = JsonDocument.Parse(buffer);
                var root = doc.RootElement;
                bool caseInsensitive = jsonTypeInfo.Options.PropertyNameCaseInsensitive;

                if (root.ValueKind == JsonValueKind.Array)
                {
                    foreach (var el in root.EnumerateArray())
                    {
                        ct.ThrowIfCancellationRequested();
                        yield return el.Deserialize(jsonTypeInfo)!;
                    }
                    yield break;
                }

                if (root.ValueKind == JsonValueKind.Object)
                {
                    var items = TryGetDataArray(root, caseInsensitive);
                    if (items is null)
                        yield break;

                    foreach (var el in items.Value.EnumerateArray())
                    {
                        ct.ThrowIfCancellationRequested();
                        yield return el.Deserialize(jsonTypeInfo)!;
                    }
                }
            }
        }

        /// <summary>
        /// Reads the HTTP content as a paged asynchronous sequence of JSON values of type T using true streaming,
        /// without loading the entire response into memory.
        /// </summary>
        /// <remarks>
        /// This method provides superior memory efficiency compared to <see cref="ReadFromJsonAsAsyncPagedEnumerable{T}(HttpContent, JsonTypeInfo{T}, CancellationToken)"/>
        /// by streaming and deserializing JSON content incrementally. It's ideal for large payloads or memory-constrained environments.
        /// The method supports both array-root JSON and object-with-items structures.
        /// The stream is read only once - pagination metadata is extracted during enumeration automatically.
        /// </remarks>
        /// <typeparam name="T">The type of elements to deserialize from the JSON content.</typeparam>
        /// <param name="jsonTypeInfo">Metadata used to control the deserialization of JSON values to type T. Cannot be null.</param>
        /// <param name="arrayPropertyName">
        /// The name of the property containing the array when JSON root is an object (e.g., "items", "data", "results").
        /// If null or empty, expects array-root JSON. Default is "items".
        /// </param>
        /// <param name="bufferSize">The size of the internal streaming buffer in bytes. Default is 16KB.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>
        /// An asynchronous paged enumerable that yields deserialized values of type T from the JSON content.
        /// The sequence will be empty if the content is empty or does not contain any matching items.
        /// </returns>
        public IAsyncPagedEnumerable<T> ReadFromJsonAsAsyncPagedEnumerableStreaming<T>(
            JsonTypeInfo<T> jsonTypeInfo,
            string? arrayPropertyName = "items",
            int bufferSize = 16 * 1024,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(content);
            ArgumentNullException.ThrowIfNull(jsonTypeInfo);

            // Shared state between pagination factory and iterator
            var sharedState = new StreamingState<T>(content, jsonTypeInfo, arrayPropertyName, bufferSize);

            return new AsyncPagedEnumerable<T>(
                StreamingIterator(sharedState, cancellationToken),
                StreamingPaginationFactory);

            async ValueTask<Pagination> StreamingPaginationFactory(CancellationToken ct)
            {
                // Initialize reader if not already done
                await sharedState.EnsureReaderInitializedAsync(ct).ConfigureAwait(false);

                // If enumeration hasn't started yet, extract pagination explicitly
                if (!sharedState.IsEnumerationStarted)
                {
                    var pagination = await sharedState.Reader!.ExtractPaginationAsync(ct).ConfigureAwait(false);
                    return pagination ?? Pagination.Empty;
                }

                // If enumeration has started, pagination was already extracted during ReadArrayAsync
                return sharedState.Reader!.GetPagination() ?? Pagination.Empty;
            }

            async IAsyncEnumerable<T> StreamingIterator(
                StreamingState<T> state,
                [EnumeratorCancellation] CancellationToken ct = default)
            {
                await using (state.ConfigureAwait(false))
                {
                    await state.EnsureReaderInitializedAsync(ct).ConfigureAwait(false);

                    state.IsEnumerationStarted = true;

                    // ReadArrayAsync will extract pagination automatically if it encounters it
                    await foreach (var item in state.Reader!.ReadArrayAsync<T>(state.ArrayPropertyName, ct).ConfigureAwait(false))
                    {
                        yield return item;
                    }
                }
            }
        }

        /// <summary>
        /// Reads the HTTP content as a paged asynchronous sequence of JSON values of type T using true streaming.
        /// </summary>
        /// <typeparam name="T">The type of elements to deserialize from the JSON content.</typeparam>
        /// <param name="options">The options to use for JSON deserialization. Cannot be null.</param>
        /// <param name="arrayPropertyName">
        /// The name of the property containing the array when JSON root is an object. Default is "items".
        /// </param>
        /// <param name="bufferSize">The size of the internal streaming buffer in bytes. Default is 16KB.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>An asynchronous paged enumerable containing the deserialized objects of type T.</returns>
        public IAsyncPagedEnumerable<T> ReadFromJsonAsAsyncPagedEnumerableStreaming<T>(
            JsonSerializerOptions options,
            string? arrayPropertyName = "items",
            int bufferSize = 16 * 1024,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(content);
            ArgumentNullException.ThrowIfNull(options);

            var jsonTypeInfo = (JsonTypeInfo<T>)options.GetTypeInfo(typeof(T));
            return ReadFromJsonAsAsyncPagedEnumerableStreaming(content, jsonTypeInfo, arrayPropertyName, bufferSize, cancellationToken);
        }
    }

    /// <summary>
    /// Shared state for streaming operations to ensure single-pass reading.
    /// </summary>
    /// <typeparam name="T">The type of elements being deserialized.</typeparam>
    private sealed class StreamingState<T> : IAsyncDisposable
    {
        private readonly HttpContent _content;
        private readonly JsonTypeInfo<T> _jsonTypeInfo;
        private readonly int _bufferSize;
        private readonly SemaphoreSlim _initLock = new(1, 1);
        private bool _readerInitialized;
        private bool _disposed;

        public string? ArrayPropertyName { get; }
        public Utf8JsonStreamReader? Reader { get; private set; }
        public bool IsEnumerationStarted { get; set; }

        [SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "<Pending>")]
        public StreamingState(
            HttpContent content,
            JsonTypeInfo<T> jsonTypeInfo,
            string? arrayPropertyName,
            int bufferSize)
        {
            _content = content;
            _jsonTypeInfo = jsonTypeInfo;
            ArrayPropertyName = arrayPropertyName;
            _bufferSize = bufferSize;
        }

        public async ValueTask EnsureReaderInitializedAsync(CancellationToken cancellationToken)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (_readerInitialized)
                return;

            await _initLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (_readerInitialized)
                    return;

                var stream = await _content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
                Reader = new Utf8JsonStreamReader(stream, _jsonTypeInfo.Options, _bufferSize, leaveOpen: false);
                _readerInitialized = true;
            }
            finally
            {
                _initLock.Release();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed)
                return;

            _disposed = true;

            if (Reader is not null)
            {
                await Reader.DisposeAsync().ConfigureAwait(false);
            }

            _initLock.Dispose();
        }
    }

    private static JsonElement? TryGetDataArray(JsonElement obj, bool caseInsensitive)
    {
        if (obj.TryGetProperty("items", out var items) && items.ValueKind == JsonValueKind.Array)
            return items;

        if (!caseInsensitive) return null;

        foreach (var prop in obj.EnumerateObject())
        {
            if (string.Equals(prop.Name, "items", StringComparison.OrdinalIgnoreCase) &&
                prop.Value.ValueKind == JsonValueKind.Array)
                return prop.Value;
        }
        return null;
    }

    private static Pagination? TryReadPagination(JsonElement obj, bool caseInsensitive)
    {
        if (!TryGetProperty(obj, "pagination", caseInsensitive, out var pc) || pc.Value.ValueKind != JsonValueKind.Object)
            return null;

        var ctx = Pagination.Empty;
        if (TryGetProperty(pc.Value, "pageSize", caseInsensitive, out var pageSize) &&
            pageSize.Value.ValueKind == JsonValueKind.Number &&
            pageSize.Value.TryGetInt32(out var ps))
            ctx = ctx with { PageSize = ps };

        if (TryGetProperty(pc.Value, "currentPage", caseInsensitive, out var currentPage) &&
            currentPage.Value.ValueKind == JsonValueKind.Number &&
            currentPage.Value.TryGetInt32(out var cp))
            ctx = ctx with { CurrentPage = cp };

        if (TryGetProperty(pc.Value, "totalCount", caseInsensitive, out var totalCount))
        {
            if (totalCount.Value.ValueKind == JsonValueKind.Number)
            {
                if (totalCount.Value.TryGetInt64(out var l)) ctx = ctx with { TotalCount = checked((int?)l) };
                else if (totalCount.Value.TryGetInt32(out var i)) ctx = ctx with { TotalCount = i };
            }
            else if (totalCount.Value.ValueKind == JsonValueKind.Null)
            {
                ctx = ctx with { TotalCount = null };
            }
        }

        if (TryGetProperty(pc.Value, "continuationToken", caseInsensitive, out var token))
        {
            if (token.Value.ValueKind == JsonValueKind.String)
                ctx = ctx with { ContinuationToken = token.Value.GetString() };
            else if (token.Value.ValueKind == JsonValueKind.Null)
                ctx = ctx with { ContinuationToken = null };
        }

        return ctx;
    }

    private static bool TryGetProperty(JsonElement obj, string name, bool caseInsensitive, [NotNullWhen(true)] out JsonElement? value)
    {
        if (obj.TryGetProperty(name, out var v))
        {
            value = v;
            return true;
        }

        if (caseInsensitive)
        {
            foreach (var prop in obj.EnumerateObject())
            {
                if (string.Equals(prop.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    value = prop.Value;
                    return true;
                }
            }
        }

        value = null;
        return false;
    }
}
