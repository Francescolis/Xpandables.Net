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

using System.ComponentModel;

namespace Xpandables.Net.Async;

/// <summary>
/// Represents a sequence of elements that can be asynchronously enumerated in discrete pages.
/// </summary>
/// <remarks>Use this interface to retrieve large result sets in manageable chunks, reducing memory usage and
/// improving performance for asynchronous operations. Each page is typically fetched on demand as the sequence is
/// iterated. This interface is commonly used for APIs that support server-side paging.</remarks>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1040:Avoid empty interfaces", Justification = "<Pending>")]
public interface IAsyncPagedEnumerable
{
    /// <summary>
    /// Gets the type of elements in the collection.
    /// </summary>
    Type Type { get; }

    /// <summary>
    /// Asynchronously retrieves the pagination information for the current page.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation and yields the current <see cref="Pagination"/>.</returns>
    Task<Pagination> GetPaginationAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents an asynchronous enumerable collection that supports pagination.
/// </summary>
/// <remarks>This interface extends <see cref="IAsyncEnumerable{T}"/> to provide additional support for paginated
/// data. It includes metadata about the pagination state and methods to retrieve this metadata asynchronously.
/// Implementations may compute pagination metadata lazily, and callers should use <see cref="IAsyncPagedEnumerable.GetPaginationAsync"/>  to
/// ensure the metadata is available.</remarks>
/// <typeparam name="T">The type of elements in the collection.</typeparam>
public interface IAsyncPagedEnumerable<out T> : IAsyncPagedEnumerable, IAsyncEnumerable<T>
    where T : allows ref struct
{
    /// <summary>
    /// Gets the type of elements in the collection.
    /// </summary>
    public new Type Type => typeof(T);

    [EditorBrowsable(EditorBrowsableState.Never)]
    Type IAsyncPagedEnumerable.Type => Type;
}