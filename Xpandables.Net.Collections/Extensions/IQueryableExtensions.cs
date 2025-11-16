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

using Xpandables.Net.Collections.Generic;

namespace Xpandables.Net.Collections.Extensions;

/// <summary>
/// Provides extension methods for working with <see cref="IQueryable{T}"/> sequences, enabling advanced asynchronous
/// paging operations.
/// </summary>
/// <remarks>The methods in this class allow conversion of <see cref="IQueryable{T}"/> queries to asynchronous
/// paged enumerables, supporting scenarios where efficient data paging and total count retrieval are required. These
/// extensions are particularly useful when working with large datasets or implementing server-side paging in
/// data-driven applications. For complex queries or non-database sources, custom total count logic can be supplied to
/// ensure accurate paging behavior.</remarks>
public static class IQueryableExtensions
{
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

            return AsyncPagedEnumerable.Create(source);
        }

        /// <summary>
        /// Converts an <see cref="IQueryable{T}"/> to an <see cref="IAsyncPagedEnumerable{T}"/> with a custom pagination factory.
        /// </summary>
        /// <param name="paginationFactory">Factory to compute the pagination metadata asynchronously.</param>
        /// <returns>An async paged enumerable.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="paginationFactory"/> is null.</exception>
        /// <remarks>
        /// Use this overload when the automatic count computation might fail 
        /// (e.g., for complex queries or non-database sources).
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IAsyncPagedEnumerable<T> ToAsyncPagedEnumerable(Func<CancellationToken, ValueTask<Pagination>> paginationFactory)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(paginationFactory);

            return AsyncPagedEnumerable.Create(source, paginationFactory);
        }
    }
}