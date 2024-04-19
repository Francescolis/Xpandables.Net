
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
public partial record struct Optional<T>
{
    /// <summary>
    /// Applies the <paramref name="some"/> method if the instance contains 
    /// a value. Otherwise, returns the current instance.
    /// </summary>
    /// <param name="some">The method that get called 
    /// when the instance contains a value.</param>
    /// <returns>The current instance where the <paramref name="some"/> 
    /// has been applied if the instance contains a value.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="some"/> is null.</exception>
    public async ValueTask<Optional<T>> MapAsync(Func<T, ValueTask<T>> some)
    {
        ArgumentNullException.ThrowIfNull(some);

        if (HasValue)
            _value = await some(Value).ConfigureAwait(false);

        return this;
    }

    /// <summary>
    /// Applies the <paramref name="some"/> method if the 
    /// instance contains a value.
    /// Otherwise, returns the current instance.
    /// </summary>
    /// <param name="some">The method that get called when the 
    /// instance contains a value.</param>
    /// <returns>The current instance where the <paramref name="some"/> h
    /// as been applied if the instance contains a value.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="some"/> is null.</exception>
    public async ValueTask<Optional<T>> MapAsync(Func<T, ValueTask> some)
    {
        ArgumentNullException.ThrowIfNull(some);

        if (HasValue)
        {
            T value = Value;
            await some(value).ConfigureAwait(false);
            _value = value;
        }

        return this;

    }

    /// <summary>
    /// Turns the current instance to a new <see cref="Optional{T}"/> 
    /// using the specified binder.
    /// </summary>
    /// <typeparam name="TU">The type of the result.</typeparam>
    /// <param name="binder">The binding method.</param>
    /// <returns>A new optional that could contain a value or not.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="binder"/> is null.</exception>
    public async readonly ValueTask<Optional<TU>> BindAsync<TU>(
        Func<T?, ValueTask<Optional<TU>>> binder)
    {
        ArgumentNullException.ThrowIfNull(binder);

        return await binder((T?)_value).ConfigureAwait(false);
    }

    /// <summary>
    /// Returns a new <see cref="Optional{T}"/> using 
    /// <paramref name="empty"/> method if the instance is empty.
    /// </summary>
    /// <param name="empty">The method that get called 
    /// when the instance is empty.</param>
    /// <returns>The replacement value.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="empty"/> is null.</exception>
    /// <returns>The current instance where the <paramref name="empty"/> 
    /// has been applied if the instance is empty.</returns>
    public async ValueTask<Optional<T>> EmptyAsync(Func<ValueTask<T>> empty)
    {
        ArgumentNullException.ThrowIfNull(empty);

        if (!HasValue)
            _value = await empty().ConfigureAwait(false);

        return this;
    }

    /// <summary>
    /// Applies the <paramref name="empty"/> method if the instance is empty.
    /// Otherwise, returns the current instance.
    /// </summary>
    /// <param name="empty">The method that get called 
    /// when the instance is empty.</param>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="empty"/> is null.</exception>
    /// <returns>The current instance where the <paramref name="empty"/> 
    /// has been applied if the instance is empty.</returns>
    public async readonly ValueTask<Optional<T>> EmptyAsync(
        Func<ValueTask> empty)
    {
        ArgumentNullException.ThrowIfNull(empty);

        if (!HasValue)
            await empty().ConfigureAwait(false);

        return this;
    }
}
