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
using System.Buffers;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Xpandables.Net.Collections.Generic;

/// <summary>
/// Provides extension methods for deserializing UTF-8 encoded JSON data into asynchronous paged enumerables using
/// System.Text.Json.
/// </summary>
/// <remarks>These extension methods enable efficient, asynchronous deserialization of large or streaming JSON
/// payloads into paged enumerables, supporting both PipeReader and Stream sources. The methods offer flexibility in
/// specifying serialization options or type metadata, and can handle top-level JSON values or collections. Use these
/// methods to process JSON data in a memory-efficient, non-blocking manner, especially when working with large datasets
/// or data streams.</remarks>
public static class JsonDeserializerExtensions
{
    /// <summary>
    /// Extension methods for the <see cref="JsonSerializer"/> class.
    /// </summary>  
    extension(JsonSerializer)
    {
        /// <summary>
        /// Deserializes a UTF-8 encoded JSON stream into an asynchronous paged enumerable of values of type
        /// <typeparamref name="TValue"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of elements to deserialize from the JSON stream.</typeparam>
        /// <param name="utf8Json">The <see cref="System.IO.Pipelines.PipeReader"/> containing the UTF-8 encoded JSON data to deserialize.</param>
        /// <param name="options">The <see cref="System.Text.Json.JsonSerializerOptions"/> to use for deserialization. Cannot be null.</param>
        /// <param name="cancellationToken">A <see cref="System.Threading.CancellationToken"/> that can be used to cancel the asynchronous operation.</param>
        /// <returns>An <see cref="IAsyncPagedEnumerable{TValue}"/> that asynchronously yields deserialized values from the JSON
        /// stream. The enumerable may be empty if the stream contains no items.</returns>
        public static IAsyncPagedEnumerable<TValue?> DeserializeAsyncPagedEnumerable<TValue>(
            PipeReader utf8Json,
            JsonSerializerOptions options,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(utf8Json);
            ArgumentNullException.ThrowIfNull(options);

            JsonTypeInfo<TValue> jsonTypeInfo = GetTypeInfo<TValue>(options);
            return DeserializeAsyncPagedEnumerable(utf8Json, jsonTypeInfo, cancellationToken);
        }

        /// <summary>
        /// Deserializes a UTF-8 encoded JSON stream into an asynchronous paged enumerable of values of type
        /// </summary>
        /// <typeparam name="TValue">The type of objects to deserialize from the JSON data.</typeparam>
        /// <param name="utf8Json">The pipe reader that provides the UTF-8 encoded JSON data to be deserialized.</param>
        /// <param name="jsonTypeInfo">Metadata used to control the deserialization of objects of type TValue.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>An asynchronous paged enumerable that yields deserialized objects of type TValue from the provided JSON
        /// data. If the input contains no data, the enumerable will be empty.</returns>
        public static IAsyncPagedEnumerable<TValue> DeserializeAsyncPagedEnumerable<TValue>(
            PipeReader utf8Json,
            JsonTypeInfo<TValue> jsonTypeInfo,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(utf8Json);
            ArgumentNullException.ThrowIfNull(jsonTypeInfo);

            return new PipeReaderPagedDeserializer<TValue>(
                utf8Json,
                jsonTypeInfo,
                cancellationToken);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static JsonTypeInfo<T> GetTypeInfo<T>(JsonSerializerOptions options)
    {
        return options.TryGetTypeInfo(typeof(T), out var typeInfo)
            ? (JsonTypeInfo<T>)typeInfo
            : throw new InvalidOperationException(
                $"The JsonSerializerOptions does not contain metadata for type {typeof(T)}. " +
                "Ensure that the options include a JsonTypeInfoResolver that can provide metadata for this type.");
    }

    private sealed class PipeReaderPagedDeserializer<TValue> : IAsyncPagedEnumerable<TValue>
    {
        private readonly PipeReader _pipeReader;
        private readonly JsonTypeInfo<TValue> _itemTypeInfo;
        private readonly CancellationToken _cancellationToken;
        private readonly JsonSerializerOptions _options;

        private Pagination? _cachedPagination;
        private IAsyncEnumerable<TValue>? _cachedItems;

        public Pagination Pagination => _cachedPagination ?? Pagination.Empty;

        internal PipeReaderPagedDeserializer(
            PipeReader pipeReader,
            JsonTypeInfo<TValue> itemTypeInfo,
            CancellationToken cancellationToken)
        {
            _pipeReader = pipeReader;
            _itemTypeInfo = itemTypeInfo;
            _cancellationToken = cancellationToken;
            _options = itemTypeInfo.Options;
        }

        public IAsyncEnumerator<TValue> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
#pragma warning disable CA2000 // Dispose objects before losing scope
            var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cancellationToken).Token;
#pragma warning restore CA2000 // Dispose objects before losing scope
            return DeserializeItemsAsync(linkedToken).GetAsyncEnumerator(linkedToken);
        }

        public async Task<Pagination> GetPaginationAsync(CancellationToken cancellationToken = default)
        {
#pragma warning disable CA2000 // Dispose objects before losing scope
            var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cancellationToken).Token;
#pragma warning restore CA2000 // Dispose objects before losing scope
            if (_cachedPagination is not null)
            {
                return _cachedPagination.HasValue ? _cachedPagination.Value : Pagination.Empty;
            }

            _cachedPagination = await ExtractPaginationAsync(linkedToken).ConfigureAwait(false);
            return _cachedPagination.HasValue ? _cachedPagination.Value : Pagination.Empty;
        }

        private async ValueTask<Pagination> ExtractPaginationAsync(CancellationToken cancellationToken)
        {
#pragma warning disable CA1031 // Do not catch general exception types
            try
            {
                using var jsonDocument = await ReadJsonDocumentAsync(cancellationToken).ConfigureAwait(false);

                if (jsonDocument is null || !jsonDocument.RootElement.TryGetProperty("pagination", out var paginationElement))
                {
                    return Pagination.Empty;
                }

                var pagination = JsonSerializer.Deserialize(
                    paginationElement.GetRawText(),
                    PaginationJsonContext.Default.Pagination);

                return pagination;
            }
            catch
            {
                return Pagination.Empty;
            }
#pragma warning restore CA1031 // Do not catch general exception types
        }

        private async IAsyncEnumerable<TValue> DeserializeItemsAsync(
           [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            if (_cachedItems is not null)
            {
                await foreach (var item in _cachedItems.WithCancellation(cancellationToken).ConfigureAwait(false))
                {
                    yield return item;
                }
                yield break;
            }

            JsonDocument? jsonDocument = null;
            try
            {
                jsonDocument = await ReadJsonDocumentAsync(cancellationToken).ConfigureAwait(false);

                if (jsonDocument is null || !jsonDocument.RootElement.TryGetProperty("items", out var itemsElement))
                {
                    // Cache empty enumerable
                    _cachedItems = AsyncEnumerable.Empty<TValue>();
                    yield break;
                }

                // Deserialize items as async enumerable directly from the JsonElement
                if (itemsElement.ValueKind == JsonValueKind.Array)
                {
                    // Store raw JSON for caching
                    string itemsJson = itemsElement.GetRawText();

                    // Create async enumerable that deserializes from the cached JSON
                    _cachedItems = DeserializeFromJsonAsync(itemsJson, cancellationToken);

                    await foreach (var item in _cachedItems.WithCancellation(cancellationToken).ConfigureAwait(false))
                    {
                        yield return item;
                    }
                }
                else
                {
                    _cachedItems = AsyncEnumerable.Empty<TValue>();
                }
            }
            finally
            {
                jsonDocument?.Dispose();
                await _pipeReader.CompleteAsync().ConfigureAwait(false);
            }
        }

        private async IAsyncEnumerable<TValue> DeserializeFromJsonAsync(
            string itemsJson,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            using var itemsStream = new MemoryStream(Encoding.UTF8.GetBytes(itemsJson));
            await foreach (var item in JsonSerializer.DeserializeAsyncEnumerable(
                itemsStream,
                _itemTypeInfo,
                cancellationToken).ConfigureAwait(false))
            {
                yield return item!;
            }
        }

        private async ValueTask<JsonDocument?> ReadJsonDocumentAsync(CancellationToken cancellationToken)
        {
#pragma warning disable CA1031 // Do not catch general exception types
            try
            {
                // Read all available bytes from PipeReader efficiently
                var result = await _pipeReader.ReadAsync(cancellationToken).ConfigureAwait(false);

                if (result.Buffer.Length == 0)
                {
                    _pipeReader.AdvanceTo(result.Buffer.End);
                    return null;
                }

                // Allocate buffer and copy sequence data
                byte[] bytes = new byte[result.Buffer.Length];
                result.Buffer.CopyTo(bytes);
                _pipeReader.AdvanceTo(result.Buffer.End);

                return JsonDocument.Parse(new ReadOnlyMemory<byte>(bytes), new JsonDocumentOptions
                {
                    AllowTrailingCommas = false,
                    CommentHandling = JsonCommentHandling.Disallow,
                    MaxDepth = 64
                });
            }
            catch
            {
                return null;
            }
#pragma warning restore CA1031 // Do not catch general exception types
        }
    }
}