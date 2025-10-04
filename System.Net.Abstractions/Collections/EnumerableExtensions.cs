namespace Xpandables.Net.Abstractions.Collections;

/// <summary>
/// Provides extension methods for working with <see cref="IEnumerable{T}"/> sequences.
/// </summary>
/// <remarks>This class includes utility methods that extend the functionality of enumerable collections, enabling
/// additional operations such as joining elements into a single string. These methods are intended to simplify common
/// tasks when working with sequences.</remarks>
[Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]
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
        public string StringJoin(string separator) => string.Join(separator, source);

        /// <summary>
        /// Concatenates the elements of the source collection, using the specified separator between each element.
        /// </summary>
        /// <param name="separator">The character to use as a separator between each element in the resulting string.</param>
        /// <returns>A string that consists of the elements in the source collection delimited by the specified separator.
        /// Returns an empty string if the source collection contains no elements.</returns>
        public string StringJoin(char separator) => string.Join(separator, source);
    }
}
