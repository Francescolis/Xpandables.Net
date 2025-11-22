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
namespace System.Linq;

/// <summary>
/// Provides ordering extension methods for <see cref="IAsyncPagedEnumerable{TSource}"/>.
/// </summary>
public static class OrderingExtensions
{
    /// <summary>
    /// Ordering operations over an <see cref="IAsyncPagedEnumerable{TSource}"/>.
    /// </summary>
    /// <typeparam name="TSource">The element type of the source sequence.</typeparam>
    /// <param name="source">The source asynchronous paged enumerable.</param>
    extension<TSource>(IAsyncPagedEnumerable<TSource> source)
    {
        #region OrderBy / OrderByDescending

        /// <summary>
        /// Sorts the elements of the asynchronous paged sequence in ascending order according to a key.
        /// </summary>
        /// <typeparam name="TKey">The type of the key returned by the key selector function.</typeparam>
        /// <param name="keySelector">A function to extract a key from an element.</param>
        /// <returns>An <see cref="IAsyncPagedEnumerable{TSource}"/> whose elements are sorted according to a key.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence or key selector is null.</exception>
        public IAsyncPagedEnumerable<TSource> OrderByPaged<TKey>(Func<TSource, TKey> keySelector)
        {
            return source.OrderByPaged(keySelector, Comparer<TKey>.Default);
        }

        /// <summary>
        /// Sorts the elements of the asynchronous paged sequence in ascending order by using a specified comparer.
        /// </summary>
        /// <typeparam name="TKey">The type of the key returned by the key selector function.</typeparam>
        /// <param name="keySelector">A function to extract a key from an element.</param>
        /// <param name="comparer">A comparer to compare keys.</param>
        /// <returns>An <see cref="IAsyncPagedEnumerable{TSource}"/> whose elements are sorted according to a key.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence or key selector is null.</exception>
        public IAsyncPagedEnumerable<TSource> OrderByPaged<TKey>(Func<TSource, TKey> keySelector, IComparer<TKey>? comparer)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(keySelector);

            comparer ??= Comparer<TKey>.Default;

            async IAsyncEnumerable<TSource> Iterator([EnumeratorCancellation] CancellationToken ct = default)
            {
                var list = new List<TSource>();
                await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
                {
                    list.Add(item);
                }

                list.Sort((x, y) => comparer.Compare(keySelector(x), keySelector(y)));

                foreach (var item in list)
                {
                    ct.ThrowIfCancellationRequested();
                    yield return item;
                }
            }

            return AsyncPagedEnumerable.Create(
                Iterator(),
                ct => new ValueTask<Pagination>(source.GetPaginationAsync(ct)));
        }

        /// <summary>
        /// Sorts the elements of the asynchronous paged sequence in descending order according to a key.
        /// </summary>
        /// <typeparam name="TKey">The type of the key returned by the key selector function.</typeparam>
        /// <param name="keySelector">A function to extract a key from an element.</param>
        /// <returns>An <see cref="IAsyncPagedEnumerable{TSource}"/> whose elements are sorted in descending order according to a key.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence or key selector is null.</exception>
        public IAsyncPagedEnumerable<TSource> OrderByDescendingPaged<TKey>(Func<TSource, TKey> keySelector)
        {
            return source.OrderByDescendingPaged(keySelector, Comparer<TKey>.Default);
        }

        /// <summary>
        /// Sorts the elements of the asynchronous paged sequence in descending order by using a specified comparer.
        /// </summary>
        /// <typeparam name="TKey">The type of the key returned by the key selector function.</typeparam>
        /// <param name="keySelector">A function to extract a key from an element.</param>
        /// <param name="comparer">A comparer to compare keys.</param>
        /// <returns>An <see cref="IAsyncPagedEnumerable{TSource}"/> whose elements are sorted in descending order according to a key.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence or key selector is null.</exception>
        public IAsyncPagedEnumerable<TSource> OrderByDescendingPaged<TKey>(Func<TSource, TKey> keySelector, IComparer<TKey>? comparer)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(keySelector);

            comparer ??= Comparer<TKey>.Default;

            async IAsyncEnumerable<TSource> Iterator([EnumeratorCancellation] CancellationToken ct = default)
            {
                var list = new List<TSource>();
                await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
                {
                    list.Add(item);
                }

                list.Sort((x, y) => comparer.Compare(keySelector(y), keySelector(x))); // Note: reversed comparison for descending

                foreach (var item in list)
                {
                    ct.ThrowIfCancellationRequested();
                    yield return item;
                }
            }

            return AsyncPagedEnumerable.Create(
                Iterator(),
                ct => new ValueTask<Pagination>(source.GetPaginationAsync(ct)));
        }

        #endregion

        #region Reverse

        /// <summary>
        /// Inverts the order of the elements in the asynchronous paged sequence.
        /// </summary>
        /// <returns>An <see cref="IAsyncPagedEnumerable{TSource}"/> whose elements correspond to those of the source sequence in reverse order.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence is null.</exception>
        public IAsyncPagedEnumerable<TSource> ReversePaged()
        {
            ArgumentNullException.ThrowIfNull(source);

            async IAsyncEnumerable<TSource> Iterator([EnumeratorCancellation] CancellationToken ct = default)
            {
                var list = new List<TSource>();
                await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
                {
                    list.Add(item);
                }

                for (int i = list.Count - 1; i >= 0; i--)
                {
                    ct.ThrowIfCancellationRequested();
                    yield return list[i];
                }
            }

            return AsyncPagedEnumerable.Create(
                Iterator(),
                ct => new ValueTask<Pagination>(source.GetPaginationAsync(ct)));
        }

        #endregion
    }
}