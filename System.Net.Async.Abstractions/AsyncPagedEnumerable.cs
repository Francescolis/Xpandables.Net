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
using System.Net.Async;
using System.Net.Async.Internals;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

namespace System.Net.Async;

/// <summary>
/// Represents an asynchronous, paged enumerable over a source sequence with optional mapping to a result sequence.
/// </summary>
/// <typeparam name="TSource">Source element type.</typeparam>
/// <typeparam name="TResult">Result element type.</typeparam>
public sealed class AsyncPagedEnumerable<TSource, TResult> : IAsyncPagedEnumerable<TResult>
{
    private readonly IAsyncEnumerable<TSource>? _source;
    private readonly IQueryable<TSource>? _queryable;
    private readonly Func<CancellationToken, ValueTask<PageContext>>? _paginationFactory;
    private readonly Func<CancellationToken, ValueTask<long>>? _totalFactory;
    private readonly Func<TSource, CancellationToken, ValueTask<TResult>>? _mapper;

    private volatile int _paginationState; // 0 = not started, 1 = computing, 2 = computed, 3 = faulted
    private Task<PageContext>? _paginationTask;
    private Exception? _paginationError;
    private PageContext _pageContext; // backing store

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncPagedEnumerable{TSource, TResult}"/> class,  which provides
    /// asynchronous enumeration over a paginated data source.
    /// </summary>
    /// <remarks>This class is designed to handle paginated data sources, where data is fetched in chunks
    /// (pages)  and processed asynchronously. The <paramref name="paginationFactory"/> is used to initialize the 
    /// pagination state, and the optional <paramref name="mapper"/> allows for transforming the data  during
    /// enumeration.</remarks>
    /// <param name="source">The asynchronous enumerable representing the data source to be paginated.  This parameter cannot be <see
    /// langword="null"/>.</param>
    /// <param name="paginationFactory">A factory function that creates a <see cref="PageContext"/> object, which defines the initial  pagination state.
    /// This parameter cannot be <see langword="null"/>.</param>
    /// <param name="mapper">An optional asynchronous mapping function that transforms each item of type <typeparamref name="TSource"/>  into
    /// an item of type <typeparamref name="TResult"/>. If <see langword="null"/>, the items are returned  without
    /// transformation.</param>
    public AsyncPagedEnumerable(
        IAsyncEnumerable<TSource> source,
        Func<CancellationToken, ValueTask<PageContext>> paginationFactory,
        Func<TSource, CancellationToken, ValueTask<TResult>>? mapper = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(paginationFactory);

        _source = source;
        _paginationFactory = paginationFactory;
        _mapper = mapper;
        _pageContext = PageContext.Create(0, 0, null, null);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncPagedEnumerable{TSource, TResult}"/> class,  which provides
    /// asynchronous, paginated enumeration over a queryable data source.
    /// </summary>
    /// <param name="query">The queryable data source to enumerate. This parameter cannot be <see langword="null"/>.</param>
    /// <param name="totalFactory">An optional delegate that asynchronously calculates the total number of items in the data source.  If <see
    /// langword="null"/>, the total count will not be calculated.</param>
    /// <param name="mapper">An optional asynchronous mapping function that transforms each item in the data source  into the desired result
    /// type. If <see langword="null"/>, the items are returned as-is.</param>
    public AsyncPagedEnumerable(
        IQueryable<TSource> query,
        Func<CancellationToken, ValueTask<long>>? totalFactory = null,
        Func<TSource, CancellationToken, ValueTask<TResult>>? mapper = null)
    {
        ArgumentNullException.ThrowIfNull(query);

        _queryable = query;
        _totalFactory = totalFactory;
        _mapper = mapper;
        _pageContext = PageContext.Create(0, 0, null, null);
    }

    /// <inheritdoc/>
    public IAsyncEnumerator<TResult> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        // Always pass current (possibly empty) page context as initial snapshot.
        PageContext initial = _paginationState == 2 ? _pageContext : PageContext.Empty;

        return (_source, _queryable) switch
        {
            (not null, _) => CreateMappedEnumerator(_source.GetAsyncEnumerator(cancellationToken), initial, cancellationToken),
            (null, not null) => CreateMappedEnumerator(_queryable.ToAsyncEnumerable().GetAsyncEnumerator(cancellationToken), initial, cancellationToken),
            _ => AsyncPagedEnumerator.Empty<TSource, TResult>(initial)
        };
    }

    /// <inheritdoc/>
    public Task<PageContext> GetPageContextAsync(CancellationToken cancellationToken = default)
    {
        // Fast path if already computed.
        if (_paginationState == 2)
        {
            return Task.FromResult(_pageContext);
        }

        // If a previous attempt faulted, propagate.
        if (_paginationState == 3)
        {
            ExceptionDispatchInfo.Capture(_paginationError!).Throw();
        }

        // Try to start computation atomically.
        if (Interlocked.CompareExchange(ref _paginationState, 1, 0) == 0)
        {
            // We won the race: create the computation task and publish it with release semantics.
            var task = ComputeAndStoreAsync(cancellationToken);
            Volatile.Write(ref _paginationTask, task);
            return task;
        }

        // Another thread is or has already computed the page context.
        // Wait until the task reference becomes visible (handles publication reordering/race).
        var existing = Volatile.Read(ref _paginationTask);
        if (existing is null)
        {
            SpinWait sw = new();
            do
            {
                sw.SpinOnce();
                existing = Volatile.Read(ref _paginationTask);
            } while (existing is null);
        }
        return existing;

        async Task<PageContext> ComputeAndStoreAsync(CancellationToken ct)
        {
            try
            {
                PageContext ctx = await ComputePaginationAsync(ct).ConfigureAwait(false);
                _pageContext = ctx;
                _ = Interlocked.Exchange(ref _paginationState, 2);
                return ctx;
            }
            catch (Exception ex)
            {
                _paginationError = ex;
                _ = Interlocked.Exchange(ref _paginationState, 3);
                throw;
            }
        }
    }

    private async ValueTask<PageContext> ComputePaginationAsync(CancellationToken cancellationToken)
    {
        return (_paginationFactory, _queryable) switch
        {
            (not null, _) => await _paginationFactory(cancellationToken).ConfigureAwait(false),
            (null, not null) => await ComputeQueryablePaginationAsync(_queryable, cancellationToken).ConfigureAwait(false),
            _ => PageContext.Create(0, 0, null, null) // Fallback empty context if no metadata strategy available.
        };
    }

    private async ValueTask<PageContext> ComputeQueryablePaginationAsync(
        IQueryable<TSource> query,
        CancellationToken cancellationToken)
    {
        (int? skip, int? take) = QueryAnalyzer.ExtractSkipTake(query.Expression);

        long totalCountLong;
        if (_totalFactory is not null)
        {
            totalCountLong = await _totalFactory(cancellationToken).ConfigureAwait(false);
        }
        else
        {
            try
            {
                IQueryable<TSource> baseQuery = QueryAnalyzer.RemoveSkipTake(query);
                totalCountLong = baseQuery.LongCount();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "Cannot compute total count from query. Provide a totalFactory for complex queries or non-database sources.", ex);
            }
        }

        int? totalCount = totalCountLong switch
        {
            < 0 => null,
            > int.MaxValue => int.MaxValue, // clamp (or choose to set null)
            _ => (int)totalCountLong
        };

        int pageSize = take ?? 0;
        int currentPage =
            (take is not null && take.Value > 0 && skip is not null && skip.Value >= 0)
                ? (skip.Value / take.Value) + 1
                : (pageSize > 0 ? 1 : 0);

        return PageContext.Create(pageSize, currentPage, continuationToken: null, totalCount);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private IAsyncEnumerator<TResult> CreateMappedEnumerator(
        IAsyncEnumerator<TSource> sourceEnumerator,
        PageContext initialContext,
        CancellationToken cancellationToken)
    {
        return _mapper switch
        {
            not null => AsyncPagedEnumerator.Create(sourceEnumerator, _mapper, initialContext, cancellationToken),
            null when typeof(TSource) == typeof(TResult) =>
                AsyncPagedEnumerator.UnsafePassthrough<TSource, TResult>(sourceEnumerator, initialContext, cancellationToken),
            _ => throw new InvalidOperationException(
                $"No mapper provided and cannot convert from {typeof(TSource)} to {typeof(TResult)}.")
        };
    }
}