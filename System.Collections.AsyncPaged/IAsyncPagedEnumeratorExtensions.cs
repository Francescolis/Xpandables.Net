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
    /// Extension methods for configuring pagination strategies.
    /// </summary>
    /// <typeparam name="T">The type of elements being enumerated.</typeparam>
    extension<T>(IAsyncPagedEnumerator<T> enumerator) where T : allows ref struct
    {
        /// <summary>
        /// Configures the enumerator to update pagination based on page boundaries.
        /// </summary>
        /// <returns>The configured enumerator instance to support fluent chaining.</returns>
        /// <remarks>
        /// This is a convenience method equivalent to calling <c>WithStrategy(PaginationStrategy.PerPage)</c>.
        /// The pagination metadata will be updated whenever a page boundary is crossed during enumeration.
        /// </remarks>
        public IAsyncPagedEnumerator<T> WithPerPageStrategy()
        {
            ArgumentNullException.ThrowIfNull(enumerator);
            return enumerator.WithStrategy(PaginationStrategy.PerPage);
        }

        /// <summary>
        /// Configures the enumerator to update pagination for each item enumerated.
        /// </summary>
        /// <returns>The configured enumerator instance to support fluent chaining.</returns>
        /// <remarks>
        /// This is a convenience method equivalent to calling <c>WithStrategy(PaginationStrategy.PerItem)</c>.
        /// The pagination metadata will be updated after each item is enumerated, providing fine-grained
        /// tracking of enumeration progress.
        /// </remarks>
        public IAsyncPagedEnumerator<T> WithPerItemStrategy()
        {
            ArgumentNullException.ThrowIfNull(enumerator);
            return enumerator.WithStrategy(PaginationStrategy.PerItem);
        }

        /// <summary>
        /// Configures the enumerator to not automatically update pagination metadata.
        /// </summary>
        /// <returns>The configured enumerator instance to support fluent chaining.</returns>
        /// <remarks>
        /// This is a convenience method equivalent to calling <c>WithStrategy(PaginationStrategy.None)</c>.
        /// The pagination metadata will remain unchanged during enumeration. This is the default behavior.
        /// </remarks>
        public IAsyncPagedEnumerator<T> WithNoStrategy()
        {
            ArgumentNullException.ThrowIfNull(enumerator);
            return enumerator.WithStrategy(PaginationStrategy.None);
        }
    }
}