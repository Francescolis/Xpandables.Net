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

///// <summary>
///// Delegate used to map a source item to a result item with optional cancellation support.
///// </summary>
///// <typeparam name="TSource">Source item type.</typeparam>
///// <typeparam name="TResult">Result item type.</typeparam>
///// <param name="item">The source item.</param>
///// <param name="cancellationToken">A cancellation token.</param>
///// <returns>The mapped result, potentially asynchronously.</returns>
//public delegate ValueTask<TResult> PagedMap<in TSource, TResult>(
//    TSource item, CancellationToken cancellationToken);

/// <summary>
/// Represents an asynchronous, paged enumerable over a source sequence with optional mapping to a result sequence.
/// </summary>
/// <typeparam name="TSource">Source element type.</typeparam>
/// <typeparam name="TResult">Result element type.</typeparam>
/// <remarks>
/// Creates an AsyncPagedEnumerable with the provided source, pagination factory and mapper.
/// </remarks>
/// <param name="source">The async source sequence. Cannot be null.</param>
/// <param name="paginationFactory">Factory that provides pagination info. Cannot be null.</param>
/// <param name="mapper">Mapper to convert TSource to TResult. If null, identity mapping is used when TSource == TResult; otherwise an exception is thrown.</param>
/// <param name="buffer">Optional buffer for one-shot materialization.</param>
public sealed class AsyncPagedEnumerable<TSource, TResult>(
    IAsyncEnumerable<TSource> source,
    Func<CancellationToken, ValueTask<Pagination>> paginationFactory,
    Func<TSource, CancellationToken, ValueTask<TResult>>? mapper = null,
    AsyncPagedEnumerableBuffer<TResult>? buffer = null) : IAsyncPagedEnumerable<TResult>
{
    private readonly IAsyncEnumerable<TSource> _source = source ?? throw new ArgumentNullException(nameof(source));
    private readonly Func<CancellationToken, ValueTask<Pagination>> _paginationFactory = paginationFactory ?? throw new ArgumentNullException(nameof(paginationFactory));
    private readonly Func<TSource, CancellationToken, ValueTask<TResult>> _mapper = mapper ?? GetIdentityMapperOrThrow();
    private volatile Pagination? _cachedPagination;
    private volatile Task<Pagination>? _paginationTask;

    /// <summary>
    /// Creates an AsyncPagedEnumerable with a synchronous mapper.
    /// </summary>
    public AsyncPagedEnumerable(
        IAsyncEnumerable<TSource> source,
        Func<CancellationToken, ValueTask<Pagination>> paginationFactory,
        Func<TSource, TResult>? mapper = default,
        AsyncPagedEnumerableBuffer<TResult>? buffer = null)
        : this(
            source,
            paginationFactory,
            mapper is not null ? WrapSyncMapper(mapper) : GetIdentityMapperOrThrow(),
            buffer)
    { }

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
#pragma warning restore CS0420

    /// <inheritdoc />
    public IAsyncEnumerator<TResult> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        var items = buffer?.Items;
        if (items is not null)
        {
            return new AsyncPagedEnumerator<TResult>(items);
        }

        return EnumerateMappedAsync(_source, _mapper, cancellationToken).GetAsyncEnumerator(cancellationToken);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Task<Pagination> EnsurePaginationTask() => _paginationFactory(CancellationToken.None).AsTask();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Func<TSource, CancellationToken, ValueTask<TResult>> WrapSyncMapper(Func<TSource, TResult> mapper) =>
        (item, _) => new ValueTask<TResult>(mapper(item));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Func<TSource, CancellationToken, ValueTask<TResult>> GetIdentityMapperOrThrow()
    {
        if (typeof(TSource) == typeof(TResult))
        {
            return static (item, _) => new ValueTask<TResult>((TResult)(object)item!);
        }

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