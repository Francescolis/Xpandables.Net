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
/// Extends <see cref="IAsyncEnumerable{T}"/> to provide pagination metadata.
/// </summary>
/// <typeparam name="T">The type of items in the collection.</typeparam>
/// <remarks>For JSON serialization, you can use the converter extension method 
/// <see cref="AsyncPagedEnumerableExtensions.ToAsyncPagedEnumerableDataAsync{TSource}(IAsyncPagedEnumerable{TSource}, CancellationToken)"/>
/// that converts the paged enumerable to a materialized data structure.</remarks>
public interface IAsyncPagedEnumerable<out T> : IAsyncEnumerable<T>
    where T : allows ref struct
{
    /// <summary>
    /// Gets the pagination information for this collection.
    /// </summary>
    /// <remarks>
    /// This property provides access to pagination metadata such as skip/take values,
    /// total count, and computed page information. The pagination info is
    /// lazily evaluated when first accessed.
    /// </remarks>
    Pagination Pagination { get; }

    /// <summary>
    /// Gets a task that completes when pagination information is available.
    /// </summary>
    /// <remarks>
    /// Use this when you need to access pagination metadata before starting enumeration.
    /// This is useful for scenarios where you need to show pagination controls
    /// before displaying data.
    /// </remarks>
    Task<Pagination> GetPaginationAsync();
}