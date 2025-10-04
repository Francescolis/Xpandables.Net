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
using System.Collections.Frozen;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Net.Abstractions.Text;
using System.Runtime.CompilerServices;

namespace System.Net.Repositories;

/// <summary>
/// Represents an immutable, high-performance entity status primitive with automatic JSON serialization support.
/// Provides predefined status values and efficient validation for .NET 10 applications.
/// </summary>
/// <remarks>
/// This implementation leverages modern .NET 10 features including frozen collections for optimal performance,
/// automatic JSON serialization through primitive converters, and comprehensive validation support.
/// Status values are cached for efficient reuse and comparison operations.
/// </remarks>
[PrimitiveJsonConverter]
public readonly record struct EntityStatus : IPrimitive<EntityStatus, string>, IComparable, IComparable<EntityStatus>
{
    #region Private Fields and Constructor

    // Pre-computed frozen collections for optimal performance - initialized inline to avoid static constructor
    private static readonly FrozenDictionary<string, EntityStatus> _statusCache = CreateStatusCache();
    private static readonly FrozenSet<string> _validStatusNames = CreateValidStatusNames();

    private static FrozenDictionary<string, EntityStatus> CreateStatusCache()
    {
        var statusPairs = new Dictionary<string, EntityStatus>(StringComparer.OrdinalIgnoreCase)
        {
            [nameof(ACTIVE)] = new(nameof(ACTIVE)),
            [nameof(PENDING)] = new(nameof(PENDING)),
            [nameof(PROCESSING)] = new(nameof(PROCESSING)),
            [nameof(DELETED)] = new(nameof(DELETED)),
            [nameof(SUSPENDED)] = new(nameof(SUSPENDED)),
            [nameof(ONERROR)] = new(nameof(ONERROR)),
            [nameof(PUBLISHED)] = new(nameof(PUBLISHED))
        };
        return statusPairs.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
    }

    private static FrozenSet<string> CreateValidStatusNames()
    {
        return new[] { nameof(ACTIVE), nameof(PENDING), nameof(PROCESSING), nameof(DELETED), 
                      nameof(SUSPENDED), nameof(ONERROR), nameof(PUBLISHED) }
            .ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    }

    private EntityStatus(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value.ToUpperInvariant();
    }

    #endregion

    #region IPrimitive<EntityStatus, string> Implementation

    /// <inheritdoc />
    public string Value { get; }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static EntityStatus Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        
        // Use cached values for known statuses for better performance
        return _statusCache.TryGetValue(value, out var cachedStatus) 
            ? cachedStatus 
            : new EntityStatus(value);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static EntityStatus GetDefault() => ACTIVE;

    /// <inheritdoc />
    public static bool TryCreate(string? value, [MaybeNullWhen(false)] out EntityStatus status)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            status = Create(value);
            return true;
        }

        status = default;
        return false;
    }

    #endregion

    #region Operators and Conversions

#pragma warning disable CA2225 // Implicit operators are fundamental to primitive design
    /// <summary>
    /// Converts an EntityStatus instance to its string representation.
    /// </summary>
    /// <param name="status">The EntityStatus to convert.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator string(EntityStatus status) => status.Value ?? string.Empty;

    /// <summary>
    /// Converts a string to an EntityStatus instance.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator EntityStatus(string value) => Create(value);
#pragma warning restore CA2225

    #endregion

    #region Comparison Implementation

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(object? obj) => obj switch
    {
        null => 1,
        EntityStatus other => CompareTo(other),
        string str => string.Compare(Value, str, StringComparison.OrdinalIgnoreCase),
        _ => throw new ArgumentException($"Object must be of type {nameof(EntityStatus)} or string.", nameof(obj))
    };

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(EntityStatus other) =>
        string.Compare(Value, other.Value, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(EntityStatus other) =>
        string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode() => 
        StringComparer.OrdinalIgnoreCase.GetHashCode(Value ?? string.Empty);

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString() => Value ?? string.Empty;

    // Comparison operators
    
    /// <summary>
    /// Determines whether one EntityStatus is less than another.
    /// </summary>
    /// <param name="left">The left EntityStatus to compare.</param>
    /// <param name="right">The right EntityStatus to compare.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <(EntityStatus left, EntityStatus right) => left.CompareTo(right) < 0;
    
    /// <summary>
    /// Determines whether one EntityStatus is less than or equal to another.
    /// </summary>
    /// <param name="left">The left EntityStatus to compare.</param>
    /// <param name="right">The right EntityStatus to compare.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <=(EntityStatus left, EntityStatus right) => left.CompareTo(right) <= 0;
    
    /// <summary>
    /// Determines whether one EntityStatus is greater than another.
    /// </summary>
    /// <param name="left">The left EntityStatus to compare.</param>
    /// <param name="right">The right EntityStatus to compare.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >(EntityStatus left, EntityStatus right) => left.CompareTo(right) > 0;
    
    /// <summary>
    /// Determines whether one EntityStatus is greater than or equal to another.
    /// </summary>
    /// <param name="left">The left EntityStatus to compare.</param>
    /// <param name="right">The right EntityStatus to compare.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >=(EntityStatus left, EntityStatus right) => left.CompareTo(right) >= 0;

    #endregion

    #region Predefined Status Values

    /// <summary>
    /// The entity is currently active and functioning.
    /// </summary>
    public static EntityStatus ACTIVE => _statusCache[nameof(ACTIVE)];

    /// <summary>
    /// The entity is pending processing or approval.
    /// </summary>
    public static EntityStatus PENDING => _statusCache[nameof(PENDING)];

    /// <summary>
    /// The entity is currently being processed.
    /// </summary>
    public static EntityStatus PROCESSING => _statusCache[nameof(PROCESSING)];

    /// <summary>
    /// The entity has been logically deleted.
    /// </summary>
    public static EntityStatus DELETED => _statusCache[nameof(DELETED)];

    /// <summary>
    /// The entity is temporarily suspended.
    /// </summary>
    public static EntityStatus SUSPENDED => _statusCache[nameof(SUSPENDED)];

    /// <summary>
    /// The entity is in an error state requiring attention.
    /// </summary>
    public static EntityStatus ONERROR => _statusCache[nameof(ONERROR)];

    /// <summary>
    /// The entity has been published and is available.
    /// </summary>
    public static EntityStatus PUBLISHED => _statusCache[nameof(PUBLISHED)];

    #endregion

    #region Utility Methods

    /// <summary>
    /// Gets all predefined entity status values.
    /// </summary>
    /// <value>A frozen dictionary containing all predefined status values.</value>
    public static FrozenDictionary<string, EntityStatus> AllStatuses => _statusCache;

    /// <summary>
    /// Determines whether the specified status name is a valid predefined status.
    /// </summary>
    /// <param name="statusName">The status name to validate.</param>
    /// <returns>true if the status name is valid; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValidStatus(string? statusName) =>
        !string.IsNullOrWhiteSpace(statusName) && _validStatusNames.Contains(statusName);

    /// <summary>
    /// Attempts to parse a string into a predefined EntityStatus.
    /// </summary>
    /// <param name="value">The string to parse.</param>
    /// <param name="status">The parsed EntityStatus if successful.</param>
    /// <returns>true if parsing was successful; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(string? value, [MaybeNullWhen(false)] out EntityStatus status)
    {
        if (!string.IsNullOrWhiteSpace(value) && _statusCache.TryGetValue(value, out status))
        {
            return true;
        }

        status = default;
        return false;
    }

    /// <summary>
    /// Determines whether this status represents a terminal state (DELETED, ONERROR).
    /// </summary>
    /// <returns>true if the status is terminal; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsTerminal() => 
        Equals(DELETED) || Equals(ONERROR);

    /// <summary>
    /// Determines whether this status represents an active state (ACTIVE, PUBLISHED).
    /// </summary>
    /// <returns>true if the status is active; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsActive() => 
        Equals(ACTIVE) || Equals(PUBLISHED);

    /// <summary>
    /// Determines whether this status represents a transitional state (PENDING, PROCESSING).
    /// </summary>
    /// <returns>true if the status is transitional; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsTransitional() => 
        Equals(PENDING) || Equals(PROCESSING);

    #endregion
}

/// <summary>
/// High-performance validation attribute for EntityStatus values with optimized lookup and caching.
/// </summary>
/// <remarks>
/// This attribute provides efficient validation of EntityStatus values using frozen collections
/// for optimal performance in .NET 10 applications. Supports both predefined and custom status values.
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class EntityStatusValidationAttribute : ValidationAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityStatusValidationAttribute"/> class.
    /// </summary>
    /// <param name="allowCustomStatuses">Whether to allow custom status values beyond predefined ones.</param>
    public EntityStatusValidationAttribute(bool allowCustomStatuses = false)
    {
        AllowCustomStatuses = allowCustomStatuses;
        ErrorMessage = allowCustomStatuses 
            ? "Status value cannot be null or whitespace."
            : "Status must be one of the predefined values: {0}.";
    }

    /// <summary>
    /// Gets whether custom status values beyond predefined ones are allowed.
    /// </summary>
    public bool AllowCustomStatuses { get; }

    /// <summary>
    /// Gets or sets whether null values are permitted.
    /// </summary>
    public bool AllowNull { get; set; }

    /// <inheritdoc />
    public override bool RequiresValidationContext => false;

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool IsValid(object? value) => value switch
    {
        null => AllowNull,
        string { Length: 0 } => false,
        string status when AllowCustomStatuses => !string.IsNullOrWhiteSpace(status),
        string status => EntityStatus.IsValidStatus(status),
        EntityStatus => true,
        _ => false
    };

    /// <inheritdoc />
    public override string FormatErrorMessage(string name)
    {
        if (AllowCustomStatuses)
        {
            return string.Format(System.Globalization.CultureInfo.CurrentCulture, 
                ErrorMessageString, name);
        }

        var validStatuses = string.Join(", ", EntityStatus.AllStatuses.Keys);
        return string.Format(System.Globalization.CultureInfo.CurrentCulture, 
            ErrorMessageString, validStatuses);
    }
}