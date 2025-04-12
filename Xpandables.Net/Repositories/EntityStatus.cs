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
using System.ComponentModel.DataAnnotations;

using Xpandables.Net.Text;

namespace Xpandables.Net.Repositories;

/// <summary>
/// Represents a read-only status string value for an entity. 
/// It provides methods for creation, comparison, and implicit conversions.
/// </summary>
public readonly partial record struct EntityStatus : IPrimitive<EntityStatus, string>, IComparable, IComparable<EntityStatus>
{
    private EntityStatus(string value) => Value = value;

    /// <summary>
    /// Represents a status string value. It provides read-only access to the underlying string.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates a new instance of EntityStatus using the provided string value.
    /// </summary>
    /// <param name="value">The input string used to initialize the new instance.</param>
    /// <returns>Returns a new EntityStatus object.</returns>
    public static EntityStatus Create(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new(value);
    }

    /// <summary>
    /// Returns the default status of an entity, which is set to ACTIVE.
    /// </summary>
    /// <returns>The method returns the EntityStatus value ACTIVE.</returns>
    public static EntityStatus Default() => ACTIVE;

    /// <summary>
    /// Converts an EntityStatus instance to its string representation using the Value property.
    /// </summary>
    /// <param name="self">An instance of EntityStatus that holds the value to be converted.</param>
    public static implicit operator string(EntityStatus self) => self.Value;

    /// <summary>
    /// Converts a string representation into an EntityStatus object.
    /// </summary>
    /// <param name="value">The string representation used to create the EntityStatus instance.</param>
#pragma warning disable CA2225 // Operator overloads have named alternates
    public static implicit operator EntityStatus(string value) => Create(value);
#pragma warning restore CA2225 // Operator overloads have named alternates

    /// <inheritdoc/>
    public int CompareTo(object? obj) => obj switch
    {
        null => 1,
        EntityStatus other => string.Compare(Value, other.Value, StringComparison.Ordinal),
        _ => throw new ArgumentException($"Object is not a {nameof(EntityStatus)}")
    };

    /// <inheritdoc/>
    public int CompareTo(EntityStatus other) =>
        string.Compare(Value, other.Value, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public static bool operator <(EntityStatus left, EntityStatus right) =>
         left.CompareTo(right) < 0;

    /// <inheritdoc/>
    public static bool operator <=(EntityStatus left, EntityStatus right) =>
        left.CompareTo(right) <= 0;

    /// <inheritdoc/>
    public static bool operator >(EntityStatus left, EntityStatus right) =>
        left.CompareTo(right) > 0;

    /// <inheritdoc/>
    public static bool operator >=(EntityStatus left, EntityStatus right) =>
        left.CompareTo(right) >= 0;
}

/// <summary>
/// Represents various statuses of an entity, including ACTIVE, PENDING, DELETED, SUSPENDED, ONERROR, and PUBLISHED.
/// Each status indicates a specific state of the entity.
/// </summary>
public readonly partial record struct EntityStatus
{
    /// <summary>
    /// It is currently functioning (is available).
    /// </summary>
    public static EntityStatus ACTIVE => Create(nameof(ACTIVE));

    /// <summary>
    /// It is pending (for any reason).
    /// </summary>
    public static EntityStatus PENDING => Create(nameof(PENDING));

    /// <summary>
    /// It is deleted (logical deletion).
    /// </summary>
    public static EntityStatus DELETED => Create(nameof(DELETED));

    /// <summary>
    /// It is suspended (for any reason).
    /// </summary>
    public static EntityStatus SUSPENDED => Create(nameof(SUSPENDED));

    /// <summary>
    /// It is in an error state.
    /// </summary>
    public static EntityStatus ONERROR => Create(nameof(ONERROR));

    /// <summary>
    /// It is published.
    /// </summary>
    public static EntityStatus PUBLISHED => Create(nameof(PUBLISHED));

    /// <summary>
    /// A dictionary that maps string representations of entity statuses to their corresponding EntityStatus values. 
    /// It includes statuses like ACTIVE, PENDING, and DELETED.
    /// </summary>
    public static readonly Dictionary<string, EntityStatus> All =
        new()
        {
            { nameof(ACTIVE), ACTIVE },
            { nameof(PENDING), PENDING },
            { nameof(DELETED), DELETED },
            { nameof(SUSPENDED), SUSPENDED },
            { nameof(ONERROR), ONERROR },
            { nameof(PUBLISHED), PUBLISHED }
        };
}

/// <summary>
/// Validates entity statuses with an option to allow null values.
/// </summary>
[AttributeUsage(AttributeTargets.Property
    | AttributeTargets.Field
    | AttributeTargets.Parameter,
    AllowMultiple = false,
    Inherited = true)]
public sealed class EntityStatusFormatAttribute : ValidationAttribute
{
    /// <summary>
    /// Indicates whether null values are permitted. 
    /// It is a boolean property that can be set to true or false.
    /// </summary>
    public bool AllowNull { get; set; }

    /// <summary>
    /// Indicates that a validation context is required for the operation. 
    /// This property always returns true.
    /// </summary>
    public override bool RequiresValidationContext => true;

    /// <summary>
    /// Validates the provided value against a set of allowed entity statuses.
    /// </summary>
    /// <param name="value">The input to be validated, which is expected to be a string representing an entity status.</param>
    /// <returns>A boolean indicating whether the input value is valid based on predefined criteria.</returns>
    public override bool IsValid(object? value)
    {
        string? status = value as string;

        return status switch
        {
            null when AllowNull => true,
            null => false,
            { } instance when !EntityStatus.All
                .Any(s => s.Key.Equals(instance, StringComparison.OrdinalIgnoreCase)) => false,
            _ => true
        };
    }
}
