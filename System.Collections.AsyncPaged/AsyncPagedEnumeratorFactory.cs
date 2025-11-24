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

using System.Runtime.CompilerServices;

namespace System.Collections.Generic;

/// <summary>
/// Provides factory methods for creating instances of <see cref="AsyncPagedEnumerator{T}"/>.
/// </summary>
/// <remarks>
/// This class simplifies the creation of asynchronous paged enumerators with cancellation token support 
/// and initial pagination context.
/// </remarks>
public static class AsyncPagedEnumerator
{
    /// <summary>
    /// Creates a new paged enumerator for the specified source.
    /// </summary>
    /// <typeparam name="T">The type of elements being enumerated.</typeparam>
    /// <param name="sourceEnumerator">The source enumerator to wrap.</param>
    /// <param name="pagination">The initial pagination context. If null, <see cref="Pagination.Empty"/> is used.</param>
    /// <param name="strategy">The pagination strategy to apply.</param>
    /// <param name="cancellationToken">The cancellation token to observe.</param>
    /// <returns>A new <see cref="AsyncPagedEnumerator{T}"/> instance.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static AsyncPagedEnumerator<T> Create<T>(
        IAsyncEnumerator<T> sourceEnumerator,
        Pagination? pagination = null,
        PaginationStrategy strategy = PaginationStrategy.None,
        CancellationToken cancellationToken = default) =>
        new(sourceEnumerator, pagination ?? Pagination.Empty,strategy, cancellationToken);

    /// <summary>
    /// Creates an empty paged enumerator with no data.
    /// </summary>
    /// <typeparam name="T">The type of elements.</typeparam>
    /// <param name="pagination">The initial pagination context. If null, <see cref="Pagination.Empty"/> is used.</param>
    /// <returns>An empty <see cref="AsyncPagedEnumerator{T}"/> instance.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static AsyncPagedEnumerator<T> Empty<T>(Pagination? pagination = null) =>
        new(pagination ?? Pagination.Empty);
}