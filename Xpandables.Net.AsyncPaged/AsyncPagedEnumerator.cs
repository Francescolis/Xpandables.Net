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
using System.Runtime.CompilerServices;

namespace Xpandables.Net.AsyncPaged;

/// <summary>
/// Provides an asynchronous enumerator for paged collections with pagination strategy support.
/// </summary>
/// <typeparam name="T">The element type being enumerated.</typeparam>
public sealed class AsyncPagedEnumerator<T> : IAsyncPagedEnumerator<T>
{
    private readonly IAsyncEnumerator<T>? _sourceEnumerator;
    private readonly CancellationToken _cancellationToken;
    private PaginationStrategy _strategy;
    private Func<int, Pagination, Pagination>? _strategyUpdater;
    private Pagination _pagination;
    private bool _disposed;
    private int _itemIndex;

    /// <inheritdoc/>
    public PaginationStrategy Strategy => _strategy;

    /// <inheritdoc/>
    public IAsyncPagedEnumerator<T> WithStrategy(PaginationStrategy strategy)
    {
        _strategy = strategy;
        _strategyUpdater = strategy switch
        {
            PaginationStrategy.None => static (_, p) => p,
            PaginationStrategy.PerItem => static (i, p) =>
                p with { PageSize = p.PageSize == 0 ? 1 : p.PageSize, CurrentPage = i },
            PaginationStrategy.PerPage => static (i, p) =>
            {
                int pageSize = p.PageSize;
                int currentPage = pageSize > 0 ? ((i - 1) / pageSize) + 1 : 1;
                return p with { PageSize = pageSize, CurrentPage = currentPage };
            }
            ,
            _ => static (_, p) => p
        };

        return this;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// The default value for <see cref="Strategy"/> is <see cref="PaginationStrategy.None"/>, 
    /// meaning no pagination strategy is applied unless explicitly set.
    /// </remarks>
    public ref readonly Pagination Pagination => ref _pagination;

    /// <summary>
    /// Gets the current element.
    /// </summary>
    public T Current { get; private set; } = default!;

    /// <summary>
    /// Initializes a new instance for an empty enumerator.
    /// </summary>
    /// <param name="pagination">The initial pagination context.</param>
    internal AsyncPagedEnumerator(Pagination pagination)
    {
        _sourceEnumerator = null;
        _cancellationToken = default;
        _pagination = pagination;
    }

    /// <summary>
    /// Initializes a new instance with a source enumerator.
    /// </summary>
    /// <param name="sourceEnumerator">The source enumerator to wrap.</param>
    /// <param name="pagination">The initial pagination context.</param>
    /// <param name="cancellationToken">The cancellation token to observe.</param>
    internal AsyncPagedEnumerator(
        IAsyncEnumerator<T> sourceEnumerator,
        Pagination pagination,
        CancellationToken cancellationToken)
    {
        _sourceEnumerator = sourceEnumerator;
        _cancellationToken = cancellationToken;
        _pagination = pagination;
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public async ValueTask<bool> MoveNextAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _cancellationToken.ThrowIfCancellationRequested();

        if (_sourceEnumerator is null)
        {
            Current = default!;
            return false;
        }

        if (!await _sourceEnumerator.MoveNextAsync().ConfigureAwait(false))
        {
            if (Strategy == PaginationStrategy.PerItem && _pagination.TotalCount is null)
            {
                _pagination = _pagination with { TotalCount = _itemIndex };
            }

            Current = default!;
            return false;
        }

        Current = _sourceEnumerator.Current;
        _itemIndex++;

        if (_strategyUpdater is not null)
        {
            _pagination = _strategyUpdater.Invoke(_itemIndex, _pagination);
        }

        return true;
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        if (_sourceEnumerator is not null)
        {
            await _sourceEnumerator.DisposeAsync().ConfigureAwait(false);
        }

        _disposed = true;
    }
}

/// <summary>
/// Provides factory methods for creating instances of <see cref="AsyncPagedEnumerator{T}"/>.
/// </summary>
/// <remarks>
/// This class simplifies the creation of asynchronous paged enumerators with cancellation token support 
/// and initial pagination context.
/// </remarks>
public static class AsyncPagedEnumerator
{
    /// <summary>
    /// Creates a new paged enumerator for the specified source.
    /// </summary>
    /// <typeparam name="T">The type of elements being enumerated.</typeparam>
    /// <param name="sourceEnumerator">The source enumerator to wrap.</param>
    /// <param name="pagination">The initial pagination context. If null, <see cref="Pagination.Empty"/> is used.</param>
    /// <param name="cancellationToken">The cancellation token to observe.</param>
    /// <returns>A new <see cref="AsyncPagedEnumerator{T}"/> instance.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static AsyncPagedEnumerator<T> Create<T>(
        IAsyncEnumerator<T> sourceEnumerator,
        Pagination? pagination = null,
        CancellationToken cancellationToken = default) =>
        new(sourceEnumerator, pagination ?? Pagination.Empty, cancellationToken);

    /// <summary>
    /// Creates an empty paged enumerator with no data.
    /// </summary>
    /// <typeparam name="T">The type of elements.</typeparam>
    /// <param name="pagination">The initial pagination context. If null, <see cref="Pagination.Empty"/> is used.</param>
    /// <returns>An empty <see cref="AsyncPagedEnumerator{T}"/> instance.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static AsyncPagedEnumerator<T> Empty<T>(Pagination? pagination = null) =>
        new(pagination ?? Pagination.Empty);
}