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

[SuppressMessage("Usage", "CA2225:Operator overloads have named alternates")]
public readonly partial record struct Optional<T>
{
    /// <summary>
    /// Determines whether the left <see cref="Optional{T}"/> instance is less than the right <see cref="Optional{T}"/> instance.
    /// </summary>
    /// <param name="left">The left operand to compare.</param>
    /// <param name="right">The right operand to compare.</param>
    /// <returns><see langword="true"/> if the left instance is less than the right instance; otherwise, <see langword="false"/>.</returns>
    public static bool operator <(Optional<T> left, Optional<T> right)
        => left.CompareTo(right) < 0;

    /// <summary>
    /// Determines whether the left <see cref="Optional{T}"/> instance is less than the right <see cref="Optional{T}"/> instance.
    /// </summary>
    /// <param name="left">The left operand to compare.</param>
    /// <param name="right">The right operand to compare.</param>
    /// <returns><see langword="true"/> if the left instance is less than the right instance; otherwise, <see langword="false"/>.</returns>
    public static bool operator <(Optional<T> left, T right)
        => left.CompareTo(right) < 0;

    /// <summary>
    /// Determines whether the left <see cref="Optional{T}"/> instance is less than the right <see cref="Optional{T}"/> instance.
    /// </summary>
    /// <param name="left">The left operand to compare.</param>
    /// <param name="right">The right operand to compare.</param>
    /// <returns><see langword="true"/> if the left instance is less than the right instance; otherwise, <see langword="false"/>.</returns>
    public static bool operator <=(Optional<T> left, Optional<T> right)
        => left.CompareTo(right) <= 0;

    /// <summary>
    /// Determines whether the left <see cref="Optional{T}"/> instance is less than the right <see cref="Optional{T}"/> instance.
    /// </summary>
    /// <param name="left">The left operand to compare.</param>
    /// <param name="right">The right operand to compare.</param>
    /// <returns><see langword="true"/> if the left instance is less than the right instance; otherwise, <see langword="false"/>.</returns>
    public static bool operator <=(Optional<T> left, T right)
        => left.CompareTo(right) <= 0;

    /// <summary>
    /// Determines whether the left <see cref="Optional{T}"/> instance is less than the right <see cref="Optional{T}"/> instance.
    /// </summary>
    /// <param name="left">The left operand to compare.</param>
    /// <param name="right">The right operand to compare.</param>
    /// <returns><see langword="true"/> if the left instance is less than the right instance; otherwise, <see langword="false"/>.</returns>
    public static bool operator >(Optional<T> left, Optional<T> right)
        => left.CompareTo(right) > 0;

    /// <summary>
    /// Determines whether the left <see cref="Optional{T}"/> instance is less than the right <see cref="Optional{T}"/> instance.
    /// </summary>
    /// <param name="left">The left operand to compare.</param>
    /// <param name="right">The right operand to compare.</param>
    /// <returns><see langword="true"/> if the left instance is less than the right instance; otherwise, <see langword="false"/>.</returns>
    public static bool operator >(Optional<T> left, T right)
        => left.CompareTo(right) > 0;

    /// <summary>
    /// Determines whether the left <see cref="Optional{T}"/> instance is less than the right <see cref="Optional{T}"/> instance.
    /// </summary>
    /// <param name="left">The left operand to compare.</param>
    /// <param name="right">The right operand to compare.</param>
    /// <returns><see langword="true"/> if the left instance is less than the right instance; otherwise, <see langword="false"/>.</returns>
    public static bool operator >=(Optional<T> left, Optional<T> right)
        => left.CompareTo(right) >= 0;

    /// <summary>
    /// Determines whether the left <see cref="Optional{T}"/> instance is less than the right <see cref="Optional{T}"/> instance.
    /// </summary>
    /// <param name="left">The left operand to compare.</param>
    /// <param name="right">The right operand to compare.</param>
    /// <returns><see langword="true"/> if the left instance is less than the right instance; otherwise, <see langword="false"/>.</returns>
    public static bool operator >=(Optional<T> left, T right)
        => left.CompareTo(right) >= 0;

    /// <summary>
    /// Converts a value to an optional.
    /// </summary>
    /// <param name="value">The value to be converted.</param>
    // ReSharper disable once UseNullableAnnotationInsteadOfAttribute
    public static implicit operator Optional<T>([AllowNull] T value)
        => value.ToOptional();

    /// <summary>
    /// Converts the <see cref="Optional{OptionalT}"/> to <see cref="Optional{T}"/>.
    /// </summary>
    /// <param name="optional">The target optional.</param>
    public static implicit operator Optional<T>(Optional<Optional<T>> optional)
        => optional.HasValue ? optional.Value : Optional.Empty<T>();

    /// <summary>
    /// Converts an optional to its value.
    /// </summary>
    /// <param name="optional">The optional to act with.</param>
    public static implicit operator T(Optional<T> optional) => optional.Value;
}