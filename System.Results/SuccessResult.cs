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
using System.Diagnostics.CodeAnalysis;

namespace System.Results;

/// <summary>
/// Represents a result that indicates a successful operation without an associated value.
/// </summary>
/// <remarks>Use this type to signal that an operation completed successfully when no additional data needs to be
/// returned. This is typically used in scenarios where only the success or failure state is relevant, and no result
/// value is required.</remarks>
public sealed record SuccessResult : Result
{
    /// <inheritdoc/>
    public sealed override bool IsGeneric => false;

    /// <inheritdoc/>
    public sealed override bool IsFailure => false;

    /// <inheritdoc/>
    public sealed override bool IsSuccess => true;

    /// <summary>
    /// Converts a non-generic SuccessResult instance to a generic success instance, preserving all result
    /// data.
    /// </summary>
    /// <remarks>This implicit conversion allows seamless use of SuccessResult in contexts where
    /// success is expected. All headers, status code, extensions, and value are copied to the new
    /// instance.</remarks>
    /// <param name="success">The SuccessResult instance to convert. Cannot be null.</param>
    [SuppressMessage("Usage", "CA2225:Operator overloads have named alternates", Justification = "<Pending>")]
    public static implicit operator SuccessResult<object>(SuccessResult success)
    {
        ArgumentNullException.ThrowIfNull(success);
        return new()
        {
            Headers = success.Headers,
            StatusCode = success.StatusCode,
            Extensions = success.Extensions,
            Value = success.Value
        };
    }
}

/// <summary>
/// Represents a successful result that contains a value of the specified type.
/// </summary>
/// <remarks>Use this type to indicate the successful completion of an operation and to provide the resulting
/// value. The <see cref="Value"/> property holds the value associated with the success. This type is typically returned
/// from methods that follow a result pattern, distinguishing between success and failure cases.</remarks>
/// <typeparam name="TValue">The type of the value contained in the result.</typeparam>
public sealed record SuccessResult<TValue> : Result<TValue>
{
    /// <inheritdoc/>
    public sealed override bool IsFailure => false;

    /// <inheritdoc/>
    public sealed override bool IsSuccess => true;

    /// <summary>
    /// Represents a URI reference that identifies a resource relevant to the result.
    /// </summary>
    public new Uri? Location { get => base.Location; init => base.Location = value; }

    /// <summary>
    /// Gets or sets the value of the result.
    /// </summary>
    [MaybeNull, AllowNull]
    public required new TValue Value
    {
        get => base.Value is TValue result ? result : default;
        init => base.Value = value;
    }

    /// <summary>
    /// Converts a generic <see cref="SuccessResult{TValue}"/> instance to a non-generic <see cref="SuccessResult"/>
    /// instance, preserving response metadata and value.
    /// </summary>
    /// <remarks>This operator allows seamless conversion when only the response metadata and value are needed
    /// without the generic type parameter. The <see cref="Value"/> property is preserved as an object.</remarks>
    /// <param name="success">The generic success result to convert. Cannot be null.</param>
    [SuppressMessage("Usage", "CA2225:Operator overloads have named alternates", Justification = "<Pending>")]
    public static implicit operator SuccessResult(SuccessResult<TValue> success)
    {
        ArgumentNullException.ThrowIfNull(success);
        return new()
        {
            Headers = success.Headers,
            StatusCode = success.StatusCode,
            Location = success.Location,
            Value = success.Value
        };
    }
}