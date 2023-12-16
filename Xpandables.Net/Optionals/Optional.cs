
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
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Xpandables.Net.Optionals;

/// <summary>
/// Describes an object that contains a value or not of a specific type.
/// You can unconditionally calls to its contents using <see cref="System.Linq"/> without testing whether the content is there or not.
/// The enumerator will only return the available value.
/// </summary>
/// <typeparam name="T">The type of the value.</typeparam>
/// <remarks>This interface is decorated with <see cref="OptionalJsonConverterFactory"/> that automatically applies JSON serialization.</remarks>
[JsonConverter(typeof(OptionalJsonConverterFactory))]
public partial record struct Optional<T> : IEnumerable<T>
{
    private object? _value = null;

    /// <summary>
    /// Gets the underlying value.
    /// </summary>
    /// <remarks>First call <see cref="IsNotEmpty"/> before accessing the value.
    /// May throws <see cref="NullReferenceException"/>.</remarks>
    public readonly T Value => (T)_value!;

    [MemberNotNullWhen(true, nameof(Value), nameof(_value))]
    private readonly bool HasValue => _value is not null;
    internal Optional(object? value) => _value = value;

    /// <summary>
    /// Determines whether the current instance is empty.
    /// </summary>
    [MemberNotNullWhen(false, nameof(Value), nameof(_value))]
    public readonly bool IsEmpty => !HasValue;

    /// <summary>
    /// Determines whether the current instance is not empty.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Value), nameof(_value))]
    public readonly bool IsNotEmpty => HasValue;

    ///<inheritdoc/>
    public readonly IEnumerator<T> GetEnumerator()
        => HasValue
            ? (new T[] { Value }).AsEnumerable().GetEnumerator()
            : Enumerable.Empty<T>().GetEnumerator();

    readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Applies the <paramref name="some"/> method if the instance contains a value.
    /// Otherwise, returns the current instance.
    /// </summary>
    /// <param name="some">The method that get called when the instance contains a value.</param>
    /// <returns>The current instance where the <paramref name="some"/> has been applied if the instance contains a value.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="some"/>  is null.</exception>
    public Optional<T> Map(Func<T, T> some)
    {
        ArgumentNullException.ThrowIfNull(some);

        if (HasValue)
            _value = some(Value);

        return this;
    }

    /// <summary>
    /// Applies the <paramref name="some"/> method if the instance contains a value.
    /// Otherwise, returns the current instance.
    /// </summary>
    /// <param name="some">The method that get called when the instance contains a value.</param>
    /// <returns>The current instance where the <paramref name="some"/> has been applied if the instance contains a value.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="some"/> is null.</exception>
    public Optional<T> Map(Action<T> some)
    {
        ArgumentNullException.ThrowIfNull(some);

        if (HasValue)
        {
            T value = Value;
            some(value);
            _value = value;
        }

        return this;
    }

    /// <summary>
    /// Turns the current instance to a new <see cref="Optional{TU}"/> using the specified binder.
    /// </summary>
    /// <typeparam name="TU">The type of the result.</typeparam>
    /// <param name="binder">The binding method.</param>
    /// <returns>A new optional that could contain a value or not.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="binder"/> is null.</exception>
    public readonly Optional<TU> Bind<TU>(Func<T?, Optional<TU>> binder)
    {
        ArgumentNullException.ThrowIfNull(binder);

        return binder((T?)_value);
    }

    /// <summary>
    /// Applies the <paramref name="empty"/> method if the instance is empty.
    /// Otherwise, returns the current instance.
    /// </summary>
    /// <param name="empty">The method that get called when the instance is empty.</param>
    /// <returns>The current instance where the <paramref name="empty"/> has been applied if the instance is empty.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="empty"/> is null.</exception>
    public Optional<T> Reduce(Func<T> empty)
    {
        ArgumentNullException.ThrowIfNull(empty);

        if (!HasValue)
            _value = empty();

        return this;
    }

    /// <summary>
    /// Applies the <paramref name="empty"/> method if the instance is empty.
    /// Otherwise, returns the current instance.
    /// </summary>
    /// <param name="empty">The method that get called when the instance is empty.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="empty"/> is null.</exception>
    /// <returns>The current instance where the <paramref name="empty"/> has been applied if the instance is empty.</returns>
    public readonly Optional<T> Reduce(Action empty)
    {
        ArgumentNullException.ThrowIfNull(empty);

        if (!HasValue)
            empty();

        return this;
    }

    /// <summary>
    /// Returns the value of the current instance if not empty, otherwise returns the default value type.
    /// </summary>
    /// <returns>The value from the instance if exists or the default one.</returns>
    [return: NotNullIfNotNull(nameof(_value))]
    public readonly T? ValueOrDefault()
    {
        if (HasValue)
            return Value;
        return default;
    }

    /// <summary>
    /// Returns the value of the current instance if not empty, otherwise returns the <paramref name="defaultValue"/>.
    /// </summary>
    /// <param name="defaultValue">The value to be returned if the instance is empty.</param>
    /// <returns>The value from the instance if exists or the <paramref name="defaultValue"/>.</returns>
    [return: NotNullIfNotNull(nameof(_value))]
    public readonly T ValueOrDefault(T defaultValue)
    {
        if (HasValue)
            return Value;
        return defaultValue;
    }

    /// <summary>
    /// Returns the value of the current instance if not empty, otherwise returns the <paramref name="defaultValue"/>.
    /// </summary>
    /// <param name="defaultValue">The delegate that returns the value if the instance is empty.</param>
    /// <returns>The value from the instance if exists or the <paramref name="defaultValue"/>.</returns>
    /// <exception cref="ArgumentNullException">the <paramref name="defaultValue"/> is null.</exception>
    public readonly T ValueOrDefault(Func<T> defaultValue)
    {
        ArgumentNullException.ThrowIfNull(defaultValue);

        if (HasValue)
            return Value;
        return defaultValue();
    }
}
