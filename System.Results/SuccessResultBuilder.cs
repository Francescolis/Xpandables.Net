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
using System.Net;

namespace System.Results;

/// <summary>
/// Represents a builder for creating successful results.
/// </summary>
public sealed class SuccessResultBuilder :
    ResultBuilder<SuccessResultBuilder>, ISuccessResultBuilder<SuccessResultBuilder>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SuccessResultBuilder"/> class 
    /// with the specified status code.
    /// </summary>
    /// <param name="statusCode">The HTTP status code indicating a 
    /// successful result.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the status 
    /// code is not between 200 and 299.</exception>
    public SuccessResultBuilder(HttpStatusCode statusCode) :
        base(statusCode) => statusCode.EnsureSuccess();

    /// <summary>
    /// Builds and returns a result object representing a successful result, using the current status code, location,
    /// headers, and exception information.
    /// </summary>
    /// <returns>A <see cref="SuccessResult"/> instance containing the current status code, location, headers, and exception
    /// details.</returns>
    /// <remarks>There is no need to use this method directly when assigning a <see cref="SuccessResultBuilder"/>,
    /// it will be implicitly converted to a <see cref="SuccessResult"/>.</remarks>
    public override SuccessResult Build() =>
        new()
        {
            StatusCode = StatusCode,
            Location = Location,
            Headers = Headers,
            Exception = Exception,
            InternalValue = Value
        };

    /// <summary>
    /// Converts a <see cref="SuccessResultBuilder"/> instance to a <see cref="SuccessResult"/> using the builder's
    /// configuration.
    /// </summary>
    /// <remarks>This implicit conversion allows seamless assignment of a <see cref="SuccessResultBuilder"/>
    /// to a <see cref="SuccessResult"/> variable, automatically invoking the builder's <c>Build</c> method.</remarks>
    /// <param name="builder">The <see cref="SuccessResultBuilder"/> to convert. Cannot be <see langword="null"/>.</param>
    [Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2225:Operator overloads have named alternates", Justification = "<Pending>")]
    public static implicit operator SuccessResult(SuccessResultBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.Build();
    }
}

/// <summary>
/// Represents a builder for creating successful results with a 
/// specified value type.
/// </summary>
/// <typeparam name="TValue">The type of the value.</typeparam>
public sealed class SuccessResultBuilder<TValue> :
    ResultBuilder<SuccessResultBuilder<TValue>, TValue>,
    ISuccessResultBuilder<SuccessResultBuilder<TValue>, TValue>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SuccessResultBuilder{TResult}"/> class 
    /// with the specified status code.
    /// </summary>
    /// <param name="statusCode">The HTTP status code indicating a 
    /// successful result.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the status 
    /// code is not between 200 and 299.</exception>
    public SuccessResultBuilder(HttpStatusCode statusCode) :
        base(statusCode) => statusCode.EnsureSuccess();

    /// <summary>
    /// Builds and returns a successful result containing the specified value and associated response metadata.
    /// </summary>
    /// <returns>A <see cref="SuccessResult{TResult}"/> instance that encapsulates the result value, status code, headers,
    /// location, and any exception information.</returns>
    /// <remarks>There is no need to use this method directly when assigning a <see cref="SuccessResultBuilder"/>,
    /// it will be implicitly converted to a <see cref="SuccessResult"/>.</remarks>
    public override SuccessResult<TValue> Build() =>
        new()
        {
            StatusCode = StatusCode,
            Location = Location,
            Headers = Headers,
            Exception = Exception,
            Value = Value
        };

    /// <summary>
    /// Converts a <see cref="SuccessResultBuilder{TValue}"/> instance to a <see cref="SuccessResult{TValue}"/>
    /// implicitly.
    /// </summary>
    /// <remarks>This operator enables implicit conversion from a builder to a result, allowing seamless
    /// assignment or usage in expressions where a <see cref="SuccessResult{TValue}"/> is expected.</remarks>
    /// <param name="builder">The builder used to construct the <see cref="SuccessResult{TValue}"/> instance. Cannot be <see
    /// langword="null"/>.</param>
    [Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2225:Operator overloads have named alternates", Justification = "<Pending>")]
    public static implicit operator SuccessResult<TValue>(SuccessResultBuilder<TValue> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.Build();
    }
}