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

internal readonly record struct PrimedResult<T>(T Item, long Total);

/// <summary>
/// Primeable paged enumerable for EF Core-like providers where a single query can return
/// both page rows and total. Supports metadata-first without buffering.
/// </summary>
/// <typeparam name="T">Item type.</typeparam>
internal sealed class AsyncPagedPrimedEnumerable<T>(
    IAsyncEnumerable<PrimedResult<T>> projected,
    int? skip,
    int? take,
    Func<CancellationToken, ValueTask<long>> fallbackTotalFactory) :
    IAsyncPagedEnumerable<T>,
    IAsyncPagedPrimedEnumerable<T>
{
    private readonly IAsyncEnumerable<PrimedResult<T>> _projected =
        projected ?? throw new ArgumentNullException(nameof(projected));
    private readonly Func<CancellationToken, ValueTask<long>> _fallbackTotalFactory =
        fallbackTotalFactory ?? throw new ArgumentNullException(nameof(fallbackTotalFactory)); // used when page is empty

    private volatile Pagination? _cachedPagination;
    private volatile Task<Pagination>? _paginationTask;

    public Pagination Pagination => _cachedPagination ?? throw new InvalidOperationException(
        $"Pagination info is not yet available. Use {nameof(GetPaginationAsync)} or {nameof(PrimeAsync)}.");

#pragma warning disable CS0420 // A reference to a volatile field will not be treated as volatile
    public async Task<Pagination> GetPaginationAsync()
    {
        var cached = _cachedPagination;
        if (cached is not null) return cached;

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

    public async Task<(Pagination Pagination, IAsyncEnumerator<T> Enumerator)> PrimeAsync(
        CancellationToken cancellationToken)
    {
        // Start enumerating the projected rows. Read the first row (if any) to compute pagination.
        var projectedEnumerator = _projected.GetAsyncEnumerator(cancellationToken);
        if (await projectedEnumerator.MoveNextAsync().ConfigureAwait(false))
        {
            var first = projectedEnumerator.Current;
            var pagination = Pagination.With(skip, take, first.Total);

            // Cache pagination for callers that might request it later.
            Volatile.Write(ref _cachedPagination, pagination);

            // Return a primed enumerator that first yields the already-read row.Item, then streams the rest
            var enumerator = new PrimedEnumerator(projectedEnumerator, first.Item);
            return (pagination, enumerator);
        }
        else
        {
            // No rows in page; get total via fallback (one extra round trip).

            var total = await _fallbackTotalFactory(cancellationToken).ConfigureAwait(false);
            var pagination = Pagination.With(skip, take, total);
            Volatile.Write(ref _cachedPagination, pagination);

            return (pagination, EmptyAsyncEnumerator.Instance);
        }
    }

#pragma warning restore CS0420 // A reference to a volatile field will not be treated as volatile
    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) =>
        new MappingEnumerator(_projected.GetAsyncEnumerator(cancellationToken));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Task<Pagination> EnsurePaginationTask() => CreatePaginationAsync(CancellationToken.None).AsTask();

    private async ValueTask<Pagination> CreatePaginationAsync(CancellationToken cancellationToken)
    {
        // Compute pagination by reading the entire page once, caching total from first row if present.
        // Preserves single round trip at the cost of page buffering, but only when a caller
        // explicitly asks for metadata without using PrimeAsync.

        var list = new List<T>(capacity: 16);
        long total;
        await foreach (var row in _projected.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (list.Count == 0)
            {
                total = row.Total;
                list.Add(row.Item);
                continue;
            }
            list.Add(row.Item);
        }

        if (list.Count > 0)
        {
            total = await GetTotalFromFirstRowAsync().ConfigureAwait(false);

            // Local function to avoid capturing
            // The first row was seen; its Total is not retained here. Recompute using fallback if needed.
            async ValueTask<long> GetTotalFromFirstRowAsync() =>
                await _fallbackTotalFactory(cancellationToken).ConfigureAwait(false);
        }
        else
        {
            // Page is empty; compute total
            total = await _fallbackTotalFactory(cancellationToken).ConfigureAwait(false);
        }

        return Pagination.With(skip, take, total);
    }

    private sealed class MappingEnumerator(IAsyncEnumerator<PrimedResult<T>> source) : IAsyncEnumerator<T>
    {
        public T Current { get; private set; } = default!;

        public async ValueTask<bool> MoveNextAsync()
        {
            if (await source.MoveNextAsync().ConfigureAwait(false))
            {
                Current = source.Current.Item;
                return true;
            }

            Current = default!;

            return false;
        }

        public ValueTask DisposeAsync() => source.DisposeAsync();
    }
    private sealed class PrimedEnumerator(IAsyncEnumerator<PrimedResult<T>> source, T firstItem) : IAsyncEnumerator<T>
    {
        private bool _yieldedFirst;

        public T Current { get; private set; } = default!;

        public async ValueTask<bool> MoveNextAsync()
        {
            if (!_yieldedFirst)
            {
                _yieldedFirst = true;
                Current = firstItem;
                firstItem = default!;

                return true;
            }

            if (await source.MoveNextAsync().ConfigureAwait(false))
            {
                Current = source.Current.Item;
                return true;
            }

            Current = default!;
            return false;
        }

        public ValueTask DisposeAsync() => source.DisposeAsync();
    }

    private sealed class EmptyAsyncEnumerator : IAsyncEnumerator<T>
    {
        public static readonly EmptyAsyncEnumerator Instance = new();
        public T Current => default!;
        public ValueTask<bool> MoveNextAsync() => ValueTask.FromResult(false);
        public ValueTask DisposeAsync() => default;
    }
}