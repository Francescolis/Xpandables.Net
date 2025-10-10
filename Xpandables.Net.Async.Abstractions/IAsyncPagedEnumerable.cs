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
using System.Diagnostics.CodeAnalysis;

namespace Xpandables.Net.Async;

/// <summary>
/// Represents a sequence of elements that can be asynchronously enumerated in discrete pages.
/// </summary>
/// <remarks>
/// Use this interface to retrieve large result sets in manageable chunks, reducing memory usage and
/// improving performance for asynchronous operations. Each page is typically fetched on demand as the sequence is
/// iterated. This interface is commonly used for APIs that support server-side paging.
/// <para>
/// The <see cref="Pagination"/> property may be computed lazily by some implementations. 
/// Use <see cref="GetPaginationAsync"/> to ensure the pagination metadata is fully computed and available.
/// </para>
/// </remarks>
[SuppressMessage("Design", "CA1040:Avoid empty interfaces", Justification = "Base interface for generic variant")]
public interface IAsyncPagedEnumerable
{
    /// <summary>
    /// Gets the type of elements in the collection.
    /// </summary>
    Type Type { get; }

    /// <summary>
    /// Gets the current pagination metadata for this enumerable.
    /// </summary>
    /// <remarks>
    /// This property provides immediate access to the pagination state. However, some implementations
    /// may compute this value lazily. If the pagination metadata is not yet available, this property
    /// may return <see cref="Pagination.Empty"/>. Use <see cref="GetPaginationAsync"/> to ensure
    /// the pagination metadata is fully computed.
    /// </remarks>
    Pagination Pagination { get; }

    /// <summary>
    /// Asynchronously retrieves the fully computed pagination information for the current page.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation and yields the current <see cref="Pagination"/>.</returns>
    /// <remarks>
    /// This method ensures that the pagination metadata is fully computed and available. Use this method
    /// when you need to guarantee that all pagination information (such as total count) has been calculated.
    /// For immediate access to potentially incomplete pagination data, use the <see cref="Pagination"/> property instead.
    /// </remarks>
    Task<Pagination> GetPaginationAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents an asynchronous enumerable collection that supports pagination.
/// </summary>
/// <remarks>
/// This interface extends <see cref="IAsyncEnumerable{T}"/> to provide additional support for paginated
/// data. It includes metadata about the pagination state and methods to retrieve this metadata asynchronously.
/// Implementations may compute pagination metadata lazily, and callers should use <see cref="IAsyncPagedEnumerable.GetPaginationAsync"/>
/// to ensure the metadata is fully available.
/// <para>
/// The generic type parameter supports ref struct types starting with .NET 10, enabling efficient pagination
/// of stack-only types.
/// </para>
/// </remarks>
/// <typeparam name="T">The type of elements in the collection.</typeparam>
public interface IAsyncPagedEnumerable<out T> : IAsyncPagedEnumerable, IAsyncEnumerable<T>
    where T : allows ref struct
{
    /// <summary>
    /// Gets the type of the generic parameter T.
    /// </summary>
    public new Type Type => typeof(T);

    [EditorBrowsable(EditorBrowsableState.Never)]
    Type IAsyncPagedEnumerable.Type => Type;

    /// <summary>
    /// Gets the current pagination metadata for this enumerable.
    /// </summary>
    /// <remarks>
    /// This property provides immediate access to the pagination state. However, some implementations
    /// may compute this value lazily. If the pagination metadata is not yet available, this property
    /// may return <see cref="Pagination.Empty"/>. Use <see cref="IAsyncPagedEnumerable.GetPaginationAsync"/> 
    /// to ensure the pagination metadata is fully computed.
    /// </remarks>
    new Pagination Pagination { get; }

    [EditorBrowsable(EditorBrowsableState.Never)]
    Pagination IAsyncPagedEnumerable.Pagination => Pagination;
}