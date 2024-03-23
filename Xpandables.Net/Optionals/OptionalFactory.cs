
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
public static class Optional
{
    /// <summary>
    /// Provides with an optional of the specific type that is empty.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <returns>An optional with no value.</returns>
    public static Optional<T> Empty<T>() => new(null);

    /// <summary>
    /// Provides with an optional that contains a value of specific type.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to be used for optional.</param>
    /// <returns>An optional with a value.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="value"/> is null.</exception>
    public static Optional<T> Some<T>(T value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return new Optional<T>(value);
    }

    /// <summary>
    /// Provides with an optional that contains a value or not.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to be used for optional.</param>
    /// <returns>An optional that may contains a value or not.</returns>
    public static Optional<T> ToOptional<T>(T? value)
        => value is { } ? Some(value) : Empty<T>();

    /// <summary>
    /// Converts the specified value to an optional instance.
    /// </summary>
    /// <typeparam name="T">The Type of the value.</typeparam>
    /// <param name="value">The value to act on.</param>
    /// <returns>An optional instance.</returns>
    public static Optional<T> AsOptional<T>(this T? value)
        => ToOptional(value);
}
