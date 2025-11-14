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

using System.Collections;
using System.Runtime.CompilerServices;

using Xpandables.Net.Collections.Generic;

namespace Xpandables.Net.Collections.Extensions;

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

                await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
                {
                    var key = keySelector(item);
                    if (!groups.TryGetValue(key, out var group))
                    {
                        group = [];
                        groups[key] = group;
                    }
                    group.Add(item);
                }

                foreach (var kvp in groups)
                {
                    ct.ThrowIfCancellationRequested();
                    yield return new Grouping<TKey, TSource>(kvp.Key, kvp.Value);
                }
            }

            return new AsyncPagedEnumerable<IGrouping<TKey, TSource>>(
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

                await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
                {
                    var key = keySelector(item);
                    var element = elementSelector(item);
                    if (!groups.TryGetValue(key, out var group))
                    {
                        group = [];
                        groups[key] = group;
                    }
                    group.Add(element);
                }

                foreach (var kvp in groups)
                {
                    ct.ThrowIfCancellationRequested();
                    yield return new Grouping<TKey, TElement>(kvp.Key, kvp.Value);
                }
            }

            return new AsyncPagedEnumerable<IGrouping<TKey, TElement>>(
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

                await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
                {
                    var key = keySelector(item);
                    if (!groups.TryGetValue(key, out var group))
                    {
                        group = [];
                        groups[key] = group;
                    }
                    group.Add(item);
                }

                foreach (var kvp in groups)
                {
                    ct.ThrowIfCancellationRequested();
                    yield return resultSelector(kvp.Key, kvp.Value);
                }
            }

            return new AsyncPagedEnumerable<TResult>(
                Iterator(),
                ct => new ValueTask<Pagination>(source.GetPaginationAsync(ct)));
        }

        #endregion

        #region ToLookup

        /// <summary>
        /// Creates a <see cref="Lookup{TKey, TSource}"/> from the asynchronous paged enumerable according to a specified key selector function.
        /// </summary>
        /// <typeparam name="TKey">The type of the key returned by the key selector function.</typeparam>
        /// <param name="keySelector">A function to extract a key from each element.</param>
        /// <param name="cancellationToken">A token to observe cancellation.</param>
        /// <returns>A <see cref="Lookup{TKey, TSource}"/> that contains keys and values.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence or key selector is null.</exception>
        public async ValueTask<ILookup<TKey, TSource>> ToLookupPagedAsync<TKey>(
            Func<TSource, TKey> keySelector,
            CancellationToken cancellationToken = default)
            where TKey : notnull
        {
            return await source.ToLookupPagedAsync(keySelector, EqualityComparer<TKey>.Default, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates a <see cref="Lookup{TKey, TSource}"/> from the asynchronous paged enumerable according to a specified key selector function and key comparer.
        /// </summary>
        /// <typeparam name="TKey">The type of the key returned by the key selector function.</typeparam>
        /// <param name="keySelector">A function to extract a key from each element.</param>
        /// <param name="comparer">An equality comparer to compare keys.</param>
        /// <param name="cancellationToken">A token to observe cancellation.</param>
        /// <returns>A <see cref="Lookup{TKey, TSource}"/> that contains keys and values.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence or key selector is null.</exception>
        public async ValueTask<ILookup<TKey, TSource>> ToLookupPagedAsync<TKey>(
            Func<TSource, TKey> keySelector,
            IEqualityComparer<TKey>? comparer,
            CancellationToken cancellationToken = default)
            where TKey : notnull
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(keySelector);

            var lookup = new Dictionary<TKey, List<TSource>>(comparer);

            await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                var key = keySelector(item);
                if (!lookup.TryGetValue(key, out var list))
                {
                    list = [];
                    lookup[key] = list;
                }
                list.Add(item);
            }

            return new Lookup<TKey, TSource>(lookup);
        }

        /// <summary>
        /// Creates a <see cref="Lookup{TKey, TElement}"/> from the asynchronous paged enumerable according to specified key selector and element selector functions.
        /// </summary>
        /// <typeparam name="TKey">The type of the key returned by the key selector function.</typeparam>
        /// <typeparam name="TElement">The type of the value returned by the element selector function.</typeparam>
        /// <param name="keySelector">A function to extract a key from each element.</param>
        /// <param name="elementSelector">A transform function to produce a result element value from each element.</param>
        /// <param name="cancellationToken">A token to observe cancellation.</param>
        /// <returns>A <see cref="Lookup{TKey, TElement}"/> that contains values of type <typeparamref name="TElement"/> selected from the input sequence.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence, key selector, or element selector is null.</exception>
        public async ValueTask<ILookup<TKey, TElement>> ToLookupPagedAsync<TKey, TElement>(
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            CancellationToken cancellationToken = default)
            where TKey : notnull
        {
            return await source.ToLookupPagedAsync(keySelector, elementSelector, EqualityComparer<TKey>.Default, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates a <see cref="Lookup{TKey, TElement}"/> from the asynchronous paged enumerable according to specified key selector function, a comparer, and an element selector function.
        /// </summary>
        /// <typeparam name="TKey">The type of the key returned by the key selector function.</typeparam>
        /// <typeparam name="TElement">The type of the value returned by the element selector function.</typeparam>
        /// <param name="keySelector">A function to extract a key from each element.</param>
        /// <param name="elementSelector">A transform function to produce a result element value from each element.</param>
        /// <param name="comparer">An equality comparer to compare keys.</param>
        /// <param name="cancellationToken">A token to observe cancellation.</param>
        /// <returns>A <see cref="Lookup{TKey, TElement}"/> that contains values of type <typeparamref name="TElement"/> selected from the input sequence.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence, key selector, or element selector is null.</exception>
        public async ValueTask<ILookup<TKey, TElement>> ToLookupPagedAsync<TKey, TElement>(
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            IEqualityComparer<TKey>? comparer,
            CancellationToken cancellationToken = default)
            where TKey : notnull
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(keySelector);
            ArgumentNullException.ThrowIfNull(elementSelector);

            var lookup = new Dictionary<TKey, List<TElement>>(comparer);

            await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                var key = keySelector(item);
                var element = elementSelector(item);
                if (!lookup.TryGetValue(key, out var list))
                {
                    list = [];
                    lookup[key] = list;
                }
                list.Add(element);
            }

            return new Lookup<TKey, TElement>(lookup);
        }

        #endregion
    }

    // Helper classes for grouping
    private sealed class Grouping<TKey, TElement>(TKey key, IEnumerable<TElement> elements) : IGrouping<TKey, TElement>
        where TKey : notnull
    {
        public TKey Key { get; } = key;

        public IEnumerator<TElement> GetEnumerator() => elements.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    private sealed class Lookup<TKey, TElement>(Dictionary<TKey, List<TElement>> groups) : ILookup<TKey, TElement>
        where TKey : notnull
    {
        public IEnumerable<TElement> this[TKey key] => groups.TryGetValue(key, out var list) ? list : [];

        public int Count => groups.Count;

        public bool Contains(TKey key) => groups.ContainsKey(key);

        public IEnumerator<IGrouping<TKey, TElement>> GetEnumerator()
        {
            foreach (var kvp in groups)
            {
                yield return new Grouping<TKey, TElement>(kvp.Key, kvp.Value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}