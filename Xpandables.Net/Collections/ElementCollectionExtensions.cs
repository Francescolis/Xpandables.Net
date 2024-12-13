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
/// Provides extension methods for converting collections of 
/// <see cref="ElementEntry"/> to <see cref="ElementCollection"/>.
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
    public static ElementCollection ToElementCollection(
        this IEnumerable<ElementEntry> entries)
        => ElementCollection.With([.. entries]);

    /// <summary>
    /// Converts an <see cref="ElementCollection"/> to a dictionary where the 
    /// keys are the element keys and the values are arrays of element values.
    /// </summary>
    /// <param name="elementCollection">The <see cref="ElementCollection"/> 
    /// to convert.</param>
    /// <returns>A dictionary with keys and values from the 
    /// <see cref="ElementCollection"/>.</returns>
    public static IDictionary<string, string[]> ToElementDictionary(
        this ElementCollection elementCollection)
        => elementCollection
            .ToDictionary(entry => entry.Key, entry => entry.Values.ToArray());

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
    public static bool Exists<T>(this T[] array, Predicate<T> match)
        => Array.Exists(array, match);

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
    public static IAsyncEnumerable<T> ToAsyncEnumerable<T>(
        this IEnumerable<T> source)
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

        using IEnumerator<T> enumerator = source.GetEnumerator();
        while (enumerator.MoveNext())
        {
            action(enumerator.Current);
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
    public static void ForEach<T>(
        this List<T> source,
        ForEachRefAction<T> action)
        where T : struct
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(action);

        Span<T> spanSource = CollectionsMarshal.AsSpan(source);
        foreach (ref T item in spanSource)
        {
            action(ref item);
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
        this IAsyncEnumerable<T> source,
        Action<T> action,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(action);

        await foreach (T item in source
            .WithCancellation(cancellationToken)
            .ConfigureAwait(false))
        {
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
        this IAsyncEnumerable<T> source,
        Func<T, CancellationToken, Task> action,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(action);

        await foreach (T item in source
            .WithCancellation(cancellationToken)
            .ConfigureAwait(false))
        {
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
    public static IReadOnlyCollection<T> ToReadOnlyCollection<T>(
        this IEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return new ReadOnlyCollectionBuilder<T>(source)
            .ToReadOnlyCollection();
    }

    /// <summary>
    /// Determines whether the current type implements or it's 
    /// <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <param name="type">The type to act on.</param>
    /// <returns><see langword="true"/> if found, otherwise 
    /// <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="type"/> is null.</exception>
    public static bool IsEnumerable(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        return !type.IsPrimitive
            && type != typeof(string)
            && type.GetInterfaces()
                .Exists(i => i.IsGenericType
                    && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
    }

    /// <summary>
    /// Determines whether the current type implements or it's 
    /// <see cref="IAsyncEnumerable{T}"/>.
    /// </summary>
    /// <param name="type">The type to act on.</param>
    /// <returns><see langword="true"/> if Ok, otherwise 
    /// <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="type"/> is null.</exception>
    public static bool IsAsyncEnumerable(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        return type.IsInterface switch
        {
            true => type.IsGenericType && type.Name
                .Equals(typeof(IAsyncEnumerable<>).Name,
                    StringComparison.OrdinalIgnoreCase),
            _ => type.GetInterfaces()
                .Exists(i => i.IsGenericType
                    && i.GetGenericTypeDefinition() == typeof(IAsyncEnumerable<>))
        };
    }
}
