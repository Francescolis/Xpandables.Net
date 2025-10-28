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

using Xpandables.Net.AsyncPaged.Extensions;

namespace Xpandables.Net.AsyncPaged.Extensions;

/// <summary>
/// Provides aggregation and ordering extension methods for <see cref="IAsyncPagedEnumerable{TSource}"/>.
/// </summary>
public static class AggregationExtensions
{
    /// <summary>
    /// Aggregation and ordering operations over an <see cref="IAsyncPagedEnumerable{TSource}"/>.
    /// </summary>
    /// <typeparam name="TSource">The element type of the source sequence.</typeparam>
    /// <param name="source">The source asynchronous paged enumerable.</param>
    extension<TSource>(IAsyncPagedEnumerable<TSource> source)
    {
        #region Count / LongCount

        /// <summary>
        /// Returns the number of elements in the asynchronous paged sequence.
        /// </summary>
        /// <param name="cancellationToken">A token to observe cancellation.</param>
        /// <returns>The number of elements in the source sequence.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence is null.</exception>
        /// <exception cref="OverflowException">Thrown when the number of elements is larger than <see cref="int.MaxValue"/>.</exception>
        public async ValueTask<int> CountPagedAsync(CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);

            int count = 0;
            await foreach (var _ in source.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                checked { count++; }
            }
            return count;
        }

        /// <summary>
        /// Returns the number of elements in the asynchronous paged sequence that satisfy a condition.
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <param name="cancellationToken">A token to observe cancellation.</param>
        /// <returns>The number of elements in the source sequence that satisfy the condition.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence or predicate is null.</exception>
        /// <exception cref="OverflowException">Thrown when the number of elements is larger than <see cref="int.MaxValue"/>.</exception>
        public async ValueTask<int> CountPagedAsync(Func<TSource, bool> predicate, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(predicate);

            int count = 0;
            await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                if (predicate(item))
                    checked { count++; }
            }
            return count;
        }

        /// <summary>
        /// Returns the number of elements in the asynchronous paged sequence as a <see cref="long"/>.
        /// </summary>
        /// <param name="cancellationToken">A token to observe cancellation.</param>
        /// <returns>The number of elements in the source sequence.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence is null.</exception>
        /// <exception cref="OverflowException">Thrown when the number of elements is larger than <see cref="long.MaxValue"/>.</exception>
        public async ValueTask<long> LongCountPagedAsync(CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);

            long count = 0;
            await foreach (var _ in source.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                checked { count++; }
            }
            return count;
        }

        /// <summary>
        /// Returns the number of elements in the asynchronous paged sequence that satisfy a condition as a <see cref="long"/>.
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <param name="cancellationToken">A token to observe cancellation.</param>
        /// <returns>The number of elements in the source sequence that satisfy the condition.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence or predicate is null.</exception>
        /// <exception cref="OverflowException">Thrown when the number of elements is larger than <see cref="long.MaxValue"/>.</exception>
        public async ValueTask<long> LongCountPagedAsync(Func<TSource, bool> predicate, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(predicate);

            long count = 0;
            await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                if (predicate(item))
                    checked { count++; }
            }
            return count;
        }

        #endregion

        #region Any / All

        /// <summary>
        /// Determines whether the asynchronous paged sequence contains any elements.
        /// </summary>
        /// <param name="cancellationToken">A token to observe cancellation.</param>
        /// <returns><see langword="true"/> if the source sequence contains any elements; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence is null.</exception>
        public async ValueTask<bool> AnyPagedAsync(CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);

            await foreach (var _ in source.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Determines whether any element of the asynchronous paged sequence satisfies a condition.
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <param name="cancellationToken">A token to observe cancellation.</param>
        /// <returns><see langword="true"/> if any elements in the source sequence pass the test in the specified predicate; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence or predicate is null.</exception>
        public async ValueTask<bool> AnyPagedAsync(Func<TSource, bool> predicate, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(predicate);

            await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                if (predicate(item))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Determines whether all elements of the asynchronous paged sequence satisfy a condition.
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <param name="cancellationToken">A token to observe cancellation.</param>
        /// <returns><see langword="true"/> if every element of the source sequence passes the test in the specified predicate, or if the sequence is empty; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence or predicate is null.</exception>
        public async ValueTask<bool> AllPagedAsync(Func<TSource, bool> predicate, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(predicate);

            await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                if (!predicate(item))
                    return false;
            }
            return true;
        }

        #endregion

        #region Contains

        /// <summary>
        /// Determines whether the asynchronous paged sequence contains a specified element by using the default equality comparer.
        /// </summary>
        /// <param name="value">The value to locate in the sequence.</param>
        /// <param name="cancellationToken">A token to observe cancellation.</param>
        /// <returns><see langword="true"/> if the source sequence contains an element that has the specified value; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence is null.</exception>
        public async ValueTask<bool> ContainsPagedAsync(TSource value, CancellationToken cancellationToken = default)
        {
            return await source.ContainsPagedAsync(value, EqualityComparer<TSource>.Default, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Determines whether the asynchronous paged sequence contains a specified element by using a specified equality comparer.
        /// </summary>
        /// <param name="value">The value to locate in the sequence.</param>
        /// <param name="comparer">An equality comparer to compare values.</param>
        /// <param name="cancellationToken">A token to observe cancellation.</param>
        /// <returns><see langword="true"/> if the source sequence contains an element that has the specified value; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence is null.</exception>
        public async ValueTask<bool> ContainsPagedAsync(TSource value, IEqualityComparer<TSource>? comparer, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);

            comparer ??= EqualityComparer<TSource>.Default;
            await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                if (comparer.Equals(item, value))
                    return true;
            }
            return false;
        }

        #endregion

        #region Aggregate / Reduce

        /// <summary>
        /// Applies an accumulator function over the asynchronous paged sequence.
        /// </summary>
        /// <param name="func">An accumulator function to be invoked on each element.</param>
        /// <param name="cancellationToken">A token to observe cancellation.</param>
        /// <returns>The final accumulator value.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence or func is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the source sequence is empty.</exception>
        public async ValueTask<TSource> AggregatePagedAsync(Func<TSource, TSource, TSource> func, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(func);

#pragma warning disable CA2007 // ConfigureAwait not applicable to await using enumerator acquisition
            await using var enumerator = source.GetAsyncEnumerator(cancellationToken);
#pragma warning restore CA2007
            if (!await enumerator.MoveNextAsync().ConfigureAwait(false))
                throw new InvalidOperationException("Sequence contains no elements.");

            TSource result = enumerator.Current;
            while (await enumerator.MoveNextAsync().ConfigureAwait(false))
            {
                result = func(result, enumerator.Current);
            }
            return result;
        }

        /// <summary>
        /// Applies an accumulator function over the asynchronous paged sequence. The specified seed value is used as the initial accumulator value.
        /// </summary>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <param name="seed">The initial accumulator value.</param>
        /// <param name="func">An accumulator function to be invoked on each element.</param>
        /// <param name="cancellationToken">A token to observe cancellation.</param>
        /// <returns>The final accumulator value.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence or func is null.</exception>
        public async ValueTask<TAccumulate> AggregatePagedAsync<TAccumulate>(TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(func);

            TAccumulate result = seed;
            await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                result = func(result, item);
            }
            return result;
        }

        /// <summary>
        /// Applies an accumulator function over the asynchronous paged sequence. The specified seed value is used as the initial accumulator value, and the specified function is used to select the result value.
        /// </summary>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <typeparam name="TResult">The type of the resulting value.</typeparam>
        /// <param name="seed">The initial accumulator value.</param>
        /// <param name="func">An accumulator function to be invoked on each element.</param>
        /// <param name="resultSelector">A function to transform the final accumulator value into the result value.</param>
        /// <param name="cancellationToken">A token to observe cancellation.</param>
        /// <returns>The transformed final accumulator value.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence, func, or resultSelector is null.</exception>
        public async ValueTask<TResult> AggregatePagedAsync<TAccumulate, TResult>(
            TAccumulate seed,
            Func<TAccumulate, TSource, TAccumulate> func,
            Func<TAccumulate, TResult> resultSelector,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(func);
            ArgumentNullException.ThrowIfNull(resultSelector);

            TAccumulate accumulate = seed;
            await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                accumulate = func(accumulate, item);
            }
            return resultSelector(accumulate);
        }

        #endregion

        #region Min / Max

        /// <summary>
        /// Returns the minimum value in the asynchronous paged sequence.
        /// </summary>
        /// <param name="cancellationToken">A token to observe cancellation.</param>
        /// <returns>The minimum value in the sequence.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the source sequence is empty.</exception>
        public async ValueTask<TSource> MinPagedAsync(CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);

#pragma warning disable CA2007 // ConfigureAwait not applicable to await using enumerator acquisition
            await using var enumerator = source.GetAsyncEnumerator(cancellationToken);
#pragma warning restore CA2007
            if (!await enumerator.MoveNextAsync().ConfigureAwait(false))
                throw new InvalidOperationException("Sequence contains no elements.");

            TSource min = enumerator.Current;
            var comparer = Comparer<TSource>.Default;

            while (await enumerator.MoveNextAsync().ConfigureAwait(false))
            {
                if (comparer.Compare(enumerator.Current, min) < 0)
                    min = enumerator.Current;
            }
            return min;
        }

        /// <summary>
        /// Returns the minimum value in the asynchronous paged sequence by using a specified selector function.
        /// </summary>
        /// <typeparam name="TResult">The type of the value returned by the selector.</typeparam>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <param name="cancellationToken">A token to observe cancellation.</param>
        /// <returns>The minimum value in the sequence.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence or selector is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the source sequence is empty.</exception>
        public async ValueTask<TResult> MinPagedAsync<TResult>(Func<TSource, TResult> selector, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(selector);

#pragma warning disable CA2007 // ConfigureAwait not applicable to await using enumerator acquisition
            await using var enumerator = source.GetAsyncEnumerator(cancellationToken);
#pragma warning restore CA2007
            if (!await enumerator.MoveNextAsync().ConfigureAwait(false))
                throw new InvalidOperationException("Sequence contains no elements.");

            TResult min = selector(enumerator.Current);
            var comparer = Comparer<TResult>.Default;

            while (await enumerator.MoveNextAsync().ConfigureAwait(false))
            {
                TResult value = selector(enumerator.Current);
                if (comparer.Compare(value, min) < 0)
                    min = value;
            }
            return min;
        }

        /// <summary>
        /// Returns the maximum value in the asynchronous paged sequence.
        /// </summary>
        /// <param name="cancellationToken">A token to observe cancellation.</param>
        /// <returns>The maximum value in the sequence.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the source sequence is empty.</exception>
        public async ValueTask<TSource> MaxPagedAsync(CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);

#pragma warning disable CA2007 // ConfigureAwait not applicable to await using enumerator acquisition
            await using var enumerator = source.GetAsyncEnumerator(cancellationToken);
#pragma warning restore CA2007
            if (!await enumerator.MoveNextAsync().ConfigureAwait(false))
                throw new InvalidOperationException("Sequence contains no elements.");

            TSource max = enumerator.Current;
            var comparer = Comparer<TSource>.Default;

            while (await enumerator.MoveNextAsync().ConfigureAwait(false))
            {
                if (comparer.Compare(enumerator.Current, max) > 0)
                    max = enumerator.Current;
            }
            return max;
        }

        /// <summary>
        /// Returns the maximum value in the asynchronous paged sequence by using a specified selector function.
        /// </summary>
        /// <typeparam name="TResult">The type of the value returned by the selector.</typeparam>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <param name="cancellationToken">A token to observe cancellation.</param>
        /// <returns>The maximum value in the sequence.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence or selector is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the source sequence is empty.</exception>
        public async ValueTask<TResult> MaxPagedAsync<TResult>(Func<TSource, TResult> selector, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(selector);

#pragma warning disable CA2007 // ConfigureAwait not applicable to await using enumerator acquisition
            await using var enumerator = source.GetAsyncEnumerator(cancellationToken);
#pragma warning restore CA2007
            if (!await enumerator.MoveNextAsync().ConfigureAwait(false))
                throw new InvalidOperationException("Sequence contains no elements.");

            TResult max = selector(enumerator.Current);
            var comparer = Comparer<TResult>.Default;

            while (await enumerator.MoveNextAsync().ConfigureAwait(false))
            {
                TResult value = selector(enumerator.Current);
                if (comparer.Compare(value, max) > 0)
                    max = value;
            }
            return max;
        }

        #endregion

        #region MinBy / MaxBy

        /// <summary>
        /// Returns the element that produces the minimum key value.
        /// </summary>
        /// <typeparam name="TKey">The type of the key returned by the key selector function.</typeparam>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <param name="cancellationToken">A token to observe cancellation.</param>
        /// <returns>The element that produces the minimum key value.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence or key selector is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the source sequence is empty.</exception>
        public async ValueTask<TSource> MinByPagedAsync<TKey>(Func<TSource, TKey> keySelector, CancellationToken cancellationToken = default)
        {
            return await source.MinByPagedAsync(keySelector, Comparer<TKey>.Default, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Returns the element that produces the minimum key value using a specified comparer.
        /// </summary>
        /// <typeparam name="TKey">The type of the key returned by the key selector function.</typeparam>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <param name="comparer">A comparer to compare keys.</param>
        /// <param name="cancellationToken">A token to observe cancellation.</param>
        /// <returns>The element that produces the minimum key value.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence or key selector is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the source sequence is empty.</exception>
        public async ValueTask<TSource> MinByPagedAsync<TKey>(Func<TSource, TKey> keySelector, IComparer<TKey>? comparer, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(keySelector);

            comparer ??= Comparer<TKey>.Default;

#pragma warning disable CA2007 // ConfigureAwait not applicable to await using enumerator acquisition
            await using var enumerator = source.GetAsyncEnumerator(cancellationToken);
#pragma warning restore CA2007
            if (!await enumerator.MoveNextAsync().ConfigureAwait(false))
                throw new InvalidOperationException("Sequence contains no elements.");

            TSource minElement = enumerator.Current;
            TKey minKey = keySelector(minElement);

            while (await enumerator.MoveNextAsync().ConfigureAwait(false))
            {
                TKey key = keySelector(enumerator.Current);
                if (comparer.Compare(key, minKey) < 0)
                {
                    minElement = enumerator.Current;
                    minKey = key;
                }
            }
            return minElement;
        }

        /// <summary>
        /// Returns the element that produces the maximum key value.
        /// </summary>
        /// <typeparam name="TKey">The type of the key returned by the key selector function.</typeparam>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <param name="cancellationToken">A token to observe cancellation.</param>
        /// <returns>The element that produces the maximum key value.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence or key selector is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the source sequence is empty.</exception>
        public async ValueTask<TSource> MaxByPagedAsync<TKey>(Func<TSource, TKey> keySelector, CancellationToken cancellationToken = default)
        {
            return await source.MaxByPagedAsync(keySelector, Comparer<TKey>.Default, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Returns the element that produces the maximum key value using a specified comparer.
        /// </summary>
        /// <typeparam name="TKey">The type of the key returned by the key selector function.</typeparam>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <param name="comparer">A comparer to compare keys.</param>
        /// <param name="cancellationToken">A token to observe cancellation.</param>
        /// <returns>The element that produces the maximum key value.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence or key selector is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the source sequence is empty.</exception>
        public async ValueTask<TSource> MaxByPagedAsync<TKey>(Func<TSource, TKey> keySelector, IComparer<TKey>? comparer, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(keySelector);

            comparer ??= Comparer<TKey>.Default;

#pragma warning disable CA2007 // ConfigureAwait not applicable to await using enumerator acquisition
            await using var enumerator = source.GetAsyncEnumerator(cancellationToken);
#pragma warning restore CA2007
            if (!await enumerator.MoveNextAsync().ConfigureAwait(false))
                throw new InvalidOperationException("Sequence contains no elements.");

            TSource maxElement = enumerator.Current;
            TKey maxKey = keySelector(maxElement);

            while (await enumerator.MoveNextAsync().ConfigureAwait(false))
            {
                TKey key = keySelector(enumerator.Current);
                if (comparer.Compare(key, maxKey) > 0)
                {
                    maxElement = enumerator.Current;
                    maxKey = key;
                }
            }
            return maxElement;
        }

        #endregion

        #region ToList / ToArray

        /// <summary>
        /// Creates a <see cref="List{T}"/> from the asynchronous paged enumerable.
        /// </summary>
        /// <param name="cancellationToken">A token to observe cancellation.</param>
        /// <returns>A <see cref="List{T}"/> that contains elements from the source sequence.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence is null.</exception>
        public async ValueTask<List<TSource>> ToListPagedAsync(CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);
            cancellationToken.ThrowIfCancellationRequested(); // Check cancellation early

            var list = new List<TSource>();
            await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                list.Add(item);
            }
            return list;
        }

        /// <summary>
        /// Creates an array from the asynchronous paged enumerable.
        /// </summary>
        /// <param name="cancellationToken">A token to observe cancellation.</param>
        /// <returns>An array that contains elements from the source sequence.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence is null.</exception>
        public async ValueTask<TSource[]> ToArrayPagedAsync(CancellationToken cancellationToken = default)
        {
            var list = await source.ToListPagedAsync(cancellationToken).ConfigureAwait(false);
            return [.. list];
        }

        #endregion
    }
}