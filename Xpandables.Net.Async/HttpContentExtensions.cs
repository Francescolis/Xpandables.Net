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
        /// Top-level array (pagination will be empty):
        /// <code>
        /// [ { ... }, { ... } ]
        /// </code>
        /// </description>
        /// </item>
        /// </list>
        /// <para>
        /// The method parses the entire response into a <see cref="JsonDocument"/> for efficient random access
        /// to both pagination metadata and items. The pagination metadata is extracted eagerly and cached,
        /// while items are deserialized on-demand during enumeration to minimize memory pressure.
        /// </para>
        /// <para>
        /// Performance characteristics:
        /// - Single HTTP read operation
        /// - Pagination metadata immediately available via <see cref="IAsyncPagedEnumerable{T}.Pagination"/>
        /// - Items deserialized lazily during enumeration
        /// - JSON document lifecycle managed automatically
        /// </para>
        /// </remarks>
        /// <typeparam name="T">The type of elements to deserialize from the JSON content.</typeparam>
        /// <param name="jsonTypeInfo">Metadata used to control the deserialization of JSON values to type T. Cannot be null.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests. The default value is None.</param>
        /// <returns>An asynchronous paged enumerable that yields deserialized values of type T from the JSON content. The
        /// sequence will be empty if the content is empty or does not contain any matching items.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="jsonTypeInfo"/> is null.</exception>
        /// <exception cref="JsonException">Thrown when the JSON content is malformed or cannot be parsed.</exception>
        public IAsyncPagedEnumerable<T> ReadFromJsonAsAsyncPagedEnumerable<T>(
            JsonTypeInfo<T> jsonTypeInfo,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(content);
            ArgumentNullException.ThrowIfNull(jsonTypeInfo);

            return new HttpContentAsyncPagedEnumerable<T>(content, jsonTypeInfo, cancellationToken);
        }
    }

    private sealed class HttpContentAsyncPagedEnumerable<T> : IAsyncPagedEnumerable<T>, IDisposable
    {
        private readonly HttpContent _content;
        private readonly JsonTypeInfo<T> _jsonTypeInfo;
        private readonly CancellationToken _cancellationToken;
        private readonly SemaphoreSlim _initLock = new(1, 1);
        
        private volatile bool _initialized;
        private volatile bool _disposed;
        private JsonDocument? _document;
        private Pagination _pagination = Pagination.Empty;
        private JsonElement _itemsElement;
        private ResponseStructure _structure = ResponseStructure.Unknown;

        public HttpContentAsyncPagedEnumerable(
            HttpContent content,
            JsonTypeInfo<T> jsonTypeInfo,
            CancellationToken cancellationToken)
        {
            _content = content;
            _jsonTypeInfo = jsonTypeInfo;
            _cancellationToken = cancellationToken;
        }

        public Type Type => typeof(T);

        public Pagination Pagination
        {
            get
            {
                // If not initialized, block and initialize synchronously for immediate access
                if (!_initialized && !_disposed)
                {
                    EnsureInitializedAsync(_cancellationToken).GetAwaiter().GetResult();
                }
                return _pagination;
            }
        }

        public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken ct = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            
            await EnsureInitializedAsync(ct).ConfigureAwait(false);

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_cancellationToken, ct);
            var token = linkedCts.Token;

            if (_document is null)
            {
                yield break;
            }

            switch (_structure)
            {
                case ResponseStructure.ObjectWithItems:
                    foreach (var item in _itemsElement.EnumerateArray())
                    {
                        token.ThrowIfCancellationRequested();
                        T? value = DeserializeElement(item);
                        if (value is not null)
                        {
                            yield return value;
                        }
                    }
                    break;

                case ResponseStructure.Array:
                    foreach (var item in _document.RootElement.EnumerateArray())
                    {
                        token.ThrowIfCancellationRequested();
                        T? value = DeserializeElement(item);
                        if (value is not null)
                        {
                            yield return value;
                        }
                    }
                    break;

                case ResponseStructure.SingleValue:
                    T? singleValue = DeserializeElement(_document.RootElement);
                    if (singleValue is not null)
                    {
                        yield return singleValue;
                    }
                    break;
            }
        }

        public async Task<Pagination> GetPaginationAsync(CancellationToken ct = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            
            await EnsureInitializedAsync(ct).ConfigureAwait(false);
            return _pagination;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _document?.Dispose();
            _initLock.Dispose();
        }

        private async Task EnsureInitializedAsync(CancellationToken ct)
        {
            if (_initialized)
            {
                return;
            }

            await _initLock.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                if (_initialized)
                {
                    return;
                }

                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_cancellationToken, ct);
                var token = linkedCts.Token;

                Stream stream = await _content.ReadAsStreamAsync(token).ConfigureAwait(false);
                
                try
                {
                    _document = await JsonDocument.ParseAsync(stream, default, token).ConfigureAwait(false);
                    ParseDocument(_document);
                }
                catch (JsonException)
                {
                    _document?.Dispose();
                    _document = null;
                    throw;
                }
                finally
                {
                    await stream.DisposeAsync().ConfigureAwait(false);
                }

                _initialized = true;
            }
            finally
            {
                _initLock.Release();
            }
        }

        private void ParseDocument(JsonDocument document)
        {
            var root = document.RootElement;

            if (root.ValueKind == JsonValueKind.Object)
            {
                // Try to extract pagination metadata
                if (root.TryGetProperty("pagination", out var paginationElement))
                {
                    _pagination = DeserializePagination(paginationElement);
                }

                // Try to get items array
                if (root.TryGetProperty("items", out var itemsElement) && 
                    itemsElement.ValueKind == JsonValueKind.Array)
                {
                    _structure = ResponseStructure.ObjectWithItems;
                    _itemsElement = itemsElement;
                }
                else
                {
                    // No items property, treat the whole object as a single value
                    _structure = ResponseStructure.SingleValue;
                }
            }
            else if (root.ValueKind == JsonValueKind.Array)
            {
                _structure = ResponseStructure.Array;
            }
            else
            {
                _structure = ResponseStructure.SingleValue;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Pagination DeserializePagination(JsonElement element)
        {
            try
            {
                return JsonSerializer.Deserialize(
                    element.GetRawText(),
                    PaginationSourceGenerationContext.Default.Pagination);
            }
            catch (JsonException)
            {
                return Pagination.Empty;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T? DeserializeElement(JsonElement element)
        {
            try
            {
                return JsonSerializer.Deserialize(element.GetRawText(), _jsonTypeInfo);
            }
            catch (JsonException)
            {
                return default;
            }
        }

        private enum ResponseStructure
        {
            Unknown,
            ObjectWithItems,
            Array,
            SingleValue
        }
    }
}