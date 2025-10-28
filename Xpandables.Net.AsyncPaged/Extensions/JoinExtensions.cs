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
/// Provides join extension methods for <see cref="IAsyncPagedEnumerable{TSource}"/>.
/// </summary>
public static class JoinExtensions
{
    /// <summary>
    /// Join operations over an <see cref="IAsyncPagedEnumerable{TSource}"/>.
    /// </summary>
    /// <typeparam name="TSource">The element type of the source sequence.</typeparam>
    /// <param name="source">The source asynchronous paged enumerable.</param>
    extension<TSource>(IAsyncPagedEnumerable<TSource> source)
    {
        #region Join

        /// <summary>
        /// Correlates the elements of two sequences based on matching keys.
        /// </summary>
        /// <typeparam name="TInner">The type of the elements of the second sequence.</typeparam>
        /// <typeparam name="TKey">The type of the keys returned by the key selector functions.</typeparam>
        /// <typeparam name="TResult">The type of the result elements.</typeparam>
        /// <param name="inner">The sequence to join to the first sequence.</param>
        /// <param name="outerKeySelector">A function to extract the join key from each element of the first sequence.</param>
        /// <param name="innerKeySelector">A function to extract the join key from each element of the second sequence.</param>
        /// <param name="resultSelector">A function to create a result element from two matching elements.</param>
        /// <returns>An <see cref="IAsyncPagedEnumerable{TResult}"/> that has elements of type <typeparamref name="TResult"/> that are obtained by performing an inner join on two sequences.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any of the parameters is null.</exception>
        public IAsyncPagedEnumerable<TResult> JoinPaged<TInner, TKey, TResult>(
            IAsyncEnumerable<TInner> inner,
            Func<TSource, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector,
            Func<TSource, TInner, TResult> resultSelector)
            where TKey : notnull
        {
            return source.JoinPaged(inner, outerKeySelector, innerKeySelector, resultSelector, EqualityComparer<TKey>.Default);
        }

        /// <summary>
        /// Correlates the elements of two sequences based on matching keys. A specified equality comparer is used to compare keys.
        /// </summary>
        /// <typeparam name="TInner">The type of the elements of the second sequence.</typeparam>
        /// <typeparam name="TKey">The type of the keys returned by the key selector functions.</typeparam>
        /// <typeparam name="TResult">The type of the result elements.</typeparam>
        /// <param name="inner">The sequence to join to the first sequence.</param>
        /// <param name="outerKeySelector">A function to extract the join key from each element of the first sequence.</param>
        /// <param name="innerKeySelector">A function to extract the join key from each element of the second sequence.</param>
        /// <param name="resultSelector">A function to create a result element from two matching elements.</param>
        /// <param name="comparer">An equality comparer to hash and compare keys.</param>
        /// <returns>An <see cref="IAsyncPagedEnumerable{TResult}"/> that has elements of type <typeparamref name="TResult"/> that are obtained by performing an inner join on two sequences.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any of the parameters is null.</exception>
        public IAsyncPagedEnumerable<TResult> JoinPaged<TInner, TKey, TResult>(
            IAsyncEnumerable<TInner> inner,
            Func<TSource, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector,
            Func<TSource, TInner, TResult> resultSelector,
            IEqualityComparer<TKey>? comparer)
            where TKey : notnull
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(inner);
            ArgumentNullException.ThrowIfNull(outerKeySelector);
            ArgumentNullException.ThrowIfNull(innerKeySelector);
            ArgumentNullException.ThrowIfNull(resultSelector);

            comparer ??= EqualityComparer<TKey>.Default;

            async IAsyncEnumerable<TResult> Iterator([EnumeratorCancellation] CancellationToken ct = default)
            {
                ct.ThrowIfCancellationRequested(); // Check cancellation before starting

                // Build lookup from inner sequence  
                var innerLookup = new Dictionary<TKey, List<TInner>>(comparer!);
                await foreach (var innerElement in inner.WithCancellation(ct).ConfigureAwait(false))
                {
                    var innerKey = innerKeySelector(innerElement);
                    if (!innerLookup.TryGetValue(innerKey, out var list))
                    {
                        list = [];
                        innerLookup[innerKey] = list;
                    }
                    list.Add(innerElement);
                }

                // Join with outer sequence
                await foreach (var outerElement in source.WithCancellation(ct).ConfigureAwait(false))
                {
                    var outerKey = outerKeySelector(outerElement);
                    if (innerLookup.TryGetValue(outerKey, out var matchingInnerElements))
                    {
                        foreach (var innerElement in matchingInnerElements)
                        {
                            ct.ThrowIfCancellationRequested();
                            yield return resultSelector(outerElement, innerElement);
                        }
                    }
                }
            }

            return new AsyncPagedEnumerable<TResult>(
                Iterator(),
                ct => new ValueTask<Pagination>(source.GetPaginationAsync(ct)));
        }

        #endregion

        #region GroupJoin

        /// <summary>
        /// Correlates the elements of two sequences based on equality of keys and groups the results.
        /// </summary>
        /// <typeparam name="TInner">The type of the elements of the second sequence.</typeparam>
        /// <typeparam name="TKey">The type of the keys returned by the key selector functions.</typeparam>
        /// <typeparam name="TResult">The type of the result elements.</typeparam>
        /// <param name="inner">The sequence to join to the first sequence.</param>
        /// <param name="outerKeySelector">A function to extract the join key from each element of the first sequence.</param>
        /// <param name="innerKeySelector">A function to extract the join key from each element of the second sequence.</param>
        /// <param name="resultSelector">A function to create a result element from an element from the first sequence and a collection of matching elements from the second sequence.</param>
        /// <returns>An <see cref="IAsyncPagedEnumerable{TResult}"/> that contains elements of type <typeparamref name="TResult"/> that are obtained by performing a grouped join on two sequences.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any of the parameters is null.</exception>
        public IAsyncPagedEnumerable<TResult> GroupJoinPaged<TInner, TKey, TResult>(
            IAsyncEnumerable<TInner> inner,
            Func<TSource, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector,
            Func<TSource, IEnumerable<TInner>, TResult> resultSelector)
            where TKey : notnull
        {
            return source.GroupJoinPaged(inner, outerKeySelector, innerKeySelector, resultSelector, EqualityComparer<TKey>.Default);
        }

        /// <summary>
        /// Correlates the elements of two sequences based on key equality and groups the results. A specified equality comparer is used to compare keys.
        /// </summary>
        /// <typeparam name="TInner">The type of the elements of the second sequence.</typeparam>
        /// <typeparam name="TKey">The type of the keys returned by the key selector functions.</typeparam>
        /// <typeparam name="TResult">The type of the result elements.</typeparam>
        /// <param name="inner">The sequence to join to the first sequence.</param>
        /// <param name="outerKeySelector">A function to extract the join key from each element of the first sequence.</param>
        /// <param name="innerKeySelector">A function to extract the join key from each element of the second sequence.</param>
        /// <param name="resultSelector">A function to create a result element from an element from the first sequence and a collection of matching elements from the second sequence.</param>
        /// <param name="comparer">An equality comparer to hash and compare keys.</param>
        /// <returns>An <see cref="IAsyncPagedEnumerable{TResult}"/> that contains elements of type <typeparamref name="TResult"/> that are obtained by performing a grouped join on two sequences.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any of the parameters is null.</exception>
        public IAsyncPagedEnumerable<TResult> GroupJoinPaged<TInner, TKey, TResult>(
            IAsyncEnumerable<TInner> inner,
            Func<TSource, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector,
            Func<TSource, IEnumerable<TInner>, TResult> resultSelector,
            IEqualityComparer<TKey>? comparer)
            where TKey : notnull
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(inner);
            ArgumentNullException.ThrowIfNull(outerKeySelector);
            ArgumentNullException.ThrowIfNull(innerKeySelector);
            ArgumentNullException.ThrowIfNull(resultSelector);

            comparer ??= EqualityComparer<TKey>.Default;

            async IAsyncEnumerable<TResult> Iterator([EnumeratorCancellation] CancellationToken ct = default)
            {
                ct.ThrowIfCancellationRequested(); // Check cancellation before starting

                // Build lookup from inner sequence
                var innerLookup = new Dictionary<TKey, List<TInner>>(comparer!);
                await foreach (var innerElement in inner.WithCancellation(ct).ConfigureAwait(false))
                {
                    var innerKey = innerKeySelector(innerElement);
                    if (!innerLookup.TryGetValue(innerKey, out var list))
                    {
                        list = [];
                        innerLookup[innerKey] = list;
                    }
                    list.Add(innerElement);
                }

                // Group join with outer sequence
                await foreach (var outerElement in source.WithCancellation(ct).ConfigureAwait(false))
                {
                    var outerKey = outerKeySelector(outerElement);
                    var matchingInnerElements = innerLookup.TryGetValue(outerKey, out var list) ? list : [];
                    yield return resultSelector(outerElement, matchingInnerElements);
                }
            }

            return new AsyncPagedEnumerable<TResult>(
                Iterator(),
                ct => new ValueTask<Pagination>(source.GetPaginationAsync(ct)));
        }

        #endregion
    }
}