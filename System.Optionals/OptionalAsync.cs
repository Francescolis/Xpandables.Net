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
    /// Asynchronously maps the value of the current <see cref="Optional{T}"/>   
    /// if it has a value.  
    /// </summary>  
    /// <param name="some">A function to apply to the value if it exists.</param>  
    /// <returns>A new <see cref="Optional{T}"/> with the mapped value, or   
    /// the current instance if it is empty.</returns>  
    public readonly async Task<Optional<T>> MapAsync(Func<T, Task<T>> some)
    {
        ArgumentNullException.ThrowIfNull(some);

        return IsNotEmpty
            ? new Optional<T>(await some(Value).ConfigureAwait(false))
            : this;
    }

    /// <summary>  
    /// Asynchronously maps the value of the current <see cref="Optional{T}"/>   
    /// if it has a value.  
    /// </summary>  
    /// <param name="some">A function to apply to the value if it exists,   
    /// returning an <see cref="Optional{T}"/>.</param>  
    /// <returns>A new <see cref="Optional{T}"/> with the mapped value, or   
    /// the current instance if it is empty.</returns>  
    public readonly async Task<Optional<T>> MapAsync(Func<T, Task<Optional<T>>> some)
    {
        ArgumentNullException.ThrowIfNull(some);

        return IsNotEmpty
            ? await some(Value).ConfigureAwait(false)
            : this;
    }

    /// <summary>  
    /// Asynchronously maps the value of the current <see cref="Optional{T}"/>   
    /// if it has a value.  
    /// </summary>  
    /// <param name="some">A function to apply to the value if it exists.</param>  
    /// <returns>A new <see cref="Optional{T}"/> with the mapped value, or   
    /// the current instance if it is empty.</returns>  
    public readonly async Task<Optional<T>> MapAsync(Func<T, Task> some)
    {
        ArgumentNullException.ThrowIfNull(some);

        if (IsNotEmpty)
        {
            T value = Value;
            await some(value).ConfigureAwait(false);
            return new Optional<T>(value);
        }

        return this;
    }

    /// <summary>  
    /// Asynchronously maps the value of the current <see cref="Optional{T}"/>   
    /// if it has a value.  
    /// </summary>  
    /// <param name="some">A function to execute if the value exists.</param>  
    /// <returns>A new <see cref="Optional{T}"/> with the mapped value, or   
    /// the current instance if it is empty.</returns>  
    public readonly async Task<Optional<T>> MapAsync(Func<Task> some)
    {
        ArgumentNullException.ThrowIfNull(some);

        if (IsNotEmpty)
        {
            await some().ConfigureAwait(false);
        }

        return this;
    }

    /// <summary>  
    /// Asynchronously binds the value of the current <see cref="Optional{T}"/>   
    /// if it has a value.  
    /// </summary>  
    /// <typeparam name="TU">The type of the result of the binder function.</typeparam>  
    /// <param name="binder">A function to apply to the value if it exists.</param>  
    /// <returns>A new <see cref="Optional{TU}"/> with the bound value, or   
    /// an empty <see cref="Optional{TU}"/> if the current instance is empty.</returns>  
    public readonly async Task<Optional<TU>> BindAsync<TU>(Func<T, Task<TU>> binder)
    {
        ArgumentNullException.ThrowIfNull(binder);

        return IsNotEmpty
            ? new Optional<TU>(await binder(Value).ConfigureAwait(false))
            : Optional.Empty<TU>();
    }

    /// <summary>  
    /// Asynchronously binds the value of the current <see cref="Optional{T}"/>   
    /// if it has a value.  
    /// </summary>  
    /// <typeparam name="TU">The type of the result of the binder function.</typeparam>  
    /// <param name="binder">A function to apply to the value if it exists,   
    /// returning an <see cref="Optional{TU}"/>.</param>  
    /// <returns>A new <see cref="Optional{TU}"/> with the bound value, or   
    /// an empty <see cref="Optional{TU}"/> if the current instance is empty.</returns>  
    public readonly async Task<Optional<TU>> BindAsync<TU>(Func<T, Task<Optional<TU>>> binder)
    {
        ArgumentNullException.ThrowIfNull(binder);

        return IsNotEmpty
            ? await binder(Value).ConfigureAwait(false)
            : Optional.Empty<TU>();
    }

    /// <summary>  
    /// Asynchronously executes the provided function if the current instance 
    /// is empty, returning a new <see cref="Optional{T}"/> with the result.
    /// </summary>  
    /// <param name="empty">A function to execute if the current instance is 
    /// empty, returning a <see cref="Task{T}"/>.</param>  
    /// <returns>A new <see cref="Optional{T}"/> with the result of the function, 
    /// or the current instance if it is not empty.</returns>  
    public readonly async Task<Optional<T>> EmptyAsync(Func<Task<T>> empty)
    {
        ArgumentNullException.ThrowIfNull(empty);

        return IsEmpty ? new Optional<T>(await empty().ConfigureAwait(false)) : this;
    }

    /// <summary>  
    /// Asynchronously executes the provided function if the current instance 
    /// is empty, returning a new <see cref="Optional{T}"/> with the result.
    /// </summary>  
    /// <param name="empty">A function to execute if the current instance is 
    /// empty, returning an <see cref="Optional{T}"/>.</param>  
    /// <returns>A new <see cref="Optional{T}"/> with the result of the function, 
    /// or the current instance if it is not empty.</returns>  
    public readonly async Task<Optional<T>> EmptyAsync(Func<Task<Optional<T>>> empty)
    {
        ArgumentNullException.ThrowIfNull(empty);

        return IsEmpty ? await empty().ConfigureAwait(false) : this;
    }

    /// <summary>  
    /// Asynchronously executes the provided function if the current instance 
    /// is empty.  
    /// </summary>  
    /// <param name="empty">A function to execute if the current instance is 
    /// empty.</param>  
    /// <returns>The current instance of <see cref="Optional{T}"/>.</returns>  
    public readonly async Task<Optional<T>> EmptyAsync(Func<Task> empty)
    {
        ArgumentNullException.ThrowIfNull(empty);

        if (IsEmpty)
        {
            await empty().ConfigureAwait(false);
        }

        return this;
    }
}
