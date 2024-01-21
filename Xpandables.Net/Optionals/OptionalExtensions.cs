
/************************************************************************************************************
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
************************************************************************************************************/
namespace Xpandables.Net.Optionals;

/// <summary>
/// Provides a set of <see langword="static"/> methods for <see cref="Optional{T}"/>.
/// </summary>
public static class OptionalExtensions
{
    /// <summary>
    /// Applies the <paramref name="some"/> method if the instance contains a value.
    /// Otherwise, returns the current instance.
    /// </summary>
    /// <typeparam name="T">The type of the optional value.</typeparam>
    /// <param name="optional">The optional to act on.</param>
    /// <param name="some">The method that get called when the instance contains a value.</param>
    /// <returns>The current instance where the <paramref name="some"/> has been applied if the instance contains a value.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="optional"/> or <paramref name="some"/> is null.</exception>
    public static async ValueTask<Optional<T>> MapAsync<T>(this ValueTask<Optional<T>> optional, Func<T, ValueTask<T>> some)
    {
        ArgumentNullException.ThrowIfNull(optional);
        ArgumentNullException.ThrowIfNull(some);

        return await (await optional.ConfigureAwait(false))
            .MapAsync(some).ConfigureAwait(false);
    }

    /// <summary>
    /// Applies the <paramref name="some"/> method if the instance contains a value.
    /// Otherwise, returns the current instance.
    /// </summary>
    /// <typeparam name="T">The type of the optional value.</typeparam>
    /// <param name="optional">The optional to act on.</param>
    /// <param name="some">The method that get called when the instance contains a value.</param>
    /// <returns>The current instance where the <paramref name="some"/> has been applied if the instance contains a value.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="optional"/> or <paramref name="some"/> is null.</exception>
    public static async ValueTask<Optional<T>> MapAsync<T>(this ValueTask<Optional<T>> optional, Func<T, ValueTask> some)
    {
        ArgumentNullException.ThrowIfNull(optional);
        ArgumentNullException.ThrowIfNull(some);

        return await (await optional.ConfigureAwait(false))
            .MapAsync(some).ConfigureAwait(false);
    }

    /// <summary>
    /// Turns the current instance to a new <see cref="Optional{T}"/> using the specified binder.
    /// </summary>
    /// <typeparam name="T">The type of the optional value.</typeparam>
    /// <typeparam name="TU">The type of the result.</typeparam>
    /// <param name="optional">The current instance.</param>
    /// <param name="binder">The binding method.</param>
    /// <returns>A new optional that contains a value or not.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="optional"/> or <paramref name="binder"/> is null.</exception>
    public static async ValueTask<Optional<TU>> BindAsync<T, TU>(this ValueTask<Optional<T>> optional, Func<T?, ValueTask<Optional<TU>>> binder)
    {
        ArgumentNullException.ThrowIfNull(optional);
        ArgumentNullException.ThrowIfNull(binder);

        return await (await optional.ConfigureAwait(false))
            .BindAsync(binder).ConfigureAwait(false);
    }

    /// <summary>
    /// Returns a new <see cref="Optional{T}"/> using <paramref name="empty"/> method if the instance is empty.
    /// </summary>
    /// <typeparam name="T">The type of the optional value.</typeparam>
    /// <param name="optional">The current instance.</param>
    /// <param name="empty">The method that get called when the instance is empty.</param>
    /// <returns>The current instance where the <paramref name="empty"/> has been applied if the instance is empty.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="optional"/> or <paramref name="empty"/> is null.</exception>
    public static async ValueTask<Optional<T>> ReduceAsync<T>(this ValueTask<Optional<T>> optional, Func<ValueTask<T>> empty)
    {
        ArgumentNullException.ThrowIfNull(optional);
        ArgumentNullException.ThrowIfNull(empty);

        return await (await optional.ConfigureAwait(false))
            .ReduceAsync(empty).ConfigureAwait(false);
    }

    /// <summary>
    /// Applies the <paramref name="empty"/> method if the instance is empty.
    /// Otherwise, returns the current instance.
    /// </summary>
    /// <typeparam name="T">The type of the optional value.</typeparam>
    /// <param name="optional">The current instance.</param>
    /// <param name="empty">The method that get called when the instance is empty.</param>
    /// <returns>The current instance where the <paramref name="empty"/> has been applied if the instance is empty.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="optional"/> or <paramref name="empty"/> is null.</exception>
    public static async ValueTask ReduceAsync<T>(this ValueTask<Optional<T>> optional, Func<ValueTask> empty)
    {
        ArgumentNullException.ThrowIfNull(optional);
        ArgumentNullException.ThrowIfNull(empty);

        _ = await (await optional.ConfigureAwait(false))
            .ReduceAsync(empty).ConfigureAwait(false);
    }

    /// <summary>
    /// Returns the value of the current instance if not empty, otherwise returns the <paramref name="defaultValue"/>.
    /// </summary>
    /// <typeparam name="T">The type of the optional value.</typeparam>
    /// <param name="optional">The current instance.</param>
    /// <param name="defaultValue">The delegate that returns the value if the instance is empty.</param>
    /// <returns>The value from the instance if exists or the <paramref name="defaultValue"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="optional"/> or <paramref name="defaultValue"/> is null.</exception>
    public static async ValueTask<T> ValueOrDefaultAsync<T>(this ValueTask<Optional<T>> optional, ValueTask<T> defaultValue)
    {
        ArgumentNullException.ThrowIfNull(optional);
        ArgumentNullException.ThrowIfNull(defaultValue);

        if ((await optional.ConfigureAwait(false)).IsEmpty)
            return await defaultValue.ConfigureAwait(false);

        return (await optional.ConfigureAwait(false)).Value;
    }

    /// <summary>
    /// Returns the value of the current instance if not empty, otherwise returns the <paramref name="defaultValue"/>.
    /// </summary>
    /// <typeparam name="T">The type of the optional value.</typeparam>
    /// <param name="optional">The current instance.</param>
    /// <param name="defaultValue">The delegate that returns the value if the instance is empty.</param>
    /// <returns>The value from the instance if exists or the <paramref name="defaultValue"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="optional"/> or <paramref name="defaultValue"/> is null.</exception>
    public static async ValueTask<T> ValueOrDefaultAsync<T>(this ValueTask<Optional<T>> optional, Func<ValueTask<T>> defaultValue)
    {
        ArgumentNullException.ThrowIfNull(optional);
        ArgumentNullException.ThrowIfNull(defaultValue);

        if ((await optional.ConfigureAwait(false)).IsEmpty)
            return await defaultValue().ConfigureAwait(false);

        return (await optional.ConfigureAwait(false)).Value;
    }
}
