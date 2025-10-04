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

using System.Text.Json.Serialization;

namespace System.Net.Async;

/// <summary>
/// Specifies the strategy for managing pagination in iterators or paginated data contexts.
/// </summary>
/// <remarks>This enumeration defines different approaches to updating and managing pagination state. Use the
/// appropriate strategy based on whether pagination is controlled at the page level, item level, or not managed at
/// all.</remarks>
public enum PageContextStrategy
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
/// Represents the context for a paginated collection, including information about the total number of items, page size,
/// current page, and continuation token for retrieving additional results.
/// </summary>
/// <remarks>This type is typically used to manage and navigate paginated data in scenarios where results are
/// divided into discrete pages. It provides metadata about the pagination state, such as the total number of items
/// available, the size of each page, and the current page being viewed. The continuation token can be used to fetch the
/// next set of results in a paginated operation.</remarks>
public readonly record struct PageContext
{
    /// <summary>
    /// The total number of items across all pages.
    /// </summary>
    public readonly required int? TotalCount { get; init; }

    /// <summary>
    /// Gets the number of items to include on each page of results.
    /// </summary>
    public readonly required int PageSize { get; init; }

    /// <summary>
    /// Gets the current page number in a paginated collection.
    /// </summary>
    public readonly required int CurrentPage { get; init; }

    /// <summary>
    /// Gets the continuation token used to retrieve the next set of results in a paginated operation.
    /// </summary>
    public readonly required string? ContinuationToken { get; init; }
}

/// <summary>
/// Provides a source generation context for serializing and deserializing PageContext objects using
/// System.Text.Json.
/// </summary>
/// <remarks>This context enables high-performance JSON serialization for the PageContext type by
/// leveraging source generation. Use this context with System.Text.Json APIs to improve serialization speed and
/// reduce runtime reflection overhead.</remarks>
[JsonSerializable(typeof(PageContext))]
public partial class PageContextSourceGenerationContext : JsonSerializerContext
{
}


/// <summary>
/// Provides extension methods for <see cref="PageContext"/> instances.
/// </summary>
/// <remarks>This class contains utility methods that extend the functionality of the <see cref="PageContext"/>
/// type. These methods are designed to simplify common operations and enhance usability.</remarks>
[Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]
public static class PageContextInstancesExtensions
{
    /// <summary>
    /// Provides extension methods for <see cref="PageContext"/> instances.
    /// </summary>
    extension(PageContext context)
    {
        /// <summary>
        /// Gets a value indicating whether the total count is unknown.
        /// </summary>
        public bool IsUnknown => (context.TotalCount ?? -1) < 0;

        /// <summary>
        /// Gets the number of items to skip based on the current page number and page size.
        /// </summary>
        /// <remarks>This property is typically used for pagination scenarios to determine the starting 
        /// point for retrieving a subset of items from a larger collection.</remarks>
        public int Skip => (context.CurrentPage - 1) * context.PageSize;

        /// <summary>
        /// Gets the number of items to retrieve in a single page of results.
        /// </summary>
        public int Take => context.PageSize;

        /// <summary>
        /// Gets a value indicating whether there is a continuation token available.
        /// </summary>
        public bool HasContinuation => !string.IsNullOrEmpty(context.ContinuationToken);

        /// <summary>
        /// Gets a value indicating whether the current page is the first page.
        /// </summary>
        public bool IsFirstPage => context.CurrentPage <= 1;

        /// <summary>
        /// Gets a value indicating whether the current page is the last page in the paginated data set.
        /// </summary>
        /// <remarks>The property evaluates whether the total number of items in the data set has been
        /// reached  based on the current page number and page size.</remarks>
        public bool IsLastPage => context.CurrentPage * context.PageSize >= (context.TotalCount ?? -1);

        /// <summary>
        /// A value indicating whether there is a previous page.
        /// </summary>
        public bool HasPreviousPage => context.CurrentPage > 1;

        /// <summary>
        /// A value indicating whether there is a next page.
        /// </summary>
        public bool HasNextPage => context.CurrentPage * context.PageSize < (context.TotalCount ?? -1);

        /// <summary>
        /// A value indicating whether pagination was detected in the query.
        /// </summary>
        public bool IsPaginated => context.Skip > 0 || context.Take > 0;
    }
}

/// <summary>
/// Provides extension methods for <see cref="PageContext"/> instances.
/// </summary>
/// <remarks>This class contains utility methods that extend the functionality of the <see cref="PageContext"/>
/// type. These methods are designed to simplify common operations and enhance usability.</remarks>
[Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]
public static class PageContextStaticExtensions
{
    /// <summary>
    /// Provides extension methods for <see cref="PageContext"/> instances.
    /// </summary>
    extension(PageContext)
    {
        /// <summary>
        /// Creates a new instance of the <see cref="PageContext"/> class with the specified pagination parameters.
        /// </summary>
        /// <param name="pageSize">The number of items per page. Must be greater than zero.</param>
        /// <param name="currentPage">The current page number. Must be zero or greater.</param>
        /// <param name="continuationToken">An optional token used to retrieve the next set of results in a paginated sequence. Can be <see
        /// langword="null"/>.</param>
        /// <param name="totalCount">An optional total count of items in the paginated collection. Can be <see langword="null"/> if the total
        /// count is unknown.</param>
        /// <returns>A new <see cref="PageContext"/> instance initialized with the specified parameters.</returns>
        public static PageContext Create(
            int pageSize, int currentPage, string? continuationToken = null, int? totalCount = null)
            => new()
            {
                PageSize = pageSize,
                CurrentPage = currentPage,
                ContinuationToken = continuationToken,
                TotalCount = totalCount
            };

        /// <summary>
        /// Creates a new instance of the <see cref="PageContext"/> class with the specified total count.
        /// </summary>
        /// <param name="totalCount">The total number of items to be represented by the <see cref="PageContext"/>.</param>
        /// <returns>A new <see cref="PageContext"/> instance with the specified total count and default values for other
        /// properties.</returns>
        public static PageContext Create(int totalCount)
            => new()
            {
                PageSize = 0,
                CurrentPage = 0,
                ContinuationToken = null,
                TotalCount = totalCount
            };

        /// <summary>
        /// Gets an empty <see cref="PageContext"/> instance with default values.
        /// </summary>
        public static PageContext Empty => new()
        {
            PageSize = 0,
            CurrentPage = 0,
            ContinuationToken = null,
            TotalCount = null
        };
    }
}