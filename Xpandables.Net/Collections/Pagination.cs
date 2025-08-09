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
/// Represents pagination metadata.
/// </summary>
/// <param name="Skip">The number of items to skip (null if no Skip was found).</param>
/// <param name="Take">The number of items to take (null if no Take was found).</param>
/// <param name="TotalCount">The total number of items across all pages.</param>
public readonly record struct Pagination(int? Skip, int? Take, long TotalCount)
{
    /// <summary>
    /// Gets the current page number (1-based) or null if pagination is not detected.
    /// </summary>
    public int? PageNumber => Skip.HasValue && Take.HasValue ? (Skip.Value / Take.Value) + 1 : null;

    /// <summary>
    /// Gets the page size or null if Take is not specified.
    /// </summary>
    public int? PageSize => Take;

    /// <summary>
    /// Gets the total number of pages or null if pagination is not fully specified.
    /// </summary>
    public int? TotalPages => Take.HasValue && Take.Value > 0
        ? (int)Math.Ceiling((double)TotalCount / Take.Value)
        : null;

    /// <summary>
    /// Gets a value indicating whether there is a previous page.
    /// </summary>
    public bool HasPreviousPage => PageNumber.HasValue && PageNumber.Value > 1;

    /// <summary>
    /// Gets a value indicating whether there is a next page.
    /// </summary>
    public bool HasNextPage => PageNumber.HasValue && TotalPages.HasValue && PageNumber.Value < TotalPages.Value;

    /// <summary>
    /// Gets a value indicating whether pagination was detected in the query.
    /// </summary>
    public bool IsPaginated => Skip.HasValue || Take.HasValue;

    /// <summary>
    /// Creates pagination info from explicit values.
    /// </summary>
    /// <param name="skip">Number of items to skip.</param>
    /// <param name="take">Number of items to take.</param>
    /// <param name="totalCount">Total count of items.</param>
    /// <returns>Pagination info.</returns>
    public static Pagination Create(int skip, int take, long totalCount) =>
        new(skip, take, totalCount);

    /// <summary>
    /// Creates pagination info without pagination (just total count).
    /// </summary>
    /// <param name="totalCount">Total count of items.</param>
    /// <returns>Pagination info without pagination values.</returns>
    public static Pagination WithoutPagination(long totalCount) =>
        new(null, null, totalCount);
}