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

namespace System.Net.Async;

/// <summary>
/// Represents an asynchronous enumerator that supports pagination, allowing iteration over a sequence of items while
/// providing access to pagination metadata.
/// </summary>
/// <remarks>This interface extends <see cref="IAsyncEnumerator{T}"/> to include pagination support. It is
/// designed for scenarios where data is retrieved in pages, such as querying paginated APIs or databases.</remarks>
/// <typeparam name="T">The type of the items being enumerated. The type must be a `ref struct`.</typeparam>
public interface IAsyncPagedEnumerator<out T> : IAsyncEnumerator<T>
    where T : allows ref struct
{
    /// <summary>
    /// Gets a read-only reference to the current page context.
    /// </summary>
    ref readonly PageContext PageContext { get; }

    /// <summary>
    /// Configures the strategy to be used for managing page context during operations.
    /// </summary>
    /// <remarks>The specified strategy influences the behavior of operations that rely on page context.
    /// Ensure the provided strategy aligns with the intended usage scenario.</remarks>
    /// <param name="strategy">The <see cref="PageContextStrategy"/> to apply. This determines how page context is handled and managed.</param>
    void WithPageContextStrategy(PageContextStrategy strategy);
}

/// <summary>
/// Provides extension methods for configuring and managing page context strategies in asynchronous paged enumerators.
/// </summary>
/// <remarks>This class contains helper methods that extend the functionality of <see
/// cref="IAsyncPagedEnumerator{T}"/> by allowing users to configure strategies for handling page context during
/// enumeration. These methods simplify the process of customizing the behavior of asynchronous paged enumerators to
/// suit specific use cases or performance requirements.</remarks>
public static class IAsyncPagedEnumeratorExtensions
{
    /// <inheritdoc/>>
#pragma warning disable CA1034 // Nested types should not be visible
    extension<T>(IAsyncPagedEnumerator<T> enumerator)
#pragma warning restore CA1034 // Nested types should not be visible
        where T : allows ref struct
    {
        /// <summary>
        /// Configures the enumerator to use the specified page context strategy.
        /// </summary>
        /// <param name="strategy">The <see cref="PageContextStrategy"/> to use for managing page context during enumeration.</param>
        /// <returns>The current instance of <see cref="IAsyncPagedEnumerator{T}"/> configured with the specified page context
        /// strategy.</returns>
        public IAsyncPagedEnumerator<T> WithPageContextStrategy(PageContextStrategy strategy)
        {
            enumerator.WithPageContextStrategy(strategy);
            return enumerator;
        }
    }
}