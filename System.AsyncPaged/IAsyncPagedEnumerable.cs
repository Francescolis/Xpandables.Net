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

using System.ComponentModel;

namespace System.Collections.Generic;

/// <summary>
/// Represents a sequence of elements that can be asynchronously enumerated in discrete pages.
/// </summary>
/// <remarks>
/// Use this interface to retrieve large result sets in manageable chunks, reducing memory usage and
/// improving performance for asynchronous operations. Each page is typically fetched on demand as the sequence is
/// iterated. This interface is commonly used for APIs that support server-side paging.
/// <para>
/// Use <see cref="GetPaginationAsync"/> to retrieve the current pagination metadata.
/// </para>
/// </remarks>
public interface IAsyncPagedEnumerable<out T> : IAsyncEnumerable<T>
    where T : allows ref struct
{
    /// <summary>
    /// Returns an enumerator that iterates asynchronously through the collection with pagination support.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that may be used to cancel the asynchronous iteration.</param>
    /// <returns>An enumerator that can be used to iterate asynchronously through the collection.</returns>
    new IAsyncPagedEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default);

    [EditorBrowsable(EditorBrowsableState.Never)]
    IAsyncEnumerator<T> IAsyncEnumerable<T>.GetAsyncEnumerator(CancellationToken cancellationToken) =>
        GetAsyncEnumerator(cancellationToken);

    /// <summary>
    /// Asynchronously retrieves the current pagination metadata for this sequence.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation and yields the current <see cref="Pagination"/>.</returns>
    Task<Pagination> GetPaginationAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Configures the strategy to be used for managing pagination.
    /// If the source is an IAsyncEnumerable that cannot be iterated twice, this should be used with caution.
    /// </summary>
    /// <remarks>
    /// This method is pure; it returns a new enumerable view or the same instance with the updated configuration,
    /// ensuring thread safety and immutability of the definition.
    /// </remarks>
    /// <param name="strategy">The <see cref="PaginationStrategy"/> to apply.</param>
    /// <returns>A configured enumerable instance.</returns>
    IAsyncPagedEnumerable<T> WithStrategy(PaginationStrategy strategy);
}
