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
using System.Diagnostics.CodeAnalysis;

namespace Xpandables.Net.Primtives.Optionals;

/// <summary>
/// Provides static methods for creating optional values without specifying a type parameter explicitly.
/// </summary>
/// <remarks>The Optional type serves as a non-generic entry point for constructing instances of the generic
/// <see cref="Optional{T}"/> type. It enables the creation of empty or value-containing optionals in a concise manner.</remarks>
[SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "<Pending>")]
public readonly record struct Optional
{
    /// <summary>
    /// Returns an empty <see cref="Optional{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <returns>An empty <see cref="Optional{T}"/>.</returns>
    public static Optional<T> Empty<T>() => new(null);

    /// <summary>
    /// Returns an <see cref="Optional{T}"/> with a value.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to wrap in an <see cref="Optional{T}"/>.</param>
    /// <returns>An <see cref="Optional{T}"/> with the specified value.</returns>
    public static Optional<T> Some<T>(T value) => new(value);
}