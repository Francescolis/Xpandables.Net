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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Xpandables.Net.Optionals;

/// <summary>
/// Represents an optional value that may or may not be present.
/// </summary>
/// <typeparam name="T">The type of the value.</typeparam>
/// <remarks>This <see langword="struct"/> is decorated 
/// with <see cref="OptionalJsonConverterFactory"/> 
/// that automatically applies JSON serialization.</remarks>
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
[JsonConverter(typeof(OptionalJsonConverterFactory))]
public readonly partial record struct Optional<T> : IEnumerable<T>
{
    private readonly object? _value = null;
    [MemberNotNullWhen(true, nameof(Value), nameof(_value))]
    private readonly bool HasValue => _value is not null;
    internal Optional(object? value) => _value = value;

    /// <summary>
    /// Gets the value of the optional if it is present.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the value is 
    /// not present.</exception>
    public readonly T Value =>
        _value is T value
            ? value
            : throw new InvalidOperationException("Value is not present.");

    /// <summary>
    /// Gets a value indicating whether the optional is empty.
    /// </summary>
    [MemberNotNullWhen(false, nameof(Value), nameof(_value))]
    public readonly bool IsEmpty => !HasValue;

    /// <summary>
    /// Gets a value indicating whether the optional is not empty.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Value), nameof(_value))]
    public readonly bool IsNotEmpty => HasValue;

    /// <inheritdoc/>
    public readonly IEnumerator<T> GetEnumerator() =>
        HasValue
            ? new List<T> { Value }.GetEnumerator()
            : Enumerable.Empty<T>().GetEnumerator();

    readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private string GetDebuggerDisplay()
    {
        if (IsEmpty)
        {
            return "Empty";
        }

        return Value?.ToString() ?? string.Empty;
    }
}
