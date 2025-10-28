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

using System.Runtime.CompilerServices;

using Xpandables.Net.AsyncPaged.Extensions;
namespace Xpandables.Net.AsyncPaged.Extensions;

/// <summary>
/// Provides pagination and filtering extension methods for <see cref="IAsyncPagedEnumerable{TSource}"/>.
/// </summary>
public static class PaginationExtensions
{
    /// <summary>
    /// Pagination and filtering operations over an <see cref="IAsyncPagedEnumerable{TSource}"/>.
    /// </summary>
    /// <typeparam name="TSource">The element type of the source sequence.</typeparam>
    /// <param name="source">The source asynchronous paged enumerable.</param>
    extension<TSource>(IAsyncPagedEnumerable<TSource> source)
    {
        #region Take / Skip

        /// <summary>
        /// Returns a specified number of contiguous elements from the start of the asynchronous paged sequence.
        /// </summary>
        /// <param name="count">The number of elements to return.</param>
        /// <returns>An <see cref="IAsyncPagedEnumerable{TSource}"/> that contains the specified number of elements from the start of the source sequence.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="count"/> is negative.</exception>
        public IAsyncPagedEnumerable<TSource> TakePaged(int count)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentOutOfRangeException.ThrowIfNegative(count);

            if (count == 0)
            {
                return new AsyncPagedEnumerable<TSource>(
                    AsyncEnumerable.Empty<TSource>(),
                    ct => new ValueTask<Pagination>(source.GetPaginationAsync(ct)));
            }

            async IAsyncEnumerable<TSource> Iterator([EnumeratorCancellation] CancellationToken ct = default)
            {
                int taken = 0;
                await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
                {
                    if (taken >= count) yield break;
                    yield return item;
                    taken++;
                }
            }

            return new AsyncPagedEnumerable<TSource>(
                Iterator(),
                ct => new ValueTask<Pagination>(source.GetPaginationAsync(ct)));
        }

        /// <summary>
        /// Bypasses a specified number of elements in the asynchronous paged sequence and returns the remaining elements.
        /// </summary>
        /// <param name="count">The number of elements to skip before returning the remaining elements.</param>
        /// <returns>An <see cref="IAsyncPagedEnumerable{TSource}"/> that contains the elements that occur after the specified index in the source sequence.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="count"/> is negative.</exception>
        public IAsyncPagedEnumerable<TSource> SkipPaged(int count)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentOutOfRangeException.ThrowIfNegative(count);

            if (count == 0) return source;

            async IAsyncEnumerable<TSource> Iterator([EnumeratorCancellation] CancellationToken ct = default)
            {
                int skipped = 0;
                await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
                {
                    if (skipped < count)
                    {
                        skipped++;
                        continue;
                    }
                    yield return item;
                }
            }

            return new AsyncPagedEnumerable<TSource>(
                Iterator(),
                ct => new ValueTask<Pagination>(source.GetPaginationAsync(ct)));
        }

        /// <summary>
        /// Returns elements from the asynchronous paged sequence as long as a specified condition is true.
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>An <see cref="IAsyncPagedEnumerable{TSource}"/> that contains elements from the source sequence that occur before the element at which the test no longer passes.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence or predicate is null.</exception>
        public IAsyncPagedEnumerable<TSource> TakeWhilePaged(Func<TSource, bool> predicate)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(predicate);

            async IAsyncEnumerable<TSource> Iterator([EnumeratorCancellation] CancellationToken ct = default)
            {
                await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
                {
                    if (!predicate(item)) yield break;
                    yield return item;
                }
            }

            return new AsyncPagedEnumerable<TSource>(
                Iterator(),
                ct => new ValueTask<Pagination>(source.GetPaginationAsync(ct)));
        }

        /// <summary>
        /// Returns elements from the asynchronous paged sequence as long as a specified condition is true, with the predicate also receiving the element's index.
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition; the second parameter represents the index of the element.</param>
        /// <returns>An <see cref="IAsyncPagedEnumerable{TSource}"/> that contains elements from the source sequence that occur before the element at which the test no longer passes.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence or predicate is null.</exception>
        public IAsyncPagedEnumerable<TSource> TakeWhilePaged(Func<TSource, int, bool> predicate)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(predicate);

            async IAsyncEnumerable<TSource> Iterator([EnumeratorCancellation] CancellationToken ct = default)
            {
                int index = 0;
                await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
                {
                    if (!predicate(item, index)) yield break;
                    yield return item;
                    index++;
                }
            }

            return new AsyncPagedEnumerable<TSource>(
                Iterator(),
                ct => new ValueTask<Pagination>(source.GetPaginationAsync(ct)));
        }

        /// <summary>
        /// Bypasses elements in the asynchronous paged sequence as long as a specified condition is true and then returns the remaining elements.
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>An <see cref="IAsyncPagedEnumerable{TSource}"/> that contains the elements from the source sequence starting at the first element in the linear series that does not pass the test specified by predicate.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence or predicate is null.</exception>
        public IAsyncPagedEnumerable<TSource> SkipWhilePaged(Func<TSource, bool> predicate)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(predicate);

            async IAsyncEnumerable<TSource> Iterator([EnumeratorCancellation] CancellationToken ct = default)
            {
                bool yielding = false;
                await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
                {
                    if (!yielding && !predicate(item))
                        yielding = true;

                    if (yielding)
                        yield return item;
                }
            }

            return new AsyncPagedEnumerable<TSource>(
                Iterator(),
                ct => new ValueTask<Pagination>(source.GetPaginationAsync(ct)));
        }

        /// <summary>
        /// Bypasses elements in the asynchronous paged sequence as long as a specified condition is true and then returns the remaining elements, with the predicate also receiving the element's index.
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition; the second parameter represents the index of the element.</param>
        /// <returns>An <see cref="IAsyncPagedEnumerable{TSource}"/> that contains the elements from the source sequence starting at the first element in the linear series that does not pass the test specified by predicate.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence or predicate is null.</exception>
        public IAsyncPagedEnumerable<TSource> SkipWhilePaged(Func<TSource, int, bool> predicate)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(predicate);

            async IAsyncEnumerable<TSource> Iterator([EnumeratorCancellation] CancellationToken ct = default)
            {
                bool yielding = false;
                int index = 0;
                await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
                {
                    if (!yielding && !predicate(item, index))
                        yielding = true;

                    if (yielding)
                        yield return item;

                    index++;
                }
            }

            return new AsyncPagedEnumerable<TSource>(
                Iterator(),
                ct => new ValueTask<Pagination>(source.GetPaginationAsync(ct)));
        }

        /// <summary>
        /// Returns the last specified number of contiguous elements from the end of the asynchronous paged sequence.
        /// </summary>
        /// <param name="count">The number of elements to return from the end of the sequence.</param>
        /// <returns>An <see cref="IAsyncPagedEnumerable{TSource}"/> that contains the specified number of elements from the end of the source sequence.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="count"/> is negative.</exception>
        public IAsyncPagedEnumerable<TSource> TakeLastPaged(int count)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentOutOfRangeException.ThrowIfNegative(count);

            if (count == 0)
            {
                return new AsyncPagedEnumerable<TSource>(
                    AsyncEnumerable.Empty<TSource>(),
                    ct => new ValueTask<Pagination>(source.GetPaginationAsync(ct)));
            }

            async IAsyncEnumerable<TSource> Iterator([EnumeratorCancellation] CancellationToken ct = default)
            {
                var buffer = new Queue<TSource>(count);
                await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
                {
                    if (buffer.Count == count)
                        buffer.Dequeue();
                    buffer.Enqueue(item);
                }

                while (buffer.Count > 0)
                    yield return buffer.Dequeue();
            }

            return new AsyncPagedEnumerable<TSource>(
                Iterator(),
                ct => new ValueTask<Pagination>(source.GetPaginationAsync(ct)));
        }

        /// <summary>
        /// Bypasses the last specified number of elements in the asynchronous paged sequence and returns the remaining elements.
        /// </summary>
        /// <param name="count">The number of elements to omit from the end of the sequence.</param>
        /// <returns>An <see cref="IAsyncPagedEnumerable{TSource}"/> that contains the elements that occur before the omitted elements at the end of the source sequence.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="count"/> is negative.</exception>
        public IAsyncPagedEnumerable<TSource> SkipLastPaged(int count)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentOutOfRangeException.ThrowIfNegative(count);

            if (count == 0) return source;

            async IAsyncEnumerable<TSource> Iterator([EnumeratorCancellation] CancellationToken ct = default)
            {
                var buffer = new Queue<TSource>(count + 1);
                await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
                {
                    buffer.Enqueue(item);
                    if (buffer.Count > count)
                        yield return buffer.Dequeue();
                }
            }

            return new AsyncPagedEnumerable<TSource>(
                Iterator(),
                ct => new ValueTask<Pagination>(source.GetPaginationAsync(ct)));
        }

        #endregion

        #region Chunk / Batch

        /// <summary>
        /// Splits the elements of the asynchronous paged sequence into chunks of a specified size.
        /// </summary>
        /// <param name="size">The size of each chunk.</param>
        /// <returns>An <see cref="IAsyncPagedEnumerable{TSource}"/> that contains arrays of elements from the source sequence, each with the specified size except potentially the last chunk.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="size"/> is less than 1.</exception>
        public IAsyncPagedEnumerable<TSource[]> ChunkPaged(int size)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentOutOfRangeException.ThrowIfLessThan(size, 1);

            async IAsyncEnumerable<TSource[]> Iterator([EnumeratorCancellation] CancellationToken ct = default)
            {
                var chunk = new List<TSource>(size);
                await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
                {
                    chunk.Add(item);
                    if (chunk.Count == size)
                    {
                        yield return [.. chunk];
                        chunk.Clear();
                    }
                }

                if (chunk.Count > 0)
                    yield return [.. chunk];
            }

            return new AsyncPagedEnumerable<TSource[]>(
                Iterator(),
                ct => new ValueTask<Pagination>(source.GetPaginationAsync(ct)));
        }

        /// <summary>
        /// Returns distinct elements from the asynchronous paged sequence using the default equality comparer to compare values.
        /// </summary>
        /// <returns>An <see cref="IAsyncPagedEnumerable{TSource}"/> that contains distinct elements from the source sequence.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence is null.</exception>
        public IAsyncPagedEnumerable<TSource> DistinctPaged()
        {
            return source.DistinctPaged(EqualityComparer<TSource>.Default);
        }

        /// <summary>
        /// Returns distinct elements from the asynchronous paged sequence using a specified equality comparer to compare values.
        /// </summary>
        /// <param name="comparer">An equality comparer to compare values.</param>
        /// <returns>An <see cref="IAsyncPagedEnumerable{TSource}"/> that contains distinct elements from the source sequence.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence is null.</exception>
        public IAsyncPagedEnumerable<TSource> DistinctPaged(IEqualityComparer<TSource>? comparer)
        {
            ArgumentNullException.ThrowIfNull(source);

            async IAsyncEnumerable<TSource> Iterator([EnumeratorCancellation] CancellationToken ct = default)
            {
                var seen = new HashSet<TSource>(comparer);
                await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
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
        /// Returns distinct elements from the asynchronous paged sequence using a specified key selector function.
        /// </summary>
        /// <typeparam name="TKey">The type of the key returned by the key selector function.</typeparam>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <returns>An <see cref="IAsyncPagedEnumerable{TSource}"/> that contains distinct elements from the source sequence.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence or key selector is null.</exception>
        public IAsyncPagedEnumerable<TSource> DistinctByPaged<TKey>(Func<TSource, TKey> keySelector)
        {
            return source.DistinctByPaged(keySelector, EqualityComparer<TKey>.Default);
        }

        /// <summary>
        /// Returns distinct elements from the asynchronous paged sequence using a specified key selector function and equality comparer.
        /// </summary>
        /// <typeparam name="TKey">The type of the key returned by the key selector function.</typeparam>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <param name="comparer">An equality comparer to compare keys.</param>
        /// <returns>An <see cref="IAsyncPagedEnumerable{TSource}"/> that contains distinct elements from the source sequence.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence or key selector is null.</exception>
        public IAsyncPagedEnumerable<TSource> DistinctByPaged<TKey>(Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(keySelector);

            async IAsyncEnumerable<TSource> Iterator([EnumeratorCancellation] CancellationToken ct = default)
            {
                var seenKeys = new HashSet<TKey>(comparer);
                await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
                {
                    var key = keySelector(item);
                    if (seenKeys.Add(key))
                        yield return item;
                }
            }

            return new AsyncPagedEnumerable<TSource>(
                Iterator(),
                ct => new ValueTask<Pagination>(source.GetPaginationAsync(ct)));
        }

        #endregion

        #region Where / Filter

        /// <summary>
        /// Filters the elements of the asynchronous paged sequence based on a predicate.
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>An <see cref="IAsyncPagedEnumerable{TSource}"/> that contains elements from the source sequence that satisfy the condition.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence or predicate is null.</exception>
        public IAsyncPagedEnumerable<TSource> WherePaged(Func<TSource, bool> predicate)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(predicate);

            async IAsyncEnumerable<TSource> Iterator([EnumeratorCancellation] CancellationToken ct = default)
            {
                await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
                {
                    if (predicate(item))
                        yield return item;
                }
            }

            return new AsyncPagedEnumerable<TSource>(
                Iterator(),
                ct => new ValueTask<Pagination>(source.GetPaginationAsync(ct)));
        }

        /// <summary>
        /// Filters the elements of the asynchronous paged sequence based on a predicate that also uses the element's index.
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition; the second parameter represents the index of the element.</param>
        /// <returns>An <see cref="IAsyncPagedEnumerable{TSource}"/> that contains elements from the source sequence that satisfy the condition.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence or predicate is null.</exception>
        public IAsyncPagedEnumerable<TSource> WherePaged(Func<TSource, int, bool> predicate)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(predicate);

            async IAsyncEnumerable<TSource> Iterator([EnumeratorCancellation] CancellationToken ct = default)
            {
                int index = 0;
                await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
                {
                    if (predicate(item, index))
                        yield return item;
                    index++;
                }
            }

            return new AsyncPagedEnumerable<TSource>(
                Iterator(),
                ct => new ValueTask<Pagination>(source.GetPaginationAsync(ct)));
        }

        #endregion
    }
}