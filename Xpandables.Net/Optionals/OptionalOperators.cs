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

namespace Xpandables.Net.Optionals;
public readonly partial record struct Optional<T>
{
    ///<inheritdoc/>
    public static bool operator <(Optional<T> left, Optional<T> right)
        => left.CompareTo(right) < 0;

    ///<inheritdoc/>
    public static bool operator <(Optional<T> left, T right)
        => left.CompareTo(right) < 0;

    ///<inheritdoc/>
    public static bool operator <=(Optional<T> left, Optional<T> right)
        => left.CompareTo(right) <= 0;

    ///<inheritdoc/>
    public static bool operator <=(Optional<T> left, T right)
        => left.CompareTo(right) <= 0;

    ///<inheritdoc/>
    public static bool operator >(Optional<T> left, Optional<T> right)
        => left.CompareTo(right) > 0;

    ///<inheritdoc/>
    public static bool operator >(Optional<T> left, T right)
        => left.CompareTo(right) > 0;

    ///<inheritdoc/>
    public static bool operator >=(Optional<T> left, Optional<T> right)
        => left.CompareTo(right) >= 0;

    ///<inheritdoc/>
    public static bool operator >=(Optional<T> left, T right)
        => left.CompareTo(right) >= 0;

    ///<inheritdoc/>
#pragma warning disable CA2225 // Operator overloads have named alternates
    public static implicit operator Optional<T>([AllowNull] T value)
#pragma warning restore CA2225 // Operator overloads have named alternates
        => value.ToOptional();

    ///<inheritdoc/>
#pragma warning disable CA2225 // Operator overloads have named alternates
    public static implicit operator Optional<T>(Optional<Optional<T>> optional)
#pragma warning restore CA2225 // Operator overloads have named alternates
        => optional.HasValue ? optional.Value : Optional.Empty<T>();

    ///<inheritdoc/>
#pragma warning disable CA2225 // Operator overloads have named alternates
    public static implicit operator T(Optional<T> optional)
#pragma warning restore CA2225 // Operator overloads have named alternates
        => optional.Value;
}
