/*******************************************************************************
 * Copyright (C) 2025 Kamersoft
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
using System.Runtime.CompilerServices;

namespace System.Entities;

/// <summary>
/// Represents an immutable, high-performance entity status primitive.
/// Provides predefined status values and efficient validation for .NET 10 applications.
/// </summary>
/// <remarks>
/// This implementation leverages modern .NET 10 features including frozen collections for optimal performance,
/// automatic JSON serialization through primitive converters, and comprehensive validation support.
/// Status values are cached for efficient reuse and comparison operations.
/// </remarks>
[PrimitiveJsonConverter<EntityStatus, string>]
#pragma warning disable CA1036 // Override methods on comparable types
public readonly record struct EntityStatus : IPrimitive<EntityStatus, string>
#pragma warning restore CA1036 // Override methods on comparable types
{
    private static readonly FrozenSet<string> _validStatusNames = CreateValidStatusNames();
    private static FrozenSet<string> CreateValidStatusNames()
    {
        return new[] { nameof(ACTIVE), nameof(PENDING), nameof(PROCESSING), nameof(DELETED), nameof(ACCEPTED),
                      nameof(SUSPENDED), nameof(ONERROR), nameof(PUBLISHED), nameof(DUPLICATE) }
            .ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc/>
    public string Value { get; }

    private EntityStatus(string value) => Value = value;

    /// <inheritdoc/>
    public override string ToString() => Value;

    /// <summary>
    /// Converts an <see cref="EntityStatus"/> instance to its underlying string value.
    /// </summary>
    /// <param name="self">The <see cref="EntityStatus"/> instance to convert.</param>
    public static implicit operator string(EntityStatus self) => self.Value;

    /// <inheritdoc/>
    public static string DefaultValue => ACTIVE;
#pragma warning disable CA2225 // Operator overloads have named alternates
    /// <summary>
    /// Converts a string value to an EntityStatus instance using implicit conversion.
    /// </summary>
    /// <remarks>This operator enables seamless assignment of string values to EntityStatus variables. If the
    /// provided string does not correspond to a valid status, the resulting EntityStatus may represent an undefined or
    /// custom status, depending on the implementation of the Create method.</remarks>
    /// <param name="value">The string value to convert to an EntityStatus. Cannot be null.</param>
    public static implicit operator EntityStatus(string value)
#pragma warning restore CA2225 // Operator overloads have named alternates
    {
        ArgumentNullException.ThrowIfNull(value);
        return Create(value);
    }

    /// <inheritdoc/>
    public bool Equals(EntityStatus other) => Value.Equals(other.Value, StringComparison.Ordinal);

    /// <inheritdoc/>
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    /// <inheritdoc/>
    public static EntityStatus Create(string value)
    {
        return _validStatusNames.Contains(value)
            ? new EntityStatus(value)
            : throw new ValidationException(
                new ValidationResult($"'{value}' is not a valid entity status.", [nameof(EntityStatus)]), null, value);
    }

    /// <summary>
    /// The entity is currently active and functioning.
    /// </summary>
    public static readonly EntityStatus ACTIVE = nameof(ACTIVE);

    /// <summary>
    /// The entity has been accepted for processing (not seen or recoverable).
    /// </summary>
    public static readonly EntityStatus ACCEPTED = nameof(ACCEPTED);

    /// <summary>
    /// The entity was alread present and is a duplicate. It must be ignored.
    /// </summary>
    public static readonly EntityStatus DUPLICATE = nameof(DUPLICATE);

    /// <summary>
    /// The entity is pending processing or approval.
    /// </summary>
    public static readonly EntityStatus PENDING = nameof(PENDING);

    /// <summary>
    /// The entity is currently being processed.
    /// </summary>
    public static readonly EntityStatus PROCESSING = nameof(PROCESSING);

    /// <summary>
    /// The entity has been logically deleted.
    /// </summary>
    public static readonly EntityStatus DELETED = nameof(DELETED);

    /// <summary>
    /// The entity is temporarily suspended.
    /// </summary>
    public static readonly EntityStatus SUSPENDED = nameof(SUSPENDED);

    /// <summary>
    /// The entity is in an error state requiring attention.
    /// </summary>
    public static readonly EntityStatus ONERROR = nameof(ONERROR);

    /// <summary>
    /// The entity has been published and is available.
    /// </summary>
    public static readonly EntityStatus PUBLISHED = nameof(PUBLISHED);

    #region Utility Methods

    /// <summary>
    /// Gets all predefined entity status values.
    /// </summary>
    /// <value>A frozen dictionary containing all predefined status values.</value>
    public static FrozenSet<string> AllStatuses => _validStatusNames;

    /// <summary>
    /// Determines whether the specified status name is a valid predefined status.
    /// </summary>
    /// <param name="statusName">The status name to validate.</param>
    /// <returns>true if the status name is valid; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValidStatus(string? statusName) =>
        !string.IsNullOrWhiteSpace(statusName) && _validStatusNames.Contains(statusName);

    /// <summary>
    /// Determines whether this status represents a terminal state (DELETED, ONERROR).
    /// </summary>
    /// <returns>true if the status is terminal; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsTerminal(string statusName)
    {
        ArgumentNullException.ThrowIfNull(statusName);
        return statusName.Equals(DELETED, StringComparison.Ordinal)
            || statusName.Equals(ONERROR, StringComparison.Ordinal);
    }

    /// <summary>
    /// Determines whether this status represents an active state (ACTIVE, PUBLISHED).
    /// </summary>
    /// <returns>true if the status is active; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsActive(string statusName)
    {
        ArgumentNullException.ThrowIfNull(statusName);
        return statusName.Equals(ACTIVE, StringComparison.Ordinal)
            || statusName.Equals(PUBLISHED, StringComparison.Ordinal);
    }

    /// <summary>
    /// Determines whether this status represents a transitional state (PENDING, PROCESSING).
    /// </summary>
    /// <returns>true if the status is transitional; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsTransitional(string statusName)
    {
        ArgumentNullException.ThrowIfNull(statusName);
        return statusName.Equals(PENDING, StringComparison.Ordinal)
            || statusName.Equals(PROCESSING, StringComparison.Ordinal);
    }

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

        var validStatuses = string.Join(", ", EntityStatus.AllStatuses);
        return string.Format(System.Globalization.CultureInfo.CurrentCulture,
            ErrorMessageString, validStatuses);
    }
}