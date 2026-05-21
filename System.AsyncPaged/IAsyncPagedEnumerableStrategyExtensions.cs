/*******************************************************************************
 * Copyright (C) 2025-2026 Kamersoft
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
/// Provides extension methods for configuring and managing pagination strategies in asynchronous paged enumerators.
/// </summary>
/// <remarks>
/// This class contains helper methods that extend the functionality of <see cref="IAsyncPagedEnumerator{T}"/>
/// by providing fluent API methods for configuring pagination strategies. These methods simplify the process
/// of customizing the behavior of asynchronous paged enumerators to suit specific use cases or performance requirements.
/// </remarks>
public static class IAsyncPagedEnumerableStrategyExtensions
{
	/// <summary>
	/// Configures the enumerable to preserve the original pagination snapshot during enumeration.
	/// </summary>
	/// <typeparam name="TSource">Source element type.</typeparam>
	/// <param name="enumerable">The source asynchronous paged sequence.</param>
	/// <returns>The configured enumerable instance to support fluent chaining.</returns>
	public static IAsyncPagedEnumerable<TSource> WithManualPagination<TSource>(this IAsyncPagedEnumerable<TSource> enumerable)
	{
		ArgumentNullException.ThrowIfNull(enumerable);
		return enumerable.WithStrategy(PaginationStrategy.Manual);
	}

	/// <summary>
	/// Configures the enumerable to update pagination on page boundaries.
	/// </summary>
	/// <typeparam name="TSource">Source element type.</typeparam>
	/// <param name="enumerable">The source asynchronous paged sequence.</param>
	/// <returns>The configured enumerable instance to support fluent chaining.</returns>
	public static IAsyncPagedEnumerable<TSource> WithPageAwarePagination<TSource>(this IAsyncPagedEnumerable<TSource> enumerable)
	{
		ArgumentNullException.ThrowIfNull(enumerable);
		return enumerable.WithStrategy(PaginationStrategy.PageAware);
	}

	/// <summary>
	/// Configures the enumerable to update pagination for each item.
	/// </summary>
	/// <typeparam name="TSource">Source element type.</typeparam>
	/// <param name="enumerable">The source asynchronous paged sequence.</param>
	/// <returns>The configured enumerable instance to support fluent chaining.</returns>
	public static IAsyncPagedEnumerable<TSource> WithItemAwarePagination<TSource>(this IAsyncPagedEnumerable<TSource> enumerable)
	{
		ArgumentNullException.ThrowIfNull(enumerable);
		return enumerable.WithStrategy(PaginationStrategy.ItemAware);
	}

	/// <summary>
	/// Configures the enumerator to update pagination based on page boundaries.
	/// </summary>
	/// <typeparam name="TSource">Source element type.</typeparam>
	/// <param name="enumerable">The source asynchronous paged sequence.</param>
	/// <returns>The configured enumerable instance to support fluent chaining.</returns>
	/// <remarks>
	/// This is a convenience method equivalent to calling <c>WithStrategy(PaginationStrategy.PerPage)</c>.
	/// The pagination metadata will be updated whenever a page boundary is crossed during enumeration.
	/// </remarks>
	public static IAsyncPagedEnumerable<TSource> WithPerPageStrategy<TSource>(this IAsyncPagedEnumerable<TSource> enumerable)
	{
		ArgumentNullException.ThrowIfNull(enumerable);
		return enumerable.WithStrategy(PaginationStrategy.PageAware);
	}

	/// <summary>
	/// Configures the enumerator to update pagination for each item enumerated.
	/// </summary>
	/// <typeparam name="TSource">Source element type.</typeparam>
	/// <param name="enumerable">The source asynchronous paged sequence.</param>
	/// <returns>The configured enumerator instance to support fluent chaining.</returns>
	/// <remarks>
	/// This is a convenience method equivalent to calling <c>WithStrategy(PaginationStrategy.PerItem)</c>.
	/// The pagination metadata will be updated after each item is enumerated, providing fine-grained
	/// tracking of enumerable progress.
	/// </remarks>
	public static IAsyncPagedEnumerable<TSource> WithPerItemStrategy<TSource>(this IAsyncPagedEnumerable<TSource> enumerable)
	{
		ArgumentNullException.ThrowIfNull(enumerable);
		return enumerable.WithStrategy(PaginationStrategy.ItemAware);
	}

	/// <summary>
	/// Configures the enumerator to not automatically update pagination metadata.
	/// </summary>
	/// <typeparam name="TSource">Source element type.</typeparam>
	/// <param name="enumerable">The source asynchronous paged sequence.</param>
	/// <returns>The configured enumerable instance to support fluent chaining.</returns>
	/// <remarks>
	/// This is a convenience method equivalent to calling <c>WithStrategy(PaginationStrategy.None)</c>.
	/// The pagination metadata will remain unchanged during enumeration. This is the default behavior.
	/// </remarks>
	public static IAsyncPagedEnumerable<TSource> WithNoStrategy<TSource>(this IAsyncPagedEnumerable<TSource> enumerable)
	{
		ArgumentNullException.ThrowIfNull(enumerable);
		return enumerable.WithStrategy(PaginationStrategy.Manual);
	}
}
