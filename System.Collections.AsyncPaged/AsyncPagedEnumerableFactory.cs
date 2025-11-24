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
namespace System.Collections.Generic;

/// <summary>
/// Provides static methods for creating asynchronous paged enumerables from various data sources, enabling efficient
/// asynchronous iteration and paging over large sequences.
/// </summary>
/// <remarks>Use the methods in this class to construct paged asynchronous enumerables from either an asynchronous
/// sequence or a queryable source. Custom pagination strategies can be supplied via a factory delegate to control how
/// pages are retrieved, which is useful for scenarios such as server-driven or dynamic paging. The class is intended
/// for scenarios where data needs to be processed in chunks, such as streaming results from a database or
/// service.</remarks>
public static class AsyncPagedEnumerable
{
    /// <summary>
    /// Creates an asynchronous paged enumerable that yields items from the specified source, optionally using a custom
    /// pagination strategy.
    /// </summary>
    /// <remarks>Use this method to enable paging over an asynchronous sequence, such as when retrieving data
    /// from a service or database in chunks. The pagination factory allows customization of how pages are determined,
    /// which can be useful for scenarios with dynamic or server-driven pagination.</remarks>
    /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
    /// <param name="source">The asynchronous sequence of items to be paged.</param>
    /// <param name="paginationFactory">An optional delegate that provides pagination information for each iteration. If not specified, default
    /// pagination behavior is used.</param>
    /// <param name="strategy">The pagination strategy to apply.</param>
    /// <returns>An asynchronous paged enumerable that iterates over the items in the source sequence according to the specified
    /// pagination strategy.</returns>
    public static IAsyncPagedEnumerable<T> Create<T>(
         IAsyncEnumerable<T> source,
        Func<CancellationToken, ValueTask<Pagination>>? paginationFactory = default,
        PaginationStrategy strategy = PaginationStrategy.None) =>
        new AsyncPagedEnumerable<T>(source, paginationFactory, strategy);

    /// <summary>
    /// Creates an asynchronous paged enumerable over the specified query, optionally using a custom pagination
    /// strategy.
    /// </summary>
    /// <remarks>The returned enumerable supports asynchronous iteration and paging, allowing efficient
    /// retrieval of large result sets. If a custom pagination factory is provided, it will be invoked for each
    /// enumeration to determine pagination behavior. This method does not execute the query until enumeration
    /// begins.</remarks>
    /// <typeparam name="T">The type of elements contained in the query and returned by the paged enumerable.</typeparam>
    /// <param name="query">The source query to enumerate asynchronously in pages. Cannot be null.</param>
    /// <param name="paginationFactory">An optional factory function that provides pagination parameters for each enumeration. If null, default
    /// pagination is used.</param>
    /// <param name="strategy">The pagination strategy to apply.</param>
    /// <returns>An asynchronous paged enumerable that yields elements of type T from the source query according to the specified
    /// or default pagination.</returns>
    public static IAsyncPagedEnumerable<T> Create<T>(
         IQueryable<T> query,
        Func<CancellationToken, ValueTask<Pagination>>? paginationFactory = default,
        PaginationStrategy strategy = PaginationStrategy.None) =>
        new AsyncPagedEnumerable<T>(query, paginationFactory, strategy);

    /// <summary>
    /// Creates an empty asynchronous paged enumerable of the specified type.
    /// </summary>
    /// <remarks>Use this method to obtain an empty paged enumerable when no items are available or as a
    /// default value. The returned enumerable yields no elements and supports pagination according to the provided or
    /// default settings.</remarks>
    /// <typeparam name="T">The type of elements in the paged enumerable.</typeparam>
    /// <param name="pagination">The pagination settings to use for the enumerable. If null, default pagination is applied.</param>
    /// <returns>An empty <see cref="IAsyncPagedEnumerable{T}"/> instance with the specified pagination settings.</returns>
    public static IAsyncPagedEnumerable<T> Empty<T>(Pagination? pagination = null)
    {
        Pagination p = pagination ?? Pagination.Empty;
        return new AsyncPagedEnumerable<T>(
            AsyncEnumerable.Empty<T>(),
            _ => ValueTask.FromResult(p));
    }
}