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
public readonly partial record struct Optional<T>
{
    /// <summary>
    /// Converts the current <see cref="Optional{T}"/> to a new <see cref="Optional{TU}"/>.
    /// </summary>
    /// <typeparam name="TU">The type to convert to.</typeparam>
    /// <returns>A new <see cref="Optional{TU}"/> with the converted value, or 
    /// an empty <see cref="Optional{TU}"/> if the current instance is empty.</returns>
    public Optional<TU> AsOptional<TU>() =>
        HasValue && Value is TU next ? Optional.Some(next) : Optional.Empty<TU>();

    /// <summary>
    /// Maps the value of the current <see cref="Optional{T}"/> to a new value 
    /// using the specified function.
    /// </summary>
    /// <param name="some">A function to apply to the value if it exists.</param>
    /// <returns>A new <see cref="Optional{T}"/> with the mapped value, or the 
    /// current instance if it is empty.</returns>
    public readonly Optional<T> Map(Func<T, T> some) =>
        HasValue ? new Optional<T>(some(Value)) : this;

    /// <summary>
    /// Maps the value of the current <see cref="Optional{T}"/> to a new 
    /// <see cref="Optional{T}"/> using the specified function.
    /// </summary>
    /// <param name="some">A function to apply to the value if it exists.</param>
    /// <returns>A new <see cref="Optional{T}"/> with the mapped value, or the 
    /// current instance if it is empty.</returns>
    public readonly Optional<T> Map(Func<T, Optional<T>> some) =>
        HasValue ? some(Value) : this;

    /// <summary>
    /// Maps the value of the current <see cref="Optional{T}"/> using the
    /// specified action.
    /// </summary>
    /// <param name="some">An action to apply to the value if it exists.</param>
    /// <returns>A new <see cref="Optional{T}"/> with the same value, or the 
    /// current instance if it is empty.</returns>
    public readonly Optional<T> Map(Action<T> some)
    {
        if (HasValue)
        {
            T value = Value;
            some(value);
            return new Optional<T>(value);
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
    public readonly Optional<TU> Bind<TU>(Func<T, TU> binder) =>
        HasValue ? new Optional<TU>(binder(Value)) : Optional.Empty<TU>();

    /// <summary>
    /// Binds the value of the current <see cref="Optional{T}"/> to a new 
    /// <see cref="Optional{TU}"/> using the specified function.
    /// </summary>
    /// <typeparam name="TU">The type of the value returned by the binder function.</typeparam>
    /// <param name="binder">A function to apply to the value if it exists.</param>
    /// <returns>A new <see cref="Optional{TU}"/> with the bound value, or an empty 
    /// <see cref="Optional{TU}"/> if the current instance is empty.</returns>
    public readonly Optional<TU> Bind<TU>(Func<T, Optional<TU>> binder) =>
        HasValue ? binder(Value) : Optional.Empty<TU>();

    /// <summary>
    /// Returns a new <see cref="Optional{T}"/> with the value provided by the 
    /// specified function if the current instance is empty.
    /// </summary>
    /// <param name="empty">A function to provide a value if the current 
    /// instance is empty.</param>
    /// <returns>A new <see cref="Optional{T}"/> with the value provided by the 
    /// function, or the current instance if it is not empty.</returns>
    public readonly Optional<T> Empty(Func<T> empty) =>
        IsEmpty ? new Optional<T>(empty()) : this;

    /// <summary>
    /// Returns a new <see cref="Optional{T}"/> with the value provided by the 
    /// specified function if the current instance is empty.
    /// </summary>
    /// <param name="empty">A function to provide a new <see cref="Optional{T}"/> 
    /// if the current instance is empty.</param>
    /// <returns>A new <see cref="Optional{T}"/> with the value provided by the 
    /// function, or the current instance if it is not empty.</returns>
    public readonly Optional<T> Empty(Func<Optional<T>> empty) =>
        IsEmpty ? empty() : this;

    /// <summary>
    /// Executes the specified action if the current instance is empty.
    /// </summary>
    /// <param name="empty">An action to execute if the current instance is 
    /// empty.</param>
    /// <returns>The current instance of <see cref="Optional{T}"/>.</returns>
    public readonly Optional<T> Empty(Action empty)
    {
        if (IsEmpty)
        {
            empty();
        }

        return this;
    }
}
