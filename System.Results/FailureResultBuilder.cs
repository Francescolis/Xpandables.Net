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
/// Represents a builder for creating failure results.  
/// </summary>  
public sealed class FailureResultBuilder :
    ResultBuilder<FailureResultBuilder>, IFailureResultBuilder<FailureResultBuilder>
{
    /// <summary>  
    /// Initializes a new instance of the <see cref="FailureResultBuilder"/> class 
    /// with the specified status code.  
    /// </summary>  
    /// <param name="statusCode">The HTTP status code for the failure.</param>  
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the status 
    /// code is between 200 and 299.</exception>  
    public FailureResultBuilder(HttpStatusCode statusCode) :
        base(statusCode) => statusCode.EnsureFailure();

    /// <summary>
    /// Creates a <see cref="FailureResult"/> instance representing a failed operation, using the current property
    /// values.
    /// </summary>
    /// <remarks>There is no need to use this method directly when assigning a <see cref="FailureResultBuilder"/>,
    /// it will be implicitly converted to a <see cref="FailureResult"/>.</remarks>
    public override FailureResult Build() =>
        new()
        {
            StatusCode = StatusCode,
            Title = Title,
            Detail = Detail,
            Errors = Errors,
            Headers = Headers,
            Extensions = Extensions,
            Exception = Exception
        };

    /// <summary>
    /// Converts a <see cref="FailureResultBuilder"/> instance to a <see cref="FailureResult"/> using the builder's
    /// configuration.
    /// </summary>
    /// <remarks>This implicit conversion allows seamless assignment of a <see cref="FailureResultBuilder"/>
    /// to a <see cref="FailureResult"/> variable, automatically invoking the builder's <c>Build</c> method.</remarks>
    /// <param name="builder">The <see cref="FailureResultBuilder"/> to convert. Cannot be <see langword="null"/>.</param>
    [Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2225:Operator overloads have named alternates", Justification = "<Pending>")]
    public static implicit operator FailureResult(FailureResultBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.Build();
    }
}

/// <summary>  
/// Represents a builder for creating failure results with a specific 
/// value type.  
/// </summary>  
/// <typeparam name="TValue">The type of the value.</typeparam>  
public sealed class FailureResultBuilder<TValue> :
   ResultBuilder<FailureResultBuilder<TValue>, TValue>,
   IFailureResultBuilder<FailureResultBuilder<TValue>, TValue>
{
    /// <summary>  
    /// Initializes a new instance of the <see cref="FailureResultBuilder{TResult}"/> class  
    /// with the specified status code.  
    /// </summary>  
    /// <param name="statusCode">The HTTP status code for the failure.</param>  
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the status  
    /// code is between 200 and 299.</exception>  
    public FailureResultBuilder(HttpStatusCode statusCode) :
        base(statusCode) => statusCode.EnsureFailure();

    /// <summary>
    /// Builds a failure result containing the specified status code, title, detail, errors, headers, extensions, and
    /// exception information.
    /// </summary>
    /// <returns>A <see cref="FailureResult{TValue}"/> instance representing the failure, populated with the configured
    /// properties.</returns>
    /// <remarks>There is no need to use this method directly when assigning a <see cref="FailureResultBuilder{TValue}"/>,
    /// it will be implicitly converted to a <see cref="FailureResult{TValue}"/>.</remarks>
    public override FailureResult<TValue> Build() =>
        new()
        {
            StatusCode = StatusCode,
            Title = Title,
            Detail = Detail,
            Errors = Errors,
            Headers = Headers,
            Extensions = Extensions,
            Exception = Exception,
        };

    /// <summary>
    /// Converts a <see cref="FailureResultBuilder{TValue}"/> instance to a <see cref="FailureResult{TValue}"/>
    /// implicitly.
    /// </summary>
    /// <remarks>This operator enables implicit conversion from a builder to its corresponding failure result,
    /// allowing streamlined usage in assignment and method calls. If <paramref name="builder"/> is <see
    /// langword="null"/>, an <see cref="ArgumentNullException"/> is thrown.</remarks>
    /// <param name="builder">The <see cref="FailureResultBuilder{TValue}"/> instance to convert. Cannot be <see langword="null"/>.</param>
    [Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2225:Operator overloads have named alternates", Justification = "<Pending>")]
    public static implicit operator FailureResult<TValue>(FailureResultBuilder<TValue> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.Build();
    }
}
