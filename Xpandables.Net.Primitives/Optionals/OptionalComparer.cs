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
using System.Collections;

namespace Xpandables.Net.Primitives.Optionals;

public readonly partial record struct Optional<T> :
    IEquatable<Optional<T>>, IEquatable<T>,
    IComparable<Optional<T>>, IComparable<T>,
    IStructuralEquatable, IStructuralComparable,
    IFormattable
{
    /// <inheritdoc/>
    public readonly int CompareTo(Optional<T> other) =>
        IsEmpty && other.IsEmpty
            ? 0
            : IsEmpty
                ? -1
                : other.IsEmpty
                    ? 1
                    : Comparer<T>.Default.Compare(Value, other.Value);

    /// <inheritdoc/>
    public readonly int CompareTo(T? other) =>
        other is not null
            ? IsNotEmpty
                ? Comparer<T>.Default.Compare(Value, other)
                : -1
            : IsNotEmpty ? 1 : 0;

    /// <inheritdoc/>
    public readonly int CompareTo(object? other, IComparer comparer) =>
        other is Optional<T> optional
            ? CompareTo(optional)
            : other is T value
                ? CompareTo(value)
                : -1;

    /// <inheritdoc/>
    public readonly bool Equals(T? other) =>
        other is not null
        && IsNotEmpty && EqualityComparer<T>.Default.Equals(Value, other);

    /// <inheritdoc/>
    public readonly bool Equals(object? other, IEqualityComparer comparer)
    {
        ArgumentNullException.ThrowIfNull(comparer);
        return other is Optional<T> optional
            && IsNotEmpty && optional.IsNotEmpty
            && comparer.Equals(Value, optional.Value);
    }

    /// <inheritdoc/>
    public readonly int GetHashCode(IEqualityComparer comparer)
    {
        ArgumentNullException.ThrowIfNull(comparer);
        HashCode hashCode = new();
        hashCode.Add(IsNotEmpty);
        if (IsNotEmpty)
        {
            hashCode.Add(comparer.GetHashCode(Value));
        }

        return hashCode.ToHashCode();
    }

    /// <summary>
    /// Returns the string representation of the value in the optional, 
    /// if not returns <see cref="string.Empty"/> .
    /// </summary>
    /// <returns>The string representation of the value.</returns>
    public override readonly string ToString()
        => IsNotEmpty ? $"{Value}" : string.Empty;

    /// <inheritdoc/>
    public readonly string ToString(string? format, IFormatProvider? formatProvider) =>
        IsNotEmpty
            ? string.Format(formatProvider, "{0:" + format + "}", Value)
            : string.Empty;
}
