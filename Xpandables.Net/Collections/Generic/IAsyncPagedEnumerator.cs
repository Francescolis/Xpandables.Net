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

namespace Xpandables.Net.Collections.Generic;

/// <summary>
/// Represents an asynchronous enumerator that supports pagination, allowing iteration over a sequence of items while
/// providing access to pagination metadata that updates as enumeration progresses.
/// </summary>
/// <remarks>
/// This interface extends <see cref="IAsyncEnumerator{T}"/> to include pagination support. It is
/// designed for scenarios where data is retrieved in pages, such as querying paginated APIs or databases.
/// <para>
/// The <see cref="Pagination"/> property provides real-time access to the current pagination state,
/// which may be updated based on the configured <see cref="PaginationStrategy"/>.
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
    /// Gets the current pagination strategy being used by this enumerator.
    /// </summary>
    /// <remarks>
    /// The strategy determines how pagination metadata is updated during enumeration.
    /// The default value is <see cref="PaginationStrategy.None"/>, meaning pagination is not automatically updated.
    /// </remarks>
    PaginationStrategy Strategy { get; }

    /// <summary>
    /// Configures the strategy to be used for managing pagination during enumeration.
    /// </summary>
    /// <remarks>
    /// The specified strategy influences how the <see cref="Pagination"/> property is updated
    /// during enumeration. This method should typically be called before beginning enumeration.
    /// <para>
    /// Available strategies:
    /// <list type="bullet">
    /// <item><see cref="PaginationStrategy.None"/>: No automatic pagination updates (default)</item>
    /// <item><see cref="PaginationStrategy.PerPage"/>: Updates pagination based on page boundaries</item>
    /// <item><see cref="PaginationStrategy.PerItem"/>: Updates pagination for each item enumerated</item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <param name="strategy">The <see cref="PaginationStrategy"/> to apply.</param>
    /// <returns>The current enumerator instance to support fluent chaining.</returns>
    IAsyncPagedEnumerator<T> WithStrategy(PaginationStrategy strategy);
}

/// <summary>
/// Provides extension methods for configuring and managing pagination strategies in asynchronous paged enumerators.
/// </summary>
/// <remarks>
/// This class contains helper methods that extend the functionality of <see cref="IAsyncPagedEnumerator{T}"/>
/// by providing fluent API methods for configuring pagination strategies. These methods simplify the process
/// of customizing the behavior of asynchronous paged enumerators to suit specific use cases or performance requirements.
/// </remarks>
public static class IAsyncPagedEnumeratorExtensions
{
    /// <summary>
    /// Configures the enumerator to update pagination based on page boundaries.
    /// </summary>
    /// <typeparam name="T">The type of elements being enumerated.</typeparam>
    /// <param name="enumerator">The enumerator to configure.</param>
    /// <returns>The configured enumerator instance to support fluent chaining.</returns>
    /// <remarks>
    /// This is a convenience method equivalent to calling <c>WithStrategy(PaginationStrategy.PerPage)</c>.
    /// The pagination metadata will be updated whenever a page boundary is crossed during enumeration.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="enumerator"/> is null.</exception>
    public static IAsyncPagedEnumerator<T> WithPerPageStrategy<T>(this IAsyncPagedEnumerator<T> enumerator)
        where T : allows ref struct
    {
        ArgumentNullException.ThrowIfNull(enumerator);
        return enumerator.WithStrategy(PaginationStrategy.PerPage);
    }

    /// <summary>
    /// Configures the enumerator to update pagination for each item enumerated.
    /// </summary>
    /// <typeparam name="T">The type of elements being enumerated.</typeparam>
    /// <param name="enumerator">The enumerator to configure.</param>
    /// <returns>The configured enumerator instance to support fluent chaining.</returns>
    /// <remarks>
    /// This is a convenience method equivalent to calling <c>WithStrategy(PaginationStrategy.PerItem)</c>.
    /// The pagination metadata will be updated after each item is enumerated, providing fine-grained
    /// tracking of enumeration progress.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="enumerator"/> is null.</exception>
    public static IAsyncPagedEnumerator<T> WithPerItemStrategy<T>(this IAsyncPagedEnumerator<T> enumerator)
        where T : allows ref struct
    {
        ArgumentNullException.ThrowIfNull(enumerator);
        return enumerator.WithStrategy(PaginationStrategy.PerItem);
    }

    /// <summary>
    /// Configures the enumerator to not automatically update pagination metadata.
    /// </summary>
    /// <typeparam name="T">The type of elements being enumerated.</typeparam>
    /// <param name="enumerator">The enumerator to configure.</param>
    /// <returns>The configured enumerator instance to support fluent chaining.</returns>
    /// <remarks>
    /// This is a convenience method equivalent to calling <c>WithStrategy(PaginationStrategy.None)</c>.
    /// The pagination metadata will remain unchanged during enumeration. This is the default behavior.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="enumerator"/> is null.</exception>
    public static IAsyncPagedEnumerator<T> WithNoStrategy<T>(this IAsyncPagedEnumerator<T> enumerator)
        where T : allows ref struct
    {
        ArgumentNullException.ThrowIfNull(enumerator);
        return enumerator.WithStrategy(PaginationStrategy.None);
    }
}