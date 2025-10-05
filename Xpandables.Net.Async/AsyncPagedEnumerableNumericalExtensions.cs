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

using Xpandables.Net.Async;
namespace Xpandables.Net.Async;

/// <summary>
/// Provides numerical and statistical extension methods for <see cref="IAsyncPagedEnumerable{TSource}"/>.
/// </summary>
[SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]
public static class AsyncPagedEnumerableNumericalExtensions
{
    /// <summary>
    /// Numerical and statistical operations over an <see cref="IAsyncPagedEnumerable{TSource}"/>.
    /// </summary>
    /// <typeparam name="TSource">The element type of the source sequence.</typeparam>
    /// <param name="source">The source asynchronous paged enumerable.</param>
    extension<TSource>(IAsyncPagedEnumerable<TSource> source)
    {
        #region Sum

        /// <summary>
        /// Computes the sum of the sequence of <see cref="int"/> values that are obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <param name="cancellationToken">A token to observe cancellation.</param>
        /// <returns>The sum of the projected values.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence or selector is null.</exception>
        /// <exception cref="OverflowException">The sum is larger than <see cref="int.MaxValue"/>.</exception>
        public async ValueTask<int> SumPagedAsync(Func<TSource, int> selector, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(selector);

            int sum = 0;
            await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                checked { sum += selector(item); }
            }
            return sum;
        }

        /// <summary>
        /// Computes the sum of the sequence of nullable <see cref="int"/> values that are obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <param name="cancellationToken">A token to observe cancellation.</param>
        /// <returns>The sum of the projected values.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence or selector is null.</exception>
        /// <exception cref="OverflowException">The sum is larger than <see cref="int.MaxValue"/>.</exception>
        public async ValueTask<int?> SumPagedAsync(Func<TSource, int?> selector, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(selector);

            int sum = 0;
            await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                var value = selector(item);
                if (value.HasValue)
                    checked { sum += value.Value; }
            }
            return sum;
        }

        /// <summary>
        /// Computes the sum of the sequence of <see cref="long"/> values that are obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <param name="cancellationToken">A token to observe cancellation.</param>
        /// <returns>The sum of the projected values.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence or selector is null.</exception>
        /// <exception cref="OverflowException">The sum is larger than <see cref="long.MaxValue"/>.</exception>
        public async ValueTask<long> SumPagedAsync(Func<TSource, long> selector, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(selector);

            long sum = 0;
            await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                checked { sum += selector(item); }
            }
            return sum;
        }

        /// <summary>
        /// Computes the sum of the sequence of nullable <see cref="long"/> values that are obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <param name="cancellationToken">A token to observe cancellation.</param>
        /// <returns>The sum of the projected values.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence or selector is null.</exception>
        /// <exception cref="OverflowException">The sum is larger than <see cref="long.MaxValue"/>.</exception>
        public async ValueTask<long?> SumPagedAsync(Func<TSource, long?> selector, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(selector);

            long sum = 0;
            await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                var value = selector(item);
                if (value.HasValue)
                    checked { sum += value.Value; }
            }
            return sum;
        }

        /// <summary>
        /// Computes the sum of the sequence of <see cref="double"/> values that are obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <param name="cancellationToken">A token to observe cancellation.</param>
        /// <returns>The sum of the projected values.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence or selector is null.</exception>
        public async ValueTask<double> SumPagedAsync(Func<TSource, double> selector, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(selector);

            double sum = 0;
            await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                sum += selector(item);
            }
            return sum;
        }

        /// <summary>
        /// Computes the sum of the sequence of nullable <see cref="double"/> values that are obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <param name="cancellationToken">A token to observe cancellation.</param>
        /// <returns>The sum of the projected values.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence or selector is null.</exception>
        public async ValueTask<double?> SumPagedAsync(Func<TSource, double?> selector, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(selector);

            double sum = 0;
            await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                var value = selector(item);
                if (value.HasValue)
                    sum += value.Value;
            }
            return sum;
        }

        /// <summary>
        /// Computes the sum of the sequence of <see cref="decimal"/> values that are obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <param name="cancellationToken">A token to observe cancellation.</param>
        /// <returns>The sum of the projected values.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence or selector is null.</exception>
        /// <exception cref="OverflowException">The sum is larger than <see cref="decimal.MaxValue"/>.</exception>
        public async ValueTask<decimal> SumPagedAsync(Func<TSource, decimal> selector, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(selector);

            decimal sum = 0;
            await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                sum += selector(item);
            }
            return sum;
        }

        /// <summary>
        /// Computes the sum of the sequence of nullable <see cref="decimal"/> values that are obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <param name="cancellationToken">A token to observe cancellation.</param>
        /// <returns>The sum of the projected values.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence or selector is null.</exception>
        /// <exception cref="OverflowException">The sum is larger than <see cref="decimal.MaxValue"/>.</exception>
        public async ValueTask<decimal?> SumPagedAsync(Func<TSource, decimal?> selector, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(selector);

            decimal sum = 0;
            await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                var value = selector(item);
                if (value.HasValue)
                    sum += value.Value;
            }
            return sum;
        }

        #endregion

        #region Average

        /// <summary>
        /// Computes the average of a sequence of <see cref="int"/> values that are obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <param name="cancellationToken">A token to observe cancellation.</param>
        /// <returns>The average of the projected values.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence or selector is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the source sequence is empty.</exception>
        /// <exception cref="OverflowException">The sum is larger than <see cref="long.MaxValue"/>.</exception>
        public async ValueTask<double> AveragePagedAsync(Func<TSource, int> selector, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(selector);

            long sum = 0;
            long count = 0;
            await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                checked
                {
                    sum += selector(item);
                    count++;
                }
            }
            if (count == 0) throw new InvalidOperationException("Sequence contains no elements.");
            return (double)sum / count;
        }

        /// <summary>
        /// Computes the average of a sequence of nullable <see cref="int"/> values that are obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <param name="cancellationToken">A token to observe cancellation.</param>
        /// <returns>The average of the projected values, or null if the source sequence is empty or contains only null values.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence or selector is null.</exception>
        public async ValueTask<double?> AveragePagedAsync(Func<TSource, int?> selector, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(selector);

            long sum = 0;
            long count = 0;
            await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                var value = selector(item);
                if (value.HasValue)
                {
                    checked
                    {
                        sum += value.Value;
                        count++;
                    }
                }
            }
            return count == 0 ? null : (double)sum / count;
        }

        /// <summary>
        /// Computes the average of a sequence of <see cref="long"/> values that are obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <param name="cancellationToken">A token to observe cancellation.</param>
        /// <returns>The average of the projected values.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence or selector is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the source sequence is empty.</exception>
        /// <exception cref="OverflowException">The sum is larger than <see cref="long.MaxValue"/>.</exception>
        public async ValueTask<double> AveragePagedAsync(Func<TSource, long> selector, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(selector);

            long sum = 0;
            long count = 0;
            await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                checked
                {
                    sum += selector(item);
                    count++;
                }
            }
            if (count == 0) throw new InvalidOperationException("Sequence contains no elements.");
            return (double)sum / count;
        }

        /// <summary>
        /// Computes the average of a sequence of nullable <see cref="long"/> values that are obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <param name="cancellationToken">A token to observe cancellation.</param>
        /// <returns>The average of the projected values, or null if the source sequence is empty or contains only null values.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence or selector is null.</exception>
        public async ValueTask<double?> AveragePagedAsync(Func<TSource, long?> selector, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(selector);

            long sum = 0;
            long count = 0;
            await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                var value = selector(item);
                if (value.HasValue)
                {
                    checked
                    {
                        sum += value.Value;
                        count++;
                    }
                }
            }
            return count == 0 ? null : (double)sum / count;
        }

        /// <summary>
        /// Computes the average of a sequence of <see cref="double"/> values that are obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <param name="cancellationToken">A token to observe cancellation.</param>
        /// <returns>The average of the projected values.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence or selector is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the source sequence is empty.</exception>
        public async ValueTask<double> AveragePagedAsync(Func<TSource, double> selector, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(selector);

            double sum = 0;
            long count = 0;
            await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                sum += selector(item);
                count++;
            }
            if (count == 0) throw new InvalidOperationException("Sequence contains no elements.");
            return sum / count;
        }

        /// <summary>
        /// Computes the average of a sequence of nullable <see cref="double"/> values that are obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <param name="cancellationToken">A token to observe cancellation.</param>
        /// <returns>The average of the projected values, or null if the source sequence is empty or contains only null values.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence or selector is null.</exception>
        public async ValueTask<double?> AveragePagedAsync(Func<TSource, double?> selector, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(selector);

            double sum = 0;
            long count = 0;
            await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                var value = selector(item);
                if (value.HasValue)
                {
                    sum += value.Value;
                    count++;
                }
            }
            return count == 0 ? null : sum / count;
        }

        /// <summary>
        /// Computes the average of a sequence of <see cref="decimal"/> values that are obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <param name="cancellationToken">A token to observe cancellation.</param>
        /// <returns>The average of the projected values.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence or selector is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the source sequence is empty.</exception>
        public async ValueTask<decimal> AveragePagedAsync(Func<TSource, decimal> selector, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(selector);

            decimal sum = 0;
            long count = 0;
            await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                sum += selector(item);
                count++;
            }
            if (count == 0) throw new InvalidOperationException("Sequence contains no elements.");
            return sum / count;
        }

        /// <summary>
        /// Computes the average of a sequence of nullable <see cref="decimal"/> values that are obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <param name="cancellationToken">A token to observe cancellation.</param>
        /// <returns>The average of the projected values, or null if the source sequence is empty or contains only null values.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence or selector is null.</exception>
        public async ValueTask<decimal?> AveragePagedAsync(Func<TSource, decimal?> selector, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(selector);

            decimal sum = 0;
            long count = 0;
            await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                var value = selector(item);
                if (value.HasValue)
                {
                    sum += value.Value;
                    count++;
                }
            }
            return count == 0 ? null : sum / count;
        }

        #endregion
    }
}