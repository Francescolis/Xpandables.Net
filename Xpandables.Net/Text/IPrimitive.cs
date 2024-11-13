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
using System.ComponentModel;

namespace Xpandables.Net.Text;

/// <summary>
/// Represents a primitive value.
/// </summary>
public interface IPrimitive
{
    /// <summary>
    /// Gets the value of the primitive.
    /// </summary>
    object Value { get; }
}

/// <summary>
/// Represents a primitive value of a specific type.
/// </summary>
/// <typeparam name="TPrimitive">The type of the primitive.</typeparam>
/// <typeparam name="TValue">The type of the value.</typeparam>
public interface IPrimitive<TPrimitive, TValue> : IPrimitive
    where TPrimitive : struct, IPrimitive<TPrimitive, TValue>
    where TValue : notnull
{
    /// <summary>
    /// Gets the value of the primitive.
    /// </summary>
    new TValue Value { get; }

    /// <summary>
    /// Creates a new instance of the primitive with the specified value.
    /// </summary>
    /// <param name="value">The value to create the primitive with.</param>
    /// <returns>A new instance of the primitive.</returns>
    static abstract TPrimitive Create(TValue value);

    /// <summary>
    /// Gets the default instance of the primitive.
    /// </summary>
    /// <returns>The default instance of the primitive.</returns>
    static abstract TPrimitive Default();

    /// <summary>
    /// Defines an implicit conversion of a primitive to its value type.
    /// </summary>
    /// <param name="self">The primitive to convert.</param>
    /// <returns>The value of the primitive.</returns>
#pragma warning disable CA2225 // Operator overloads have named alternates
    static abstract implicit operator TValue(TPrimitive self);
#pragma warning restore CA2225 // Operator overloads have named alternates

    /// <summary>
    /// Defines an implicit conversion of a value type to its primitive type.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A new instance of the primitive.</returns>
#pragma warning disable CA2225 // Operator overloads have named alternates
    static abstract implicit operator TPrimitive(TValue value);
#pragma warning restore CA2225 // Operator overloads have named alternates

    /// <summary>
    /// Defines an implicit conversion of a primitive to string.
    /// </summary>
    /// <param name="self">The primitive to convert.</param>
    /// <returns>The string value of the primitive.</returns>
#pragma warning disable CA2225 // Operator overloads have named alternates
    static abstract implicit operator string(TPrimitive self);
#pragma warning restore CA2225 // Operator overloads have named alternates

    [EditorBrowsable(EditorBrowsableState.Never)]
    object IPrimitive.Value => Value;
}