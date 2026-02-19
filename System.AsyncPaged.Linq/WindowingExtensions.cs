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
/// Provides windowing and analytical extension methods for <see cref="IAsyncPagedEnumerable{TSource}"/>.
/// </summary>
public static class WindowingExtensions
{
    /// <summary>
    /// Windowing and analytical operations over an <see cref="IAsyncPagedEnumerable{TSource}"/>.
    /// </summary>
    /// <typeparam name="TSource">The element type of the source sequence.</typeparam>
    /// <param name="source">The source asynchronous paged enumerable.</param>
    extension<TSource>(IAsyncPagedEnumerable<TSource> source)
    {
        #region Windowed Operations

        /// <summary>
        /// Returns a sliding window of elements from the source sequence with the specified window size.
        /// </summary>
        /// <param name="windowSize">The size of the sliding window.</param>
        /// <returns>An <see cref="IAsyncPagedEnumerable{T}"/> where each element is an array representing a window of consecutive elements.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="windowSize"/> is less than 1.</exception>
        /// <remarks>
        /// Only full windows are returned. If the source sequence has fewer elements than the window size,
        /// no windows are yielded. This behavior is consistent with other windowed aggregate methods.
        /// </remarks>
        public IAsyncPagedEnumerable<TSource[]> WindowPaged(int windowSize)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentOutOfRangeException.ThrowIfLessThan(windowSize, 1);

            async IAsyncEnumerable<TSource[]> Iterator([EnumeratorCancellation] CancellationToken ct = default)
            {
                ct.ThrowIfCancellationRequested();

                var window = new Queue<TSource>(windowSize);
                await foreach (TSource? item in source.WithCancellation(ct).ConfigureAwait(false))
                {
                    window.Enqueue(item);
                    if (window.Count > windowSize)
					{
						window.Dequeue();
					}

					if (window.Count == windowSize)
					{
						yield return [.. window];
					}
				}
            }

            return AsyncPagedEnumerable.Create(
                Iterator(),
                ct => new ValueTask<Pagination>(source.GetPaginationAsync(ct)));
        }

        /// <summary>
        /// Computes a windowed sum over the sequence using the specified window size and value selector.
        /// </summary>
        /// <param name="windowSize">The size of the sliding window.</param>
        /// <param name="selector">A function to extract a numeric value from each element.</param>
        /// <returns>An <see cref="IAsyncPagedEnumerable{T}"/> where each element is the sum of values in the current window.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence or selector is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="windowSize"/> is less than 1.</exception>
        public IAsyncPagedEnumerable<int> WindowedSumPaged(int windowSize, Func<TSource, int> selector)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(selector);
            ArgumentOutOfRangeException.ThrowIfLessThan(windowSize, 1);

            async IAsyncEnumerable<int> Iterator([EnumeratorCancellation] CancellationToken ct = default)
            {
                ct.ThrowIfCancellationRequested();

                var window = new Queue<int>(windowSize);
                int currentSum = 0;

                await foreach (TSource? item in source.WithCancellation(ct).ConfigureAwait(false))
                {
					int value = selector(item);
                    window.Enqueue(value);
                    currentSum += value;

                    if (window.Count > windowSize)
                    {
						int removedValue = window.Dequeue();
                        currentSum -= removedValue;
                    }

                    if (window.Count == windowSize)
					{
						yield return currentSum;
					}
				}
            }

            return AsyncPagedEnumerable.Create(
                Iterator(),
                ct => new ValueTask<Pagination>(source.GetPaginationAsync(ct)));
        }

        /// <summary>
        /// Computes a windowed sum over the sequence using the specified window size and value selector for <see cref="long"/> values.
        /// </summary>
        /// <param name="windowSize">The size of the sliding window.</param>
        /// <param name="selector">A function to extract a numeric value from each element.</param>
        /// <returns>An <see cref="IAsyncPagedEnumerable{T}"/> where each element is the sum of values in the current window.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence or selector is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="windowSize"/> is less than 1.</exception>
        public IAsyncPagedEnumerable<long> WindowedSumPaged(int windowSize, Func<TSource, long> selector)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(selector);
            ArgumentOutOfRangeException.ThrowIfLessThan(windowSize, 1);

            async IAsyncEnumerable<long> Iterator([EnumeratorCancellation] CancellationToken ct = default)
            {
                ct.ThrowIfCancellationRequested();

                var window = new Queue<long>(windowSize);
                long currentSum = 0;

                await foreach (TSource? item in source.WithCancellation(ct).ConfigureAwait(false))
                {
					long value = selector(item);
                    window.Enqueue(value);
                    currentSum += value;

                    if (window.Count > windowSize)
                    {
						long removedValue = window.Dequeue();
                        currentSum -= removedValue;
                    }

                    if (window.Count == windowSize)
					{
						yield return currentSum;
					}
				}
            }

            return AsyncPagedEnumerable.Create(
                Iterator(),
                ct => new ValueTask<Pagination>(source.GetPaginationAsync(ct)));
        }

        /// <summary>
        /// Computes a windowed sum over the sequence using the specified window size and value selector for <see cref="double"/> values.
        /// </summary>
        /// <param name="windowSize">The size of the sliding window.</param>
        /// <param name="selector">A function to extract a numeric value from each element.</param>
        /// <returns>An <see cref="IAsyncPagedEnumerable{T}"/> where each element is the sum of values in the current window.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence or selector is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="windowSize"/> is less than 1.</exception>
        public IAsyncPagedEnumerable<double> WindowedSumPaged(int windowSize, Func<TSource, double> selector)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(selector);
            ArgumentOutOfRangeException.ThrowIfLessThan(windowSize, 1);

                async IAsyncEnumerable<double> Iterator([EnumeratorCancellation] CancellationToken ct = default)
                {
                    ct.ThrowIfCancellationRequested();

                    var window = new Queue<double>(windowSize);
                    double currentSum = 0;

                    await foreach (TSource? item in source.WithCancellation(ct).ConfigureAwait(false))
                    {
					double value = selector(item);
                        window.Enqueue(value);
                        currentSum += value;

                        if (window.Count > windowSize)
                        {
						double removedValue = window.Dequeue();
                            currentSum -= removedValue;
                        }

                        if (window.Count == windowSize)
					{
						yield return currentSum;
					}
				}
                }

                return AsyncPagedEnumerable.Create(
                    Iterator(),
                    ct => new ValueTask<Pagination>(source.GetPaginationAsync(ct)));
            }

            /// <summary>
            /// Computes a windowed average over the sequence using the specified window size and value selector.
            /// </summary>
        /// <param name="windowSize">The size of the sliding window.</param>
        /// <param name="selector">A function to extract a numeric value from each element.</param>
        /// <returns>An <see cref="IAsyncPagedEnumerable{T}"/> where each element is the average of values in the current window.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence or selector is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="windowSize"/> is less than 1.</exception>
        public IAsyncPagedEnumerable<double> WindowedAveragePaged(int windowSize, Func<TSource, double> selector)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(selector);
            ArgumentOutOfRangeException.ThrowIfLessThan(windowSize, 1);

                async IAsyncEnumerable<double> Iterator([EnumeratorCancellation] CancellationToken ct = default)
                {
                    ct.ThrowIfCancellationRequested();

                    var window = new Queue<double>(windowSize);
                    double currentSum = 0;

                    await foreach (TSource? item in source.WithCancellation(ct).ConfigureAwait(false))
                    {
					double value = selector(item);
                        window.Enqueue(value);
                        currentSum += value;

                        if (window.Count > windowSize)
                        {
						double removedValue = window.Dequeue();
                            currentSum -= removedValue;
                        }

                        if (window.Count == windowSize)
					{
						yield return currentSum / windowSize;
					}
				}
                }

                return AsyncPagedEnumerable.Create(
                    Iterator(),
                    ct => new ValueTask<Pagination>(source.GetPaginationAsync(ct)));
            }

            /// <summary>
            /// Computes a windowed minimum over the sequence using the specified window size and value selector.
            /// </summary>
            /// <typeparam name="TValue">The type of the value returned by the selector.</typeparam>
            /// <param name="windowSize">The size of the sliding window.</param>
            /// <param name="selector">A function to extract a value from each element.</param>
            /// <returns>An <see cref="IAsyncPagedEnumerable{T}"/> where each element is the minimum of values in the current window.</returns>
            /// <exception cref="ArgumentNullException">Thrown when the source sequence or selector is null.</exception>
            /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="windowSize"/> is less than 1.</exception>
            /// <remarks>
            /// This method uses a monotonic deque algorithm for O(n) performance instead of O(n²).
            /// </remarks>
            public IAsyncPagedEnumerable<TValue> WindowedMinPaged<TValue>(int windowSize, Func<TSource, TValue> selector)
                where TValue : IComparable<TValue>
            {
                ArgumentNullException.ThrowIfNull(source);
                ArgumentNullException.ThrowIfNull(selector);
                ArgumentOutOfRangeException.ThrowIfLessThan(windowSize, 1);

                async IAsyncEnumerable<TValue> Iterator([EnumeratorCancellation] CancellationToken ct = default)
                {
                    ct.ThrowIfCancellationRequested();

                    // Monotonic deque: stores (index, value) pairs where values are increasing from front to back
                    var deque = new LinkedList<(int Index, TValue Value)>();
                    int index = 0;

                    await foreach (TSource? item in source.WithCancellation(ct).ConfigureAwait(false))
                    {
                        TValue value = selector(item);

                        // Remove elements from back that are greater than current value (they can never be minimum)
                        while (deque.Count > 0 && deque.Last!.Value.Value.CompareTo(value) >= 0)
					{
						deque.RemoveLast();
					}

					// Add current element
					deque.AddLast((index, value));

                        // Remove elements that are outside the window from front
                        while (deque.Count > 0 && deque.First!.Value.Index <= index - windowSize)
					{
						deque.RemoveFirst();
					}

					// Yield minimum (front of deque) when we have a full window
					if (index >= windowSize - 1)
					{
						yield return deque.First!.Value.Value;
					}

					index++;
                    }
                }

                return AsyncPagedEnumerable.Create(
                    Iterator(),
                    ct => new ValueTask<Pagination>(source.GetPaginationAsync(ct)));
            }

        /// <summary>
        /// Computes a windowed maximum over the sequence using the specified window size and value selector.
        /// </summary>
        /// <typeparam name="TValue">The type of the value returned by the selector.</typeparam>
        /// <param name="windowSize">The size of the sliding window.</param>
        /// <param name="selector">A function to extract a value from each element.</param>
        /// <returns>An <see cref="IAsyncPagedEnumerable{T}"/> where each element is the maximum of values in the current window.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence or selector is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="windowSize"/> is less than 1.</exception>
        /// <remarks>
        /// This method uses a monotonic deque algorithm for O(n) performance instead of O(n²).
        /// </remarks>
        public IAsyncPagedEnumerable<TValue> WindowedMaxPaged<TValue>(int windowSize, Func<TSource, TValue> selector)
            where TValue : IComparable<TValue>
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(selector);
            ArgumentOutOfRangeException.ThrowIfLessThan(windowSize, 1);

            async IAsyncEnumerable<TValue> Iterator([EnumeratorCancellation] CancellationToken ct = default)
            {
                ct.ThrowIfCancellationRequested();

                // Monotonic deque: stores (index, value) pairs where values are decreasing from front to back
                var deque = new LinkedList<(int Index, TValue Value)>();
                int index = 0;

                await foreach (TSource? item in source.WithCancellation(ct).ConfigureAwait(false))
                {
                    TValue value = selector(item);

                    // Remove elements from back that are less than current value (they can never be maximum)
                    while (deque.Count > 0 && deque.Last!.Value.Value.CompareTo(value) <= 0)
					{
						deque.RemoveLast();
					}

					// Add current element
					deque.AddLast((index, value));

                    // Remove elements that are outside the window from front
                    while (deque.Count > 0 && deque.First!.Value.Index <= index - windowSize)
					{
						deque.RemoveFirst();
					}

					// Yield maximum (front of deque) when we have a full window
					if (index >= windowSize - 1)
					{
						yield return deque.First!.Value.Value;
					}

					index++;
                }
            }

            return AsyncPagedEnumerable.Create(
                Iterator(),
                ct => new ValueTask<Pagination>(source.GetPaginationAsync(ct)));
        }

        #endregion

        #region Pairwise Operations

        /// <summary>
        /// Returns a sequence of pairs containing the current and previous elements.
        /// </summary>
        /// <returns>An <see cref="IAsyncPagedEnumerable{T}"/> where each element is a tuple containing the previous and current elements.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence is null.</exception>
        public IAsyncPagedEnumerable<(TSource Previous, TSource Current)> PairwisePaged()
        {
            ArgumentNullException.ThrowIfNull(source);

            async IAsyncEnumerable<(TSource Previous, TSource Current)> Iterator([EnumeratorCancellation] CancellationToken ct = default)
            {
                ct.ThrowIfCancellationRequested();

                TSource? previous = default;
                bool hasPrevious = false;

                await foreach (TSource? current in source.WithCancellation(ct).ConfigureAwait(false))
                {
                    if (hasPrevious)
					{
						yield return (previous!, current);
					}

					previous = current;
                    hasPrevious = true;
                }
            }

            return AsyncPagedEnumerable.Create(
                Iterator(),
                ct => new ValueTask<Pagination>(source.GetPaginationAsync(ct)));
        }

        /// <summary>
        /// Applies a function to each pair of consecutive elements and returns the results.
        /// </summary>
        /// <typeparam name="TResult">The type of the result elements.</typeparam>
        /// <param name="selector">A function to apply to each pair of consecutive elements.</param>
        /// <returns>An <see cref="IAsyncPagedEnumerable{TResult}"/> where each element is the result of applying the selector to consecutive pairs.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence or selector is null.</exception>
        public IAsyncPagedEnumerable<TResult> PairwisePaged<TResult>(Func<TSource, TSource, TResult> selector)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(selector);

            async IAsyncEnumerable<TResult> Iterator([EnumeratorCancellation] CancellationToken ct = default)
            {
                ct.ThrowIfCancellationRequested();

                TSource? previous = default;
                bool hasPrevious = false;

                await foreach (TSource? current in source.WithCancellation(ct).ConfigureAwait(false))
                {
                    if (hasPrevious)
					{
						yield return selector(previous!, current);
					}

					previous = current;
                    hasPrevious = true;
                }
            }

            return AsyncPagedEnumerable.Create(
                Iterator(),
                ct => new ValueTask<Pagination>(source.GetPaginationAsync(ct)));
        }

        #endregion

        #region Scan / Running Operations

        /// <summary>
        /// Returns a sequence containing the progressive application of an accumulator function.
        /// </summary>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <param name="seed">The initial accumulator value.</param>
        /// <param name="func">An accumulator function to be invoked on each element.</param>
        /// <returns>An <see cref="IAsyncPagedEnumerable{TAccumulate}"/> containing the running accumulator values.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence or func is null.</exception>
        public IAsyncPagedEnumerable<TAccumulate> ScanPaged<TAccumulate>(TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(func);

            async IAsyncEnumerable<TAccumulate> Iterator([EnumeratorCancellation] CancellationToken ct = default)
            {
                ct.ThrowIfCancellationRequested();

				TAccumulate? accumulator = seed;
                yield return accumulator;

                await foreach (TSource? item in source.WithCancellation(ct).ConfigureAwait(false))
                {
                    accumulator = func(accumulator, item);
                    yield return accumulator;
                }
            }

            return AsyncPagedEnumerable.Create(
                Iterator(),
                ct => new ValueTask<Pagination>(source.GetPaginationAsync(ct)));
        }

        /// <summary>
        /// Returns a sequence containing the progressive application of an accumulator function, starting with the first element.
        /// </summary>
        /// <param name="func">An accumulator function to be invoked on each element.</param>
        /// <returns>An <see cref="IAsyncPagedEnumerable{TSource}"/> containing the running accumulator values.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence or func is null.</exception>
        public IAsyncPagedEnumerable<TSource> ScanPaged(Func<TSource, TSource, TSource> func)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(func);

            async IAsyncEnumerable<TSource> Iterator([EnumeratorCancellation] CancellationToken ct = default)
            {
                ct.ThrowIfCancellationRequested();

                TSource? accumulator = default;
                bool hasAccumulator = false;

                await foreach (TSource? item in source.WithCancellation(ct).ConfigureAwait(false))
                {
                    if (!hasAccumulator)
                    {
                        accumulator = item;
                        hasAccumulator = true;
                        yield return accumulator;
                    }
                    else
                    {
                        accumulator = func(accumulator!, item);
                        yield return accumulator;
                    }
                }
            }

            return AsyncPagedEnumerable.Create(
                Iterator(),
                ct => new ValueTask<Pagination>(source.GetPaginationAsync(ct)));
        }

        #endregion
    }
}