/*******************************************************************************
 * Copyright (C) 2025-2026 Kamersoft
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

namespace System.Optionals;

/// <summary>
/// Provides extension methods for working with synchronous and asynchronous enumerable sequences, enabling additional
/// operations such as retrieving elements as optional values.
/// </summary>
/// <remarks>These extension methods are designed to enhance the usability of standard enumerable types by
/// offering convenient patterns for handling sequences that may be empty. Methods in this class can be used with both
/// LINQ and asynchronous streams to simplify common tasks, such as safely obtaining the first element of a sequence
/// without throwing exceptions when the sequence is empty.</remarks>
public static class EnumerableExtensions
{
	/// <summary>
	/// Returns the first element of the sequence as an optional value, or an empty optional if the sequence
	/// contains no elements.
	/// </summary>
	/// <returns>An <see cref="Optional{TSource}"/> containing the first element of the sequence, or an empty optional if the
	/// sequence is empty.</returns>
	public static Optional<TSource> FirstOrEmpty<TSource>(this IEnumerable<TSource> source)
	{
		ArgumentNullException.ThrowIfNull(source);

		foreach (TSource? item in source)
		{
			return Optional.Some(item);
		}
		return Optional.Empty<TSource>();
	}

	/// <summary>
	/// Returns the first element that matches the specified predicate, or an empty optional if no
	/// such element is found.
	/// </summary>
	/// <param name="source"></param>
	/// <param name="predicate">A function to test each element for a condition. Cannot be null.</param>
	/// <returns>An <see cref="Optional{TSource}"/> containing the first element of the sequence, or an empty optional if the
	/// sequence is empty.</returns>
	public static Optional<TSource> FirstOrEmpty<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		ArgumentNullException.ThrowIfNull(source);
		ArgumentNullException.ThrowIfNull(predicate);

		foreach (TSource? item in source)
		{
			if (predicate(item))
			{
				return Optional.Some(item);
			}
		}
		return Optional.Empty<TSource>();
	}

	/// <summary>
	/// Returns a sequence of values contained in non-empty optional elements from the source collection.
	/// </summary>
	/// <returns>An enumerable collection of values of type TSource extracted from non-empty optional elements. The
	/// collection is empty if no optional elements contain a value.</returns>
	public static IEnumerable<TSource> WhereSome<TSource>(this IEnumerable<Optional<TSource>> source)
	{
		ArgumentNullException.ThrowIfNull(source);
		return source
			.Where(optional => optional.IsNotEmpty)
			.Select(optional => optional.Value);
	}

	/// <summary>
	/// Returns the first element of the sequence as an optional value, or an empty optional if the sequence
	/// contains no elements.
	/// </summary>
	/// <returns>An <see cref="Optional{TSource}"/> containing the first element of the sequence, or an empty optional if the
	/// sequence is empty.</returns>
	public static async Task<Optional<TSource>> FirstOrEmptyAsync<TSource>(this IAsyncEnumerable<TSource> source)
	{
		ArgumentNullException.ThrowIfNull(source);
		await foreach (TSource? item in source.ConfigureAwait(false))
		{
			return Optional.Some(item);
		}

		return Optional.Empty<TSource>();
	}

	/// <summary>
	/// Asynchronously returns the first element that matches the specified predicate, or an empty optional if no
	/// such element is found.
	/// </summary>
	/// <param name="source"></param>
	/// <param name="predicate">A function to test each element for a condition. Cannot be null.</param>
	/// <returns>A task that represents the asynchronous operation. The task result contains an <see
	/// cref="Optional{TSource}"/> with the first matching element, or an empty optional if no element matches the
	/// predicate.</returns>
	public static async Task<Optional<TSource>> FirstOrEmptyAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		ArgumentNullException.ThrowIfNull(source);
		ArgumentNullException.ThrowIfNull(predicate);

		await foreach (TSource? item in source.ConfigureAwait(false))
		{
			if (predicate(item))
			{
				return Optional.Some(item);
			}
		}

		return Optional.Empty<TSource>();
	}

	/// <summary>
	/// Returns a sequence of values contained in non-empty optional elements from the source collection.
	/// </summary>
	/// <returns>An enumerable collection of values of type TSource extracted from non-empty optional elements. The
	/// collection is empty if no optional elements contain a value.</returns>
	public static IAsyncEnumerable<TSource> WhereSomeAsync<TSource>(this IAsyncEnumerable<Optional<TSource>> source)
	{
		ArgumentNullException.ThrowIfNull(source);
		return source
			.Where(optional => optional.IsNotEmpty)
			.Select(optional => optional.Value);
	}
}
