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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace System.Results;

/// <summary>
/// Provides a base type for representing the result of an operation, including status information, error details, and
/// associated metadata.
/// </summary>
/// <remarks>This record serves as the foundation for operation result types, encapsulating common
/// properties such as HTTP status code, error collections, headers, and operation details. It is designed for
/// extensibility and serialization, allowing derived types to represent both successful and failed outcomes. Use the
/// provided properties to inspect the result, access error information, and handle operation metadata. The type
/// supports integration with HTTP-based APIs and can be used to standardize result handling across application
/// layers.</remarks>
[DebuggerDisplay($"{{{nameof(DebuggerDisplay)}(),nq}}")]
public record Result : _Result
{
    /// <summary>
    /// Initializes a new instance of the Result class for use by derived types and JSON deserialization.
    /// </summary>
    /// <remarks>This constructor is intended for use by subclasses and JSON serialization frameworks. It
    /// cannot be called directly from external assemblies.</remarks>
    [JsonConstructor]
    protected internal Result() { }

    /// <summary>
    /// Indicates whether the operation result is generic. The value is false for non-generic result types.
    /// </summary>
    public override bool IsGeneric => false;
}

/// <summary>
/// Provides a base type for representing the result of an operation of <typeparamref name="TValue"/>, including status information, error details, and
/// associated metadata.
/// </summary>
/// <remarks>This record serves as the foundation for operation result types, encapsulating common
/// properties such as HTTP status code, error collections, headers, and operation details. It is designed for
/// extensibility and serialization, allowing derived types to represent both successful and failed outcomes. Use the
/// provided properties to inspect the result, access error information, and handle operation metadata. The type
/// supports integration with HTTP-based APIs and can be used to standardize result handling across application
/// layers.</remarks>
/// <typeparam name="TValue">The type of the value contained in the result.</typeparam>
public record Result<TValue> : _Result
{
    /// <summary>
    /// Initializes a new instance of the Result class for use by derived types and JSON deserialization.
    /// </summary>
    /// <remarks>This constructor is intended for use by subclasses and JSON serialization frameworks. It
    /// should not be called directly in application code.</remarks>
    [JsonConstructor]
    protected internal Result() { }

    /// <summary>
    /// Indicates whether the operation result is generic. The value is true for generic result types.
    /// </summary>
    public sealed override bool IsGeneric => true;

    /// <summary>
    /// Gets or sets the value of the result.
    /// </summary>
    [MaybeNull, AllowNull]
    public new TValue Value
    {
        get => base.Value is TValue result ? result : default;
        protected internal init => base.Value = value;
    }

    /// <summary>
    /// Converts a generic <see cref="Result{TValue}"/> instance to a non-generic <see cref="Result"/> instance,
    /// preserving all relevant result information.
    /// </summary>
    /// <remarks>This operator enables seamless conversion from a generic result to a non-generic result,
    /// allowing code that expects a non-generic <see cref="Result"/> to accept a <see cref="Result{TValue}"/> without
    /// explicit casting. All status, error, and metadata fields are copied; the value is assigned to the non-generic
    /// result's value property.</remarks>
    /// <param name="result">The generic result to convert. Cannot be null.</param>
    [SuppressMessage("Usage", "CA2225:Operator overloads have named alternates", Justification = "<Pending>")]
    public static implicit operator Result(Result<TValue> result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return new()
        {
            StatusCode = result.StatusCode,
            Title = result.Title,
            Detail = result.Detail,
            Location = result.Location,
            Errors = result.Errors,
            Headers = result.Headers,
            Extensions = result.Extensions,
            Exception = result.Exception,
            Value = result.Value
        };
    }

    /// <summary>
    /// Defines an implicit conversion from a non-generic Result to a generic <see cref="Result{TValue}"/> instance.
    /// </summary>
    /// <remarks>All properties from the source Result are copied to the new <see cref="Result{TValue}"/> instance. The
    /// Value property is set to the source Value if it is of type TValue; otherwise, it is set to the default value of
    /// TValue.</remarks>
    /// <param name="result">The non-generic Result instance to convert. Cannot be null.</param>
    [SuppressMessage("Usage", "CA2225:Operator overloads have named alternates", Justification = "<Pending>")]
    public static implicit operator Result<TValue>(Result result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return new()
        {
            StatusCode = result.StatusCode,
            Title = result.Title,
            Detail = result.Detail,
            Location = result.Location,
            Errors = result.Errors,
            Headers = result.Headers,
            Extensions = result.Extensions,
            Exception = result.Exception,
            Value = result.Value is TValue value ? value : default
        };
    }

    /// <summary>
    /// Converts a generic <see cref="Result{TValue}"/> instance to a non-generic <see langword="Result{object}"/> instance,
    /// preserving all status and metadata information.
    /// </summary>
    /// <remarks>This operator allows code to treat a generic result as a non-generic result, which can be
    /// useful when the value type is not known at compile time. All properties, including status, errors, and
    /// extensions, are copied to the new instance.</remarks>
    /// <param name="result">The generic result to convert. Cannot be null.</param>
    [SuppressMessage("Usage", "CA2225:Operator overloads have named alternates", Justification = "<Pending>")]
    public static implicit operator Result<object>(Result<TValue> result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return new()
        {
            StatusCode = result.StatusCode,
            Title = result.Title,
            Detail = result.Detail,
            Location = result.Location,
            Errors = result.Errors,
            Headers = result.Headers,
            Extensions = result.Extensions,
            Exception = result.Exception,
            Value = result.Value
        };
    }
}