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
using System.Runtime.CompilerServices;

namespace Xpandables.Net.Async;

/// <summary>
/// Provides extension methods to convert various enumerable types to <see cref="IAsyncPagedEnumerable{T}"/>.
/// </summary>
[SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Extension pattern with C# 14")]
public static class AsyncPagedEnumerable
{
    /// <summary>
    /// Extension methods for <see cref="IAsyncEnumerable{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The source async enumerable.</param>
    extension<T>(IAsyncEnumerable<T> source)
    {
        #region IAsyncEnumerable Extensions

        /// <summary>
        /// Converts an <see cref="IAsyncEnumerable{T}"/> to an <see cref="IAsyncPagedEnumerable{T}"/> with pagination metadata.
        /// </summary>
        /// <param name="paginationFactory">Factory to create pagination metadata.</param>
        /// <returns>An async paged enumerable.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="paginationFactory"/> is null.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IAsyncPagedEnumerable<T> ToAsyncPagedEnumerable(
            Func<CancellationToken, ValueTask<Pagination>> paginationFactory)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(paginationFactory);

            return new AsyncPagedEnumerable<T>(source, paginationFactory);
        }

        /// <summary>
        /// Converts an <see cref="IAsyncEnumerable{T}"/> to an <see cref="IAsyncPagedEnumerable{T}"/> with explicit pagination.
        /// </summary>
        /// <param name="pagination">The pagination metadata.</param>
        /// <returns>An async paged enumerable.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IAsyncPagedEnumerable<T> ToAsyncPagedEnumerable(
            Pagination pagination)
        {
            ArgumentNullException.ThrowIfNull(source);

            return new AsyncPagedEnumerable<T>(source, _ => ValueTask.FromResult(pagination));
        }

        /// <summary>
        /// Converts an <see cref="IAsyncEnumerable{T}"/> to an <see cref="IAsyncPagedEnumerable{T}"/> with total count.
        /// </summary>
        /// <param name="totalCount">The total count of items across all pages. Must be zero or greater.</param>
        /// <returns>An async paged enumerable.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="totalCount"/> is negative.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IAsyncPagedEnumerable<T> ToAsyncPagedEnumerable(
            int totalCount)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentOutOfRangeException.ThrowIfNegative(totalCount);

            return new AsyncPagedEnumerable<T>(
                source,
                _ => ValueTask.FromResult(Pagination.FromTotalCount(totalCount)));
        }

        #endregion
    }

    /// <summary>
    /// Extension methods for <see cref="IQueryable{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The source queryable.</param>
    extension<T>(IQueryable<T> source)
    {
        #region IQueryable Extensions

        /// <summary>
        /// Converts an <see cref="IQueryable{T}"/> to an <see cref="IAsyncPagedEnumerable{T}"/>.
        /// </summary>
        /// <returns>An async paged enumerable.</returns>
        /// <remarks>
        /// This method automatically extracts Skip/Take operations from the query expression 
        /// and computes the total count by executing the base query without pagination.
        /// For complex queries or non-database sources, consider using the overload with a total factory.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IAsyncPagedEnumerable<T> ToAsyncPagedEnumerable()
        {
            ArgumentNullException.ThrowIfNull(source);

            return new AsyncPagedEnumerable<T>(source);
        }

        /// <summary>
        /// Converts an <see cref="IQueryable{T}"/> to an <see cref="IAsyncPagedEnumerable{T}"/> with a custom total count factory.
        /// </summary>
        /// <param name="totalFactory">Factory to compute the total count asynchronously.</param>
        /// <returns>An async paged enumerable.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="totalFactory"/> is null.</exception>
        /// <remarks>
        /// Use this overload when the automatic count computation might fail 
        /// (e.g., for complex queries or non-database sources).
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IAsyncPagedEnumerable<T> ToAsyncPagedEnumerable(
            Func<CancellationToken, ValueTask<long>> totalFactory)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(totalFactory);

            return new AsyncPagedEnumerable<T>(source, totalFactory);
        }

        #endregion
    }
}