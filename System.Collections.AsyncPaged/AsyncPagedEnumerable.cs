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

namespace System.Collections.Generic;

/// <summary>
/// Represents an asynchronous, paged enumerable over a sequence with lazy pagination metadata computation.
/// </summary>
/// <typeparam name="T">The type of elements in the sequence.</typeparam>
public sealed class AsyncPagedEnumerable<T> : IAsyncPagedEnumerable<T>, IDisposable
{
    private readonly IAsyncEnumerable<T>? _source;
    private readonly IQueryable<T>? _queryable;
    private readonly Func<CancellationToken, ValueTask<Pagination>> _paginationFactory;
    private readonly PaginationStrategy _strategy;

    // Lazy materialization
    private List<T>? _materializedItems;
    private readonly SemaphoreSlim _materializationLock = new(1, 1);

    private volatile int _paginationState; // 0 = not started, 1 = computing, 2 = computed, 3 = faulted
    private Task<Pagination>? _paginationTask;
    private Exception? _paginationError;
    private Pagination _pagination; // backing store

    /// <inheritdoc/>
    public Pagination Pagination => _paginationState == 2 ? _pagination : Pagination.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncPagedEnumerable{T}"/> class with an async enumerable source.
    /// </summary>
    /// <remarks>
    /// This constructor is designed for scenarios where pagination metadata can be provided via a factory function.
    /// The pagination state is computed lazily when <see cref="GetPaginationAsync"/> is called.
    /// </remarks>
    /// <param name="source">The asynchronous enumerable representing the data source. Cannot be <see langword="null"/>.</param>
    /// <param name="paginationFactory">A factory function that creates <see cref="Pagination"/> metadata.
    /// If null, pagination will be inferred from the source that gets materialized.</param>
    /// <param name="strategy">The pagination strategy to apply.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> or <paramref name="paginationFactory"/> is null.</exception>
    internal AsyncPagedEnumerable(
        IAsyncEnumerable<T> source,
        Func<CancellationToken, ValueTask<Pagination>>? paginationFactory = default,
        PaginationStrategy strategy = PaginationStrategy.None)
    {
        ArgumentNullException.ThrowIfNull(source);

        _source = source;
        _paginationFactory = paginationFactory ?? AsyncEnumerablePaginationFactory(source);
        _pagination = Pagination.Empty;
        _strategy = strategy;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncPagedEnumerable{T}"/> class with a queryable source.
    /// </summary>
    /// <remarks>
    /// This constructor is designed for IQueryable sources where pagination metadata can be extracted from
    /// the query expression or via a custom factory.
    /// </remarks>
    /// <param name="query">The queryable data source. Cannot be <see langword="null"/>.</param>
    /// <param name="paginationFactory">A factory function that creates <see cref="Pagination"/> metadata. 
    /// If null, pagination will be inferred from the query.</param>
    /// <param name="strategy">The pagination strategy to apply.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="query"/> is null.</exception>
    internal AsyncPagedEnumerable(
        IQueryable<T> query,
        Func<CancellationToken, ValueTask<Pagination>>? paginationFactory = default,
        PaginationStrategy strategy = PaginationStrategy.None)
    {
        ArgumentNullException.ThrowIfNull(query);

        _queryable = query;
        _paginationFactory = paginationFactory ?? QueryablePaginationFactory(query);
        _pagination = Pagination.Empty;
        _strategy = strategy;
    }

    /// <inheritdoc/>
    public IAsyncPagedEnumerable<T> WithStrategy(PaginationStrategy strategy)
    {
        // Return a new instance with the updated strategy but sharing the same source definition.
        // Note: This creates a new view. If the source is an IAsyncEnumerable that cannot be iterated twice,
        // this should be used with caution. However, for IQueryable or List-based sources, it is safe.
        if (_source is not null)
        {
            return new AsyncPagedEnumerable<T>(_source, _paginationFactory, strategy);
        }
        
        if (_queryable is not null)
        {
            return new AsyncPagedEnumerable<T>(_queryable, _paginationFactory, strategy);
        }

        return this;
    }

    /// <inheritdoc/>
    public IAsyncPagedEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        Pagination initial = Pagination;

        var enumerator = (_source, _queryable) switch
        {
            (not null, _) when (_materializedItems is not null) => _materializedItems.ToAsyncEnumerable().GetAsyncEnumerator(cancellationToken),
            (not null, _) => _source.GetAsyncEnumerator(cancellationToken),
            (null, not null) => _queryable.ToAsyncEnumerable().GetAsyncEnumerator(cancellationToken),
            _ => AsyncPagedEnumerator.Empty<T>(initial)
        };

        return AsyncPagedEnumerator.Create(enumerator, initial,_strategy, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<Pagination> GetPaginationAsync(CancellationToken cancellationToken = default)
    {
        // Fast path if already computed.
        if (_paginationState == 2)
        {
            return Task.FromResult(_pagination);
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
                Pagination ctx = await _paginationFactory(ct).ConfigureAwait(false);
                _pagination = _pagination with
                {
                    PageSize = ctx.PageSize,
                    CurrentPage = ctx.CurrentPage,
                    ContinuationToken = ctx.ContinuationToken,
                    TotalCount = ctx.TotalCount
                };

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

    /// <inheritdoc/>
    public void Dispose() => _materializationLock.Dispose();

    private static Func<CancellationToken, ValueTask<Pagination>> QueryablePaginationFactory(IQueryable<T> queryable)
    {
        var (normalizedQuery, skip, take) = QueryPaginationNormalizer.Normalize(queryable);
        return async cancellationToken =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            long totalCountLong = normalizedQuery.LongCount();
            int? totalCount = totalCountLong switch
            {
                < 0 => null,
                > int.MaxValue => int.MaxValue,
                _ => (int)totalCountLong
            };

            int pageSize = take ?? 0;
            string? continuationToken = QueryPaginationNormalizer.ExtractWhereToken(normalizedQuery) is { } value
                ? $"cursor:{value}"
                : (skip, take) switch
                {
                    (not null and > 0, not null and > 0) => $"offset:{skip.Value + take.Value}",
                    _ => null
                };

            int currentPage = (take, skip) switch
            {
                (not null and > 0, not null and >= 0) => (skip.Value / take.Value) + 1,
                _ => pageSize > 0 ? 1 : 0
            };

            return Pagination.Create(pageSize, currentPage, continuationToken: continuationToken, totalCount);
        };
    }

    private Func<CancellationToken, ValueTask<Pagination>> AsyncEnumerablePaginationFactory(IAsyncEnumerable<T> source)
    {
        return async cancellationToken =>
        {
            await _materializationLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (_materializedItems is not null)
                {
                    int count = _materializedItems.Count;
                    return Pagination.Create(pageSize: count, currentPage: count > 0 ? 1 : 0, totalCount: count);
                }

                _materializedItems = await source.ToListAsync(cancellationToken).ConfigureAwait(false);
                int totalCount = _materializedItems.Count;

                return Pagination.Create(pageSize: totalCount, currentPage: totalCount > 0 ? 1 : 0, totalCount: totalCount);
            }
            finally
            {
                _materializationLock.Release();
            }
        };
    }

}
