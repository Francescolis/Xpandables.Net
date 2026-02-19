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
using System.Collections;
using System.Runtime.CompilerServices;

namespace System.Linq;

/// <summary>
/// Provides grouping extension methods for <see cref="IAsyncPagedEnumerable{TSource}"/>.
/// </summary>
public static class GroupingExtensions
{
    /// <summary>
    /// Grouping operations over an <see cref="IAsyncPagedEnumerable{TSource}"/>.
    /// </summary>
    /// <typeparam name="TSource">The element type of the source sequence.</typeparam>
    /// <param name="source">The source asynchronous paged enumerable.</param>
    extension<TSource>(IAsyncPagedEnumerable<TSource> source)
    {
        #region GroupBy

        /// <summary>
        /// Groups the elements of the asynchronous paged sequence according to a specified key selector function.
        /// </summary>
        /// <typeparam name="TKey">The type of the key returned by the key selector function.</typeparam>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <returns>An <see cref="IAsyncPagedEnumerable{T}"/> where each element is an <see cref="IGrouping{TKey, TSource}"/> object containing a sequence of objects and a key.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence or key selector is null.</exception>
        public IAsyncPagedEnumerable<IGrouping<TKey, TSource>> GroupByPaged<TKey>(Func<TSource, TKey> keySelector)
            where TKey : notnull
        {
            return source.GroupByPaged(keySelector, EqualityComparer<TKey>.Default);
        }

        /// <summary>
        /// Groups the elements of the asynchronous paged sequence according to a specified key selector function and compares the keys by using a specified comparer.
        /// </summary>
        /// <typeparam name="TKey">The type of the key returned by the key selector function.</typeparam>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <param name="comparer">An equality comparer to compare keys.</param>
        /// <returns>An <see cref="IAsyncPagedEnumerable{T}"/> where each element is an <see cref="IGrouping{TKey, TSource}"/> object containing a sequence of objects and a key.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence or key selector is null.</exception>
        public IAsyncPagedEnumerable<IGrouping<TKey, TSource>> GroupByPaged<TKey>(Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer)
            where TKey : notnull
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(keySelector);

            comparer ??= EqualityComparer<TKey>.Default;

            async IAsyncEnumerable<IGrouping<TKey, TSource>> Iterator([EnumeratorCancellation] CancellationToken ct = default)
            {
                ct.ThrowIfCancellationRequested(); // Check cancellation before starting

                var groups = new Dictionary<TKey, List<TSource>>(comparer);

                await foreach (TSource? item in source.WithCancellation(ct).ConfigureAwait(false))
                {
                    TKey key = keySelector(item)
                        ?? throw new InvalidOperationException($"Key selector returned null for element of type '{typeof(TSource).Name}'.");
                    if (!groups.TryGetValue(key, out List<TSource>? group))
                    {
                        group = [];
                        groups[key] = group;
                    }
                    group.Add(item);
                }

                foreach (KeyValuePair<TKey, List<TSource>> kvp in groups)
                {
                    ct.ThrowIfCancellationRequested();
                    yield return new Grouping<TKey, TSource>(kvp.Key, kvp.Value);
                }
            }

            return AsyncPagedEnumerable.Create(
                Iterator(),
                ct => new ValueTask<Pagination>(source.GetPaginationAsync(ct)));
        }

        /// <summary>
        /// Groups the elements of the asynchronous paged sequence according to a specified key selector function and projects the elements for each group by using a specified function.
        /// </summary>
        /// <typeparam name="TKey">The type of the key returned by the key selector function.</typeparam>
        /// <typeparam name="TElement">The type of the elements in the <see cref="IGrouping{TKey, TElement}"/>.</typeparam>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <param name="elementSelector">A function to map each source element to an element in the <see cref="IGrouping{TKey, TElement}"/>.</param>
        /// <returns>An <see cref="IAsyncPagedEnumerable{T}"/> where each element is an <see cref="IGrouping{TKey, TElement}"/> object containing a collection of objects of type <typeparamref name="TElement"/> and a key.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence, key selector, or element selector is null.</exception>
        public IAsyncPagedEnumerable<IGrouping<TKey, TElement>> GroupByPaged<TKey, TElement>(
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector)
            where TKey : notnull
        {
            return source.GroupByPaged(keySelector, elementSelector, EqualityComparer<TKey>.Default);
        }

        /// <summary>
        /// Groups the elements of the asynchronous paged sequence according to a key selector function. The keys are compared by using a comparer and each group's elements are projected by using a specified function.
        /// </summary>
        /// <typeparam name="TKey">The type of the key returned by the key selector function.</typeparam>
        /// <typeparam name="TElement">The type of the elements in the <see cref="IGrouping{TKey, TElement}"/>.</typeparam>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <param name="elementSelector">A function to map each source element to an element in the <see cref="IGrouping{TKey, TElement}"/>.</param>
        /// <param name="comparer">An equality comparer to compare keys.</param>
        /// <returns>An <see cref="IAsyncPagedEnumerable{T}"/> where each element is an <see cref="IGrouping{TKey, TElement}"/> object containing a collection of objects of type <typeparamref name="TElement"/> and a key.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence, key selector, or element selector is null.</exception>
        public IAsyncPagedEnumerable<IGrouping<TKey, TElement>> GroupByPaged<TKey, TElement>(
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            IEqualityComparer<TKey>? comparer)
            where TKey : notnull
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(keySelector);
            ArgumentNullException.ThrowIfNull(elementSelector);

            comparer ??= EqualityComparer<TKey>.Default;

            async IAsyncEnumerable<IGrouping<TKey, TElement>> Iterator([EnumeratorCancellation] CancellationToken ct = default)
            {
                ct.ThrowIfCancellationRequested(); // Check cancellation before starting

                var groups = new Dictionary<TKey, List<TElement>>(comparer);

                await foreach (TSource? item in source.WithCancellation(ct).ConfigureAwait(false))
                {
                    TKey key = keySelector(item)
                        ?? throw new InvalidOperationException($"Key selector returned null for element of type '{typeof(TSource).Name}'.");
					TElement? element = elementSelector(item);
                    if (!groups.TryGetValue(key, out List<TElement>? group))
                    {
                        group = [];
                        groups[key] = group;
                    }
                    group.Add(element);
                }

                foreach (KeyValuePair<TKey, List<TElement>> kvp in groups)
                {
                    ct.ThrowIfCancellationRequested();
                    yield return new Grouping<TKey, TElement>(kvp.Key, kvp.Value);
                }
            }

            return AsyncPagedEnumerable.Create(
                Iterator(),
                ct => new ValueTask<Pagination>(source.GetPaginationAsync(ct)));
        }

        /// <summary>
        /// Groups the elements of the asynchronous paged sequence according to a specified key selector function and creates a result value from each group and its key.
        /// </summary>
        /// <typeparam name="TKey">The type of the key returned by the key selector function.</typeparam>
        /// <typeparam name="TResult">The type of the result value returned by the result selector function.</typeparam>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <param name="resultSelector">A function to create a result value from each group.</param>
        /// <returns>An <see cref="IAsyncPagedEnumerable{TResult}"/> that has a type argument of <typeparamref name="TResult"/> and contains one element for each group.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence, key selector, or result selector is null.</exception>
        public IAsyncPagedEnumerable<TResult> GroupByPaged<TKey, TResult>(
            Func<TSource, TKey> keySelector,
            Func<TKey, IEnumerable<TSource>, TResult> resultSelector)
            where TKey : notnull
        {
            return source.GroupByPaged(keySelector, resultSelector, EqualityComparer<TKey>.Default);
        }

        /// <summary>
        /// Groups the elements of the asynchronous paged sequence according to a specified key selector function and creates a result value from each group and its key. The keys are compared by using a specified comparer.
        /// </summary>
        /// <typeparam name="TKey">The type of the key returned by the key selector function.</typeparam>
        /// <typeparam name="TResult">The type of the result elements.</typeparam>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <param name="resultSelector">A function to create a result value from each group.</param>
        /// <param name="comparer">An equality comparer to compare keys.</param>
        /// <returns>An <see cref="IAsyncPagedEnumerable{TResult}"/> that has a type argument of <typeparamref name="TResult"/> and contains one element for each group.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence, key selector, or result selector is null.</exception>
        public IAsyncPagedEnumerable<TResult> GroupByPaged<TKey, TResult>(
            Func<TSource, TKey> keySelector,
            Func<TKey, IEnumerable<TSource>, TResult> resultSelector,
            IEqualityComparer<TKey>? comparer)
            where TKey : notnull
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(keySelector);
            ArgumentNullException.ThrowIfNull(resultSelector);

            comparer ??= EqualityComparer<TKey>.Default;

            async IAsyncEnumerable<TResult> Iterator([EnumeratorCancellation] CancellationToken ct = default)
            {
                ct.ThrowIfCancellationRequested(); // Check cancellation before starting

                var groups = new Dictionary<TKey, List<TSource>>(comparer);

                await foreach (TSource? item in source.WithCancellation(ct).ConfigureAwait(false))
                {
                    TKey key = keySelector(item)
                        ?? throw new InvalidOperationException($"Key selector returned null for element of type '{typeof(TSource).Name}'.");
                    if (!groups.TryGetValue(key, out List<TSource>? group))
                    {
                        group = [];
                        groups[key] = group;
                    }
                    group.Add(item);
                }

                foreach (KeyValuePair<TKey, List<TSource>> kvp in groups)
                {
                    ct.ThrowIfCancellationRequested();
                    yield return resultSelector(kvp.Key, kvp.Value);
                }
            }

            return AsyncPagedEnumerable.Create(
                Iterator(),
                ct => new ValueTask<Pagination>(source.GetPaginationAsync(ct)));
        }

        #endregion
    }

    /// <summary>
    /// Represents a collection of objects that have a common key.
    /// </summary>
    private sealed record Grouping<TKey, TElement>(TKey Key, IReadOnlyList<TElement> Elements) : IGrouping<TKey, TElement>
        where TKey : notnull
    {
        public IEnumerator<TElement> GetEnumerator() => Elements.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}