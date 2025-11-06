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

namespace Xpandables.Net.AsyncPaged.Extensions;

/// <summary>
/// Provides performance-optimized extension methods for materializing and precomputing pagination metadata
/// for <see cref="IAsyncPagedEnumerable{T}"/> sequences.
/// </summary>
/// <remarks>
/// These extensions help avoid double enumeration when pagination metadata requires counting items.
/// Use materialization for small to medium datasets where enumeration will happen multiple times or
/// when accurate pagination metadata is critical.
/// </remarks>
public static class MaterializationExtensions
{
    /// <summary>
    /// Extension methods for <see cref="IAsyncPagedEnumerable{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The source async paged enumerable.</param>
    extension<T>(IAsyncPagedEnumerable<T> source)
    {
        /// <summary>
        /// Eagerly computes pagination metadata to avoid computation during enumeration.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The same paged enumerable with pre-computed pagination metadata.</returns>
        /// <remarks>
        /// PERFORMANCE: Use this when you know pagination will be accessed multiple times or before enumeration starts.
        /// This triggers the lazy computation mechanism once and caches the result.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<IAsyncPagedEnumerable<T>> PrecomputePaginationAsync(
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);
            
            // Force pagination computation by calling GetPaginationAsync
            // This will trigger the lazy computation and cache the result
            _ = await source.GetPaginationAsync(cancellationToken).ConfigureAwait(false);
            
            return source;
        }
    }

    /// <summary>
    /// Fully materializes the async paged enumerable into memory with accurate pagination metadata.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The source async paged enumerable.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A new paged enumerable with all items materialized and accurate pagination.</returns>
    /// <remarks>
    /// PERFORMANCE: This loads all items into memory. Use for:
    /// - Small to medium datasets (typically &lt; 10,000 items)
    /// - Scenarios requiring multiple enumerations
    /// - When accurate total count is critical
    /// - When source enumeration is expensive (e.g., database queries)
    /// 
    /// Avoid for large datasets that would cause memory pressure.
    /// Consider using streaming with PrecomputePaginationAsync instead.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is null.</exception>
    public static async ValueTask<IAsyncPagedEnumerable<T>> MaterializeAsync<T>(
        this IAsyncPagedEnumerable<T> source,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        
        // Enumerate and collect all items
        var items = await source.ToListAsync(cancellationToken).ConfigureAwait(false);
        
        // Create pagination with accurate count
        var pagination = Pagination.Create(
            pageSize: items.Count,
            currentPage: 1,
            totalCount: items.Count);
        
        // Return new paged enumerable with materialized items
        return new AsyncPagedEnumerable<T>(
            items.ToAsyncEnumerable(),
            _ => ValueTask.FromResult(pagination));
    }

    /// <summary>
    /// Materializes the async paged enumerable into memory with custom page size.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The source async paged enumerable.</param>
    /// <param name="pageSize">The page size to use in pagination metadata.</param>
    /// <param name="currentPage">The current page number (1-based).</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A new paged enumerable with all items materialized.</returns>
    /// <remarks>
    /// PERFORMANCE: Use this to materialize a subset of results with specific pagination metadata.
    /// Useful when implementing server-side pagination where you want to materialize just the current page.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="pageSize"/> or <paramref name="currentPage"/> is less than or equal to zero.</exception>
    public static async ValueTask<IAsyncPagedEnumerable<T>> MaterializeAsync<T>(
        this IAsyncPagedEnumerable<T> source,
        int pageSize,
        int currentPage = 1,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(currentPage);
        
        // Enumerate and collect all items
        var items = await source.ToListAsync(cancellationToken).ConfigureAwait(false);
        
        // Create pagination with accurate count and specified page size
        var pagination = Pagination.Create(
            pageSize: pageSize,
            currentPage: currentPage,
            totalCount: items.Count);
        
        // Return new paged enumerable with materialized items
        return new AsyncPagedEnumerable<T>(
            items.ToAsyncEnumerable(),
            _ => ValueTask.FromResult(pagination));
    }

    /// <summary>
    /// Converts an async enumerable to a materialized paged enumerable with accurate pagination.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The source async enumerable.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A paged enumerable with all items materialized.</returns>
    /// <remarks>
    /// PERFORMANCE: This is a convenience method that enumerates the source once,
    /// counts items, and creates a paged enumerable with accurate pagination metadata.
    /// Best for small to medium datasets where accurate count is needed.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is null.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async ValueTask<IAsyncPagedEnumerable<T>> ToMaterializedAsyncPagedEnumerable<T>(
        this IAsyncEnumerable<T> source,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        
        // Enumerate and collect all items in one pass
        var items = await source.ToListAsync(cancellationToken).ConfigureAwait(false);
        
        // Create pagination with accurate count
        var pagination = Pagination.FromTotalCount(items.Count);
        
        // Return paged enumerable with pre-materialized items
        return new AsyncPagedEnumerable<T>(
            items.ToAsyncEnumerable(),
            _ => ValueTask.FromResult(pagination));
    }

    /// <summary>
    /// Converts an async enumerable to a materialized paged enumerable with custom pagination.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The source async enumerable.</param>
    /// <param name="pageSize">The page size to use in pagination metadata.</param>
    /// <param name="currentPage">The current page number (1-based).</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A paged enumerable with all items materialized.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="pageSize"/> or <paramref name="currentPage"/> is less than or equal to zero.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async ValueTask<IAsyncPagedEnumerable<T>> ToMaterializedAsyncPagedEnumerable<T>(
        this IAsyncEnumerable<T> source,
        int pageSize,
        int currentPage = 1,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(currentPage);
        
        // Enumerate and collect all items
        var items = await source.ToListAsync(cancellationToken).ConfigureAwait(false);
        
        // Create pagination with specified parameters
        var pagination = Pagination.Create(
            pageSize: pageSize,
            currentPage: currentPage,
            totalCount: items.Count);
        
        return new AsyncPagedEnumerable<T>(
            items.ToAsyncEnumerable(),
            _ => ValueTask.FromResult(pagination));
    }
}
