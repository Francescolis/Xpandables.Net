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
/// Provides extension methods for working with <see cref="IAsyncEnumerable{T}"/> sequences, enabling conversion to
/// paged asynchronous enumerables with pagination metadata.
/// </summary>
/// <remarks>These extension methods facilitate the transformation of asynchronous enumerables into paged
/// enumerables, allowing for the inclusion of pagination information such as total item count or custom pagination
/// metadata. This is useful when implementing APIs or data sources that support paged results in asynchronous
/// scenarios.</remarks>
public static class AsyncEnumerableExtensions
{
    /// <summary>
    /// Extension methods for <see cref="IAsyncEnumerable{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The source async enumerable.</param>
    extension<T>(IAsyncEnumerable<T> source)
    {
        /// <summary>
        /// Creates an asynchronous paged enumerable that enables iteration over the source collection in pages.
        /// </summary>
        /// <returns>An <see cref="IAsyncPagedEnumerable{T}"/> that provides asynchronous, paged access to the source collection.</returns>
        public IAsyncPagedEnumerable<T> ToAsyncPagedEnumerable()
        {
            ArgumentNullException.ThrowIfNull(source);
            return AsyncPagedEnumerable.Create(source);
        }

        /// <summary>
        /// Converts an <see cref="IAsyncEnumerable{T}"/> to an <see cref="IAsyncPagedEnumerable{T}"/> with pagination metadata.
        /// </summary>
        /// <param name="paginationFactory">Factory to create pagination metadata.</param>
        /// <returns>An async paged enumerable.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="paginationFactory"/> is null.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IAsyncPagedEnumerable<T> ToAsyncPagedEnumerable(Func<CancellationToken, ValueTask<Pagination>> paginationFactory)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(paginationFactory);

            return AsyncPagedEnumerable.Create(source, paginationFactory);
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
}
