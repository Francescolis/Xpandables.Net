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

using Xpandables.Net.Async;

namespace Xpandables.Net.Async;

/// <summary>
/// Provides extension methods to convert various enumerable types to <see cref="IAsyncPagedEnumerable{T}"/>.
/// </summary>
[Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]
public static class AsyncPagedEnumerable
{
    /// <summary>
    ///  
    /// </summary>
    /// <typeparam name="TSource">The type of elements in the collection.</typeparam>
    /// <param name="source">The source async enumerable.</param>
    extension<TSource>(IAsyncEnumerable<TSource> source)
    {
        #region IAsyncEnumerable Extensions

        /// <summary>
        /// Converts an <see cref="IAsyncEnumerable{T}"/> to an <see cref="IAsyncPagedEnumerable{T}"/> with pagination metadata.
        /// </summary>
        /// <param name="paginationFactory">Factory to create pagination metadata.</param>
        /// <returns>An async paged enumerable.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="paginationFactory"/> is null.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IAsyncPagedEnumerable<TSource> ToAsyncPagedEnumerable(
            Func<CancellationToken, ValueTask<PageContext>> paginationFactory)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(paginationFactory);

            return new AsyncPagedEnumerable<TSource, TSource>(source, paginationFactory);
        }

        /// <summary>
        /// Converts an <see cref="IAsyncEnumerable{T}"/> to an <see cref="IAsyncPagedEnumerable{T}"/> with explicit pagination.
        /// </summary>
        /// <param name="pagination">The pagination metadata.</param>
        /// <returns>An async paged enumerable.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="pagination"/> is null.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IAsyncPagedEnumerable<TSource> ToAsyncPagedEnumerable(
            PageContext pagination)
        {
            ArgumentNullException.ThrowIfNull(source);

            return new AsyncPagedEnumerable<TSource, TSource>(source, _ => ValueTask.FromResult(pagination));
        }

        /// <summary>
        /// Converts an <see cref="IAsyncEnumerable{T}"/> to an <see cref="IAsyncPagedEnumerable{T}"/> with total count.
        /// </summary>
        /// <param name="totalCount">The total count of items across all pages.</param>
        /// <returns>An async paged enumerable.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="totalCount"/> is negative.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IAsyncPagedEnumerable<TSource> ToAsyncPagedEnumerable(
            int totalCount)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentOutOfRangeException.ThrowIfNegative(totalCount);

            return new AsyncPagedEnumerable<TSource, TSource>(
                source,
                _ => ValueTask.FromResult(PageContext.Create(totalCount)));
        }

        /// <summary>
        /// Converts an <see cref="IAsyncEnumerable{T}"/> to an <see cref="IAsyncPagedEnumerable{TResult}"/> with mapping and pagination.
        /// </summary>
        /// <typeparam name="TResult">The type of result elements.</typeparam>
        /// <param name="paginationFactory">Factory to create pagination metadata.</param>
        /// <param name="mapper">Function to map source elements to result elements.</param>
        /// <returns>An async paged enumerable of mapped elements.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IAsyncPagedEnumerable<TResult> ToAsyncPagedEnumerable<TResult>(
            Func<CancellationToken, ValueTask<PageContext>> paginationFactory,
            Func<TSource, TResult> mapper)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(paginationFactory);
            ArgumentNullException.ThrowIfNull(mapper);

            return new AsyncPagedEnumerable<TSource, TResult>(
                source, paginationFactory, (s, _) => ValueTask.FromResult(mapper(s)));
        }

        /// <summary>
        /// Converts an <see cref="IAsyncEnumerable{T}"/> to an <see cref="IAsyncPagedEnumerable{TResult}"/> with async mapping and pagination.
        /// </summary>
        /// <typeparam name="TResult">The type of result elements.</typeparam>
        /// <param name="paginationFactory">Factory to create pagination metadata.</param>
        /// <param name="asyncMapper">Async function to map source elements to result elements.</param>
        /// <returns>An async paged enumerable of mapped elements.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IAsyncPagedEnumerable<TResult> ToAsyncPagedEnumerable<TResult>(
            Func<CancellationToken, ValueTask<PageContext>> paginationFactory,
            Func<TSource, CancellationToken, ValueTask<TResult>> asyncMapper)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(paginationFactory);
            ArgumentNullException.ThrowIfNull(asyncMapper);

            return new AsyncPagedEnumerable<TSource, TResult>(source, paginationFactory, asyncMapper);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TSource">The type of elements in the collection.</typeparam>
    /// <param name="source">The source queryable.</param>
    extension<TSource>(IQueryable<TSource> source)
    {
        #endregion

        #region IQueryable Extensions

        /// <summary>
        /// Converts an <see cref="IQueryable{T}"/> to an <see cref="IAsyncPagedEnumerable{T}"/>.
        /// Pagination metadata is automatically extracted from the query expression.
        /// </summary>
        /// <returns>An async paged enumerable.</returns>
        /// <remarks>
        /// This method automatically extracts Skip/Take operations from the query expression 
        /// and computes the total count by executing the base query without pagination.
        /// For complex queries or non-database sources, consider using the overload with a total factory.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IAsyncPagedEnumerable<TSource> ToAsyncPagedEnumerable()
        {
            ArgumentNullException.ThrowIfNull(source);

            return new AsyncPagedEnumerable<TSource, TSource>(source);
        }

        /// <summary>
        /// Converts an <see cref="IQueryable{T}"/> to an <see cref="IAsyncPagedEnumerable{T}"/> with a custom total count factory.
        /// </summary>
        /// <param name="totalFactory">Factory to compute the total count.</param>
        /// <returns>An async paged enumerable.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="totalFactory"/> is null.</exception>
        /// <remarks>
        /// Use this overload when the automatic count computation might fail 
        /// (e.g., for complex queries or non-database sources).
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IAsyncPagedEnumerable<TSource> ToAsyncPagedEnumerable(
            Func<CancellationToken, ValueTask<long>> totalFactory)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(totalFactory);

            return new AsyncPagedEnumerable<TSource, TSource>(source, totalFactory);
        }

        /// <summary>
        /// Converts an <see cref="IQueryable{T}"/> to an <see cref="IAsyncPagedEnumerable{TResult}"/> with synchronous mapping.
        /// </summary>
        /// <typeparam name="TResult">The type of result elements.</typeparam>
        /// <param name="mapper">Function to map source elements to result elements.</param>
        /// <returns>An async paged enumerable of mapped elements.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IAsyncPagedEnumerable<TResult> ToAsyncPagedEnumerable<TResult>(
            Func<TSource, TResult> mapper)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(mapper);

            return new AsyncPagedEnumerable<TSource, TResult>(
                source, null, (s, _) => ValueTask.FromResult(mapper(s)));
        }

        /// <summary>
        /// Converts an <see cref="IQueryable{T}"/> to an <see cref="IAsyncPagedEnumerable{TResult}"/> with asynchronous mapping.
        /// </summary>
        /// <typeparam name="TResult">The type of result elements.</typeparam>
        /// <param name="asyncMapper">Async function to map source elements to result elements.</param>
        /// <returns>An async paged enumerable of mapped elements.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IAsyncPagedEnumerable<TResult> ToAsyncPagedEnumerable<TResult>(
            Func<TSource, CancellationToken, ValueTask<TResult>> asyncMapper)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(asyncMapper);

            return new AsyncPagedEnumerable<TSource, TResult>(source, null, asyncMapper);
        }

        /// <summary>
        /// Converts an <see cref="IQueryable{T}"/> to an <see cref="IAsyncPagedEnumerable{TResult}"/> with mapping and custom total factory.
        /// </summary>
        /// <typeparam name="TResult">The type of result elements.</typeparam>
        /// <param name="totalFactory">Factory to compute the total count.</param>
        /// <param name="mapper">Function to map source elements to result elements.</param>
        /// <returns>An async paged enumerable of mapped elements.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IAsyncPagedEnumerable<TResult> ToAsyncPagedEnumerable<TResult>(
            Func<CancellationToken, ValueTask<long>> totalFactory,
            Func<TSource, TResult> mapper)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(totalFactory);
            ArgumentNullException.ThrowIfNull(mapper);

            return new AsyncPagedEnumerable<TSource, TResult>(source, totalFactory, (s, _) => ValueTask.FromResult(mapper(s)));
        }

        /// <summary>
        /// Converts an <see cref="IQueryable{T}"/> to an <see cref="IAsyncPagedEnumerable{TResult}"/> with async mapping and custom total factory.
        /// </summary>
        /// <typeparam name="TResult">The type of result elements.</typeparam>
        /// <param name="totalFactory">Factory to compute the total count.</param>
        /// <param name="asyncMapper">Async function to map source elements to result elements.</param>
        /// <returns>An async paged enumerable of mapped elements.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IAsyncPagedEnumerable<TResult> ToAsyncPagedEnumerable<TResult>(
            Func<CancellationToken, ValueTask<long>> totalFactory,
            Func<TSource, CancellationToken, ValueTask<TResult>> asyncMapper)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(totalFactory);
            ArgumentNullException.ThrowIfNull(asyncMapper);

            return new AsyncPagedEnumerable<TSource, TResult>(source, totalFactory, asyncMapper);
        }
    }

    #endregion
}