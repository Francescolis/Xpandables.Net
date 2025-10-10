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

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using Xpandables.Net.Async;
namespace Xpandables.Net.Async;

/// <summary>
/// Provides set operation and joining extension methods for <see cref="IAsyncPagedEnumerable{TSource}"/>.
/// </summary>
[SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]
public static class AsyncPagedEnumerableSetExtensions
{
    /// <summary>
    /// Set operation and joining operations over an <see cref="IAsyncPagedEnumerable{TSource}"/>.
    /// </summary>
    /// <typeparam name="TSource">The element type of the source sequence.</typeparam>
    /// <param name="source">The source asynchronous paged enumerable.</param>
    extension<TSource>(IAsyncPagedEnumerable<TSource> source)
    {
        #region Union / Intersect / Except

        /// <summary>
        /// Produces the set union of two sequences by using the default equality comparer.
        /// </summary>
        /// <param name="other">The second sequence whose distinct elements form the union.</param>
        /// <returns>An async paged enumerable that contains the elements from both input sequences, excluding duplicates.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the current sequence or the other sequence is null.</exception>
        public IAsyncPagedEnumerable<TSource> UnionPaged(IAsyncEnumerable<TSource> other)
        {
            return source.UnionPaged(other, EqualityComparer<TSource>.Default);
        }

        /// <summary>
        /// Produces the set union of two sequences by using a specified equality comparer.
        /// </summary>
        /// <param name="other">The second sequence whose distinct elements form the union.</param>
        /// <param name="comparer">An equality comparer to compare values.</param>
        /// <returns>An async paged enumerable that contains the elements from both input sequences, excluding duplicates.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the current sequence or the other sequence is null.</exception>
        public IAsyncPagedEnumerable<TSource> UnionPaged(IAsyncEnumerable<TSource> other, IEqualityComparer<TSource>? comparer)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(other);

            comparer ??= EqualityComparer<TSource>.Default;

            async IAsyncEnumerable<TSource> Iterator([EnumeratorCancellation] CancellationToken ct = default)
            {
                var seen = new HashSet<TSource>(comparer);

                await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
                {
                    if (seen.Add(item))
                        yield return item;
                }

                await foreach (var item in other.WithCancellation(ct).ConfigureAwait(false))
                {
                    if (seen.Add(item))
                        yield return item;
                }
            }

            return new AsyncPagedEnumerable<TSource>(
                Iterator(),
                ct => new ValueTask<Pagination>(source.GetPaginationAsync(ct)));
        }

        /// <summary>
        /// Produces the set intersection of two sequences by using the default equality comparer.
        /// </summary>
        /// <param name="other">A sequence whose distinct elements that also appear in the first sequence will be returned.</param>
        /// <returns>An async paged enumerable that contains the elements that form the set intersection of two sequences.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the current sequence or the other sequence is null.</exception>
        public IAsyncPagedEnumerable<TSource> IntersectPaged(IAsyncEnumerable<TSource> other)
        {
            return source.IntersectPaged(other, EqualityComparer<TSource>.Default);
        }

        /// <summary>
        /// Produces the set intersection of two sequences by using the specified equality comparer.
        /// </summary>
        /// <param name="other">A sequence whose distinct elements that also appear in the first sequence will be returned.</param>
        /// <param name="comparer">An equality comparer to compare values.</param>
        /// <returns>An async paged enumerable that contains the elements that form the set intersection of two sequences.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the current sequence or the other sequence is null.</exception>
        public IAsyncPagedEnumerable<TSource> IntersectPaged(IAsyncEnumerable<TSource> other, IEqualityComparer<TSource>? comparer)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(other);

            comparer ??= EqualityComparer<TSource>.Default;

            async IAsyncEnumerable<TSource> Iterator([EnumeratorCancellation] CancellationToken ct = default)
            {
                var otherSet = new HashSet<TSource>(comparer);
                await foreach (var item in other.WithCancellation(ct).ConfigureAwait(false))
                {
                    otherSet.Add(item);
                }

                var yielded = new HashSet<TSource>(comparer);
                await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
                {
                    if (otherSet.Contains(item) && yielded.Add(item))
                        yield return item;
                }
            }

            return new AsyncPagedEnumerable<TSource>(
                Iterator(),
                ct => new ValueTask<Pagination>(source.GetPaginationAsync(ct)));
        }

        /// <summary>
        /// Produces the set difference of two sequences by using the default equality comparer.
        /// </summary>
        /// <param name="other">A sequence whose elements that also occur in the first sequence will cause those elements to be removed from the returned sequence.</param>
        /// <returns>An async paged enumerable that contains the set difference of the elements of two sequences.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the current sequence or the other sequence is null.</exception>
        public IAsyncPagedEnumerable<TSource> ExceptPaged(IAsyncEnumerable<TSource> other)
        {
            return source.ExceptPaged(other, EqualityComparer<TSource>.Default);
        }

        /// <summary>
        /// Produces the set difference of two sequences by using the specified equality comparer.
        /// </summary>
        /// <param name="other">A sequence whose elements that also occur in the first sequence will cause those elements to be removed from the returned sequence.</param>
        /// <param name="comparer">An equality comparer to compare values.</param>
        /// <returns>An async paged enumerable that contains the set difference of the elements of two sequences.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the current sequence or the other sequence is null.</exception>
        public IAsyncPagedEnumerable<TSource> ExceptPaged(IAsyncEnumerable<TSource> other, IEqualityComparer<TSource>? comparer)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(other);

            comparer ??= EqualityComparer<TSource>.Default;

            async IAsyncEnumerable<TSource> Iterator([EnumeratorCancellation] CancellationToken ct = default)
            {
                var otherSet = new HashSet<TSource>(comparer);
                await foreach (var item in other.WithCancellation(ct).ConfigureAwait(false))
                {
                    otherSet.Add(item);
                }

                var yielded = new HashSet<TSource>(comparer);
                await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
                {
                    if (!otherSet.Contains(item) && yielded.Add(item))
                        yield return item;
                }
            }

            return new AsyncPagedEnumerable<TSource>(
                Iterator(),
                ct => new ValueTask<Pagination>(source.GetPaginationAsync(ct)));
        }

        #endregion

        #region Concat / Prepend / Append

        /// <summary>
        /// Concatenates two sequences.
        /// </summary>
        /// <param name="other">The sequence to concatenate to the first sequence.</param>
        /// <returns>An async paged enumerable that contains the concatenated elements of the two input sequences.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the current sequence or the other sequence is null.</exception>
        public IAsyncPagedEnumerable<TSource> ConcatPaged(IAsyncEnumerable<TSource> other)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(other);

            async IAsyncEnumerable<TSource> Iterator([EnumeratorCancellation] CancellationToken ct = default)
            {
                await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
                {
                    yield return item;
                }

                await foreach (var item in other.WithCancellation(ct).ConfigureAwait(false))
                {
                    yield return item;
                }
            }

            return new AsyncPagedEnumerable<TSource>(
                Iterator(),
                ct => new ValueTask<Pagination>(source.GetPaginationAsync(ct)));
        }

        /// <summary>
        /// Adds a value to the beginning of the sequence.
        /// </summary>
        /// <param name="element">The value to prepend to the source sequence.</param>
        /// <returns>An async paged enumerable that begins with the specified element.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the current sequence is null.</exception>
        public IAsyncPagedEnumerable<TSource> PrependPaged(TSource element)
        {
            ArgumentNullException.ThrowIfNull(source);

            async IAsyncEnumerable<TSource> Iterator([EnumeratorCancellation] CancellationToken ct = default)
            {
                yield return element;

                await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
                {
                    yield return item;
                }
            }

            return new AsyncPagedEnumerable<TSource>(
                Iterator(),
                ct => new ValueTask<Pagination>(source.GetPaginationAsync(ct)));
        }

        /// <summary>
        /// Adds a value to the end of the sequence.
        /// </summary>
        /// <param name="element">The value to append to the source sequence.</param>
        /// <returns>An async paged enumerable that ends with the specified element.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the current sequence is null.</exception>
        public IAsyncPagedEnumerable<TSource> AppendPaged(TSource element)
        {
            ArgumentNullException.ThrowIfNull(source);

            async IAsyncEnumerable<TSource> Iterator([EnumeratorCancellation] CancellationToken ct = default)
            {
                await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
                {
                    yield return item;
                }

                yield return element;
            }

            return new AsyncPagedEnumerable<TSource>(
                Iterator(),
                ct => new ValueTask<Pagination>(source.GetPaginationAsync(ct)));
        }

        #endregion

        #region Zip

        /// <summary>
        /// Applies a specified function to the corresponding elements of two sequences, producing a sequence of the results.
        /// </summary>
        /// <typeparam name="TOther">The type of the elements of the second input sequence.</typeparam>
        /// <typeparam name="TResult">The type of the elements of the result sequence.</typeparam>
        /// <param name="other">The second sequence to merge.</param>
        /// <param name="resultSelector">A function that specifies how to merge the elements from the two sequences.</param>
        /// <returns>An async paged enumerable that contains merged elements of two input sequences.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the current sequence, the other sequence, or the result selector is null.</exception>
        [SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "ConfigureAwait is not applicable to await using")]
        public IAsyncPagedEnumerable<TResult> ZipPaged<TOther, TResult>(
            IAsyncEnumerable<TOther> other,
            Func<TSource, TOther, TResult> resultSelector)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(other);
            ArgumentNullException.ThrowIfNull(resultSelector);

            async IAsyncEnumerable<TResult> Iterator([EnumeratorCancellation] CancellationToken ct = default)
            {
                await using var sourceEnumerator = source.GetAsyncEnumerator(ct);
                await using var otherEnumerator = other.GetAsyncEnumerator(ct);

                while (await sourceEnumerator.MoveNextAsync().ConfigureAwait(false) &&
                       await otherEnumerator.MoveNextAsync().ConfigureAwait(false))
                {
                    yield return resultSelector(sourceEnumerator.Current, otherEnumerator.Current);
                }
            }

            return new AsyncPagedEnumerable<TResult>(
                Iterator(),
                ct => new ValueTask<Pagination>(source.GetPaginationAsync(ct)));
        }

        /// <summary>
        /// Produces a sequence of tuples with elements from the two specified sequences.
        /// </summary>
        /// <typeparam name="TOther">The type of the elements of the second input sequence.</typeparam>
        /// <param name="other">The second sequence to merge.</param>
        /// <returns>An async paged enumerable that contains merged elements of two input sequences as tuples.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the current sequence or the other sequence is null.</exception>
        public IAsyncPagedEnumerable<(TSource First, TOther Second)> ZipPaged<TOther>(IAsyncEnumerable<TOther> other)
        {
            return source.ZipPaged(other, (first, second) => (first, second));
        }

        #endregion

        #region DefaultIfEmpty

        /// <summary>
        /// Returns the elements of the specified sequence or the type parameter's default value in a singleton collection if the sequence is empty.
        /// </summary>
        /// <returns>An async paged enumerable that contains the default value if the source sequence is empty; otherwise, the source sequence itself.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the current sequence is null.</exception>
        public IAsyncPagedEnumerable<TSource?> DefaultIfEmptyPaged()
        {
            return source.DefaultIfEmptyPaged(default);
        }

        /// <summary>
        /// Returns the elements of the specified sequence or the specified value in a singleton collection if the sequence is empty.
        /// </summary>
        /// <param name="defaultValue">The value to return if the sequence is empty.</param>
        /// <returns>An async paged enumerable that contains the default value if the source sequence is empty; otherwise, the source sequence itself.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the current sequence is null.</exception>
        public IAsyncPagedEnumerable<TSource?> DefaultIfEmptyPaged(TSource? defaultValue)
        {
            ArgumentNullException.ThrowIfNull(source);

            async IAsyncEnumerable<TSource?> Iterator([EnumeratorCancellation] CancellationToken ct = default)
            {
                bool hasElements = false;
                await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
                {
                    hasElements = true;
                    yield return item;
                }

                if (!hasElements)
                    yield return defaultValue;
            }

            return new AsyncPagedEnumerable<TSource?>(
                Iterator(),
                ct => new ValueTask<Pagination>(source.GetPaginationAsync(ct)));
        }

        #endregion
    }
}