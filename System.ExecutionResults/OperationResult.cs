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
using System.Text.Json.Serialization;

namespace System.ExecutionResults;

/// <summary>
/// Represents the result of an operation, including status information, error details, and additional metadata.
/// </summary>
/// <remarks>This record serves as a base type for operation results that encapsulate HTTP status codes,
/// error collections, and optional metadata such as titles, details, and locations.The class is designed to facilitate consistent handling of operation
/// outcomes, including error reporting and extension data.</remarks>
[DebuggerDisplay($"{{{nameof(DebuggerDisplay)}(),nq}}")]
public sealed record OperationResult : OperationResultBase
{
    /// <summary>
    /// Specifies the result status of an operation.
    /// </summary>
    /// <remarks>Use this enumeration to indicate whether an operation completed successfully or failed. The
    /// values can be used for control flow or error handling in application logic.</remarks>
    public enum Status
    {
        /// <summary>
        /// Indicates that the operation completed successfully.
        /// </summary>
        Success,

        /// <summary>
        /// Indicates that an operation did not complete successfully.
        /// </summary>
        Failure
    }

    /// <summary>
    /// Initializes a new instance of the OperationResult class for deserialization purposes.
    /// </summary>
    /// <remarks>This constructor is intended for use by JSON serialization frameworks to create an
    /// OperationResult instance during deserialization. It should not be called directly in application code.</remarks>
    [JsonConstructor]
    internal OperationResult() { }

    /// <summary>
    /// Indicates whether the operation result is generic. The value is always false for this non-generic type.
    /// </summary>
    public sealed override bool IsGeneric => false;

    /// <summary>
    /// Indicates whether the HTTP status code of the operation result signifies a successful outcome.
    /// </summary>
    public sealed override bool IsSuccess => StatusCode.IsSuccess;

    ///// <summary>
    ///// Converts an operation instance to its corresponding HttpStatusCode value.
    ///// </summary>
    ///// <remarks>This operator enables implicit conversion from operation to HttpStatusCode.</remarks>
    ///// <param name="operation">The operation instance to convert. Cannot be null.</param>
    //public static implicit operator HttpStatusCode(OperationResult operation)
    //{
    //    ArgumentNullException.ThrowIfNull(operation);
    //    return operation.ToHttpStatusCode();
    //}

    ///// <summary>
    ///// Enables implicit conversion of an untyped operation result to a typed operation result with an object payload.
    ///// </summary>
    ///// <remarks>This operator allows seamless conversion from an untyped operation result to a generic
    ///// operation result, preserving the original status and data. Use this conversion when you need to work with a
    ///// generic payload type but only have an untyped result.</remarks>
    ///// <param name="operation">The operation result instance to convert. Cannot be null.</param>
    //public static implicit operator OperationResult<object>(OperationResult operation)
    //{
    //    ArgumentNullException.ThrowIfNull(operation);
    //    return operation.ToOperationResult();
    //}

    /// <summary>
    /// Converts the current instance to an OperationResult of the specified result type.
    /// </summary>
    /// <typeparam name="TResult">The type of the result value to be returned in the converted OperationResult.</typeparam>
    /// <returns>An <see cref="OperationResult{TResult}"/> containing the same metadata and value as the current instance, with the value cast
    /// to the specified type or set to the default value of TResult if the cast is not possible.</returns>
    public OperationResult<TResult> ToOperationResult<TResult>() =>
        new()
        {
            Detail = Detail,
            Errors = Errors,
            Extensions = Extensions,
            Headers = Headers,
            Location = Location,
            Value = Value is TResult result ? result : default,
            StatusCode = StatusCode,
            Title = Title
        };

    ///// <summary>
    ///// Creates a new <see langword="ExecutionResult{object}"/> instance that represents the current operation result.
    ///// </summary>
    ///// <returns>An <see langword="ExecutionResult{object}"/> containing the detail, errors, extensions, headers, location, value,
    ///// status code, and title from the current instance.</returns>
    //public OperationResult<object> ToExecutionResult() =>
    //    new()
    //    {
    //        Detail = Detail,
    //        Errors = Errors,
    //        Extensions = Extensions,
    //        Headers = Headers,
    //        Location = Location,
    //        Value = Value,
    //        StatusCode = StatusCode,
    //        Title = Title
    //    };
}