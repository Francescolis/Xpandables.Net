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
using System.Diagnostics.CodeAnalysis;

namespace System.Collections.Generic;

/// <summary>
/// Provides extension methods for working with asynchronous paged enumerables.
/// </summary>
/// <remarks>This static class contains helper methods that extend the functionality of types implementing <see
/// cref="IAsyncPagedEnumerable"/>. Use these methods to simplify common operations, such as retrieving type information
/// or manipulating paged asynchronous sequences.</remarks>
public static class IAsyncPagedEnumerableExtensions
{
    extension<T>(IAsyncPagedEnumerable<T> source)
    {
        /// <summary>
        /// Gets the runtime type of the generic argument parameter.
        /// </summary>
        /// <returns>A <see cref="Type"/> object representing the type parameter <c>T</c>.</returns>
        public Type ArgumentType => typeof(T);
    }

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
        [RequiresUnreferencedCode("This method uses reflection to discover implemented interfaces, which may be incompatible with trimming.")]
        public Type GetArgumentType()
        {
			Type sourceType = source.GetType();

            // Check if the concrete type itself is a generic type matching IAsyncPagedEnumerable<T>
            if (sourceType.IsGenericType)
            {
				Type genericDef = sourceType.GetGenericTypeDefinition();
                if (genericDef == typeof(IAsyncPagedEnumerable<>))
                {
                    return sourceType.GetGenericArguments()[0];
                }
            }

            // Search implemented interfaces for IAsyncPagedEnumerable<T>
            foreach (Type iface in sourceType.GetInterfaces())
            {
                if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IAsyncPagedEnumerable<>))
                {
                    return iface.GetGenericArguments()[0];
                }
            }

            throw new InvalidOperationException("The source does not implement IAsyncPagedEnumerable<T>.");
        }
    }
}