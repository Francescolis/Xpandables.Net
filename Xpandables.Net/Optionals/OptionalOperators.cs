
/************************************************************************************************************
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
************************************************************************************************************/
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Xpandables.Net.Optionals;

public partial record struct Optional<T> :
    IEquatable<Optional<T>>, IEquatable<T>,
    IComparable<Optional<T>>, IComparable<T>,
    IStructuralEquatable, IStructuralComparable,
    IFormattable
{
    ///<inheritdoc/>
    public static bool operator <(Optional<T> left, Optional<T> right) => left.CompareTo(right) < 0;

    ///<inheritdoc/>
    public static bool operator <(Optional<T> left, T right) => left.CompareTo(right) < 0;

    ///<inheritdoc/>
    public static bool operator <=(Optional<T> left, Optional<T> right) => left.CompareTo(right) <= 0;

    ///<inheritdoc/>
    public static bool operator <=(Optional<T> left, T right) => left.CompareTo(right) <= 0;

    ///<inheritdoc/>
    public static bool operator >(Optional<T> left, Optional<T> right) => left.CompareTo(right) > 0;

    ///<inheritdoc/>
    public static bool operator >(Optional<T> left, T right) => left.CompareTo(right) > 0;

    ///<inheritdoc/>
    public static bool operator >=(Optional<T> left, Optional<T> right) => left.CompareTo(right) >= 0;

    ///<inheritdoc/>
    public static bool operator >=(Optional<T> left, T right) => left.CompareTo(right) >= 0;

    ///<inheritdoc/>
    public static implicit operator Optional<T>([AllowNull] T value) => ToOptional(value);

    ///<inheritdoc/>
    public static implicit operator Optional<T>(Optional<Optional<T>> optional)
        => optional.HasValue ? optional.Value : Optional.Empty<T>();

    ///<inheritdoc/>
    public static implicit operator T(Optional<T> optional) => optional.Value;

    ///<inheritdoc/>
    public readonly T ToT(Optional<T> optional) => optional.Value;

    ///<inheritdoc/>
    public static Optional<T> ToOptional([AllowNull] T value) => Optional.ToOptional(value);

    ///<inheritdoc/>
    public readonly int CompareTo(Optional<T> other)
    {
        if (HasValue && other.HasValue)
            return Comparer<T>.Default.Compare(Value, other.Value);

        if (HasValue && !other.HasValue)
            return 1;
        else
            return -1;
    }

    ///<inheritdoc/>
    public readonly int CompareTo([AllowNull] T other)
    {
        if (HasValue && other is null)
            return 1;

        if (!HasValue && other is { })
            return -1;

        if (HasValue && other is { })
            return Comparer<T>.Default.Compare(Value, other);

        return 0;
    }

    ///<inheritdoc/>
    public readonly int CompareTo(object? other, IComparer comparer)
    {
        ArgumentNullException.ThrowIfNull(comparer);

        if (other is Optional<T> optional)
            return comparer.Compare(this, optional);

        if (other is T value && HasValue)
            return comparer.Compare(Value, value);

        return -1;
    }

    ///<inheritdoc/>
    public readonly bool Equals(Optional<T> other)
        => HasValue && other.HasValue
            ? Value.Equals(other.Value)
            : !HasValue && !other.HasValue;

    ///<inheritdoc/>
    public readonly bool Equals([AllowNull] T other) => HasValue && other is { };

    ///<inheritdoc/>
    public readonly override int GetHashCode()
    {
        const int hash = 17;
        if (HasValue)
            return Value.GetHashCode() ^ 31;

        return hash ^ 29;
    }

    ///<inheritdoc/>
    public readonly bool Equals(object? other, IEqualityComparer comparer)
    {
        ArgumentNullException.ThrowIfNull(comparer);

        return other is Optional<T> optional
            && comparer.Equals(Value, optional.Value);
    }

    ///<inheritdoc/>
    public readonly int GetHashCode(IEqualityComparer comparer)
    {
        ArgumentNullException.ThrowIfNull(comparer);

        const int hash = 17;
        if (HasValue)
            return GetHashCode() ^ comparer.GetHashCode(Value);

        return hash ^ 29;
    }

    /// <summary>
    /// Returns the string representation of the value in the optional, if not returns <see cref="string.Empty"/> .
    /// </summary>
    /// <returns>The string representation of the value.</returns>
    public readonly override string ToString() => HasValue ? $"{Value}" : string.Empty;

    /// <summary>
    /// Formats the value of the current instance using the specified format.
    /// </summary>
    /// <param name="format">The format to use.
    ///  -or-
    ///  A null reference (<see langword="Nothing" /> in Visual Basic) to use the default format defined
    ///  for the type of the <see cref="IFormattable" /> implementation.</param>
    /// <param name="formatProvider">The provider to use to format the value.
    ///  -or-
    ///  A null reference (<see langword="Nothing" /> in Visual Basic) to obtain the numeric
    ///  format information from the current locale setting of the operating system.</param>
    /// <returns>The value of the current instance in the specified format.</returns>
    public readonly string ToString(string? format, IFormatProvider? formatProvider)
        => HasValue ? string.Format(formatProvider, "{0:" + format + "}", Value) : string.Empty;
}
