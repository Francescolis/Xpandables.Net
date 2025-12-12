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
using System.Text.Json.Serialization.Metadata;

namespace System.Text.Json;

/// <summary>
/// An implementation of <see cref="IAsyncPagedEnumerable{TValue}"/> that deserializes
/// paged data from a <see cref="PipeReader"/> using System.Text.Json.
/// </summary>
/// <typeparam name="TValue">The type of elements to deserialize.</typeparam>
public sealed class AsyncPagedEnumerableDeserializer<TValue> : IAsyncPagedEnumerable<TValue>
{
    private readonly PipeReader _pipeReader;
    private readonly JsonTypeInfo<TValue> _itemTypeInfo;
    private readonly CancellationToken _cancellationToken;
    private readonly PaginationStrategy _strategy;
    private readonly Lock _lock = new();

    private JsonDocument? _document;
    private Pagination _cachedPagination = Pagination.Empty;
    private JsonElement _itemsElement;
    private bool _isDeserialized;
    private bool _pipeReaderCompleted;

    /// <inheritdoc/>
    public Pagination Pagination => _cachedPagination;

    internal AsyncPagedEnumerableDeserializer(
        PipeReader pipeReader,
        JsonTypeInfo<TValue> itemTypeInfo,
        CancellationToken cancellationToken,
        PaginationStrategy strategy = PaginationStrategy.None)
    {
        ArgumentNullException.ThrowIfNull(pipeReader);
        ArgumentNullException.ThrowIfNull(itemTypeInfo);

        _pipeReader = pipeReader;
        _itemTypeInfo = itemTypeInfo;
        _cancellationToken = cancellationToken;
        _strategy = strategy;
    }

    /// <inheritdoc/>
    public IAsyncPagedEnumerable<TValue> WithStrategy(PaginationStrategy strategy) =>
        new AsyncPagedEnumerableDeserializer<TValue>(
            _pipeReader,
            _itemTypeInfo,
            _cancellationToken,
            strategy);

    /// <inheritdoc/>
    public IAsyncPagedEnumerator<TValue> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            _cancellationToken);

        GC.KeepAlive(linkedTokenSource);
        return new Enumerator(this, linkedTokenSource);
    }

    /// <inheritdoc/>
    public async Task<Pagination> GetPaginationAsync(CancellationToken cancellationToken = default)
    {
        await EnsureDeserializedAsync(cancellationToken).ConfigureAwait(false);
        return _cachedPagination;
    }

    private async ValueTask EnsureDeserializedAsync(CancellationToken cancellationToken)
    {
        if (_isDeserialized)
        {
            return;
        }

        // Use lock to prevent concurrent deserialization
        lock (_lock)
        {
            if (_isDeserialized)
            {
                return;
            }
        }

        // Read and parse outside the lock to avoid blocking
        var document = await ReadAndParseJsonAsync(cancellationToken).ConfigureAwait(false);

        lock (_lock)
        {
            if (_isDeserialized)
            {
                // Another thread completed deserialization; dispose our document
                document?.Dispose();
                return;
            }

            _document = document;

            if (_document is not null)
            {
                var root = _document.RootElement;

                if (root.TryGetProperty("pagination"u8, out var paginationElement))
                {
                    _cachedPagination = paginationElement.Deserialize(
                        PaginationJsonContext.Default.Pagination);
                }

                if (root.TryGetProperty("items"u8, out var itemsElement)
                    && itemsElement.ValueKind is JsonValueKind.Array)
                {
                    _itemsElement = itemsElement;
                }
            }

            _isDeserialized = true;
        }
    }

    private async ValueTask<JsonDocument?> ReadAndParseJsonAsync(CancellationToken cancellationToken)
    {
#pragma warning disable CA1031 // Do not catch general exception types
        try
        {
            // Use ArrayBufferWriter to accumulate data efficiently
            var bufferWriter = new ArrayBufferWriter<byte>();

            while (true)
            {
                var result = await _pipeReader
                    .ReadAsync(cancellationToken)
                    .ConfigureAwait(false);

                var buffer = result.Buffer;

                if (buffer.Length > 0)
                {
                    // Copy to our buffer
                    foreach (var segment in buffer)
                    {
                        bufferWriter.Write(segment.Span);
                    }
                }

                _pipeReader.AdvanceTo(buffer.End);

                if (result.IsCompleted)
                {
                    break;
                }
            }

            await CompletePipeReaderAsync().ConfigureAwait(false);

            if (bufferWriter.WrittenCount == 0)
            {
                return null;
            }

            return JsonDocument.Parse(
                bufferWriter.WrittenMemory,
                new JsonDocumentOptions
                {
                    AllowTrailingCommas = false,
                    CommentHandling = JsonCommentHandling.Disallow,
                    MaxDepth = 64
                });
        }
        catch (JsonException)
        {
            throw; // Re-throw JSON parsing errors for caller to handle
        }
        catch (OperationCanceledException)
        {
            throw; // Re-throw cancellation
        }
        catch
        {
            await CompletePipeReaderAsync().ConfigureAwait(false);
            return null;
        }
#pragma warning restore CA1031 // Do not catch general exception types
    }

    private async ValueTask CompletePipeReaderAsync()
    {
        if (_pipeReaderCompleted)
        {
            return;
        }

        _pipeReaderCompleted = true;
        await _pipeReader.CompleteAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Internal enumerator that manages the document lifetime and streams items.
    /// </summary>
    private sealed class Enumerator(
        AsyncPagedEnumerableDeserializer<TValue> parent,
        CancellationTokenSource linkedTokenSource) : IAsyncPagedEnumerator<TValue>
    {
        private readonly CancellationToken _token = linkedTokenSource.Token;
        private JsonElement.ArrayEnumerator _arrayEnumerator;
        private Pagination _pagination = Pagination.Empty;
        private int _itemIndex;
        private bool _initialized;
        private bool _disposed;

        /// <inheritdoc/>
        public TValue Current { get; private set; } = default!;

        /// <inheritdoc/>
        public ref readonly Pagination Pagination => ref _pagination;

        /// <inheritdoc/>
        public PaginationStrategy Strategy => parent._strategy;

        /// <inheritdoc/>
        public async ValueTask<bool> MoveNextAsync()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            _token.ThrowIfCancellationRequested();

            if (!_initialized)
            {
                await parent.EnsureDeserializedAsync(_token).ConfigureAwait(false);
                _pagination = parent._cachedPagination;
                _arrayEnumerator = parent._itemsElement.EnumerateArray();
                _initialized = true;
            }

            if (!_arrayEnumerator.MoveNext())
            {
                UpdatePaginationOnComplete();
                Current = default!;
                return false;
            }

            var element = _arrayEnumerator.Current;
            var item = element.Deserialize(parent._itemTypeInfo);

            if (item is null)
            {
                // Skip null items and try next
                return await MoveNextAsync().ConfigureAwait(false);
            }

            Current = item;
            _itemIndex++;
            UpdatePagination();

            return true;
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _arrayEnumerator.Dispose();
            linkedTokenSource.Dispose();

            // Dispose the document when enumeration is complete
            lock (parent._lock)
            {
                parent._document?.Dispose();
                parent._document = null;
            }

            await parent.CompletePipeReaderAsync().ConfigureAwait(false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdatePagination()
        {
            if (Strategy is PaginationStrategy.None)
            {
                return;
            }

            if (Strategy is PaginationStrategy.PerItem)
            {
                _pagination = _pagination with
                {
                    PageSize = _pagination.PageSize == 0 ? 1 : _pagination.PageSize,
                    CurrentPage = _itemIndex
                };
                return;
            }

            if (Strategy is PaginationStrategy.PerPage)
            {
                int pageSize = _pagination.PageSize;
                int currentPage = pageSize > 0 ? ((_itemIndex - 1) / pageSize) + 1 : 1;
                _pagination = _pagination with { CurrentPage = currentPage };
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdatePaginationOnComplete()
        {
            if (Strategy is PaginationStrategy.PerItem && _pagination.TotalCount is null)
            {
                _pagination = _pagination with { TotalCount = _itemIndex };
            }
        }
    }
}