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

using Xpandables.Net.Async;

namespace Xpandables.Net.Async;

/// <summary>
/// Provides an asynchronous enumerator for paged collections with optional mapping from source to result type.
/// </summary>
/// <typeparam name="TSource">The source element type.</typeparam>
/// <typeparam name="TResult">The result element type.</typeparam>
public sealed class AsyncPagedEnumerator<TSource, TResult> : IAsyncPagedEnumerator<TResult>
{
    private readonly IAsyncEnumerator<TSource>? _sourceEnumerator;
    private readonly Func<TSource, CancellationToken, ValueTask<TResult>>? _mapper;
    private readonly CancellationToken _cancellationToken;
    private PageContextStrategy _strategy;
    private PageContext _pageContext;
    private bool _disposed;
    private int _itemIndex; // 1-based logical item counter

    /// <inheritdoc/>
    public void WithPageContextStrategy(PageContextStrategy strategy) => _strategy = strategy;

    /// <inheritdoc/>
    /// <remarks>
    /// The default value for <see cref="PageContextStrategy"/> is <see cref="PageContextStrategy.None"/>, meaning no pagination strategy is applied unless explicitly set.
    /// </remarks>
    public ref readonly PageContext PageContext => ref _pageContext;

    /// <summary>
    /// Gets the current element.
    /// </summary>
    public TResult Current { get; private set; } = default!;

    /// <summary>
    /// Empty enumerator constructor.
    /// </summary>
    internal AsyncPagedEnumerator(PageContext initialContext)
    {
        _sourceEnumerator = null;
        _mapper = null;
        _cancellationToken = default;
        _pageContext = initialContext;
    }

    internal AsyncPagedEnumerator(
        IAsyncEnumerator<TSource> sourceEnumerator,
        PageContext initialContext,
        CancellationToken cancellationToken)
    {
        _sourceEnumerator = sourceEnumerator;
        _cancellationToken = cancellationToken;
        _pageContext = initialContext;
    }

    internal AsyncPagedEnumerator(
        IAsyncEnumerator<TSource> sourceEnumerator,
        Func<TSource, CancellationToken, ValueTask<TResult>> mapper,
        PageContext initialContext,
        CancellationToken cancellationToken)
    {
        _sourceEnumerator = sourceEnumerator;
        _mapper = mapper;
        _cancellationToken = cancellationToken;
        _pageContext = initialContext;
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
            if (_strategy == PageContextStrategy.PerItem && _pageContext.TotalCount is null)
            {
                int total = _itemIndex;
                _pageContext = new PageContext
                {
                    PageSize = _pageContext.PageSize == 0 ? 1 : _pageContext.PageSize,
                    CurrentPage = _pageContext.CurrentPage,
                    ContinuationToken = null,
                    TotalCount = total
                };
            }

            Current = default!;
            return false;
        }

        Current = _mapper switch
        {
            not null => await _mapper(_sourceEnumerator.Current, _cancellationToken).ConfigureAwait(false),
            _ => (TResult)(object)_sourceEnumerator.Current!
        };

        _itemIndex++;

        // Update page context based on strategy
        switch (_strategy)
        {
            case PageContextStrategy.None:
                // No changes to page context
                break;
            case PageContextStrategy.PerItem:
                {
                    int pageSize = _pageContext.PageSize == 0 ? 1 : _pageContext.PageSize;
                    int currentPage = _itemIndex;
                    _pageContext = new PageContext
                    {
                        PageSize = pageSize,
                        CurrentPage = currentPage,
                        ContinuationToken = null,
                        // Preserve pre-existing total if already known; keep null otherwise until end
                        TotalCount = _pageContext.TotalCount
                    };
                    break;
                }

            case PageContextStrategy.PerPage:
                {
                    int pageSize = _pageContext.PageSize;
                    if (pageSize > 0)
                    {
                        int zeroBased = _itemIndex - 1;
                        int currentPage = (zeroBased / pageSize) + 1;
                        _pageContext = new PageContext
                        {
                            PageSize = pageSize,
                            CurrentPage = currentPage,
                            ContinuationToken = null,
                            TotalCount = _pageContext.TotalCount
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
/// Provides factory methods for creating instances of <see cref="AsyncPagedEnumerator{TSource, TResult}"/> to
/// facilitate asynchronous enumeration with paging support.
/// </summary>
/// <remarks>This class includes methods to create paged enumerators with optional mapping functions, cancellation
/// token support, and initial paging context. It is designed to simplify the creation of asynchronous paged enumerators
/// for scenarios involving data streams or collections that require paging.</remarks>
public static class AsyncPagedEnumerator
{
    /// <summary>
    /// Creates a new instance of <see cref="AsyncPagedEnumerator{T, T}"/> to enumerate items asynchronously with paging
    /// support.
    /// </summary>
    /// <typeparam name="TSource">The type of the items being enumerated.</typeparam>
    /// <param name="sourceEnumerator">The source enumerator providing the items to be paged.</param>
    /// <param name="initialContext">An optional <see cref="PageContext"/> specifying the initial paging context. If not provided, a default empty
    /// context is used.</param>
    /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> to observe while enumerating.</param>
    /// <returns>An <see cref="AsyncPagedEnumerator{T, T}"/> instance configured with the specified source enumerator,
    /// cancellation token, and paging context.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static AsyncPagedEnumerator<TSource, TSource> Create<TSource>(
        IAsyncEnumerator<TSource> sourceEnumerator,
        PageContext? initialContext = null,
        CancellationToken cancellationToken = default) =>
        new(sourceEnumerator, initialContext ?? PageContext.Empty, cancellationToken);

    /// <summary>
    /// Creates a new instance of the <see cref="AsyncPagedEnumerator{TSource, TResult}"/> class,  which asynchronously
    /// maps and enumerates paged data from the specified source.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source enumerator.</typeparam>
    /// <typeparam name="TResult">The type of the elements in the resulting enumeration.</typeparam>
    /// <param name="sourceEnumerator">The asynchronous enumerator that provides the source data.</param>
    /// <param name="asyncMapper">A function that asynchronously maps each element of the source data to the resulting type. The function takes a
    /// source element and a <see cref="CancellationToken"/> as parameters.</param>
    /// <param name="initialContext">An optional <see cref="PageContext"/> that specifies the initial paging context.  If not provided, an empty
    /// context is used.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the operation.</param>
    /// <returns>An <see cref="AsyncPagedEnumerator{TSource, TResult}"/> that asynchronously enumerates  the mapped paged data.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static AsyncPagedEnumerator<TSource, TResult> Create<TSource, TResult>(
        IAsyncEnumerator<TSource> sourceEnumerator,
        Func<TSource, CancellationToken, ValueTask<TResult>> asyncMapper,
        PageContext? initialContext = null,
        CancellationToken cancellationToken = default) =>
        new(sourceEnumerator, asyncMapper, initialContext ?? PageContext.Empty, cancellationToken);

    /// <summary>
    /// Creates a new instance of the <see cref="AsyncPagedEnumerator{TSource, TResult}"/> class      to asynchronously
    /// enumerate pages of data transformed by the specified mapping function.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source enumerator.</typeparam>
    /// <typeparam name="TResult">The type of the elements in the resulting enumeration.</typeparam>
    /// <param name="sourceEnumerator">The asynchronous enumerator that provides the source data.</param>
    /// <param name="syncMapper">A synchronous function that maps each source element to a result element.</param>
    /// <param name="initialContext">An optional <see cref="PageContext"/> that specifies the initial paging context. Defaults to an empty context if
    /// not provided.</param>
    /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> to observe while enumerating. Defaults to <see
    /// cref="CancellationToken.None"/>.</param>
    /// <returns>An <see cref="AsyncPagedEnumerator{TSource, TResult}"/> that asynchronously enumerates the transformed pages of
    /// data.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static AsyncPagedEnumerator<TSource, TResult> Create<TSource, TResult>(
        IAsyncEnumerator<TSource> sourceEnumerator,
        Func<TSource, TResult> syncMapper,
        PageContext? initialContext = null,
        CancellationToken cancellationToken = default) =>
        new(sourceEnumerator, (s, _) => ValueTask.FromResult(syncMapper(s)), initialContext ?? PageContext.Empty, cancellationToken);

    /// <summary>
    /// Creates an empty <see cref="AsyncPagedEnumerator{TSource, TResult}"/> instance.
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="initialContext">An optional initial <see cref="PageContext"/> to associate with the enumerator. If <paramref
    /// name="initialContext"/> is <see langword="null"/>, a default empty context is used.</param>
    /// <returns>An empty <see cref="AsyncPagedEnumerator{TSource, TResult}"/> instance with no data to enumerate.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static AsyncPagedEnumerator<TSource, TResult> Empty<TSource, TResult>(
        PageContext? initialContext = null) =>
        new(initialContext ?? PageContext.Empty);

    /// <summary>
    /// Wraps an existing enumerator when TSource == TResult without copying logic.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static IAsyncEnumerator<TResult> UnsafePassthrough<TSource, TResult>(
        IAsyncEnumerator<TSource> source,
        PageContext initialContext,
        CancellationToken cancellationToken) =>
        // We still need a paged enumerator wrapper to expose PageContext & strategy support.
        new AsyncPagedEnumerator<TSource, TResult>(source, initialContext, cancellationToken);
}