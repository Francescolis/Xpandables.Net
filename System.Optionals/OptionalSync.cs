/*******************************************************************************
 * Copyright (C) 2025 Kamersoft
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

public readonly partial record struct Optional<T>
{
    /// <summary>
    /// Converts the current <see cref="Optional{T}"/> to a new <see langword="Optional&lt;TU&gt;"/>.
    /// </summary>
    /// <typeparam name="TU">The type to convert to.</typeparam>
    /// <returns>A new <see langword="Optional&lt;TU&gt;"/> with the converted value, or 
    /// an empty <see langword="Optional&lt;TU&gt;"/> if the current instance is empty.</returns>
    public Optional<TU> ToOptional<TU>() =>
        IsNotEmpty && Value is TU next ? Optional.Some(next) : Optional.Empty<TU>();

    /// <summary>
    /// Maps the value of the current <see cref="Optional{T}"/> to a new value 
    /// using the specified function.
    /// </summary>
    /// <param name="some">A function to apply to the value if it exists.</param>
    /// <returns>A new <see cref="Optional{T}"/> with the mapped value, or the 
    /// current instance if it is empty.</returns>
    public readonly Optional<T> Map(Func<T, T> some)
    {
        ArgumentNullException.ThrowIfNull(some);
        return IsNotEmpty ? new Optional<T>(some(Value)) : this;
    }

    /// <summary>
    /// Maps the value of the current <see cref="Optional{T}"/> to a new 
    /// <see cref="Optional{T}"/> using the specified function.
    /// </summary>
    /// <param name="some">A function to apply to the value if it exists.</param>
    /// <returns>A new <see cref="Optional{T}"/> with the mapped value, or the 
    /// current instance if it is empty.</returns>
    public readonly Optional<T> Map(Func<T, Optional<T>> some)
    {
        ArgumentNullException.ThrowIfNull(some);
        return IsNotEmpty ? some(Value) : this;
    }

    /// <summary>
    /// Maps the value of the current <see cref="Optional{T}"/> using the
    /// specified action.
    /// </summary>
    /// <param name="some">An action to apply to the value if it exists.</param>
    /// <returns>A new <see cref="Optional{T}"/> with the same value, or the 
    /// current instance if it is empty.</returns>
    public readonly Optional<T> Map(Action<T> some)
    {
        ArgumentNullException.ThrowIfNull(some);

        if (IsNotEmpty)
        {
            T value = Value;
            some(value);
            return new Optional<T>(value);
        }

        return this;
    }

    /// <summary>
    /// Executes the specified action if the current <see cref="Optional{T}"/> 
    /// has a value.
    /// </summary>
    /// <param name="some">An action to execute if a value is present.</param>
    /// <returns>The current instance of <see cref="Optional{T}"/>.</returns>
    public readonly Optional<T> Map(Action some)
    {
        ArgumentNullException.ThrowIfNull(some);

        if (IsNotEmpty)
        {
            some();
        }

        return this;
    }

    /// <summary>
    /// Binds the value of the current <see cref="Optional{T}"/> to a new value 
    /// of type <typeparamref name="TU"/> using the specified function.
    /// </summary>
    /// <typeparam name="TU">The type of the value returned by the binder 
    /// function.</typeparam>
    /// <param name="binder">A function to apply to the value if it exists.</param>
    /// <returns>A new <see cref="Optional{TU}"/> with the bound value, or an 
    /// empty <see cref="Optional{TU}"/> if the current instance is empty.</returns>
    public readonly Optional<TU> Bind<TU>(Func<T, TU> binder)
    {
        ArgumentNullException.ThrowIfNull(binder);
        return IsNotEmpty ? new Optional<TU>(binder(Value)) : Optional.Empty<TU>();
    }

    /// <summary>
    /// Binds the value of the current <see cref="Optional{T}"/> to a new 
    /// <see langword="Optional&lt;TU&gt;"/> using the specified function.
    /// </summary>
    /// <typeparam name="TU">The type of the value returned by the binder function.</typeparam>
    /// <param name="binder">A function to apply to the value if it exists.</param>
    /// <returns>A new <see langword="Optional&lt;TU&gt;"/> with the bound value, or an empty 
    /// <see langword="Optional&lt;TU&gt;"/> if the current instance is empty.</returns>
    public readonly Optional<TU> Bind<TU>(Func<T, Optional<TU>> binder)
    {
        ArgumentNullException.ThrowIfNull(binder);
        return IsNotEmpty ? binder(Value) : Optional.Empty<TU>();
    }

    /// <summary>
    /// Returns a new <see cref="Optional{T}"/> with the value provided by the 
    /// specified function if the current instance is empty.
    /// </summary>
    /// <param name="empty">A function to provide a value if the current 
    /// instance is empty.</param>
    /// <returns>A new <see cref="Optional{T}"/> with the value provided by the 
    /// function, or the current instance if it is not empty.</returns>
    public readonly Optional<T> Empty(Func<T> empty)
    {
        ArgumentNullException.ThrowIfNull(empty);
        return IsEmpty ? new Optional<T>(empty()) : this;
    }

    /// <summary>
    /// Returns a new <see cref="Optional{T}"/> with the value provided by the 
    /// specified function if the current instance is empty.
    /// </summary>
    /// <param name="empty">A function to provide a new <see cref="Optional{T}"/> 
    /// if the current instance is empty.</param>
    /// <returns>A new <see cref="Optional{T}"/> with the value provided by the 
    /// function, or the current instance if it is not empty.</returns>
    public readonly Optional<T> Empty(Func<Optional<T>> empty)
    {
        ArgumentNullException.ThrowIfNull(empty);
        return IsEmpty ? empty() : this;
    }

    /// <summary>
    /// Executes the specified action if the current instance is empty.
    /// </summary>
    /// <param name="empty">An action to execute if the current instance is 
    /// empty.</param>
    /// <returns>The current instance of <see cref="Optional{T}"/>.</returns>
    public readonly Optional<T> Empty(Action empty)
    {
        ArgumentNullException.ThrowIfNull(empty);

        if (IsEmpty)
        {
            empty();
        }

        return this;
    }
}
