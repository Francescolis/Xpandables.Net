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
/// Represents an asynchronous enumerable collection that supports pagination.
/// </summary>
/// <remarks>This interface extends <see cref="IAsyncEnumerable{T}"/> to provide additional support for paginated
/// data. It includes metadata about the pagination state and methods to retrieve this metadata asynchronously.
/// Implementations may compute pagination metadata lazily, and callers should use <see cref="GetPaginationAsync"/>  to
/// ensure the metadata is available.</remarks>
/// <typeparam name="T">The type of elements in the collection.</typeparam>
public interface IAsyncPagedEnumerable<out T> : IAsyncEnumerable<T>
    where T : allows ref struct
{
    /// <summary>
    /// Gets the pagination information for this collection.
    /// </summary>
    /// <remarks>
    /// Accessing this property before metadata is available throws.
    /// Use <see cref="GetPaginationAsync"/> for async access.
    /// </remarks>
    Pagination Pagination { get; }

    /// <summary>
    /// Asynchronously ensures pagination metadata is available.
    /// Implementations may use the token to prime the underlying stream.
    /// </summary>
    /// <remarks>
    /// Pagination metadata is computed lazily and only when required.
    /// For database-backed sources, the implementation strives to use a single round trip.
    /// </remarks>
    Task<Pagination> GetPaginationAsync(CancellationToken cancellationToken = default);
}