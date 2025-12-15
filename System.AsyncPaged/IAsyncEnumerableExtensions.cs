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

namespace System.Collections.Generic;

/// <summary>
/// Provides extension methods for working with asynchronous enumerables.
/// </summary>
/// <remarks>This static class contains helper methods that extend the functionality of types implementing <see
/// cref="IAsyncEnumerable{T}"/>. Use these methods to simplify common operations, such as retrieving type information
/// or manipulating paged asynchronous sequences.</remarks>
public static class IAsyncEnumerableExtensions
{
    extension<T>(IAsyncEnumerable<T> source)
    {
        /// <summary>
        /// Creates an asynchronous paged enumerable that enables iteration over the source collection in pages.
        /// </summary>
        /// <param name="strategy">The pagination strategy to apply.</param>
        /// <returns>An <see cref="IAsyncPagedEnumerable{T}"/> that provides asynchronous, paged access to the source collection.</returns>
        public IAsyncPagedEnumerable<T> ToAsyncPagedEnumerable(PaginationStrategy strategy = PaginationStrategy.None)
        {
            ArgumentNullException.ThrowIfNull(source);
            return AsyncPagedEnumerable.Create(source, strategy: strategy);
        }

        /// <summary>
        /// Converts an <see cref="IAsyncEnumerable{T}"/> to an <see cref="IAsyncPagedEnumerable{T}"/> with pagination metadata.
        /// </summary>
        /// <param name="paginationFactory">Factory to create pagination metadata.</param>
        /// <param name="strategy">The pagination strategy to apply.</param>
        /// <returns>An async paged enumerable.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="paginationFactory"/> is null.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IAsyncPagedEnumerable<T> ToAsyncPagedEnumerable(
            Func<CancellationToken, ValueTask<Pagination>> paginationFactory,
            PaginationStrategy strategy = PaginationStrategy.None)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(paginationFactory);

            return AsyncPagedEnumerable.Create(source, paginationFactory, strategy);
        }

        /// <summary>
        /// Converts an <see cref="IAsyncEnumerable{T}"/> to an <see cref="IAsyncPagedEnumerable{T}"/> with explicit pagination.
        /// </summary>
        /// <param name="pagination">The pagination metadata.</param>
        /// <returns>An async paged enumerable.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IAsyncPagedEnumerable<T> ToAsyncPagedEnumerable(Pagination pagination)
        {
            ArgumentNullException.ThrowIfNull(source);

            return AsyncPagedEnumerable.Create(source, _ => ValueTask.FromResult(pagination));
        }

        /// <summary>
        /// Converts an <see cref="IAsyncEnumerable{T}"/> to an <see cref="IAsyncPagedEnumerable{T}"/> with total count.
        /// </summary>
        /// <param name="totalCount">The total count of items across all pages. Must be zero or greater.</param>
        /// <returns>An async paged enumerable.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="totalCount"/> is negative.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IAsyncPagedEnumerable<T> ToAsyncPagedEnumerable(int totalCount)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentOutOfRangeException.ThrowIfNegative(totalCount);

            return AsyncPagedEnumerable.Create(
                source,
                _ => ValueTask.FromResult(Pagination.FromTotalCount(totalCount)));
        }
    }

    /// <summary>
    /// Extension methods for <see cref="IQueryable{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The source queryable.</param>
    extension<T>(IQueryable<T> source)
    {
        /// <summary>
        /// Converts an <see cref="IQueryable{T}"/> to an <see cref="IAsyncPagedEnumerable{T}"/>.
        /// </summary>
        /// <param name="strategy">The pagination strategy to apply.</param>
        /// <returns>An async paged enumerable.</returns>
        /// <remarks>
        /// This method automatically extracts Skip/Take operations from the query expression 
        /// and computes the total count by executing the base query without pagination.
        /// For complex queries or non-database sources, consider using the overload with a total factory.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IAsyncPagedEnumerable<T> ToAsyncPagedEnumerable(PaginationStrategy strategy = PaginationStrategy.None)
        {
            ArgumentNullException.ThrowIfNull(source);

            return AsyncPagedEnumerable.Create(source, strategy: strategy);
        }

        /// <summary>
        /// Converts an <see cref="IQueryable{T}"/> to an <see cref="IAsyncPagedEnumerable{T}"/> with a custom pagination factory.
        /// </summary>
        /// <param name="paginationFactory">Factory to compute the pagination metadata asynchronously.</param>
        /// <param name="strategy">The pagination strategy to apply.</param>
        /// <returns>An async paged enumerable.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="paginationFactory"/> is null.</exception>
        /// <remarks>
        /// Use this overload when the automatic count computation might fail 
        /// (e.g., for complex queries or non-database sources).
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IAsyncPagedEnumerable<T> ToAsyncPagedEnumerable(
            Func<CancellationToken, ValueTask<Pagination>> paginationFactory,
            PaginationStrategy strategy = PaginationStrategy.None)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(paginationFactory);

            return AsyncPagedEnumerable.Create(source, paginationFactory, strategy);
        }
    }
}