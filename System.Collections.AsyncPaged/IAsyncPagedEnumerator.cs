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
namespace System.Collections.Generic;

/// <summary>
/// Represents an asynchronous enumerator that supports pagination, allowing iteration over a sequence of items while
/// providing access to pagination metadata that updates as enumeration progresses.
/// </summary>
/// <remarks>
/// This interface extends <see cref="IAsyncEnumerator{T}"/> to include pagination support.
/// <para>
/// The <see cref="Pagination"/> property provides real-time access to the current pagination state,
/// which is updated based on the strategy defined in the source <see cref="IAsyncPagedEnumerable{T}"/>.
/// </para>
/// <para>
/// The generic type parameter supports ref struct types starting with .NET 10, enabling efficient enumeration
/// of stack-only types.
/// </para>
/// </remarks>
/// <typeparam name="T">The type of the items being enumerated.</typeparam>
public interface IAsyncPagedEnumerator<out T> : IAsyncEnumerator<T>
    where T : allows ref struct
{
    /// <summary>
    /// Gets a read-only reference to the current pagination metadata.
    /// </summary>
    /// <remarks>
    /// This property provides a snapshot of the current pagination state. The returned reference
    /// is read-only and reflects the state at the time of access. The pagination metadata may be
    /// updated as enumeration progresses, depending on the configured <see cref="PaginationStrategy"/>.
    /// <para>
    /// Use this property to access pagination information without allocating new objects, making it
    /// efficient for high-performance scenarios.
    /// </para>
    /// </remarks>
    ref readonly Pagination Pagination { get; }

    /// <summary>
    /// Gets the pagination strategy currently active for this enumerator.
    /// </summary>
    /// <remarks>
    /// The strategy determines how pagination metadata is updated during enumeration.
    /// This value is immutable for the lifetime of the enumerator.
    /// </remarks>
    PaginationStrategy Strategy { get; }
}