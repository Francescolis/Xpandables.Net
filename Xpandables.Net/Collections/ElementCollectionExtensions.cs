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
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Xpandables.Net.Collections;

/// <summary>
/// Provides extension methods for collections, including conversion to ElementCollection, async enumeration, and type
/// checks. Supports operations like ForEach and Exists on various collection types.
/// </summary>
public static class ElementCollectionExtensions
{
    /// <summary>
    /// Converts an <see cref="IEnumerable{ElementEntry}"/> to 
    /// an <see cref="ElementCollection"/>.
    /// </summary>
    /// <param name="entries">The collection of <see cref="ElementEntry"/>
    /// to convert.</param>
    /// <returns>An <see cref="ElementCollection"/> containing the provided 
    /// entries.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ElementCollection ToElementCollection(this IEnumerable<ElementEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        if (entries is ICollection<ElementEntry> collection)
        {
            // If we know the count, we can create a list with the right capacity
            List<ElementEntry> entryList = [.. collection];
            return ElementCollection.With(entryList);
        }

        // Otherwise we need to materialize the list first
        List<ElementEntry> list = [.. entries];
        return ElementCollection.With(list);
    }

    /// <summary>
    /// Contains the public <see cref="Array.Empty"/> method.
    /// </summary>
    public static readonly MethodInfo ArrayEmptyMethod =
        typeof(Array).GetMethod(nameof(Array.Empty))!;

    /// <summary>
    /// Contains the public <see cref="AsyncEnumerable.Empty"/> method.
    /// </summary>
    public static readonly MethodInfo AsyncArrayEmptyMethod =
        typeof(AsyncEnumerable).GetMethod(nameof(AsyncEnumerable.Empty))!;

    /// <summary>
    /// Returns an array of <see cref="Type"/> objects that represent the type 
    /// arguments of the specified closed generic type.
    /// </summary>
    /// <param name="type">The type to act on.</param>
    /// <returns>An array of <see cref="Type"/> objects that represent the 
    /// type arguments</returns>
    /// <remarks>If the type is not a generic enumerable, returns an empty 
    /// collection.</remarks>
    public static ReadOnlySpan<Type> GetEnumerableGenericTypes(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        return type.IsEnumerable() || type.IsAsyncEnumerable()
                ? type.GetGenericArguments()
                : Type.EmptyTypes;
    }

    /// <summary>
    /// Determines whether the specified array contains elements that match 
    /// the conditions defined by the specified predicate.
    /// </summary>
    /// <typeparam name="T">The type of elements.</typeparam>
    /// <param name="array">The array to act on.</param>
    /// <param name="match">The predicate each element should match to.</param>
    /// <returns><see langword="true"/> if <paramref name="array"/> contains one
    /// or more elements that match the conditions defined by the specified 
    /// predicate; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="array"/> 
    /// or <paramref name="match"/> is null.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Exists<T>(this T[] array, Predicate<T> match)
    {
        ArgumentNullException.ThrowIfNull(array);
        ArgumentNullException.ThrowIfNull(match);

        // Inline the Array.Exists logic for better performance
        for (int i = 0; i < array.Length; i++)
        {
            if (match(array[i]))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Converts the collection to exposes an enumerator that provides 
    /// asynchronous iteration over values of <typeparamref name="T"/> type.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the 
    /// collection.</typeparam>
    /// <param name="source">The collection of elements.</param>
    /// <returns>An async-enumerable sequence.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> 
    /// is null.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new AsyncEnumerable<T>(source);
    }

    /// <summary>
    /// The action delegate to be applied on the ref source.
    /// </summary>
    /// <typeparam name="TItem">Type of the element in the sequence.</typeparam>
    /// <param name="item">The item to act on.</param>
    public delegate void ForEachRefAction<TItem>(ref TItem item)
        where TItem : struct;

    /// <summary>
    /// Enumerates the collection source and performs the specified action 
    /// on each element.
    /// </summary>
    /// <typeparam name="T">Type of the element in the sequence.</typeparam>
    /// <param name="source">The source of the sequence.</param>
    /// <param name="action">Action to invoke for each element.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> 
    /// or <paramref name="action"/> is null.</exception>
    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(action);

        // Optimize for arrays and List<T> which are common types
        if (source is T[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                action(array[i]);
            }
            return;
        }

        if (source is List<T> list)
        {
            int count = list.Count;
            for (int i = 0; i < count; i++)
            {
                action(list[i]);
            }
            return;
        }

        // Fall back to using enumerator for other collection types
        foreach (T item in source)
        {
            action(item);
        }
    }

    /// <summary>
    /// Performs the specified action on each <see cref="ValueType"/> in the 
    /// <see cref="List{T}"/> without copying it.
    /// </summary>
    /// <typeparam name="T">Type of the element in the sequence.</typeparam>
    /// <param name="source">The source of the sequence.</param>
    /// <param name="action">Action to invoke for each element.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> 
    /// or <paramref name="action"/> is null.</exception>
    /// <remarks>Items should not be added or removed from the 
    /// <see cref="List{T}"/> while
    /// the <see cref="Span{T}"/> is in use.</remarks>
    public static void ForEach<T>(this List<T> source, ForEachRefAction<T> action)
        where T : struct
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(action);

        if (source.Count == 0)
            return;

        Span<T> spanSource = CollectionsMarshal.AsSpan(source);
        ref T firstElement = ref MemoryMarshal.GetReference(spanSource);

        // Manually iterate through span for better performance
        int length = spanSource.Length;
        for (int i = 0; i < length; i++)
        {
            ref T current = ref Unsafe.Add(ref firstElement, i);
            action(ref current);
        }
    }

    /// <summary>
    /// Asynchronously enumerates the collection source and performs 
    /// the specified action on each element.
    /// </summary>
    /// <typeparam name="T">Type of the element in the sequence.</typeparam>
    /// <param name="source">The source of the sequence.</param>
    /// <param name="action">Action to invoke on each element.</param>
    /// <param name="cancellationToken">A CancellationToken to observe 
    /// while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> 
    /// or <paramref name="action"/> is null.</exception>
    /// <exception cref="OperationCanceledException">The operation has been 
    /// canceled.</exception>
    public static async Task ForEachAsync<T>(
        this IAsyncEnumerable<T> source, Action<T> action, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(action);

        cancellationToken.ThrowIfCancellationRequested();

        await foreach (T item in source
            .WithCancellation(cancellationToken)
            .ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();
            action(item);
        }
    }

    /// <summary>
    /// Asynchronously enumerates the collection source and performs 
    /// the specified action on each element.
    /// </summary>
    /// <typeparam name="T">Type of the element in the sequence.</typeparam>
    /// <param name="source">The source of the sequence.</param>
    /// <param name="action">Action to asynchronously invoke on each 
    /// element.</param>
    /// <param name="cancellationToken">A CancellationToken to observe 
    /// while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> 
    /// or <paramref name="action"/> is null.</exception>
    public static async Task ForEachAsync<T>(
        this IAsyncEnumerable<T> source, Func<T, CancellationToken, Task> action,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(action);

        cancellationToken.ThrowIfCancellationRequested();

        await foreach (T item in source
            .WithCancellation(cancellationToken)
            .ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();
            await action(item, cancellationToken)
                .ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Converts an <see cref="IEnumerable{T}"/> to a read only collection.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="source">An instance of the collection to be 
    /// converted.</param>
    /// <returns>An implementation of <see cref="IReadOnlyCollection{T}"/>
    /// </returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> 
    /// is null.</exception>
    public static IReadOnlyCollection<T> ToReadOnlyCollection<T>(this IEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        // Optimize for already read-only collections
        if (source is IReadOnlyCollection<T> readOnlyCollection)
            return readOnlyCollection;

        return new ReadOnlyCollectionBuilder<T>(source)
            .ToReadOnlyCollection();
    }

    /// <summary>
    /// Determines whether the current type implements or it's <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <param name="type">The type to act on.</param>
    /// <returns><see langword="true"/> if found, otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="type"/> is null.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsEnumerable(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        // Fast checks for common types
        if (type.IsPrimitive || type == typeof(string))
            return false;

        Type[] interfaces = type.GetInterfaces();
        for (int i = 0; i < interfaces.Length; i++)
        {
            Type iface = interfaces[i];
            if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Determines whether the current type implements or it's <see cref="IAsyncEnumerable{T}"/>.
    /// </summary>
    /// <param name="type">The type to act on.</param>
    /// <returns><see langword="true"/> if Ok, otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="type"/> is null.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAsyncEnumerable(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        // Direct check if type is IAsyncEnumerable<T>
        if (type.IsInterface && type.IsGenericType &&
            string.Equals(type.Name, typeof(IAsyncEnumerable<>).Name, StringComparison.Ordinal))
            return true;

        // Check implemented interfaces
        Type[] interfaces = type.GetInterfaces();
        for (int i = 0; i < interfaces.Length; i++)
        {
            Type iface = interfaces[i];
            if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IAsyncEnumerable<>))
                return true;
        }

        return false;
    }
}
