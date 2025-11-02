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
namespace Xpandables.Net.Primitives.Collections;

/// <summary>
/// Provides extension methods for working with <see cref="IEnumerable{T}"/> sequences.
/// </summary>
/// <remarks>This class includes utility methods that extend the functionality of enumerable collections, enabling
/// additional operations such as joining elements into a single string. These methods are intended to simplify common
/// tasks when working with sequences.</remarks>
public static class EnumerableExtensions
{
    /// <summary>
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
    /// <param name="source">The sequence to act on.</param>
    extension<TSource>(IEnumerable<TSource> source)
    {
        /// <summary>
        /// Concatenates the elements of the collection, using the specified separator between each element.
        /// </summary>
        /// <param name="separator">The string to use as a separator. The separator is included in the returned string only if the collection
        /// has more than one element. Can be null, in which case an empty string is used as the separator.</param>
        /// <returns>A string that consists of the elements of the collection delimited by the separator string. Returns an empty
        /// string if the collection contains no elements.</returns>
        /// <exception cref="ArgumentNullException">Thrown when source is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when an element in the source does not implement <see cref="IFormattable"/>.</exception>
        /// <exception cref="FormatException">Thrown when an element in the source is not in a valid format.</exception>
        /// <exception cref="OverflowException">Thrown when an element in the source represents a number that is out of the range of the <see cref="int"/> type.</exception>
        public string StringJoin(string separator) => string.Join(separator, source);

        /// <summary>
        /// Concatenates the elements of the source collection, using the specified separator between each element.
        /// </summary>
        /// <param name="separator">The character to use as a separator between each element in the resulting string.</param>
        /// <returns>A string that consists of the elements in the source collection delimited by the specified separator.
        /// Returns an empty string if the source collection contains no elements.</returns>
        /// <exception cref="ArgumentNullException">Thrown when source is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when an element in the source does not implement <see cref="IFormattable"/>.</exception>
        /// <exception cref="FormatException">Thrown when an element in the source is not in a valid format.</exception>
        /// <exception cref="OverflowException">Thrown when an element in the source represents a number that is out of the range of the <see cref="int"/> type.</exception>        /// 
        public string StringJoin(char separator) => string.Join(separator, source);
    }
}
