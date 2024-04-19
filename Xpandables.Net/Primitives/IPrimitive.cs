
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
using System.ComponentModel;
using System.Text.Json.Serialization;

using Xpandables.Net.Primitives.Converters;

namespace Xpandables.Net.Primitives;

/// <summary>
/// Defines the contract to implement primitive types.
/// </summary>
public interface IPrimitive
{
    /// <summary>
    /// Gets the value of the primitive type.
    /// </summary>
    object Value { get; }

    /// <summary>
    /// Returns the <see cref="string"/> representation 
    /// of the <see cref="Value"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> value 
    /// or <see cref="string.Empty"/>.</returns>
    public string AsString() => Value.ToString() ?? string.Empty;

    /// <summary>
    /// Gets a value indicating whether or not the underlying 
    /// primitive is new (string.IsNullOrEmpty(AsString())).
    /// </summary>
    public bool IsNew() => string.IsNullOrEmpty(AsString());
}

/// <summary>
///  Defines the contract to implement a generic primitive types.
/// </summary>
/// <typeparam name="TValue">The type of the primitive value.</typeparam>
public interface IPrimitive<out TValue> : IPrimitive
    where TValue : notnull
{
    /// <summary>
    /// Gets the value of the primitive type.
    /// </summary>
    /// <remarks>May be <see langword="null"/>.</remarks>
    new TValue Value { get; }

    [JsonIgnore]
    [EditorBrowsable(EditorBrowsableState.Never)]
    object IPrimitive.Value => Value;
}

/// <summary>
/// Defines the contract to implement a generic primitive types.
/// </summary>
/// <typeparam name="TPrimitive">The type of primitive.</typeparam>
/// <typeparam name="TValue">The type of the primitive value.</typeparam>
/// <remarks>Decorate your <see langword="struct"/> implementation 
/// with <see cref="PrimitiveJsonConverterAttribute"/>.</remarks>
public interface IPrimitive<TPrimitive, TValue> : IPrimitive<TValue>
    where TPrimitive : struct, IPrimitive<TPrimitive, TValue>
    where TValue : notnull
{
    /// <summary>
    /// Returns a default instance of <typeparamref name="TPrimitive"/>.
    /// </summary>
    /// <returns>An instance of <typeparamref name="TPrimitive"/>.</returns>
    static abstract TPrimitive DefaultInstance();

    /// <summary>
    /// Creates a new instance of <typeparamref name="TPrimitive"/> type.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>Returns an instance 
    /// of <typeparamref name="TPrimitive"/> with the new value.</returns>
    static abstract TPrimitive CreateInstance(TValue value);

    /// <summary>
    /// Converts the <typeparamref name="TPrimitive"/> type to 
    /// <typeparamref name="TValue"/>.
    /// </summary>
    /// <param name="self">The current instance.</param>
#pragma warning disable CA2225 // Operator overloads have named alternates
    static abstract implicit operator TValue(TPrimitive self);

    /// <summary>
    /// Converts the <typeparamref name="TValue"/> type to 
    /// <typeparamref name="TPrimitive"/>
    /// </summary>
    /// <param name="value"></param>
    static abstract implicit operator TPrimitive(TValue value);

    /// <summary>
    /// Converts the <typeparamref name="TPrimitive"/> type to string.
    /// </summary>
    /// <param name="self"></param>
    static abstract implicit operator string(TPrimitive self);
#pragma warning restore CA2225 // Operator overloads have named alternates
}