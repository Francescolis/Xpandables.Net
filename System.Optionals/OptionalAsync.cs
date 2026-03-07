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
namespace System.Optionals;

public readonly partial record struct Optional<T>
{
    /// <summary>  
    /// Asynchronously transforms the contained value using a function that returns a new value.
    /// </summary>  
    /// <param name="some">A function that receives the current value and returns a replacement value.</param>  
    /// <returns>A new <see cref="Optional{T}"/> wrapping the returned value, or   
    /// the current instance if it is empty.</returns>
    /// <remarks>
    /// <para><strong>Overload guide — <c>MapAsync</c></strong> (async equivalent of <c>Map</c>):</para>
    /// <list type="bullet">
    ///   <item><c>MapAsync(Func&lt;T, Task&lt;T&gt;&gt;)</c> — <em>this overload</em>: transform the value, keep the result.</item>
    ///   <item><c>MapAsync(Func&lt;T, Task&lt;Optional&lt;T&gt;&gt;&gt;)</c> — transform and return an <see cref="Optional{T}"/> (flat-map / bind-like).</item>
    ///   <item><c>MapAsync(Func&lt;T, Task&gt;)</c> — execute a side-effect using the value; the value is preserved unchanged.</item>
    ///   <item><c>MapAsync(Func&lt;Task&gt;)</c> — execute a side-effect that ignores the value; the value is preserved unchanged.</item>
    /// </list>
    /// </remarks>
    public readonly async Task<Optional<T>> MapAsync(Func<T, Task<T>> some)
    {
        ArgumentNullException.ThrowIfNull(some);

        return IsNotEmpty
            ? new Optional<T>(await some(Value).ConfigureAwait(false))
            : this;
    }

    /// <summary>  
    /// Asynchronously transforms the contained value using a function that returns an <see cref="Optional{T}"/>.
    /// </summary>  
    /// <param name="some">A function that receives the current value and returns an <see cref="Optional{T}"/>.</param>  
    /// <returns>The <see cref="Optional{T}"/> returned by <paramref name="some"/>, or   
    /// the current instance if it is empty.</returns>
    /// <remarks>
    /// This overload is a flat-map (monadic bind). The function decides whether the result is
    /// empty or non-empty, unlike the <c>Func&lt;T, Task&lt;T&gt;&gt;</c> overload which always wraps.
    /// See <see cref="MapAsync(Func{T, Task{T}})"/> for the full overload guide.
    /// </remarks>
    public readonly async Task<Optional<T>> MapAsync(Func<T, Task<Optional<T>>> some)
    {
        ArgumentNullException.ThrowIfNull(some);

        return IsNotEmpty
            ? await some(Value).ConfigureAwait(false)
            : this;
    }

    /// <summary>  
    /// Asynchronously executes a side-effect using the contained value, preserving the value unchanged.
    /// </summary>  
    /// <param name="some">An asynchronous action that receives the current value (e.g., logging, persistence).</param>  
    /// <returns>A new <see cref="Optional{T}"/> containing the original value, or   
    /// the current instance if it is empty.</returns>
    /// <remarks>
    /// Use this overload for fire-and-forget async operations where you need the value but don't transform it.
    /// The value is captured before the task runs and re-wrapped afterward.
    /// See <see cref="MapAsync(Func{T, Task{T}})"/> for the full overload guide.
    /// </remarks>
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
    /// Asynchronously executes a side-effect when the optional has a value, ignoring the value itself.
    /// </summary>  
    /// <param name="some">An asynchronous action to execute (e.g., cache warm-up, notifications).</param>  
    /// <returns>The current <see cref="Optional{T}"/> instance unchanged.</returns>
    /// <remarks>
    /// This is the lightest-weight overload — it neither reads nor transforms the value.
    /// See <see cref="MapAsync(Func{T, Task{T}})"/> for the full overload guide.
    /// </remarks>
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
    /// Asynchronously transforms the contained value into a different type.
    /// </summary>  
    /// <typeparam name="TU">The target type produced by the binder function.</typeparam>  
    /// <param name="binder">A function that receives the current value and returns a <typeparamref name="TU"/>.</param>  
    /// <returns>A new <see cref="Optional{TU}"/> wrapping the result, or   
    /// an empty <see cref="Optional{TU}"/> if the current instance is empty.</returns>
    /// <remarks>
    /// <para><strong>Overload guide — <c>BindAsync</c></strong> (async equivalent of <c>Bind</c>):</para>
    /// <list type="bullet">
    ///   <item><c>BindAsync&lt;TU&gt;(Func&lt;T, Task&lt;TU&gt;&gt;)</c> — <em>this overload</em>: transform to a new type, always wraps in <see cref="Optional{TU}"/>.</item>
    ///   <item><c>BindAsync&lt;TU&gt;(Func&lt;T, Task&lt;Optional&lt;TU&gt;&gt;&gt;)</c> — flat-map: the function controls whether the result is empty or non-empty.</item>
    /// </list>
    /// </remarks>
    public readonly async Task<Optional<TU>> BindAsync<TU>(Func<T, Task<TU>> binder)
    {
        ArgumentNullException.ThrowIfNull(binder);

        return IsNotEmpty
            ? new Optional<TU>(await binder(Value).ConfigureAwait(false))
            : Optional.Empty<TU>();
    }

    /// <summary>  
    /// Asynchronously transforms the contained value into a different optional type (flat-map).
    /// </summary>  
    /// <typeparam name="TU">The target type produced by the binder function.</typeparam>  
    /// <param name="binder">A function that receives the current value and returns an <see cref="Optional{TU}"/>.</param>  
    /// <returns>The <see cref="Optional{TU}"/> returned by <paramref name="binder"/>, or   
    /// an empty <see cref="Optional{TU}"/> if the current instance is empty.</returns>
    /// <remarks>
    /// This is the monadic-bind overload. The function can return <see cref="Optional.Empty{TU}"/>
    /// to signal failure, unlike the <c>Func&lt;T, Task&lt;TU&gt;&gt;</c> overload which always wraps.
    /// See <see cref="BindAsync{TU}(Func{T, Task{TU}})"/> for the full overload guide.
    /// </remarks>
    public readonly async Task<Optional<TU>> BindAsync<TU>(Func<T, Task<Optional<TU>>> binder)
    {
        ArgumentNullException.ThrowIfNull(binder);

        return IsNotEmpty
            ? await binder(Value).ConfigureAwait(false)
            : Optional.Empty<TU>();
    }

    /// <summary>  
    /// Asynchronously provides a fallback value when the optional is empty.
    /// </summary>  
    /// <param name="empty">A function that produces a fallback value of type <typeparamref name="T"/>.</param>  
    /// <returns>A new <see cref="Optional{T}"/> wrapping the fallback value, 
    /// or the current instance if it is not empty.</returns>
    /// <remarks>
    /// <para><strong>Overload guide — <c>EmptyAsync</c></strong> (async equivalent of <c>Empty</c>):</para>
    /// <list type="bullet">
    ///   <item><c>EmptyAsync(Func&lt;Task&lt;T&gt;&gt;)</c> — <em>this overload</em>: supply a fallback value, always wraps.</item>
    ///   <item><c>EmptyAsync(Func&lt;Task&lt;Optional&lt;T&gt;&gt;&gt;)</c> — supply a fallback <see cref="Optional{T}"/> (may itself be empty).</item>
    ///   <item><c>EmptyAsync(Func&lt;Task&gt;)</c> — execute a side-effect on empty; returns the current (empty) instance.</item>
    /// </list>
    /// </remarks>
    public readonly async Task<Optional<T>> EmptyAsync(Func<Task<T>> empty)
    {
        ArgumentNullException.ThrowIfNull(empty);

        return IsEmpty ? new Optional<T>(await empty().ConfigureAwait(false)) : this;
    }

    /// <summary>  
    /// Asynchronously provides a fallback <see cref="Optional{T}"/> when the optional is empty.
    /// </summary>  
    /// <param name="empty">A function that produces a fallback <see cref="Optional{T}"/> (which may itself be empty).</param>  
    /// <returns>The <see cref="Optional{T}"/> returned by <paramref name="empty"/>, 
    /// or the current instance if it is not empty.</returns>
    /// <remarks>
    /// Use this overload when the fallback logic can itself fail (returning <see cref="Optional.Empty{T}"/>).
    /// See <see cref="EmptyAsync(Func{Task{T}})"/> for the full overload guide.
    /// </remarks>
    public readonly async Task<Optional<T>> EmptyAsync(Func<Task<Optional<T>>> empty)
    {
        ArgumentNullException.ThrowIfNull(empty);

        return IsEmpty ? await empty().ConfigureAwait(false) : this;
    }

    /// <summary>  
    /// Asynchronously executes a side-effect when the optional is empty, without providing a value.
    /// </summary>  
    /// <param name="empty">An asynchronous action to execute when empty (e.g., logging, alerting).</param>  
    /// <returns>The current (empty) instance of <see cref="Optional{T}"/>.</returns>
    /// <remarks>
    /// The optional remains empty after this call — use the value-producing overloads to provide a fallback.
    /// See <see cref="EmptyAsync(Func{Task{T}})"/> for the full overload guide.
    /// </remarks>
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
