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
/// Represents an asynchronous, paged enumerable over a source sequence with optional mapping to a result sequence.
/// </summary>
/// <typeparam name="TSource">Source element type.</typeparam>
/// <typeparam name="TResult">Result element type.</typeparam>
public sealed class AsyncPagedEnumerable<TSource, TResult> : IAsyncPagedEnumerable<TResult>
{
    private enum Mode { Normal, Prime }

    private readonly Mode _mode;

    // Normal-mode fields
    private readonly IAsyncEnumerable<TSource>? _source;
    private readonly Func<CancellationToken, ValueTask<Pagination>>? _paginationFactory;

    // Prime-mode fields
    private readonly IAsyncEnumerable<PrimedResult<TSource>>? _projected;
    private readonly int? _skip;
    private readonly int? _take;
    private readonly Func<CancellationToken, ValueTask<long>>? _fallbackTotalFactory;

    // Common
    private readonly Func<TSource, CancellationToken, ValueTask<TResult>> _mapper;
    private readonly AsyncPagedEnumerableBuffer<TResult>? _buffer;

    // Pagination state
    private volatile Pagination? _cachedPagination;
    private volatile Task<Pagination>? _paginationTask;

    // Prepared prime state (if GetPaginationAsync was called first)
    private IAsyncEnumerator<PrimedResult<TSource>>? _preparedEnumerator;
    private TSource _preparedFirstItem = default!;
    private bool _preparedHasFirstItem;
    private bool _preparedConsumed;

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncPagedEnumerable{TSource, TResult}"/> class,  which provides
    /// asynchronous, paginated enumeration over a data source in <see cref="Mode.Normal"/>.
    /// </summary>
    /// <param name="source">The asynchronous enumerable representing the data source to be paginated.  Cannot be <see langword="null"/>.</param>
    /// <param name="paginationFactory">A factory function that creates a <see cref="Pagination"/> object, which defines the pagination  behavior. This
    /// function is invoked with a <see cref="CancellationToken"/> to support cancellation.  Cannot be <see
    /// langword="null"/>.</param>
    /// <param name="mapper">An optional mapping function that transforms each item of type <typeparamref name="TSource"/>  into an item of
    /// type <typeparamref name="TResult"/>. The function is invoked with the source item  and a <see
    /// cref="CancellationToken"/>. If <see langword="null"/>, an identity mapper is used.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> or <paramref name="paginationFactory"/> is <see langword="null"/>.</exception>
    public AsyncPagedEnumerable(
        IAsyncEnumerable<TSource> source,
        Func<CancellationToken, ValueTask<Pagination>> paginationFactory,
        Func<TSource, CancellationToken, ValueTask<TResult>>? mapper = null)
    {
        _mode = Mode.Normal;
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _paginationFactory = paginationFactory ?? throw new ArgumentNullException(nameof(paginationFactory));
        _mapper = mapper ?? GetIdentityMapperOrThrow();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncPagedEnumerable{TSource, TResult}"/> class, enabling
    /// asynchronous enumeration of paged data with optional mapping and buffering in <see cref="Mode.Normal"/>.
    /// </summary>
    /// <remarks>This constructor allows for flexible configuration of asynchronous pagination, including the
    /// ability to map source items to a different type and to use a buffer for improved performance in certain
    /// scenarios.</remarks>
    /// <param name="source">The asynchronous sequence of items to be paginated.</param>
    /// <param name="paginationFactory">A factory function that creates a <see cref="Pagination"/> object, which defines the pagination behavior for the
    /// sequence. The function is invoked with a <see cref="CancellationToken"/> to support cancellation.</param>
    /// <param name="mapper">An optional function to transform items of type <typeparamref name="TSource"/> into items of type <typeparamref
    /// name="TResult"/>. If <c>null</c>, the items are not transformed.</param>
    public AsyncPagedEnumerable(
        IAsyncEnumerable<TSource> source,
        Func<CancellationToken, ValueTask<Pagination>> paginationFactory,
        Func<TSource, TResult>? mapper)
        : this(source, paginationFactory,
              mapper is not null ? WrapSyncMapper(mapper) : GetIdentityMapperOrThrow())
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncPagedEnumerable{TSource, TResult}"/> class,  which provides
    /// asynchronous, paginated enumeration over a sequence of items in <see cref="Mode.Prime"/>.
    /// </summary>
    /// <param name="projected">The source sequence of items, represented as an asynchronous enumerable of primed results. This sequence is used
    /// to generate the paginated results.</param>
    /// <param name="skip">The number of items to skip from the beginning of the sequence. If null, no items are skipped.</param>
    /// <param name="take">The maximum number of items to include in the paginated results. If null, all remaining items are included.</param>
    /// <param name="fallbackTotalFactory">A factory function that asynchronously computes the total number of items in the sequence,  used when the total
    /// count is not directly available.</param>
    /// <param name="mapper">An optional asynchronous mapping function that transforms each source item into a result item.  If null, an
    /// identity mapper is used, returning the source items as-is.</param>
    /// <param name="buffer">An optional buffer to store intermediate results for optimized enumeration. If null, no buffering is used.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="projected"/> or <paramref name="fallbackTotalFactory"/> is null.</exception>
    public AsyncPagedEnumerable(
        IAsyncEnumerable<PrimedResult<TSource>> projected,
        int? skip,
        int? take,
        Func<CancellationToken, ValueTask<long>> fallbackTotalFactory,
        Func<TSource, CancellationToken, ValueTask<TResult>>? mapper = null,
        AsyncPagedEnumerableBuffer<TResult>? buffer = null)
    {
        _mode = Mode.Prime;
        _projected = projected ?? throw new ArgumentNullException(nameof(projected));
        _skip = skip;
        _take = take;
        _fallbackTotalFactory = fallbackTotalFactory ?? throw new ArgumentNullException(nameof(fallbackTotalFactory));
        _mapper = mapper ?? GetIdentityMapperOrThrow();
        _buffer = buffer;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncPagedEnumerable{TSource, TResult}"/> class,  representing an
    /// asynchronous sequence of paged results with optional mapping and buffering in <see cref="Mode.Prime"/>.
    /// </summary>
    /// <remarks>This constructor allows for flexible configuration of asynchronous paging, including support
    /// for skipping,  taking, mapping, and buffering. The <paramref name="fallbackTotalFactory"/> is invoked only when
    /// the total  count of items is required but not already available.</remarks>
    /// <param name="projected">The asynchronous sequence of <see cref="PrimedResult{TSource}"/> items to be paged.</param>
    /// <param name="skip">The number of items to skip in the sequence. Specify <see langword="null"/> to skip no items.</param>
    /// <param name="take">The maximum number of items to take from the sequence. Specify <see langword="null"/> to take all items.</param>
    /// <param name="fallbackTotalFactory">A factory function that asynchronously computes the total number of items in the sequence,  used when the total
    /// count is not already available.</param>
    /// <param name="mapper">An optional function to transform items of type <typeparamref name="TSource"/> into items of type <typeparamref
    /// name="TResult"/>.  If <see langword="null"/>, the sequence will use an identity mapping.</param>
    /// <param name="buffer">An optional buffer to store intermediate results for improved performance during enumeration.  If <see
    /// langword="null"/>, no buffering is applied.</param>
    public AsyncPagedEnumerable(
        IAsyncEnumerable<PrimedResult<TSource>> projected,
        int? skip,
        int? take,
        Func<CancellationToken, ValueTask<long>> fallbackTotalFactory,
        Func<TSource, TResult>? mapper,
        AsyncPagedEnumerableBuffer<TResult>? buffer = null)
        : this(projected, skip, take, fallbackTotalFactory,
               mapper is not null ? WrapSyncMapper(mapper) : GetIdentityMapperOrThrow(),
               buffer)
    { }

    /// <inheritdoc/>
    public Pagination Pagination =>
        _cachedPagination ?? throw new InvalidOperationException(
            $"Pagination info is not yet available. Use {nameof(GetPaginationAsync)} for async access.");

#pragma warning disable CS0420
    /// <inheritdoc/>
    public async Task<Pagination> GetPaginationAsync(CancellationToken cancellationToken = default)
    {
        var cached = _cachedPagination;
        if (cached is not null) return cached;

        var existing = _paginationTask;
        if (existing is null)
        {
            Task<Pagination> created = _mode == Mode.Normal
                ? _paginationFactory!(cancellationToken).AsTask()
                : PrimeEnsurePaginationAsync(cancellationToken).AsTask();

            existing = Interlocked.CompareExchange(ref _paginationTask, created, null) ?? created;
        }

        var pagination = await existing.ConfigureAwait(false);
        Volatile.Write(ref _cachedPagination, pagination);
        return pagination;
    }
#pragma warning restore CS0420

    /// <inheritdoc/>
    public IAsyncEnumerator<TResult> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        // If a buffer was materialized earlier, stream from it.
        var items = _buffer?.Items;
        if (items is not null)
        {
            return new AsyncPagedEnumerator<TResult>(items);
        }

        if (_mode == Mode.Normal)
        {
            return EnumerateMappedAsync(_source!, _mapper, cancellationToken).GetAsyncEnumerator(cancellationToken);
        }

        // Prime mode:
        if (_preparedEnumerator is not null && !_preparedConsumed)
        {
            _preparedConsumed = true; // single pass
            return new PrimedProjectedMappingEnumerator(_preparedEnumerator, _preparedHasFirstItem ? _preparedFirstItem : default!, _preparedHasFirstItem, _mapper, cancellationToken);
        }

        // Lazy priming: compute pagination on first row, then continue streaming.
        return new LazyPrimingProjectedMappingEnumerator(this, _projected!.GetAsyncEnumerator(cancellationToken), _mapper, _skip, _take, cancellationToken);
    }

    // Prime-mode: ensure pagination and keep enumerator if possible so we don't re-enumerate
#pragma warning disable CS0420 // A reference to a volatile field will not be treated as volatile
    private async ValueTask<Pagination> PrimeEnsurePaginationAsync(CancellationToken ct)
    {
        // If already prepared by a prior GetPaginationAsync, return.
        if (_preparedEnumerator is not null || _preparedConsumed)
        {
            return _cachedPagination ?? Pagination.With(_skip, _take, totalCount: 0);
        }

        var e = _projected!.GetAsyncEnumerator(ct);
        if (await e.MoveNextAsync().ConfigureAwait(false))
        {
            var first = e.Current;
            _preparedEnumerator = e;
            _preparedFirstItem = first.Item;
            _preparedHasFirstItem = true;

            var pagination = Pagination.With(_skip, _take, first.Total);
            Volatile.Write(ref _cachedPagination, pagination);
            return pagination;
        }

        // Empty page; compute total via fallback
        await e.DisposeAsync().ConfigureAwait(false);
        var total = await _fallbackTotalFactory!(ct).ConfigureAwait(false);
        var emptyPagination = Pagination.With(_skip, _take, total);
        Volatile.Write(ref _cachedPagination, emptyPagination);

        // Provide an empty prepared enumerator to consume
        _preparedEnumerator = EmptyProjectedEnumerator.Instance;
        _preparedFirstItem = default!;
        _preparedHasFirstItem = false;

        return emptyPagination;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Func<TSource, CancellationToken, ValueTask<TResult>> WrapSyncMapper(Func<TSource, TResult> mapper) =>
        (item, _) => new ValueTask<TResult>(mapper(item));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Func<TSource, CancellationToken, ValueTask<TResult>> GetIdentityMapperOrThrow()
    {
        if (typeof(TSource) == typeof(TResult))
            return static (item, _) => new ValueTask<TResult>((TResult)(object)item!);

        throw new InvalidOperationException(
            $"A mapper must be provided when {typeof(TSource).Name} cannot be assigned to {typeof(TResult).Name}.");
    }

    private static async IAsyncEnumerable<TResult> EnumerateMappedAsync(
        IAsyncEnumerable<TSource> source,
        Func<TSource, CancellationToken, ValueTask<TResult>> mapper,
        [EnumeratorCancellation] CancellationToken ct)
    {
        await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
        {
            ct.ThrowIfCancellationRequested();
            var result = await mapper(item, ct).ConfigureAwait(false);
            yield return result;
        }
    }

    private sealed class PrimedProjectedMappingEnumerator(
        IAsyncEnumerator<PrimedResult<TSource>> source,
        TSource firstItem,
        bool hasFirst,
        Func<TSource, CancellationToken, ValueTask<TResult>> mapper,
        CancellationToken ct) : IAsyncEnumerator<TResult>
    {
        private bool _yieldedFirst = !hasFirst;
        public TResult Current { get; private set; } = default!;

        public async ValueTask<bool> MoveNextAsync()
        {
            if (!_yieldedFirst)
            {
                _yieldedFirst = true;
                Current = await mapper(firstItem, ct).ConfigureAwait(false);
                firstItem = default!;
                return true;
            }

            if (await source.MoveNextAsync().ConfigureAwait(false))
            {
                Current = await mapper(source.Current.Item, ct).ConfigureAwait(false);
                return true;
            }

            Current = default!;
            return false;
        }

        public ValueTask DisposeAsync() => source.DisposeAsync();
    }

    private sealed class LazyPrimingProjectedMappingEnumerator(
        AsyncPagedEnumerable<TSource, TResult> owner,
        IAsyncEnumerator<PrimedResult<TSource>> source,
        Func<TSource, CancellationToken, ValueTask<TResult>> mapper,
        int? skip,
        int? take,
        CancellationToken ct) : IAsyncEnumerator<TResult>
    {
        private bool _primed;
        public TResult Current { get; private set; } = default!;

        public async ValueTask<bool> MoveNextAsync()
        {
            if (!_primed)
            {
                if (await source.MoveNextAsync().ConfigureAwait(false))
                {
                    var first = source.Current;
                    Volatile.Write(ref owner._cachedPagination, Pagination.With(skip, take, first.Total));
                    _primed = true;
                    Current = await mapper(first.Item, ct).ConfigureAwait(false);
                    return true;
                }
                else
                {
                    // No rows; ensure pagination from fallback if not set
                    if (owner._cachedPagination is null)
                    {
                        var total = await owner._fallbackTotalFactory!(ct).ConfigureAwait(false);
                        Volatile.Write(ref owner._cachedPagination, Pagination.With(skip, take, total));
                    }
                    Current = default!;
                    return false;
                }
            }

            if (await source.MoveNextAsync().ConfigureAwait(false))
            {
                Current = await mapper(source.Current.Item, ct).ConfigureAwait(false);
                return true;
            }

            Current = default!;
            return false;
        }

        public ValueTask DisposeAsync() => source.DisposeAsync();
    }
#pragma warning restore CS0420 // A reference to a volatile field will not be treated as volatile
    private sealed class EmptyProjectedEnumerator : IAsyncEnumerator<PrimedResult<TSource>>
    {
        public static readonly EmptyProjectedEnumerator Instance = new();
        public PrimedResult<TSource> Current => default;
        public ValueTask<bool> MoveNextAsync() => ValueTask.FromResult(false);
        public ValueTask DisposeAsync() => default;
    }
}

/// <summary>
/// Shared immutable buffer for one-shot materialization.
/// </summary>
public sealed class AsyncPagedEnumerableBuffer<TItem>
{
    private IReadOnlyList<TItem>? _items;
    /// <summary>
    /// Gets the collection of items currently stored in the object.
    /// </summary>
    public IReadOnlyList<TItem>? Items => Volatile.Read(ref _items);

    /// <summary>
    /// Attempts to set the collection of items if it has not already been set.
    /// </summary>
    /// <param name="items">The collection of items to set. This parameter cannot be null.</param>
    /// <returns><see langword="true"/> if the collection was successfully set; otherwise, <see langword="false"/> if the
    /// collection was already set.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TrySet(IReadOnlyList<TItem> items) =>
        Interlocked.CompareExchange(ref _items, items, comparand: null) is null;
}

/// <summary>
/// Represents the result of an operation that includes an item and its associated total value.
/// </summary>
/// <remarks>This structure is immutable and is intended to encapsulate both the result of an operation and a
/// related total value, such as a count, sum, or other aggregate metric.</remarks>
/// <typeparam name="T">The type of the item included in the result.</typeparam>
/// <param name="Item"></param>
/// <param name="Total"></param>
public readonly record struct PrimedResult<T>(T Item, long Total);
