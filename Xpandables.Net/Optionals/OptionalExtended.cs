
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
/// Provides extended methods for optional types.
/// </summary>
public static class OptionalExtended
{
    /// <summary>
    /// Asynchronously maps the value of the optional if it is present.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="optional">The optional value.</param>
    /// <param name="some">The function to apply if the value is present.</param>
    /// <returns>A task that represents the asynchronous operation. 
    /// The task result contains the mapped optional value.</returns>
    public static async Task<Optional<T>> MapAsync<T>(
        this Task<Optional<T>> optional, Func<T, Task<T>> some) =>
        await (await optional.ConfigureAwait(false))
        .MapAsync(some).ConfigureAwait(false);

    /// <summary>
    /// Asynchronously maps the value of the optional if it is present.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="optional">The optional value.</param>
    /// <param name="some">The function to apply if the value is present.</param>
    /// <returns>A task that represents the asynchronous operation. 
    /// The task result contains the mapped optional value.</returns>
    public static async Task<Optional<T>> MapAsync<T>(
        this Task<Optional<T>> optional, Func<T, Task<Optional<T>>> some) =>
        await (await optional.ConfigureAwait(false))
        .MapAsync(some).ConfigureAwait(false);

    /// <summary>
    /// Asynchronously maps the value of the optional if it is present.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="optional">The optional value.</param>
    /// <param name="some">The function to apply if the value is present.</param>
    /// <returns>A task that represents the asynchronous operation. 
    /// The task result contains the mapped optional value.</returns>
    public static async Task<Optional<T>> MapAsync<T>(
        this Task<Optional<T>> optional, Func<T, Task> some) =>
        await (await optional.ConfigureAwait(false))
        .MapAsync(some).ConfigureAwait(false);

    /// <summary>
    /// Asynchronously binds the value of the optional if it is present.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TU">The type of the result value.</typeparam>
    /// <param name="optional">The optional value.</param>
    /// <param name="some">The function to apply if the value is present.</param>
    /// <returns>A task that represents the asynchronous operation. 
    /// The task result contains the bound optional value.</returns>
    public static async Task<Optional<TU>> BindAsync<T, TU>(
        this Task<Optional<T>> optional, Func<T, Task<TU>> some) =>
        await (await optional.ConfigureAwait(false))
        .BindAsync(some).ConfigureAwait(false);

    /// <summary>
    /// Asynchronously binds the value of the optional if it is present.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TU">The type of the result value.</typeparam>
    /// <param name="optional">The optional value.</param>
    /// <param name="some">The function to apply if the value is present.</param>
    /// <returns>A task that represents the asynchronous operation. 
    /// The task result contains the bound optional value.</returns>
    public static async Task<Optional<TU>> BindAsync<T, TU>(
        this Task<Optional<T>> optional, Func<T, Task<Optional<TU>>> some) =>
        await (await optional.ConfigureAwait(false))
        .BindAsync(some).ConfigureAwait(false);

    /// <summary>
    /// Asynchronously handles the empty state of the optional if it is empty.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="optional">The optional value.</param>
    /// <param name="empty">The function to apply if the value is empty.</param>
    /// <returns>A task that represents the asynchronous operation. 
    /// The task result contains the optional value.</returns>
    public static async Task<Optional<T>> EmptyAsync<T>(
        this Task<Optional<T>> optional, Func<Task<T>> empty) =>
        await (await optional.ConfigureAwait(false))
        .EmptyAsync(empty).ConfigureAwait(false);

    /// <summary>
    /// Asynchronously handles the empty state of the optional if it is empty.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="optional">The optional value.</param>
    /// <param name="empty">The function to apply if the value is empty.</param>
    /// <returns>A task that represents the asynchronous operation. 
    /// The task result contains the optional value.</returns>
    public static async Task<Optional<T>> EmptyAsync<T>(
        this Task<Optional<T>> optional, Func<Task<Optional<T>>> empty) =>
        await (await optional.ConfigureAwait(false))
        .EmptyAsync(empty).ConfigureAwait(false);

    /// <summary>
    /// Asynchronously handles the empty state of the optional if it is empty.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="optional">The optional value.</param>
    /// <param name="empty">The function to apply if the value is empty.</param>
    /// <returns>A task that represents the asynchronous operation. 
    /// The task result contains the optional value.</returns>
    public static async Task<Optional<T>> EmptyAsync<T>(
        this Task<Optional<T>> optional, Func<Task> empty) =>
        await (await optional.ConfigureAwait(false))
        .EmptyAsync(empty).ConfigureAwait(false);
}
