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

namespace System.Collections.Generic;

/// <summary>
/// Provides an asynchronous enumerator for paged collections with pagination strategy support.
/// </summary>
/// <typeparam name="T">The element type being enumerated.</typeparam>
public sealed class AsyncPagedEnumerator<T> : IAsyncPagedEnumerator<T>
{
    private readonly IAsyncEnumerator<T>? _sourceEnumerator;
    private readonly CancellationToken _cancellationToken;
    private PaginationStrategy _strategy;
    private Pagination _pagination;
    private bool _disposed;
    private int _itemIndex;

    /// <inheritdoc/>
    public PaginationStrategy Strategy => _strategy;

    /// <inheritdoc/>
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
        _strategy = PaginationStrategy.None;
    }

    /// <summary>
    /// Initializes a new instance with a source enumerator.
    /// </summary>
    /// <param name="sourceEnumerator">The source enumerator to wrap.</param>
    /// <param name="pagination">The initial pagination context.</param>
    /// <param name="strategy">The pagination strategy to apply.</param>
    /// <param name="cancellationToken">The cancellation token to observe.</param>
    internal AsyncPagedEnumerator(
        IAsyncEnumerator<T> sourceEnumerator,
        Pagination pagination,
        PaginationStrategy strategy,
        CancellationToken cancellationToken)
    {
        _sourceEnumerator = sourceEnumerator;
        _cancellationToken = cancellationToken;
        _pagination = pagination;
        _strategy = strategy;
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

        UpdatePagination(_itemIndex);

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void UpdatePagination(int index)
    {
        if (_strategy == PaginationStrategy.None) return;

        if (_strategy == PaginationStrategy.PerItem)
        {
            _pagination = _pagination with
            {
                PageSize = _pagination.PageSize == 0 ? 1 : _pagination.PageSize,
                CurrentPage = index
            };
            return;
        }

        if (_strategy == PaginationStrategy.PerPage)
        {
            int pageSize = _pagination.PageSize;
            int currentPage = pageSize > 0 ? ((index - 1) / pageSize) + 1 : 1;
            _pagination = _pagination with { PageSize = pageSize, CurrentPage = currentPage };
        }
    }
}
