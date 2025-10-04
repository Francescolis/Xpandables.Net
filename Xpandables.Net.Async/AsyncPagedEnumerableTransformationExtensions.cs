/*******************************************************************************
 * Copyright (C) 2025 Francis-Black EWANE
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

/*******************************************************************************
 * Copyright (C) 2025 Francis-Black EWANE
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

using Xpandables.Net.Async;
namespace Xpandables.Net.Async;

/// <summary>
/// Provides transformation extension methods for <see cref="IAsyncPagedEnumerable{TSource}"/>.
/// </summary>
[Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]
public static class AsyncPagedEnumerableTransformationExtensions
{
    /// <summary>
    /// Transformation operations over an <see cref="IAsyncPagedEnumerable{TSource}"/>.
    /// </summary>
    /// <typeparam name="TSource">The element type of the source sequence.</typeparam>
    /// <param name="source">The source asynchronous paged enumerable.</param>
    extension<TSource>(IAsyncPagedEnumerable<TSource> source)
    {
        #region SelectMany / Flatten
        /// <summary>
        /// Projects each element of the source sequence to an <see cref="IEnumerable{TCollection}"/> and flattens the resulting sequences.
        /// </summary>
        /// <typeparam name="TCollection">The element type of the inner collections.</typeparam>
        /// <param name="collectionSelector">A transform function to apply to each source element that returns an inner (synchronous) sequence.</param>
        /// <returns>An async paged enumerable whose elements are the concatenation of the inner sequences.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence or <paramref name="collectionSelector"/> is null.</exception>
        public IAsyncPagedEnumerable<TCollection> SelectManyPaged<TCollection>(
            Func<TSource, IEnumerable<TCollection>> collectionSelector)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(collectionSelector);

            async IAsyncEnumerable<TCollection> Iterator([Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
            {
                await foreach (var outer in source.WithCancellation(ct).ConfigureAwait(false))
                {
                    var innerSeq = collectionSelector(outer) ?? throw new InvalidOperationException("Collection selector returned null.");
                    foreach (var inner in innerSeq)
                    {
                        ct.ThrowIfCancellationRequested();
                        yield return inner;
                    }
                }
            }

            return new AsyncPagedEnumerable<TCollection, TCollection>(
                Iterator(),
                ct => new ValueTask<PageContext>(source.GetPageContextAsync(ct)));
        }

        /// <summary>
        /// Projects each element of the source to an <see cref="IEnumerable{TCollection}"/> and flattens the results, invoking a result selector for each pair.
        /// </summary>
        /// <typeparam name="TCollection">Inner collection element type.</typeparam>
        /// <typeparam name="TResult">Result element type.</typeparam>
        /// <param name="collectionSelector">Selector producing inner (synchronous) sequence.</param>
        /// <param name="resultSelector">Selector applied to each source element and each inner element.</param>
        /// <returns>A flattened async paged enumerable of projected elements.</returns>
        public IAsyncPagedEnumerable<TResult> SelectManyPaged<TCollection, TResult>(
            Func<TSource, IEnumerable<TCollection>> collectionSelector,
            Func<TSource, TCollection, TResult> resultSelector)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(collectionSelector);
            ArgumentNullException.ThrowIfNull(resultSelector);

            async IAsyncEnumerable<TResult> Iterator([Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
            {
                await foreach (var outer in source.WithCancellation(ct).ConfigureAwait(false))
                {
                    var innerSeq = collectionSelector(outer) ?? throw new InvalidOperationException("Collection selector returned null.");
                    foreach (var inner in innerSeq)
                    {
                        ct.ThrowIfCancellationRequested();
                        yield return resultSelector(outer, inner);
                    }
                }
            }

            return new AsyncPagedEnumerable<TResult, TResult>(
                Iterator(),
                ct => new ValueTask<PageContext>(source.GetPageContextAsync(ct)));
        }

        /// <summary>
        /// Projects each element of the source sequence to an asynchronous inner sequence and flattens the results.
        /// </summary>
        /// <typeparam name="TCollection">Inner async collection element type.</typeparam>
        /// <param name="asyncCollectionSelector">Selector producing an async sequence for each source element.</param>
        /// <returns>A flattened async paged enumerable.</returns>
        public IAsyncPagedEnumerable<TCollection> SelectManyPagedAsync<TCollection>(
            Func<TSource, IAsyncEnumerable<TCollection>> asyncCollectionSelector)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(asyncCollectionSelector);

            async IAsyncEnumerable<TCollection> Iterator([Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
            {
                await foreach (var outer in source.WithCancellation(ct).ConfigureAwait(false))
                {
                    var innerAsync = asyncCollectionSelector(outer) ?? throw new InvalidOperationException("Collection selector returned null.");
                    await foreach (var inner in innerAsync.WithCancellation(ct).ConfigureAwait(false))
                        yield return inner;
                }
            }

            return new AsyncPagedEnumerable<TCollection, TCollection>(
                Iterator(),
                ct => new ValueTask<PageContext>(source.GetPageContextAsync(ct)));
        }

        /// <summary>
        /// Projects each element of the source sequence to an asynchronous inner sequence and flattens the results using a result selector.
        /// </summary>
        /// <typeparam name="TCollection">Inner async collection element type.</typeparam>
        /// <typeparam name="TResult">Result element type.</typeparam>
        /// <param name="asyncCollectionSelector">Selector producing an async sequence for each source element.</param>
        /// <param name="resultSelector">Selector applied to each outer and inner element.</param>
        public IAsyncPagedEnumerable<TResult> SelectManyPagedAsync<TCollection, TResult>(
            Func<TSource, IAsyncEnumerable<TCollection>> asyncCollectionSelector,
            Func<TSource, TCollection, TResult> resultSelector)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(asyncCollectionSelector);
            ArgumentNullException.ThrowIfNull(resultSelector);

            async IAsyncEnumerable<TResult> Iterator([Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
            {
                await foreach (var outer in source.WithCancellation(ct).ConfigureAwait(false))
                {
                    var innerAsync = asyncCollectionSelector(outer) ?? throw new InvalidOperationException("Collection selector returned null.");
                    await foreach (var inner in innerAsync.WithCancellation(ct).ConfigureAwait(false))
                        yield return resultSelector(outer, inner);
                }
            }

            return new AsyncPagedEnumerable<TResult, TResult>(
                Iterator(),
                ct => new ValueTask<PageContext>(source.GetPageContextAsync(ct)));
        }

        /// <summary>
        /// Projects each element of the source sequence to a cancellation-aware asynchronous inner sequence and flattens the results.
        /// </summary>
        /// <typeparam name="TCollection">Inner async collection element type.</typeparam>
        /// <param name="asyncCollectionSelector">Cancellation-aware selector producing an async sequence for each source element.</param>
        public IAsyncPagedEnumerable<TCollection> SelectManyPagedAsync<TCollection>(
            Func<TSource, CancellationToken, IAsyncEnumerable<TCollection>> asyncCollectionSelector)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(asyncCollectionSelector);

            async IAsyncEnumerable<TCollection> Iterator([Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
            {
                await foreach (var outer in source.WithCancellation(ct).ConfigureAwait(false))
                {
                    var innerAsync = asyncCollectionSelector(outer, ct) ?? throw new InvalidOperationException("Collection selector returned null.");
                    await foreach (var inner in innerAsync.WithCancellation(ct).ConfigureAwait(false))
                        yield return inner;
                }
            }

            return new AsyncPagedEnumerable<TCollection, TCollection>(
                Iterator(),
                ct => new ValueTask<PageContext>(source.GetPageContextAsync(ct)));
        }

        /// <summary>
        /// Projects each element of the source sequence to a cancellation-aware asynchronous inner sequence and flattens the results using a result selector.
        /// </summary>
        /// <typeparam name="TCollection">Inner async collection element type.</typeparam>
        /// <typeparam name="TResult">Result element type.</typeparam>
        /// <param name="asyncCollectionSelector">Cancellation-aware selector producing an async sequence for each source element.</param>
        /// <param name="resultSelector">Selector applied to each outer and inner element.</param>
        public IAsyncPagedEnumerable<TResult> SelectManyPagedAsync<TCollection, TResult>(
            Func<TSource, CancellationToken, IAsyncEnumerable<TCollection>> asyncCollectionSelector,
            Func<TSource, TCollection, TResult> resultSelector)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(asyncCollectionSelector);
            ArgumentNullException.ThrowIfNull(resultSelector);

            async IAsyncEnumerable<TResult> Iterator([Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
            {
                await foreach (var outer in source.WithCancellation(ct).ConfigureAwait(false))
                {
                    var innerAsync = asyncCollectionSelector(outer, ct) ?? throw new InvalidOperationException("Collection selector returned null.");
                    await foreach (var inner in innerAsync.WithCancellation(ct).ConfigureAwait(false))
                        yield return resultSelector(outer, inner);
                }
            }

            return new AsyncPagedEnumerable<TResult, TResult>(
                Iterator(),
                ct => new ValueTask<PageContext>(source.GetPageContextAsync(ct)));
        }
        #endregion
    }
}