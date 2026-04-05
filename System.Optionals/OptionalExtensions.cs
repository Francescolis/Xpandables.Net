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
using System.Diagnostics.CodeAnalysis;

namespace System.Optionals;

/// <summary>
/// Provides extension methods for optional types.
/// </summary>
public static class OptionalExtensions
{
	/// <summary>
	/// Converts a nullable value to an <see cref="Optional{T}"/>.
	/// </summary>
	/// <returns>An <see cref="Optional{T}"/> with the specified value if it 
	/// is not null; otherwise, an empty <see cref="Optional{T}"/>.</returns>
	public static Optional<T> ToOptional<T>(this T? value) => value is not null ? Optional.Some(value) : Optional.Empty<T>();

	/// <summary>
	/// Safely tries to get the value from the optional.
	/// </summary>
	/// <typeparam name="T">The type of the value.</typeparam>
	/// <param name="optional">The optional value.</param>
	/// <param name="value">When this method returns, contains the value if present; otherwise, the default value.</param>
	/// <returns>true if the optional has a value; otherwise, false.</returns>
	public static bool TryGetValue<T>(this Optional<T> optional, [MaybeNullWhen(false)] out T value)
	{
		if (optional.IsNotEmpty)
		{
			value = optional.Value;
			return true;
		}

		value = default;
		return false;
	}

	/// <summary>
	/// Filters the optional value based on a predicate.
	/// </summary>
	/// <typeparam name="T">The type of the value.</typeparam>
	/// <param name="optional">The optional value.</param>
	/// <param name="predicate">The predicate to apply to the value.</param>
	/// <returns>The original optional if the predicate is true, otherwise 
	/// an empty optional.</returns>
	public static Optional<T> Where<T>(this Optional<T> optional, Func<T, bool> predicate)
	{
		ArgumentNullException.ThrowIfNull(predicate);

		return optional.IsNotEmpty && predicate(optional.Value)
			? optional
			: Optional.Empty<T>();
	}

	/// <summary>
	/// Projects the value of the optional to a new form.
	/// </summary>
	/// <typeparam name="T">The type of the value.</typeparam>
	/// <param name="optional">The optional value.</param>
	/// <typeparam name="TU">The type of the result.</typeparam>
	/// <param name="selector">A transform function to apply to the value.</param>
	/// <returns>An optional containing the transformed value.</returns>
	// ReSharper disable once MemberCanBePrivate.Global
	public static Optional<TU> Select<T, TU>(this Optional<T> optional, Func<T, TU> selector)
	{
		ArgumentNullException.ThrowIfNull(selector);
		return optional.Bind(selector);
	}

	/// <summary>
	/// Projects the value of the optional to a new form using a specified function.
	/// </summary>
	/// <typeparam name="T">The type of the value.</typeparam>
	/// <param name="optional">The optional value.</param>
	/// <typeparam name="TU">The type of the result.</typeparam>
	/// <param name="selector">A transform function to apply to the value.</param>
	/// <returns>An optional containing the transformed value.</returns>
	// ReSharper disable once MemberCanBePrivate.Global
	public static Optional<TU> SelectMany<T, TU>(this Optional<T> optional, Func<T, Optional<TU>> selector)
	{
		ArgumentNullException.ThrowIfNull(selector);
		return optional.Bind(selector);
	}

	/// <summary>
	/// Projects the value of the optional to a new form using a specified 
	/// function and a result selector.
	/// </summary>
	/// <typeparam name="T">The type of the value.</typeparam>
	/// <param name="optional">The optional value.</param>
	/// <typeparam name="TR">The type of the intermediate result.</typeparam>
	/// <typeparam name="TU">The type of the result.</typeparam>
	/// <param name="selector">A transform function to apply to the value.</param>
	/// <param name="resultSelector">A transform function to apply to the 
	/// intermediate result.</param>
	/// <returns>An optional containing the transformed value.</returns>
	public static Optional<TU> SelectMany<T, TR, TU>(this Optional<T> optional, Func<T, Optional<TR>> selector, Func<T, TR, TU> resultSelector)
	{
		ArgumentNullException.ThrowIfNull(selector);
		ArgumentNullException.ThrowIfNull(resultSelector);

		return optional
			.SelectMany(x =>
				selector(x).Select(y =>
					resultSelector(x, y)));
	}

	/// <summary>
	/// Projects the value of the optional to a new form asynchronously.
	/// </summary>
	/// <typeparam name="T">The type of the value.</typeparam>
	/// <param name="optional">The optional value.</param>
	/// <typeparam name="TU">The type of the result.</typeparam>
	/// <param name="selector">A transform function to apply to the value.</param>
	/// <returns>A task that represents the asynchronous operation. 
	/// The task result contains an optional containing the transformed value.</returns>
	public static async Task<Optional<TU>> SelectAsync<T, TU>(this Task<Optional<T>> optional, Func<T, Task<TU>> selector)
	{
		ArgumentNullException.ThrowIfNull(optional);
		ArgumentNullException.ThrowIfNull(selector);

		return await optional.BindAsync(selector).ConfigureAwait(false);
	}

	/// <summary>
	/// Projects the value of the optional to a new form asynchronously.
	/// </summary>
	/// <typeparam name="T">The type of the value.</typeparam>
	/// <param name="optional">The optional value.</param>
	/// <typeparam name="TU">The type of the result.</typeparam>
	/// <param name="selector">A transform function to apply to the value.</param>
	/// <returns>A task that represents the asynchronous operation. 
	/// The task result contains an optional containing the transformed value.</returns>
	public static async Task<Optional<TU>> SelectManyAsync<T, TU>(this Task<Optional<T>> optional, Func<T, Task<Optional<TU>>> selector)
	{
		ArgumentNullException.ThrowIfNull(optional);
		ArgumentNullException.ThrowIfNull(selector);

		return await optional.BindAsync(selector).ConfigureAwait(false);
	}

	/// <summary>
	/// Projects the value of the optional to a new form asynchronously using a
	/// specified function and a result selector.
	/// </summary>
	/// <typeparam name="T">The type of the value.</typeparam>
	/// <param name="optional">The optional value.</param>
	/// <typeparam name="TR">The type of the intermediate result.</typeparam>
	/// <typeparam name="TU">The type of the result.</typeparam>
	/// <param name="selector">A transform function to apply to the value.</param>
	/// <param name="resultSelector">A transform function to apply to the 
	/// intermediate result.</param>
	/// <returns>A task that represents the asynchronous operation. 
	/// The task result contains an optional containing the transformed value.</returns>
	public static async Task<Optional<TU>> SelectManyAsync<T, TR, TU>(this Task<Optional<T>> optional, Func<T, Task<Optional<TR>>> selector, Func<T, TR, Task<TU>> resultSelector)
	{
		ArgumentNullException.ThrowIfNull(optional);
		ArgumentNullException.ThrowIfNull(selector);
		ArgumentNullException.ThrowIfNull(resultSelector);

		return await optional.BindAsync(x =>
			selector(x).BindAsync(y =>
				resultSelector(x, y))).ConfigureAwait(false);
	}

	/// <summary>
	/// Asynchronously maps the value of the optional if it is present.
	/// </summary>
	/// <typeparam name="T">The type of the value.</typeparam>
	/// <param name="optional">The optional value.</param>
	/// <param name="some">The function to apply if the value is present.</param>
	/// <returns>A task that represents the asynchronous operation. 
	/// The task result contains the mapped optional value.</returns>
	public static async Task<Optional<T>> MapAsync<T>(this Task<Optional<T>> optional, Func<T, Task<T>> some)
	{
		ArgumentNullException.ThrowIfNull(optional);
		ArgumentNullException.ThrowIfNull(some);

		return await (await optional.ConfigureAwait(false))
			.MapAsync(some).ConfigureAwait(false);
	}

	/// <summary>
	/// Asynchronously maps the value of the optional if it is present.
	/// </summary>
	/// <typeparam name="T">The type of the value.</typeparam>
	/// <param name="optional">The optional value.</param>
	/// <param name="some">The function to apply if the value is present.</param>
	/// <returns>A task that represents the asynchronous operation. 
	/// The task result contains the mapped optional value.</returns>
	public static async Task<Optional<T>> MapAsync<T>(this Task<Optional<T>> optional, Func<T, Task<Optional<T>>> some)
	{
		ArgumentNullException.ThrowIfNull(optional);
		ArgumentNullException.ThrowIfNull(some);

		return await (await optional.ConfigureAwait(false))
			.MapAsync(some).ConfigureAwait(false);
	}

	/// <summary>
	/// Asynchronously maps the value of the optional if it is present.
	/// </summary>
	/// <typeparam name="T">The type of the value.</typeparam>
	/// <param name="optional">The optional value.</param>
	/// <param name="some">The function to apply if the value is present.</param>
	/// <returns>A task that represents the asynchronous operation. 
	/// The task result contains the mapped optional value.</returns>
	public static async Task<Optional<T>> MapAsync<T>(this Task<Optional<T>> optional, Func<T, Task> some)
	{
		ArgumentNullException.ThrowIfNull(optional);
		ArgumentNullException.ThrowIfNull(some);

		return await (await optional.ConfigureAwait(false))
			.MapAsync(some).ConfigureAwait(false);
	}

	/// <summary>
	/// Asynchronously binds the value of the optional if it is present.
	/// </summary>
	/// <typeparam name="T">The type of the value.</typeparam>
	/// <param name="optional">The optional value.</param>
	/// <typeparam name="TU">The type of the result value.</typeparam>
	/// <param name="some">The function to apply if the value is present.</param>
	/// <returns>A task that represents the asynchronous operation. 
	/// The task result contains the bound optional value.</returns>
	public static async Task<Optional<TU>> BindAsync<T, TU>(this Task<Optional<T>> optional, Func<T, Task<TU>> some)
	{
		ArgumentNullException.ThrowIfNull(optional);
		ArgumentNullException.ThrowIfNull(some);

		return await (await optional.ConfigureAwait(false))
			.BindAsync(some).ConfigureAwait(false);
	}

	/// <summary>
	/// Asynchronously binds the value of the optional if it is present.
	/// </summary>
	/// <typeparam name="T">The type of the value.</typeparam>
	/// <param name="optional">The optional value.</param>
	/// <typeparam name="TU">The type of the result value.</typeparam>
	/// <param name="some">The function to apply if the value is present.</param>
	/// <returns>A task that represents the asynchronous operation. 
	/// The task result contains the bound optional value.</returns>
	public static async Task<Optional<TU>> BindAsync<T, TU>(this Task<Optional<T>> optional, Func<T, Task<Optional<TU>>> some)
	{
		ArgumentNullException.ThrowIfNull(optional);
		ArgumentNullException.ThrowIfNull(some);

		return await (await optional.ConfigureAwait(false))
			.BindAsync(some).ConfigureAwait(false);
	}

	/// <summary>
	/// Asynchronously handles the empty state of the optional if it is empty.
	/// </summary>
	/// <typeparam name="T">The type of the value.</typeparam>
	/// <param name="optional">The optional value.</param>
	/// <param name="empty">The function to apply if the value is empty.</param>
	/// <returns>A task that represents the asynchronous operation. 
	/// The task result contains the optional value.</returns>
	public static async Task<Optional<T>> EmptyAsync<T>(this Task<Optional<T>> optional, Func<Task<T>> empty)
	{
		ArgumentNullException.ThrowIfNull(optional);
		ArgumentNullException.ThrowIfNull(empty);

		return await (await optional.ConfigureAwait(false))
			.EmptyAsync(empty).ConfigureAwait(false);
	}

	/// <summary>
	/// Asynchronously handles the empty state of the optional if it is empty.
	/// </summary>
	/// <typeparam name="T">The type of the value.</typeparam>
	/// <param name="optional">The optional value.</param>
	/// <param name="empty">The function to apply if the value is empty.</param>
	/// <returns>A task that represents the asynchronous operation. 
	/// The task result contains the optional value.</returns>
	public static async Task<Optional<T>> EmptyAsync<T>(this Task<Optional<T>> optional, Func<Task<Optional<T>>> empty)
	{
		ArgumentNullException.ThrowIfNull(optional);
		ArgumentNullException.ThrowIfNull(empty);

		return await (await optional.ConfigureAwait(false))
			.EmptyAsync(empty).ConfigureAwait(false);
	}

	/// <summary>
	/// Asynchronously handles the empty state of the optional if it is empty.
	/// </summary>
	/// <typeparam name="T">The type of the value.</typeparam>
	/// <param name="optional">The optional value.</param>
	/// <param name="empty">The function to apply if the value is empty.</param>
	/// <returns>A task that represents the asynchronous operation. 
	/// The task result contains the optional value.</returns>
	public static async Task<Optional<T>> EmptyAsync<T>(this Task<Optional<T>> optional, Func<Task> empty)
	{
		ArgumentNullException.ThrowIfNull(optional);
		ArgumentNullException.ThrowIfNull(empty);

		return await (await optional.ConfigureAwait(false))
			.EmptyAsync(empty).ConfigureAwait(false);
	}
}
