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
using System.Net;
using System.Text.Json.Serialization;

namespace System.OperationResults;

/// <summary>
/// Represents the result of an operation, including its status code, outcome value, error information, and related
/// metadata. This generic type encapsulates both successful and failed operation results, providing access to the
/// operation's output and associated details.
/// </summary>
/// <remarks>Use <see cref="OperationResult{TResult}"/> to capture the outcome of an operation along with its HTTP
/// status code, error details, and additional metadata. The <see cref="Value"/> property holds the result of the
/// operation and may require type conversion after deserialization if the source is a complex type. Implicit
/// conversions are provided for interoperability with non-generic <see cref="OperationResult"/> and <see
/// cref="HttpStatusCode"/>. Thread safety is not guaranteed; if multiple threads access the same instance concurrently,
/// external synchronization is required.</remarks>
/// <typeparam name="TResult">The type of the value returned by the operation. This can be any type representing the result of the operation.</typeparam>
[DebuggerDisplay($"{{{nameof(DebuggerDisplay)}(),nq}}")]
public sealed record OperationResult<TResult> : OperationResultBase
{
    /// <summary>
    /// Initializes a new instance of the OperationResult class for deserialization purposes.
    /// </summary>
    /// <remarks>This constructor is intended for use by JSON serialization frameworks and should not be
    /// called directly in application code.</remarks>
    [JsonConstructor]
    internal OperationResult() { }

    /// <summary>
    /// Gets or sets the value of the result.
    /// </summary>
    [MaybeNull, AllowNull]
    public new TResult Value { get => base.Value is TResult result ? result : default; init => base.Value = value; }

    /// <summary>
    /// Indicates whether the operation result is generic. The value is always true for this generic type.
    /// </summary>
    public sealed override bool IsGeneric => true;

    /// <summary>
    /// Indicates whether the HTTP status code of the operation result signifies a successful outcome.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Value))]
    public sealed override bool IsSuccess => StatusCode.IsSuccess;

    ///// <summary>
    ///// Converts an ExecutionResult instance to its corresponding HttpStatusCode value.
    ///// </summary>
    ///// <remarks>This operator enables implicit conversion from _ExecutionResult to HttpStatusCode, allowing
    ///// ExecutionResult objects to be used where an HttpStatusCode is expected.</remarks>
    ///// <param name="operation">The ExecutionResult instance to convert. Cannot be null.</param>
    //public static implicit operator HttpStatusCode(OperationResult<TResult> operation) =>
    //    operation.ToHttpStatusCode();

    /// <summary>
    /// Enables implicit conversion from an <see cref="OperationResult{TResult}"/> instance to an OperationResult.
    /// </summary>
    /// <remarks>This operator allows <see cref="OperationResult{TResult}"/> objects to be used where an OperationResult is
    /// expected, facilitating interoperability between generic and non-generic result types.</remarks>
    /// <param name="operation">The <see cref="OperationResult{TResult}"/> instance to convert. Cannot be null.</param>
    public static implicit operator OperationResult(OperationResult<TResult> operation)
    {
        ArgumentNullException.ThrowIfNull(operation);
        return operation.ToOperationResult();
    }

    /// <summary>
    /// Converts an untyped <see cref="OperationResult"/> to a typed <see cref="OperationResult{TResult}"/> instance,
    /// copying status and metadata while attempting to cast the value to <typeparamref name="TResult"/>.
    /// </summary>
    /// <remarks>If the <c>Value</c> property of <paramref name="operation"/> is not of type <typeparamref
    /// name="TResult"/>, the resulting <see cref="OperationResult{TResult}"/> will have its <c>Value</c> property set
    /// to the default value of <typeparamref name="TResult"/>. All other properties are copied directly.</remarks>
    /// <param name="operation">The source <see cref="OperationResult"/> to convert. Cannot be <see langword="null"/>.</param>
    public static implicit operator OperationResult<TResult>(OperationResult operation)
    {
        ArgumentNullException.ThrowIfNull(operation);
        return new()
        {
            StatusCode = operation.StatusCode,
            Title = operation.Title,
            Detail = operation.Detail,
            Location = operation.Location,
            Value = operation.Value is TResult resultValue ? resultValue : default,
            Errors = operation.Errors,
            Headers = operation.Headers,
            Extensions = operation.Extensions
        };
    }

    /// <summary>
    /// Creates a new <see cref="OperationResult"/> instance that represents the current state of this object.
    /// </summary>
    /// <returns>An <see cref="OperationResult"/> containing the status code, title, detail, location, value, errors, headers,
    /// and extensions from this object.</returns>
    public OperationResult ToOperationResult() => new()
    {
        StatusCode = StatusCode,
        Title = Title,
        Detail = Detail,
        Location = Location,
        Value = Value,
        Errors = Errors,
        Headers = Headers,
        Extensions = Extensions
    };
}