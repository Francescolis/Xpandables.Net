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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Text.Json;
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

    private Pagination? _cachedPagination;
    private IAsyncEnumerable<TValue>? _cachedItems;
    private bool _isDeserialized;
    
    // We need to keep the document alive if we are enumerating its elements
    private JsonDocument? _jsonDocument;    

    /// <inheritdoc/>
    public Pagination Pagination => _cachedPagination ?? Pagination.Empty;

    internal AsyncPagedEnumerableDeserializer(
        PipeReader pipeReader,
        JsonTypeInfo<TValue> itemTypeInfo,
        CancellationToken cancellationToken,
        PaginationStrategy strategy = PaginationStrategy.None)
    {
        _pipeReader = pipeReader;
        _itemTypeInfo = itemTypeInfo;
        _cancellationToken = cancellationToken;
        _strategy = strategy;
    }        

    /// <inheritdoc/>
    public IAsyncPagedEnumerable<TValue> WithStrategy(PaginationStrategy strategy)
    {
        // Return a new view sharing the same pipe reader.
        // Note: PipeReader is single-consumer. This assumes only one of the views will be enumerated.
        return new AsyncPagedEnumerableDeserializer<TValue>(
            _pipeReader,
            _itemTypeInfo,
            _cancellationToken,
            strategy);
    }

    /// <inheritdoc/>
    public IAsyncPagedEnumerator<TValue> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        // Combine tokens
        using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cancellationToken);
        var token = linkedTokenSource.Token;

        // We need to ensure deserialization has happened to get the source enumerator
        // Since GetAsyncEnumerator is synchronous, we wrap the lazy initialization in the async stream
        IAsyncEnumerator<TValue> sourceEnumerator = DeserializeAndEnumerateAsync(token).GetAsyncEnumerator(token);

        // We pass the linkedTokenSource to the enumerator so it can be disposed when the enumerator is disposed
        // However, AsyncPagedEnumerator doesn't own the CTS. We rely on the struct/class disposal chain.
        // To avoid leaking the CTS, we can register its disposal on the enumerator's disposal if possible,
        // or just rely on the caller to handle cancellation correctly. 
        // For simplicity and safety in this pattern, we don't attach the CTS to the enumerator directly 
        // but the cancellation token is passed through.
        
        // Note: The Pagination passed here is the "initial" state. 
        // If deserialization hasn't happened, it's Empty. 
        // The enumerator will need to access the updated pagination if it changes, 
        // but for this specific implementation, pagination is static once parsed.
        return AsyncPagedEnumerator.Create(
            sourceEnumerator, 
            _cachedPagination ?? Pagination.Empty, 
            _strategy, 
            token);
    }    

    /// <inheritdoc/>
    public async Task<Pagination> GetPaginationAsync(CancellationToken cancellationToken = default)
    {
        using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cancellationToken);
        await EnsureDeserializedAsync(linkedTokenSource.Token).ConfigureAwait(false);
        return _cachedPagination ?? Pagination.Empty;
    }        

    private async IAsyncEnumerable<TValue> DeserializeAndEnumerateAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await EnsureDeserializedAsync(cancellationToken).ConfigureAwait(false);

        if (_cachedItems is not null)
        {
            await foreach (var item in _cachedItems.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                yield return item;
            }
        }
    }        

    [MemberNotNull(nameof(_cachedItems), nameof(_cachedPagination))]
    private async ValueTask EnsureDeserializedAsync(CancellationToken cancellationToken)
    {
        if (_isDeserialized) 
        {
            Debug.Assert(_cachedItems is not null && _cachedPagination is not null);
            return;
        }

        try
        {
            using var _cachedDocument = await ReadJsonDocumentAsync(cancellationToken).ConfigureAwait(false);
            if(_cachedDocument is not null)
            {             
                if (_cachedPagination is null
                    && _cachedDocument.RootElement.TryGetProperty("pagination", out var paginationElement))
                {
                    _cachedPagination = paginationElement.Deserialize(PaginationJsonContext.Default.Pagination);
                }

                if (_cachedItems is null
                    && _cachedDocument.RootElement.TryGetProperty("items", out var itemsElement))
                {
                    if (itemsElement.ValueKind == JsonValueKind.Array)
                    {
                        _cachedItems = EnumerateJsonArray(itemsElement);
                    }
                }
            }
        }
        finally
        {
            _isDeserialized = true;
            _cachedPagination ??= Pagination.Empty;
            _cachedItems ??= AsyncEnumerable.Empty<TValue>();
            await _pipeReader.CompleteAsync().ConfigureAwait(false);
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
    private async IAsyncEnumerable<TValue> EnumerateJsonArray(JsonElement arrayElement)
    {
        // Yield to ensure we are async
        await Task.Yield();

        foreach (JsonElement element in arrayElement.EnumerateArray())
        {
            // AOT Safe: Use the pre-calculated JsonTypeInfo
            TValue? item = element.Deserialize(_itemTypeInfo);
            if (item is not null)
            {
                yield return item;
            }
        }
        
        // Dispose the document when enumeration is complete
        _jsonDocument?.Dispose();
        _jsonDocument = null;
    }        
}