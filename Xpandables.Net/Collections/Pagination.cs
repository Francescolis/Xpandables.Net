/*******************************************************************************
 * Copyright (C) 2024 Francis-Black EWANE
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
namespace Xpandables.Net.Collections;

/// <summary>
/// Contains pagination source information extracted from a query.
/// </summary>
public readonly record struct PaginationSource<TResult>
{
    /// <summary>
    /// The number of items to skip.
    /// </summary>
    public readonly required int? Skip { get; init; }

    /// <summary>
    /// The number of items to take.
    /// </summary>
    public readonly required int? Take { get; init; }

    /// <summary>
    /// The original query without Skip/Take methods applied.
    /// </summary>
    public readonly required IQueryable<TResult> QueryWithoutPagination { get; init; }
}

/// <summary>
/// Represents pagination metadata.
/// </summary>
public readonly record struct Pagination
{
    /// <summary>
    /// The number of items to skip (null if no Skip was found).
    /// </summary>
    public readonly required int? Skip { get; init; }

    /// <summary>
    /// The number of items to take (null if no Take was found).
    /// </summary>
    public readonly required int? Take { get; init; }

    /// <summary>
    /// The total number of items across all pages.
    /// </summary>
    public readonly required long TotalCount { get; init; }

    /// <summary>
    /// The current page number (1-based) or null if pagination is not detected.
    /// </summary>
    public readonly int? PageNumber => Skip.HasValue && Take.HasValue ? (Skip.Value / Take.Value) + 1 : null;

    /// <summary>
    /// The page size or null if Take is not specified.
    /// </summary>
    public readonly int? PageSize => Take;

    /// <summary>
    /// The total number of pages or null if pagination is not fully specified.
    /// </summary>
    public readonly int? TotalPages => Take.HasValue && Take.Value > 0
        ? (int)Math.Ceiling((double)TotalCount / Take.Value)
        : null;

    /// <summary>
    /// A value indicating whether there is a previous page.
    /// </summary>
    public readonly bool HasPreviousPage => PageNumber.HasValue && PageNumber.Value > 1;

    /// <summary>
    /// A value indicating whether there is a next page.
    /// </summary>
    public readonly bool HasNextPage => PageNumber.HasValue && TotalPages.HasValue && PageNumber.Value < TotalPages.Value;

    /// <summary>
    /// A value indicating whether pagination was detected in the query.
    /// </summary>
    public readonly bool IsPaginated => Skip.HasValue || Take.HasValue;

    /// <summary>
    /// Creates pagination info from explicit values.
    /// </summary>
    /// <param name="skip">Number of items to skip.</param>
    /// <param name="take">Number of items to take.</param>
    /// <param name="totalCount">Total count of items.</param>
    /// <returns>Pagination info.</returns>
    public static Pagination With(int? skip, int? take, long totalCount) =>
        new() { Skip = skip, Take = take, TotalCount = totalCount };

    /// <summary>
    /// Creates pagination info without pagination (just total count).
    /// </summary>
    /// <param name="totalCount">Total count of items.</param>
    /// <returns>Pagination info without pagination values.</returns>
    public static Pagination Without(long totalCount) => With(null, null, totalCount);

    /// <summary>
    /// Creates pagination info without pagination.
    /// </summary>
    /// <returns>Pagination info without pagination values.</returns>
    public static Pagination Without() => Without(0);

    /// <summary>
    /// Creates a new instance of <see cref="PaginationSource{TResult}"/> with the specified skip, take, and query.
    /// </summary>
    /// <param name="skip">The number of items to skip (null if no Skip was found).</param>
    /// <param name="take">The number of items to take (null if no Take was found).</param>
    /// <param name="queryWithoutPagination">The original query without pagination applied.</param>
    /// <returns>The new <see cref="PaginationSource{TResult}"/> instance.</returns>
    public static PaginationSource<TSource> WithSource<TSource>(
        int? skip,
        int? take,
        IQueryable<TSource> queryWithoutPagination) => new()
        {
            Skip = skip,
            Take = take,
            QueryWithoutPagination = queryWithoutPagination
        };
}