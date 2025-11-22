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
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization.Metadata;

namespace System.Text.Json;

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
        /// Deserializes a UTF-8 encoded JSON pipe reader into an asynchronous paged enumerable of values of type
        /// <typeparamref name="TValue"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of elements to deserialize from the JSON stream.</typeparam>
        /// <param name="utf8Json">The <see cref="PipeReader"/> containing the UTF-8 encoded JSON data to deserialize.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions"/> to use for deserialization. Cannot be null.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the asynchronous operation.</param>
        /// <returns>An <see cref="IAsyncPagedEnumerable{TValue}"/> that asynchronously yields deserialized values from the JSON
        /// stream. The enumerable may be empty if the stream contains no items.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="utf8Json"/> or <paramref name="options"/> is null.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
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
        /// Deserializes a UTF-8 encoded JSON pipe reader into an asynchronous paged enumerable of values of type
        /// </summary>
        /// <typeparam name="TValue">The type of objects to deserialize from the JSON data.</typeparam>
        /// <param name="utf8Json">The pipe reader that provides the UTF-8 encoded JSON data to be deserialized.</param>
        /// <param name="jsonTypeInfo">Metadata used to control the deserialization of objects of type TValue.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>An asynchronous paged enumerable that yields deserialized objects of type TValue from the provided JSON
        /// data. If the input contains no data, the enumerable will be empty.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="utf8Json"/> or <paramref name="jsonTypeInfo"/> is null.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
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

        /// <summary>
        /// Deserializes a UTF-8 encoded JSON stream into an asynchronous paged enumerable of elements of type TValue.
        /// </summary>
        /// <remarks>The returned enumerable reads and deserializes items from the stream as they are
        /// requested, enabling efficient processing of large or paged JSON datasets. The caller is responsible for
        /// disposing the stream when enumeration is complete.</remarks>
        /// <typeparam name="TValue">The type of elements to deserialize from the JSON stream.</typeparam>
        /// <param name="utf8Json">The stream containing UTF-8 encoded JSON data representing a paged collection of TValue elements. Must not
        /// be null.</param>
        /// <param name="options">The options to use when deserializing the JSON data. Must not be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous enumeration operation.</param>
        /// <returns>An asynchronous paged enumerable that yields deserialized TValue elements from the provided JSON stream.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="utf8Json"/> or <paramref name="options"/> is null.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
        public static IAsyncPagedEnumerable<TValue> DeserializeAsyncPagedEnumerable<TValue>(
            Stream utf8Json,
            JsonSerializerOptions options,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(utf8Json);
            ArgumentNullException.ThrowIfNull(options);

            var pipeReader = PipeReader.Create(utf8Json);
            JsonTypeInfo<TValue> jsonTypeInfo = GetTypeInfo<TValue>(options);

            return new PipeReaderPagedDeserializer<TValue>(
                pipeReader,
                jsonTypeInfo,
                cancellationToken);
        }

        /// <summary>
        /// Deserializes a UTF-8 encoded JSON stream into an asynchronous paged enumerable of values of the specified
        /// type.
        /// </summary>
        /// <remarks>The returned enumerable reads and deserializes data from the provided stream as pages
        /// are requested. The caller is responsible for disposing the stream when enumeration is complete.</remarks>
        /// <typeparam name="TValue">The type of elements to deserialize from the JSON stream.</typeparam>
        /// <param name="utf8Json">The stream containing UTF-8 encoded JSON data representing a paged collection of values. The stream must be
        /// readable and positioned at the start of the JSON content.</param>
        /// <param name="jsonTypeInfo">Metadata used to control the deserialization of elements of type <typeparamref name="TValue"/>.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests. Optional.</param>
        /// <returns>An <see cref="IAsyncPagedEnumerable{TValue}"/> that asynchronously yields deserialized values from the JSON
        /// stream in pages.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="utf8Json"/> or <paramref name="jsonTypeInfo"/> is null.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
        public static IAsyncPagedEnumerable<TValue> DeserializeAsyncPagedEnumerable<TValue>(
            Stream utf8Json,
            JsonTypeInfo<TValue> jsonTypeInfo,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(utf8Json);
            ArgumentNullException.ThrowIfNull(jsonTypeInfo);

            var pipeReader = PipeReader.Create(utf8Json);
            return new PipeReaderPagedDeserializer<TValue>(
                pipeReader,
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

        private Pagination? _cachedPagination;
        private IAsyncEnumerable<TValue>? _cachedItems;
        private bool _isDeserialized;

        public Pagination Pagination => _cachedPagination ?? Pagination.Empty;

        internal PipeReaderPagedDeserializer(
            PipeReader pipeReader,
            JsonTypeInfo<TValue> itemTypeInfo,
            CancellationToken cancellationToken)
        {
            _pipeReader = pipeReader;
            _itemTypeInfo = itemTypeInfo;
            _cancellationToken = cancellationToken;
        }

        public IAsyncEnumerator<TValue> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            using var linkedTokenSource = CancellationTokenSource
                .CreateLinkedTokenSource(cancellationToken, _cancellationToken);

            return DeserializeItemsAsync(linkedTokenSource.Token).GetAsyncEnumerator(linkedTokenSource.Token);
        }

        public async Task<Pagination> GetPaginationAsync(CancellationToken cancellationToken = default)
        {
            using var linkedTokenSource = CancellationTokenSource
                .CreateLinkedTokenSource(cancellationToken, _cancellationToken);

            await DeserializeCoreAsync(linkedTokenSource.Token).ConfigureAwait(false);

            return _cachedPagination.Value;
        }

        [MemberNotNull(nameof(_cachedItems), nameof(_cachedPagination))]
        private async ValueTask DeserializeCoreAsync(CancellationToken cancellationToken)
        {
#pragma warning disable CS8774 // Member must have a non-null value when exiting.
            if (_isDeserialized)
            {
                return;
            }

            _isDeserialized = true;

            try
            {
                using var _cachedDocument = await ReadJsonDocumentAsync(cancellationToken).ConfigureAwait(false);
#pragma warning restore CS8774 // Member must have a non-null value when exiting.

                if (_cachedDocument is null)
                {
                    _cachedItems = AsyncEnumerable.Empty<TValue>();
                    _cachedPagination = Pagination.Empty;
                    return;
                }

                if (_cachedPagination is null
                    && _cachedDocument.RootElement.TryGetProperty("pagination", out var paginationElement))
                {
                    _cachedPagination = JsonSerializer.Deserialize(
                        paginationElement.GetRawText(),
                        PaginationJsonContext.Default.Pagination);
                }

                if (_cachedItems is null
                    && _cachedDocument.RootElement.TryGetProperty("items", out var itemsElement))
                {
                    if (itemsElement.ValueKind == JsonValueKind.Array)
                    {
                        string itemsJson = itemsElement.GetRawText();
                        _cachedItems = DeserializeFromJsonAsync(itemsJson, cancellationToken);
                    }
                }
            }
            finally
            {
                _cachedItems ??= AsyncEnumerable.Empty<TValue>();
                _cachedPagination ??= Pagination.Empty;
                await _pipeReader.CompleteAsync().ConfigureAwait(false);
            }
        }

        private async IAsyncEnumerable<TValue> DeserializeItemsAsync([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await DeserializeCoreAsync(cancellationToken).ConfigureAwait(false);

            await foreach (var item in _cachedItems.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                yield return item;
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

        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        private async ValueTask<JsonDocument?> ReadJsonDocumentAsync(CancellationToken cancellationToken)
        {
            try
            {
                ReadResult result = await _pipeReader.ReadAsync(cancellationToken).ConfigureAwait(false);

                if (result.Buffer.Length == 0)
                {
                    _pipeReader.AdvanceTo(result.Buffer.End);
                    return null;
                }

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
        }
    }
}