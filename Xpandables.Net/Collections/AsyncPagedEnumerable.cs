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
using System.Runtime.CompilerServices;

namespace Xpandables.Net.Collections;

/// <summary>
/// Represents an asynchronous enumerable that supports pagination, allowing efficient retrieval of paged data.
/// </summary>
/// <remarks>This class provides functionality for consuming asynchronous data streams with pagination support. It
/// allows access to pagination metadata via the <see cref="Pagination"/> property or the asynchronous <see
/// cref="GetPaginationAsync"/> method. The enumerable can be iterated asynchronously using <see
/// cref="GetAsyncEnumerator(CancellationToken)"/>.</remarks>
/// <typeparam name="T">The type of elements in the enumerable.</typeparam>
public sealed class AsyncPagedEnumerable<T> : IAsyncPagedEnumerable<T>
{
    private readonly IAsyncEnumerable<T> _source;
    private readonly Func<CancellationToken, ValueTask<Pagination>> _paginationFactory;
    private readonly PageBuffer<T>? _buffer;

    private volatile Pagination? _cachedPagination;
    private volatile Task<Pagination>? _paginationTask;

    internal AsyncPagedEnumerable(
        IAsyncEnumerable<T> source,
        Func<CancellationToken, ValueTask<Pagination>> paginationFactory,
        PageBuffer<T>? buffer)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _paginationFactory = paginationFactory ?? throw new ArgumentNullException(nameof(paginationFactory));
        _buffer = buffer;
    }

    /// <inheritdoc />
    public Pagination Pagination
    {
        get
        {
            var cached = _cachedPagination;
            if (cached is not null)
            {
                return cached;
            }

            throw new InvalidOperationException(
                $"Pagination info is not yet available. Use {nameof(GetPaginationAsync)} for async access.");
        }
    }

#pragma warning disable CS0420 // A reference to a volatile field will not be treated as volatile
    /// <inheritdoc />
    public async Task<Pagination> GetPaginationAsync()
    {
        var cached = _cachedPagination;
        if (cached is not null)
        {
            return cached;
        }

        var existing = _paginationTask;
        if (existing is null)
        {
            var created = EnsurePaginationTask();
            existing = Interlocked.CompareExchange(ref _paginationTask, created, null) ?? created;
        }

        var pagination = await existing.ConfigureAwait(false);
        Volatile.Write(ref _cachedPagination, pagination);
        return pagination;
    }
#pragma warning restore CS0420 // A reference to a volatile field will not be treated as volatile

    /// <inheritdoc />
    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        var items = _buffer?.Items;
        if (items is not null)
        {
            return new AsyncPagedEnumerator<T>(items);
        }

        return _source.GetAsyncEnumerator(cancellationToken);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Task<Pagination> EnsurePaginationTask() => _paginationFactory(CancellationToken.None).AsTask();

    /// <summary>
    /// Shared immutable buffer for one-shot materialization.
    /// </summary>
    internal sealed class PageBuffer<TItem>
    {
        private IReadOnlyList<TItem>? _items;
        public IReadOnlyList<TItem>? Items => Volatile.Read(ref _items);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySet(IReadOnlyList<TItem> items) =>
            Interlocked.CompareExchange(ref _items, items, comparand: null) is null;
    }
}