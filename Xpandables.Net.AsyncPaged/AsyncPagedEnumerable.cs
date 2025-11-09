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
using System.Runtime.ExceptionServices;

using Xpandables.Net.AsyncPaged.Internals;

namespace Xpandables.Net.AsyncPaged;

/// <summary>
/// Represents an asynchronous, paged enumerable over a sequence with lazy pagination metadata computation.
/// </summary>
/// <typeparam name="T">The type of elements in the sequence.</typeparam>
public sealed class AsyncPagedEnumerable<T> : IAsyncPagedEnumerable<T>
{
    private readonly IAsyncEnumerable<T>? _source;
    private readonly IQueryable<T>? _queryable;
    private readonly Func<CancellationToken, ValueTask<Pagination>>? _paginationFactory;
    private readonly Func<CancellationToken, ValueTask<long>>? _totalFactory;

    private volatile int _paginationState; // 0 = not started, 1 = computing, 2 = computed, 3 = faulted
    private Task<Pagination>? _paginationTask;
    private Exception? _paginationError;
    private Pagination _pageContext; // backing store

    /// <inheritdoc/>
    public Pagination Pagination => _paginationState == 2 ? _pageContext : Pagination.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncPagedEnumerable{T}"/> class with an async enumerable source.
    /// </summary>
    /// <remarks>
    /// This constructor is designed for scenarios where pagination metadata is provided via a factory function.
    /// The pagination state is computed lazily when <see cref="GetPaginationAsync"/> is called.
    /// </remarks>
    /// <param name="source">The asynchronous enumerable representing the data source. Cannot be <see langword="null"/>.</param>
    /// <param name="paginationFactory">A factory function that creates <see cref="Pagination"/> metadata. Cannot be <see langword="null"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> or <paramref name="paginationFactory"/> is null.</exception>
    public AsyncPagedEnumerable(
        IAsyncEnumerable<T> source,
        Func<CancellationToken, ValueTask<Pagination>> paginationFactory)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(paginationFactory);

        _source = source;
        _paginationFactory = paginationFactory;
        _pageContext = Pagination.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncPagedEnumerable{T}"/> class with a queryable source.
    /// </summary>
    /// <remarks>
    /// This constructor is designed for IQueryable sources where pagination metadata is extracted from 
    /// the query expression (Skip/Take) and total count is computed automatically or via a custom factory.
    /// </remarks>
    /// <param name="query">The queryable data source. Cannot be <see langword="null"/>.</param>
    /// <param name="totalFactory">An optional delegate to compute the total count. If <see langword="null"/>, 
    /// the count is computed automatically from the query.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="query"/> is null.</exception>
    public AsyncPagedEnumerable(
        IQueryable<T> query,
        Func<CancellationToken, ValueTask<long>>? totalFactory = null)
    {
        ArgumentNullException.ThrowIfNull(query);

        _queryable = query;
        _totalFactory = totalFactory;
        _pageContext = Pagination.Empty;
    }

    /// <inheritdoc/>
    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        Pagination initial = Pagination;

        var enumerator = (_source, _queryable) switch
        {
            (not null, _) => _source.GetAsyncEnumerator(cancellationToken),
            (null, not null) => _queryable.ToAsyncEnumerable().GetAsyncEnumerator(cancellationToken),
            _ => AsyncPagedEnumerator.Empty<T>(initial)
        };

        return AsyncPagedEnumerator.Create(enumerator, initial, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<Pagination> GetPaginationAsync(CancellationToken cancellationToken = default)
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

        async Task<Pagination> ComputeAndStoreAsync(CancellationToken ct)
        {
            try
            {
                Pagination ctx = await ComputePaginationAsync(ct).ConfigureAwait(false);
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

    private async ValueTask<Pagination> ComputePaginationAsync(CancellationToken cancellationToken)
    {
        return (_paginationFactory, _queryable) switch
        {
            (not null, _) => await _paginationFactory(cancellationToken).ConfigureAwait(false),
            (null, not null) => await ComputeQueryablePaginationAsync(_queryable, cancellationToken).ConfigureAwait(false),
            _ => Pagination.Empty
        };
    }

    private async ValueTask<Pagination> ComputeQueryablePaginationAsync(
        IQueryable<T> query,
        CancellationToken cancellationToken)
    {
        var (normalizedQuery, skip, take) = QueryPaginationNormalizer.Normalize(query);

        long totalCountLong;
        if (_totalFactory is not null)
        {
            totalCountLong = await _totalFactory(cancellationToken).ConfigureAwait(false);
        }
        else
        {
            try
            {
                totalCountLong = normalizedQuery.LongCount();
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
            > int.MaxValue => int.MaxValue,
            _ => (int)totalCountLong
        };

        int pageSize = take ?? 0;
        int currentPage =
            (take is not null && take.Value > 0 && skip is not null && skip.Value >= 0)
                ? (skip.Value / take.Value) + 1
                : (pageSize > 0 ? 1 : 0);

        return Pagination.Create(pageSize, currentPage, continuationToken: null, totalCount);
    }
}