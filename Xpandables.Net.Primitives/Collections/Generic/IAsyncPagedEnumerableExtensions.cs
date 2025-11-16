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
namespace Xpandables.Net.Collections.Generic;

/// <summary>
/// Provides extension methods for working with asynchronous paged enumerables.
/// </summary>
/// <remarks>This static class contains helper methods that extend the functionality of types implementing <see
/// cref="IAsyncPagedEnumerable"/>. Use these methods to simplify common operations, such as retrieving type information
/// or manipulating paged asynchronous sequences.</remarks>
public static class IAsyncPagedEnumerableExtensions
{
    extension(IAsyncPagedEnumerable source)
    {
        /// <summary>
        /// Gets the type of the argument used by the underlying <see cref="IAsyncPagedEnumerable{T}"/> source.
        /// </summary>
        /// <remarks>Use this method to determine the element type produced by the asynchronous paged
        /// enumerable source. This is useful when working with generic collections or when type information is required
        /// for reflection or dynamic operations.</remarks>
        /// <returns>The Type representing the generic argument T of the <see cref="IAsyncPagedEnumerable{T}"/> implemented by the source.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the source does not implement <see cref="IAsyncPagedEnumerable{T}"/>.</exception>
        public Type GetArgumentType()
        {
            var sourceType = source.GetType();
            if (!sourceType.IsGenericType)
                throw new InvalidOperationException("The source does not implement IAsyncPagedEnumerable<T>.");

            return sourceType.GetGenericArguments()[0];
        }

        /// <summary>
        /// Ensures that the source object implements the <see cref="IAsyncPagedEnumerable{T}"/> interface.
        /// Throws an exception if the source does not meet this requirement.
        /// </summary>
        /// <remarks>Use this method to validate that the source is compatible with asynchronous paged
        /// enumeration before performing operations that require <see cref="IAsyncPagedEnumerable{T}"/> support.</remarks>
        /// <exception cref="InvalidOperationException">Thrown if the source object does not implement <see cref="IAsyncPagedEnumerable{T}"/>.</exception>
        public void EnsureIsAsyncPagedEnumerableOfT()
        {
            var sourceType = source.GetType();
            if (!sourceType.IsGenericType || (sourceType.GetGenericTypeDefinition() != typeof(IAsyncPagedEnumerable<>)
                 || sourceType.GetGenericTypeDefinition() != typeof(AsyncPagedEnumerable<>)))
                throw new InvalidOperationException("The source does not implement IAsyncPagedEnumerable<T>.");
        }
    }
}