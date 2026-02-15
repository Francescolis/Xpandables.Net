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
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace System.Collections.Generic;

/// <summary>
/// Specifies the strategy for managing pagination in iterators or paginated data contexts.
/// </summary>
/// <remarks>This enumeration defines different approaches to updating and managing pagination state. Use the
/// appropriate strategy based on whether pagination is controlled at the page level, item level, or not managed at
/// all.</remarks>
public enum PaginationStrategy
{
    /// <summary>
    /// No specific strategy is applied. Pagination may not be updated or managed inside iterators.
    /// </summary>
    None = 0,
    /// <summary>
    /// The pagination gets updated based on page numbers, where each page corresponds to a fixed number of items.
    /// </summary>
    PerPage = 1,
    /// <summary>
    /// The pagination gets updated based on individual items, allowing for more granular control over the pagination state.
    /// </summary>
    PerItem = 2
}

/// <summary>
/// Represents the pagination metadata, including information about the total number of items, page size,
/// current page, and continuation token for retrieving additional results.
/// </summary>
/// <remarks>This type is typically used to manage and navigate paginated data in scenarios where results are
/// divided into discrete pages. It provides metadata about the pagination state, such as the total number of items
/// available, the size of each page, and the current page being viewed. The continuation token can be used to fetch the
/// next set of results in a paginated operation.
/// <para>
/// Page numbers are 1-based by convention: the first page is page 1. A value of 0 indicates no page is selected.
/// </para>
/// </remarks>
public readonly record struct Pagination
{
    private readonly int _pageSize;
    private readonly int _currentPage;
    private readonly int? _totalCount;

    /// <summary>
    /// Gets the total number of items across all pages. 
    /// A null value indicates the total count is unknown.
    /// </summary>
    public required int? TotalCount
    {
        get => _totalCount;
        init
        {
            if (value.HasValue)
            {
                ArgumentOutOfRangeException.ThrowIfNegative(value.Value, nameof(TotalCount));
            }
            _totalCount = value;
        }
    }

    /// <summary>
    /// Gets the number of items to include on each page of results.
    /// Must be zero or greater.
    /// </summary>
    public required int PageSize
    {
        get => _pageSize;
        init
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value, nameof(PageSize));
            _pageSize = value;
        }
    }

    /// <summary>
    /// Gets the current page number in a paginated collection.
    /// Must be zero or greater. Page numbers are 1-based; 0 indicates no page is selected.
    /// </summary>
    public required int CurrentPage
    {
        get => _currentPage;
        init
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value, nameof(CurrentPage));
            _currentPage = value;
        }
    }

    /// <summary>
    /// Gets the continuation token used to retrieve the next set of results in a paginated operation.
    /// A null or empty value indicates no continuation token is available.
    /// </summary>
    public required string? ContinuationToken { get; init; }

    /// <summary>
    /// Gets an empty <see cref="Pagination"/> instance with default values.
    /// </summary>
    public static Pagination Empty { get; } = new()
    {
        PageSize = 0,
        CurrentPage = 0,
        ContinuationToken = null,
        TotalCount = null
    };

    /// <summary>
    /// Creates a new instance of the <see cref="Pagination"/> struct with the specified pagination parameters.
    /// </summary>
    /// <param name="pageSize">The number of items per page. Must be zero or greater.</param>
    /// <param name="currentPage">The current page number. Must be zero or greater.</param>
    /// <param name="continuationToken">An optional token used to retrieve the next set of results in a paginated sequence. Can be <see
    /// langword="null"/>.</param>
    /// <param name="totalCount">An optional total count of items in the paginated collection. Can be <see langword="null"/> if the total
    /// count is unknown.</param>
    /// <returns>A new <see cref="Pagination"/> instance initialized with the specified parameters.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="pageSize"/> or <paramref name="currentPage"/> is negative.</exception>
    public static Pagination Create(
        int pageSize,
        int currentPage,
        string? continuationToken = null,
        int? totalCount = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(pageSize);
        ArgumentOutOfRangeException.ThrowIfNegative(currentPage);
        if (totalCount.HasValue)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(totalCount.Value);
        }

        return new()
        {
            PageSize = pageSize,
            CurrentPage = currentPage,
            ContinuationToken = continuationToken,
            TotalCount = totalCount
        };
    }

    /// <summary>
    /// Creates a new instance of the <see cref="Pagination"/> struct with the specified total count only.
    /// </summary>
    /// <param name="totalCount">The total number of items to be represented by the <see cref="Pagination"/>. Must be zero or greater.</param>
    /// <returns>A new <see cref="Pagination"/> instance with the specified total count and default values for other
    /// properties.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="totalCount"/> is negative.</exception>
    public static Pagination FromTotalCount(int totalCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(totalCount);

        return new()
        {
            PageSize = 0,
            CurrentPage = 0,
            ContinuationToken = null,
            TotalCount = totalCount
        };
    }

    /// <summary>
    /// Creates a new <see cref="Pagination"/> instance by advancing to the next page.
    /// </summary>
    /// <param name="continuationToken">An optional new continuation token for the next page.</param>
    /// <returns>A new <see cref="Pagination"/> instance representing the next page.</returns>
    public Pagination NextPage(string? continuationToken = null) => this with
    {
        CurrentPage = CurrentPage + 1,
        ContinuationToken = continuationToken
    };

    /// <summary>
    /// Creates a new <see cref="Pagination"/> instance by moving to the previous page.
    /// </summary>
    /// <returns>A new <see cref="Pagination"/> instance representing the previous page, 
    /// or the current instance if already on the first page.</returns>
    public Pagination PreviousPage() => HasPreviousPage
        ? this with { CurrentPage = CurrentPage - 1, ContinuationToken = null }
        : this;

    /// <summary>
    /// Creates a new <see cref="Pagination"/> instance with an updated total count.
    /// </summary>
    /// <param name="totalCount">The new total count value.</param>
    /// <returns>A new <see cref="Pagination"/> instance with the updated total count.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="totalCount"/> is negative.</exception>
    public Pagination WithTotalCount(int totalCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(totalCount);
        return this with { TotalCount = totalCount };
    }

    /// <summary>
    /// Gets a value indicating whether the total count is unknown.
    /// </summary>
    [MemberNotNullWhen(false, nameof(TotalCount))]
    public bool IsUnknown => !TotalCount.HasValue || TotalCount.Value < 0;

    /// <summary>
    /// Gets the number of items to skip based on the current page number and page size.
    /// </summary>
    /// <remarks>This property is typically used for pagination scenarios to determine the starting 
    /// point for retrieving a subset of items from a larger collection.</remarks>
    public int Skip => CurrentPage > 0 && PageSize > 0
        ? (CurrentPage - 1) * PageSize
        : 0;

    /// <summary>
    /// Gets the number of items to retrieve in a single page of results.
    /// </summary>
    public int Take => PageSize;

    /// <summary>
    /// Gets a value indicating whether there is a continuation token available.
    /// </summary>
    [MemberNotNullWhen(true, nameof(ContinuationToken))]
    public bool HasContinuation => !string.IsNullOrEmpty(ContinuationToken);

    /// <summary>
    /// Gets a value indicating whether the current page is the first page.
    /// </summary>
    public bool IsFirstPage => CurrentPage <= 1;

    /// <summary>
    /// Gets a value indicating whether the current page is the last page in the paginated data set.
    /// </summary>
    /// <remarks>Returns false if the total count is unknown.</remarks>
    public bool IsLastPage => TotalCount.HasValue
        && PageSize > 0
        && CurrentPage * PageSize >= TotalCount.Value;

    /// <summary>
    /// Gets a value indicating whether there is a previous page available.
    /// </summary>
    public bool HasPreviousPage => CurrentPage > 1;

    /// <summary>
    /// Gets a value indicating whether there is a next page available.
    /// </summary>
    /// <remarks>Returns false if the total count is unknown.</remarks>
    public bool HasNextPage => TotalCount.HasValue
        && PageSize > 0
        && CurrentPage * PageSize < TotalCount.Value;

    /// <summary>
    /// Gets a value indicating whether pagination is active in the query.
    /// </summary>
    public bool IsPaginated => Skip > 0 || Take > 0;

    /// <summary>
    /// Gets the total number of pages based on the page size and total count.
    /// </summary>
    /// <remarks>Returns null if the total count is unknown or page size is zero.</remarks>
    public int? TotalPages => TotalCount.HasValue && PageSize > 0
        ? (TotalCount.Value + PageSize - 1) / PageSize
        : null;
}

/// <summary>
/// Provides a source generation context for serializing and deserializing Pagination objects using
/// System.Text.Json.
/// </summary>
/// <remarks>This context enables high-performance JSON serialization for the Pagination type by
/// leveraging source generation. Use this context with System.Text.Json APIs to improve serialization speed and
/// reduce runtime reflection overhead.</remarks>
[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(Pagination))]
public partial class PaginationJsonContext : JsonSerializerContext
{
}