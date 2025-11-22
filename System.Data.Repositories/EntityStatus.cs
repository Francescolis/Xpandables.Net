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

namespace System.Data.Repositories;

/// <summary>
/// Represents an immutable, high-performance entity status primitive.
/// Provides predefined status values and efficient validation for .NET 10 applications.
/// </summary>
/// <remarks>
/// This implementation leverages modern .NET 10 features including frozen collections for optimal performance,
/// automatic JSON serialization through primitive converters, and comprehensive validation support.
/// Status values are cached for efficient reuse and comparison operations.
/// </remarks>
public static class EntityStatus
{
    private static readonly FrozenSet<string> _validStatusNames = CreateValidStatusNames();
    private static FrozenSet<string> CreateValidStatusNames()
    {
        return new[] { nameof(ACTIVE), nameof(PENDING), nameof(PROCESSING), nameof(DELETED),
                      nameof(SUSPENDED), nameof(ONERROR), nameof(PUBLISHED) }
            .ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// The entity is currently active and functioning.
    /// </summary>
    public static string ACTIVE => nameof(ACTIVE);

    /// <summary>
    /// The entity is pending processing or approval.
    /// </summary>
    public static string PENDING => nameof(PENDING);

    /// <summary>
    /// The entity is currently being processed.
    /// </summary>
    public static string PROCESSING => nameof(PROCESSING);

    /// <summary>
    /// The entity has been logically deleted.
    /// </summary>
    public static string DELETED => nameof(DELETED);

    /// <summary>
    /// The entity is temporarily suspended.
    /// </summary>
    public static string SUSPENDED => nameof(SUSPENDED);

    /// <summary>
    /// The entity is in an error state requiring attention.
    /// </summary>
    public static string ONERROR => nameof(ONERROR);

    /// <summary>
    /// The entity has been published and is available.
    /// </summary>
    public static string PUBLISHED => nameof(PUBLISHED);

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
    public static bool IsValidStatus(this string? statusName) =>
        !string.IsNullOrWhiteSpace(statusName) && _validStatusNames.Contains(statusName);

    /// <summary>
    /// Determines whether this status represents a terminal state (DELETED, ONERROR).
    /// </summary>
    /// <returns>true if the status is terminal; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsTerminal(this string statusName)
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
    public static bool IsActive(this string statusName)
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
    public static bool IsTransitional(this string statusName)
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