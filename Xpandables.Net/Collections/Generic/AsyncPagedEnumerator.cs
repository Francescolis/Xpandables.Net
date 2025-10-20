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

using Xpandables.Net;

namespace Xpandables.Net.Collections.Generic;

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
    private int _itemIndex; // 1-based logical item counter

    /// <inheritdoc/>
    public PaginationStrategy Strategy => _strategy;

    /// <inheritdoc/>
    public IAsyncPagedEnumerator<T> WithStrategy(PaginationStrategy strategy)
    {
        _strategy = strategy;
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
    /// <param name="initialContext">The initial pagination context.</param>
    internal AsyncPagedEnumerator(Pagination initialContext)
    {
        _sourceEnumerator = null;
        _cancellationToken = default;
        _pagination = initialContext;
    }

    /// <summary>
    /// Initializes a new instance with a source enumerator.
    /// </summary>
    /// <param name="sourceEnumerator">The source enumerator to wrap.</param>
    /// <param name="initialContext">The initial pagination context.</param>
    /// <param name="cancellationToken">The cancellation token to observe.</param>
    internal AsyncPagedEnumerator(
        IAsyncEnumerator<T> sourceEnumerator,
        Pagination initialContext,
        CancellationToken cancellationToken)
    {
        _sourceEnumerator = sourceEnumerator;
        _cancellationToken = cancellationToken;
        _pagination = initialContext;
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public async ValueTask<bool> MoveNextAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_sourceEnumerator is null)
        {
            Current = default!;
            return false;
        }

        if (!await _sourceEnumerator.MoveNextAsync().ConfigureAwait(false))
        {
            // End of sequence: if PerItem strategy and no total count, finalize total
            if (_strategy == PaginationStrategy.PerItem && _pagination.TotalCount is null)
            {
                _pagination = _pagination.WithTotalCount(_itemIndex);
            }

            Current = default!;
            return false;
        }

        Current = _sourceEnumerator.Current;
        _itemIndex++;

        // Update page context based on strategy
        switch (_strategy)
        {
            case PaginationStrategy.None:
                // No changes to page context
                break;

            case PaginationStrategy.PerItem:
                {
                    int pageSize = _pagination.PageSize == 0 ? 1 : _pagination.PageSize;
                    _pagination = new Pagination
                    {
                        PageSize = pageSize,
                        CurrentPage = _itemIndex,
                        ContinuationToken = null,
                        // Preserve pre-existing total if already known; keep null otherwise until end
                        TotalCount = _pagination.TotalCount
                    };
                    break;
                }

            case PaginationStrategy.PerPage:
                {
                    int pageSize = _pagination.PageSize;
                    if (pageSize > 0)
                    {
                        int zeroBased = _itemIndex - 1;
                        int currentPage = (zeroBased / pageSize) + 1;
                        _pagination = new Pagination
                        {
                            PageSize = pageSize,
                            CurrentPage = currentPage,
                            ContinuationToken = null,
                            TotalCount = _pagination.TotalCount
                        };
                    }
                    break;
                }

            default:
                break;
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
    /// <param name="initialContext">The initial pagination context. If null, <see cref="Pagination.Empty"/> is used.</param>
    /// <param name="cancellationToken">The cancellation token to observe.</param>
    /// <returns>A new <see cref="AsyncPagedEnumerator{T}"/> instance.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static AsyncPagedEnumerator<T> Create<T>(
        IAsyncEnumerator<T> sourceEnumerator,
        Pagination? initialContext = null,
        CancellationToken cancellationToken = default) =>
        new(sourceEnumerator, initialContext ?? Pagination.Empty, cancellationToken);

    /// <summary>
    /// Creates an empty paged enumerator with no data.
    /// </summary>
    /// <typeparam name="T">The type of elements.</typeparam>
    /// <param name="initialContext">The initial pagination context. If null, <see cref="Pagination.Empty"/> is used.</param>
    /// <returns>An empty <see cref="AsyncPagedEnumerator{T}"/> instance.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static AsyncPagedEnumerator<T> Empty<T>(Pagination? initialContext = null) =>
        new(initialContext ?? Pagination.Empty);
}