
/*******************************************************************************
 * Copyright (C) 2023 Francis-Black EWANE
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
/// Provides a set of <see langword="static"/> methods 
/// for <see cref="Optional{T}"/>.
/// </summary>
public static class OptionalExtensions
{
    /// <summary>
    /// Applies the <paramref name="some"/> method if the 
    /// instance contains a value.
    /// Otherwise, returns the current instance.
    /// </summary>
    /// <typeparam name="T">The type of the optional value.</typeparam>
    /// <param name="optional">The optional to act on.</param>
    /// <param name="some">The method that get called when the i
    /// nstance contains a value.</param>
    /// <returns>The current instance where the <paramref name="some"/> 
    /// has been applied if the instance contains a value.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="optional"/>
    /// or <paramref name="some"/> is null.</exception>
    public static async Task<Optional<T>> MapAsync<T>(
        this Task<Optional<T>> optional,
        Func<T, Task<T>> some)
    {
        ArgumentNullException.ThrowIfNull(optional);
        ArgumentNullException.ThrowIfNull(some);

        return await (await optional.ConfigureAwait(false))
            .MapAsync(some).ConfigureAwait(false);
    }

    /// <summary>
    /// Applies the <paramref name="some"/> method if the instance
    /// contains a value.
    /// Otherwise, returns the current instance.
    /// </summary>
    /// <typeparam name="T">The type of the optional value.</typeparam>
    /// <param name="optional">The optional to act on.</param>
    /// <param name="some">The method that get called when the 
    /// instance contains a value.</param>
    /// <returns>The current instance where the <paramref name="some"/> 
    /// has been applied if the instance contains a value.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="optional"/> 
    /// or <paramref name="some"/> is null.</exception>
    public static async Task<Optional<T>> MapAsync<T>(
        this Task<Optional<T>> optional,
        Func<T, Task> some)
    {
        ArgumentNullException.ThrowIfNull(optional);
        ArgumentNullException.ThrowIfNull(some);

        return await (await optional.ConfigureAwait(false))
            .MapAsync(some).ConfigureAwait(false);
    }

    /// <summary>
    /// Turns the current instance to a new <see cref="Optional{T}"/> 
    /// using the specified binder.
    /// </summary>
    /// <typeparam name="T">The type of the optional value.</typeparam>
    /// <typeparam name="TU">The type of the result.</typeparam>
    /// <param name="optional">The current instance.</param>
    /// <param name="binder">The binding method.</param>
    /// <returns>A new optional that contains a value or not.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="optional"/> 
    /// or <paramref name="binder"/> is null.</exception>
    public static async Task<Optional<TU>> BindAsync<T, TU>(
        this Task<Optional<T>> optional,
        Func<T, Task<Optional<TU>>> binder)
    {
        ArgumentNullException.ThrowIfNull(optional);
        ArgumentNullException.ThrowIfNull(binder);

        Optional<T> value = await optional.ConfigureAwait(false);
        return value.IsNotEmpty
            ? await binder(value.Value).ConfigureAwait(false)
            : Optional.Empty<TU>();
    }

    /// <summary>
    /// Turns the current instance to a new type <typeparamref name="TU"/> 
    /// using the specified binder.
    /// </summary>
    /// <typeparam name="T">The type of the optional value.</typeparam>
    /// <typeparam name="TU">The type of the result.</typeparam>
    /// <param name="optional">The current instance.</param>
    /// <param name="binder">The binding method.</param>
    /// <returns>A new optional that could contain a value or not.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="binder"/> is null.</exception>
    public static async Task<Optional<TU>> BindAsync<T, TU>(
        this Task<Optional<T>> optional,
        Func<T, Task<TU>> binder)
    {
        ArgumentNullException.ThrowIfNull(optional);
        ArgumentNullException.ThrowIfNull(binder);

        Optional<T> value = await optional.ConfigureAwait(false);
        return value.IsNotEmpty
            ? await binder(value.Value).ConfigureAwait(false)
            : Optional.Empty<TU>();
    }

    /// <summary>
    /// Returns a new <see cref="Optional{T}"/> using <paramref name="empty"/> 
    /// method if the instance is empty.
    /// </summary>
    /// <typeparam name="T">The type of the optional value.</typeparam>
    /// <param name="optional">The current instance.</param>
    /// <param name="empty">The method that get called when the 
    /// instance is empty.</param>
    /// <returns>The current instance where the <paramref name="empty"/> 
    /// has been applied if the instance is empty.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="optional"/> 
    /// or <paramref name="empty"/> is null.</exception>
    public static async Task<Optional<T>> EmptyAsync<T>(
        this Task<Optional<T>> optional,
        Func<Task<T>> empty)
    {
        ArgumentNullException.ThrowIfNull(optional);
        ArgumentNullException.ThrowIfNull(empty);

        return await (await optional.ConfigureAwait(false))
            .EmptyAsync(empty).ConfigureAwait(false);
    }

    /// <summary>
    /// Applies the <paramref name="empty"/> method if the instance is empty.
    /// Otherwise, returns the current instance.
    /// </summary>
    /// <typeparam name="T">The type of the optional value.</typeparam>
    /// <param name="optional">The current instance.</param>
    /// <param name="empty">The method that get called 
    /// when the instance is empty.</param>
    /// <returns>The current instance where the <paramref name="empty"/> 
    /// has been applied if the instance is empty.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="optional"/> 
    /// or <paramref name="empty"/> is null.</exception>
    public static async Task EmptyAsync<T>(
        this Task<Optional<T>> optional,
        Func<Task> empty)
    {
        ArgumentNullException.ThrowIfNull(optional);
        ArgumentNullException.ThrowIfNull(empty);

        _ = await (await optional.ConfigureAwait(false))
            .EmptyAsync(empty).ConfigureAwait(false);
    }

    /// <summary>
    /// Returns the value of the current instance if not empty, 
    /// otherwise returns the <paramref name="defaultValue"/>.
    /// </summary>
    /// <typeparam name="T">The type of the optional value.</typeparam>
    /// <param name="optional">The current instance.</param>
    /// <param name="defaultValue">The delegate that returns 
    /// the value if the instance is empty.</param>
    /// <returns>The value from the instance if exists 
    /// or the <paramref name="defaultValue"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="optional"/> 
    /// or <paramref name="defaultValue"/> is null.</exception>
    public static async Task<T> ValueOrDefaultAsync<T>(
        this Task<Optional<T>> optional,
        Task<T> defaultValue)
    {
        ArgumentNullException.ThrowIfNull(optional);
        ArgumentNullException.ThrowIfNull(defaultValue);

        if ((await optional.ConfigureAwait(false)).IsEmpty)
        {
            return await defaultValue.ConfigureAwait(false);
        }

        return (await optional.ConfigureAwait(false)).Value;
    }

    /// <summary>
    /// Returns the value of the current instance if not empty, 
    /// otherwise returns the <paramref name="defaultValue"/>.
    /// </summary>
    /// <typeparam name="T">The type of the optional value.</typeparam>
    /// <param name="optional">The current instance.</param>
    /// <param name="defaultValue">The delegate that returns 
    /// the value if the instance is empty.</param>
    /// <returns>The value from the instance if exists 
    /// or the <paramref name="defaultValue"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="optional"/> 
    /// or <paramref name="defaultValue"/> is null.</exception>
    public static async Task<T> ValueOrDefaultAsync<T>(
        this Task<Optional<T>> optional,
        Func<Task<T>> defaultValue)
    {
        ArgumentNullException.ThrowIfNull(optional);
        ArgumentNullException.ThrowIfNull(defaultValue);

        if ((await optional.ConfigureAwait(false)).IsEmpty)
        {
            return await defaultValue().ConfigureAwait(false);
        }

        return (await optional.ConfigureAwait(false)).Value;
    }

    /// <summary>
    /// Applies the <paramref name="some"/> method if the
    /// instance contains a value,
    /// otherwise, returns the <paramref name="empty"/> method.
    /// </summary>
    /// <typeparam name="T">The type of the optional value.</typeparam>
    /// <typeparam name="TU">The type of the result.</typeparam>
    /// <param name="optional">The current instance.</param>
    /// <param name="some">The method that get called when the 
    /// instance contains a value.</param>
    /// <param name="empty">The method that get called 
    /// when the instance is empty.</param>
    /// <returns>The result of the <paramref name="some"/> 
    /// or <paramref name="empty"/> method.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="some"/> 
    /// or <paramref name="empty"/> is null.</exception>
    public static TU Match<T, TU>(
        this Optional<T> optional,
        Func<T, TU> some,
        Func<TU> empty)
    {
        ArgumentNullException.ThrowIfNull(optional);
        ArgumentNullException.ThrowIfNull(some);
        ArgumentNullException.ThrowIfNull(empty);

        return optional.IsNotEmpty
            ? some(optional.Value)
            : empty();
    }

    /// <summary>
    /// Applies the <paramref name="some"/> method if the instance 
    /// contains a value,
    /// otherwise, apply the <paramref name="empty"/> method.
    /// </summary>
    /// <typeparam name="T">The type of the optional value.</typeparam>
    /// <param name="optional">The current instance.</param>
    /// <param name="some">The method that get called when the instance 
    /// contains a value.</param>
    /// <param name="empty">The method that get called 
    /// when the instance is empty.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="some"/> 
    /// or <paramref name="empty"/> is null.</exception>
    public static void Match<T>(
        this Optional<T> optional,
        Action<T> some,
        Action empty)
    {
        ArgumentNullException.ThrowIfNull(optional);
        ArgumentNullException.ThrowIfNull(some);
        ArgumentNullException.ThrowIfNull(empty);

        if (optional.IsNotEmpty)
        {
            some(optional.Value);
        }
        else
        {
            empty();
        }
    }

    /// <summary>
    /// Applies the <paramref name="some"/> method if the instance 
    /// contains a value,
    /// otherwise, applies the <paramref name="empty"/> method.
    /// </summary>
    /// <typeparam name="TU">The type of the result.</typeparam>
    /// <typeparam name="T">The type of the optional value.</typeparam>
    /// <param name="optional">The current instance.</param>
    /// <param name="some">The method that get called 
    /// when the instance contains a value.</param>
    /// <param name="empty">The method that get called 
    /// when the instance is empty.</param>
    /// <returns>The result of the <paramref name="some"/> method 
    /// if the instance contains a value,
    /// otherwise, the result of the <paramref name="empty"/> method.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="some"/> 
    /// or <paramref name="empty"/> is null.</exception>
    public static async Task<TU> MatchAsync<T, TU>(
        this Task<Optional<T>> optional,
        Func<T, Task<TU>> some,
        Func<Task<TU>> empty)
    {
        ArgumentNullException.ThrowIfNull(optional);
        ArgumentNullException.ThrowIfNull(some);
        ArgumentNullException.ThrowIfNull(empty);

        Optional<T> value = await optional.ConfigureAwait(false);
        return value.IsNotEmpty
            ? await some(value.Value).ConfigureAwait(false)
            : await empty().ConfigureAwait(false);
    }

    /// <summary>
    /// Applies the <paramref name="some"/> method if the 
    /// instance contains a value,
    /// otherwise, applies the <paramref name="empty"/> method.
    /// </summary>
    /// <typeparam name="T">The type of the optional value.</typeparam>
    /// <param name="optional">The current instance.</param>
    /// <param name="some">The method that get called 
    /// when the instance contains a value.</param>
    /// <param name="empty">The method that get called 
    /// when the instance is empty.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="some"/> 
    /// or <paramref name="empty"/> is null.</exception>
    public static async Task MatchAsync<T>(
        this Task<Optional<T>> optional,
        Func<T, Task> some,
        Func<Task> empty)
    {
        ArgumentNullException.ThrowIfNull(optional);
        ArgumentNullException.ThrowIfNull(some);
        ArgumentNullException.ThrowIfNull(empty);

        Optional<T> value = await optional.ConfigureAwait(false);
        if (value.IsNotEmpty)
        {
            await some(value.Value).ConfigureAwait(false);
        }
        else
        {
            await empty().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Returns the value of the first element in the sequence if not empty, 
    /// otherwise returns the empty optional.
    /// </summary>
    /// <typeparam name="T">The type of the optional value.</typeparam>
    /// <param name="source">The sequence to act on.</param>
    /// <returns>The value from the sequence if exists or the empty optional.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="source"/> is null.</exception>
    public static Optional<T> FirstOrEmpty<T>(this IEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        using IEnumerator<T> enumerator = source.GetEnumerator();
        return enumerator.MoveNext()
            ? Optional.Some(enumerator.Current)
            : Optional.Empty<T>();
    }

    /// <summary>
    /// Returns the value of the first element 
    /// in the asynchronous sequence if not empty,
    /// otherwise returns the empty optional.
    /// </summary>
    /// <typeparam name="T">The type of the optional value.</typeparam>
    /// <param name="source">The sequence to act on.</param>
    /// <returns>The value from the sequence if exists 
    /// or the empty optional.</returns>
    /// <exception cref="ArgumentNullException">The
    /// <paramref name="source"/> is null.</exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Reliability",
        "CA2007:Consider calling ConfigureAwait on the awaited task",
        Justification = "<Pending>")]
    public static async Task<Optional<T>> FirstOrEmptyAsync<T>(
        this IAsyncEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        await using IAsyncEnumerator<T> enumerator = source.GetAsyncEnumerator();
        return await enumerator.MoveNextAsync().ConfigureAwait(false)
            ? Optional.Some(enumerator.Current)
            : Optional.Empty<T>();
    }

    /// <summary>
    /// Returns the value of the first element in the 
    /// sequence that satisfies a condition if not empty,
    /// otherwise returns the empty optional.
    /// </summary>
    /// <typeparam name="T">The type of the optional value.</typeparam>
    /// <param name="source">The sequence to act on.</param>
    /// <param name="predicate">A function to test each element 
    /// for a condition.</param>
    /// <returns>The value from the sequence if exists 
    /// or the empty optional.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> 
    /// or <paramref name="predicate"/> is null.</exception>"
    public static Optional<T> FirstOrEmpty<T>(
        this IEnumerable<T> source,
        Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

        return source.Where(predicate).FirstOrEmpty();
    }

    /// <summary>
    /// Projects each element of a sequence into a new form and returns the value 
    /// of the first element in the sequence that satisfies a condition 
    /// if not empty, 
    /// otherwise returns the empty optional.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="source">The sequence to act on.</param>
    /// <param name="predicate">A function to test each element 
    /// for a condition.</param>
    /// <returns>The value from the sequence if exists 
    /// or the empty optional.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="source"/> or <paramref name="predicate"/> is null
    /// .</exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Reliability",
        "CA2007:Consider calling ConfigureAwait on the awaited task",
        Justification = "<Pending>")]
    public static async Task<Optional<T>> FirstOrEmptyAsync<T>(
        this IAsyncEnumerable<T> source,
        Func<T, Task<bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

        await using IAsyncEnumerator<T> enumerator =
            source.GetAsyncEnumerator();
        while (await enumerator.MoveNextAsync().ConfigureAwait(false))
        {
            if (await predicate(enumerator.Current).ConfigureAwait(false))
            {
                return Optional.Some(enumerator.Current);
            }
        }

        return Optional.Empty<T>();
    }

    /// <summary>
    /// Filters the current instance using the specified predicate.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="optional">The current instance.</param>
    /// <param name="predicate">The predicate method.</param>
    /// <returns>The current instance where the 
    /// <paramref name="predicate"/> has been applied.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="predicate"/> is null.</exception>
    public static Optional<T> Where<T>(
        this Optional<T> optional,
        Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(optional);
        ArgumentNullException.ThrowIfNull(predicate);

        return optional.IsNotEmpty && !predicate(optional.Value)
            ? Optional.Empty<T>()
            : optional;
    }

    /// <summary>
    /// Projects the value of the current instance to a new form.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TU">The type of the result.</typeparam>
    /// <param name="optional">The current instance.</param>
    /// <param name="selector">The projection method.</param>
    /// <returns>The current instance where the 
    /// <paramref name="selector"/> has been applied.</returns>
    /// <exception cref="ArgumentNullException">
    /// The <paramref name="selector"/> is null.</exception>
    public static Optional<TU> Select<T, TU>(
        this Optional<T> optional,
        Func<T, TU> selector)
    {
        ArgumentNullException.ThrowIfNull(optional);
        ArgumentNullException.ThrowIfNull(selector);

        return optional.IsNotEmpty
            ? selector(optional.Value)
            : Optional.Empty<TU>();
    }

    /// <summary>
    /// Projects each element of a sequence to an <see cref="IEnumerable{T}"/>,
    /// flattens the resulting sequences into one sequence, 
    /// and invokes a result selector
    /// function on each element therein.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TU">The type of the result.</typeparam>
    /// <param name="optional">The current instance.</param>
    /// <param name="selector">The projection method.</param>
    /// <returns>The current instance where the 
    /// <paramref name="selector"/> has been applied.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="selector"/></exception>"
    public static Optional<TU> SelectMany<T, TU>(
        this Optional<T> optional,
        Func<T, Optional<TU>> selector)
    {
        ArgumentNullException.ThrowIfNull(optional);
        ArgumentNullException.ThrowIfNull(selector);

        return optional.Bind(selector);
    }

    /// <summary>
    /// Projects each element of a sequence to an <see cref="IEnumerable{T}"/>,
    /// flattens the resulting sequences into one sequence, 
    /// and invokes a result selector
    /// function on each element therein.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TU">The type of the result.</typeparam>
    /// <typeparam name="TR">The type of the intermediate result.</typeparam>
    /// <param name="optional">The current instance.</param>
    /// <param name="selector">The projection method.</param>
    /// <param name="resultSelector">The result selector method.</param>
    /// <returns>The current instance where the <paramref name="selector"/> 
    /// and <paramref name="resultSelector"/> has been applied.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="selector"/> 
    /// or <paramref name="resultSelector"/> is null.</exception>"
    public static Optional<TU> SelectMany<T, TR, TU>(
        this Optional<T> optional,
        Func<T, Optional<TR>> selector,
        Func<T, TR, TU> resultSelector)
    {
        ArgumentNullException.ThrowIfNull(optional);
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(resultSelector);

        return optional
            .SelectMany(x => selector(x)
                        .Select(y => resultSelector(x, y)));
    }

    /// <summary>
    /// Projects the value of the current instance to a new form.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TU">The type of the result.</typeparam>
    /// <param name="optional">The current instance.</param>
    /// <param name="selector">The projection method.</param>
    /// <returns>The current instance where the 
    /// <paramref name="selector"/> has been applied.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="selector"/> is null.</exception>
    public static async Task<Optional<TU>> SelectAsync<T, TU>(
        this Task<Optional<T>> optional,
        Func<T, Task<TU>> selector)
    {
        ArgumentNullException.ThrowIfNull(optional);
        ArgumentNullException.ThrowIfNull(selector);

        Optional<T> value = await optional.ConfigureAwait(false);
        return value.IsNotEmpty
            ? await selector(value.Value).ConfigureAwait(false)
            : Optional.Empty<TU>();
    }

    /// <summary>
    /// Projects each element of a sequence to an <see cref="IEnumerable{T}"/>,
    /// flattens the resulting sequences into one sequence, 
    /// and invokes a result selector
    /// function on each element therein.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TU">The type of the result.</typeparam>
    /// <param name="optional">The current instance.</param>
    /// <param name="selector">The projection method.</param>
    /// <returns>The current instance where the 
    /// <paramref name="selector"/> has been applied.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="selector"/></exception>"
    public static async Task<Optional<TU>> SelectManyAsync<T, TU>(
        this Task<Optional<T>> optional,
        Func<T, Task<Optional<TU>>> selector)
    {
        ArgumentNullException.ThrowIfNull(optional);
        ArgumentNullException.ThrowIfNull(selector);

        return await optional
                .BindAsync(async x => await selector(x).ConfigureAwait(false))
                .ConfigureAwait(false);
    }

    /// <summary>
    /// Projects each element of a sequence to an <see cref="IEnumerable{T}"/>,
    /// flattens the resulting sequences into one sequence, 
    /// and invokes a result selector
    /// function on each element therein.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TU">The type of the result.</typeparam>
    /// <typeparam name="TR">The type of the intermediate result.</typeparam>
    /// <param name="optional">The current instance.</param>
    /// <param name="selector">The projection method.</param>
    /// <param name="resultSelector">The result selector method.</param>
    /// <returns>The current instance where the <paramref name="selector"/> 
    /// and <paramref name="resultSelector"/> has been applied.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="selector"/> 
    /// or <paramref name="resultSelector"/> is null.</exception>"
    public static async Task<Optional<TU>> SelectManyAsync<T, TR, TU>(
        this Task<Optional<T>> optional,
        Func<T, Task<Optional<TR>>> selector,
        Func<T, TR, Task<TU>> resultSelector)
    {
        ArgumentNullException.ThrowIfNull(optional);
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(resultSelector);

        return await optional
            .SelectManyAsync(async x => await selector(x)
                        .SelectAsync(y => resultSelector(x, y))
                        .ConfigureAwait(false))
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Filters the current instance and returns the value 
    /// of the optionals that are not empty.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="source">The sequence to act on.</param>
    /// <returns>The values of the optionals that are not empty.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="source"/> is null.</exception>
    public static IEnumerable<T> WhereSome<T>(
        this IEnumerable<Optional<T>> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.SelectMany(x => x, (x, y) => y);
    }

    /// <summary>
    /// Filters the current instance and returns 
    /// the value of the optionals that are not empty.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="source">The sequence to act on.</param>
    /// <returns>The values of the optionals that are not empty.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="source"/> is null.</exception>
    public static async IAsyncEnumerable<T> WhereSomeAsync<T>(
        this IAsyncEnumerable<Optional<T>> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        await foreach (Optional<T> optional in source)
        {
            if (optional.IsNotEmpty)
            {
                yield return optional.Value;
            }
        }
    }
}
