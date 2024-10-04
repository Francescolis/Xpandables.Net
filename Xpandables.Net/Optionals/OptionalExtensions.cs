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
namespace Xpandables.Net.Optionals;
/// <summary>
/// Provides extension methods for optional types.
/// </summary>
public static class OptionalExtensions
{
    /// <summary>
    /// Returns the first element of the sequence as an optional value, or an empty optional if the sequence contains 
    /// no elements.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the sequence.</typeparam>
    /// <param name="source">The sequence to return the first element of.</param>
    /// <returns>An optional containing the first element of the sequence, 
    /// or an empty optional if the sequence contains no elements.</returns>
    public static Optional<T> FirstOrEmpty<T>(this IEnumerable<T> source) =>
        source.Any() ? Optional.Some(source.First()) : Optional.Empty<T>();

    /// <summary>
    /// Returns the first element of the asynchronous sequence as an optional value, or an empty optional if the sequence contains 
    /// no elements.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the sequence.</typeparam>
    /// <param name="source">The asynchronous sequence to return the first element of.</param>
    /// <returns>A task that represents the asynchronous operation. 
    /// The task result contains an optional containing the first element of the sequence, 
    /// or an empty optional if the sequence contains no elements.</returns>
    public static async Task<Optional<T>> FirstOrEmptyAsync<T>(
        this IAsyncEnumerable<T> source) =>
        await source.AnyAsync()
            ? Optional.Some(await source.FirstAsync())
            : Optional.Empty<T>();

    /// <summary>
    /// Filters the optional value based on a predicate.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="optional">The optional value.</param>
    /// <param name="predicate">The predicate to apply to the value.</param>
    /// <returns>The original optional if the predicate is true, otherwise 
    /// an empty optional.</returns>
    public static Optional<T> Where<T>(
        this Optional<T> optional,
        Func<T, bool> predicate) =>
        optional.IsNotEmpty && predicate(optional.Value)
            ? optional
            : Optional.Empty<T>();

    /// <summary>
    /// Filters the sequence of optional values, returning only the values that
    /// are not empty.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="source">The sequence of optional values.</param>
    /// <returns>A sequence of values that are not empty.</returns>
    public static IEnumerable<T> WhereSome<T>(
        this IEnumerable<Optional<T>> source) =>
        source
            .Where(optional => optional.IsNotEmpty)
            .Select(optional => optional.Value);

    /// <summary>
    /// Filters the asynchronous sequence of optional values, returning only the
    /// values that are not empty.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="source">The asynchronous sequence of optional values.</param>
    /// <returns>An asynchronous sequence of values that are not empty.</returns>
    public static IAsyncEnumerable<T> WhereSome<T>(
        this IAsyncEnumerable<Optional<T>> source) =>
        source
            .WhereAwait(optional => ValueTask.FromResult(optional.IsNotEmpty))
            .SelectAwait(optional => ValueTask.FromResult(optional.Value));

    /// <summary>
    /// Projects the value of the optional to a new form.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TU">The type of the result.</typeparam>
    /// <param name="optional">The optional value.</param>
    /// <param name="selector">A transform function to apply to the value.</param>
    /// <returns>An optional containing the transformed value.</returns>
    public static Optional<TU> Select<T, TU>(
        this Optional<T> optional,
        Func<T, TU> selector) => optional.Bind(selector);

    /// <summary>
    /// Projects the value of the optional to a new form asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TU">The type of the result.</typeparam>
    /// <param name="optional">The task representing the optional value.</param>
    /// <param name="selector">A transform function to apply to the value.</param>
    /// <returns>A task that represents the asynchronous operation. 
    /// The task result contains an optional containing the transformed value.</returns>
    public static async Task<Optional<TU>> SelectAsync<T, TU>(
        this Task<Optional<T>> optional,
        Func<T, Task<TU>> selector) =>
        await optional.BindAsync(selector);

    /// <summary>
    /// Projects the value of the optional to a new form using a specified function.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TU">The type of the result.</typeparam>
    /// <param name="optional">The optional value.</param>
    /// <param name="selector">A transform function to apply to the value.</param>
    /// <returns>An optional containing the transformed value.</returns>
    public static Optional<TU> SelectMany<T, TU>(
        this Optional<T> optional,
        Func<T, Optional<TU>> selector) => optional.Bind(selector);

    /// <summary>
    /// Projects the value of the optional to a new form asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TU">The type of the result.</typeparam>
    /// <param name="optional">The task representing the optional value.</param>
    /// <param name="selector">A transform function to apply to the value.</param>
    /// <returns>A task that represents the asynchronous operation. 
    /// The task result contains an optional containing the transformed value.</returns>
    public static async Task<Optional<TU>> SelectManyAsync<T, TU>(
        this Task<Optional<T>> optional,
        Func<T, Task<Optional<TU>>> selector) =>
        await optional.BindAsync(selector);

    /// <summary>
    /// Projects the value of the optional to a new form using a specified 
    /// function and a result selector.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TR">The type of the intermediate result.</typeparam>
    /// <typeparam name="TU">The type of the result.</typeparam>
    /// <param name="optional">The optional value.</param>
    /// <param name="selector">A transform function to apply to the value.</param>
    /// <param name="resultSelector">A transform function to apply to the 
    /// intermediate result.</param>
    /// <returns>An optional containing the transformed value.</returns>
    public static Optional<TU> SelectMany<T, TR, TU>(
        this Optional<T> optional,
        Func<T, Optional<TR>> selector,
        Func<T, TR, TU> resultSelector) =>
        optional
            .SelectMany(x =>
                selector(x).Select(y =>
                    resultSelector(x, y)));

    /// <summary>
    /// Projects the value of the optional to a new form asynchronously using a
    /// specified function and a result selector.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TR">The type of the intermediate result.</typeparam>
    /// <typeparam name="TU">The type of the result.</typeparam>
    /// <param name="optional">The task representing the optional value.</param>
    /// <param name="selector">A transform function to apply to the value.</param>
    /// <param name="resultSelector">A transform function to apply to the 
    /// intermediate result.</param>
    /// <returns>A task that represents the asynchronous operation. 
    /// The task result contains an optional containing the transformed value.</returns>
    public static async Task<Optional<TU>> SelectManyAsync<T, TR, TU>(
        this Task<Optional<T>> optional,
        Func<T, Task<Optional<TR>>> selector,
        Func<T, TR, Task<TU>> resultSelector) =>
        await optional.BindAsync(x =>
            selector(x).BindAsync(y =>
                resultSelector(x, y)));
}
