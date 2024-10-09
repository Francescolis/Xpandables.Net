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
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json.Serialization;

using Xpandables.Net.Collections;

namespace Xpandables.Net.Operations;

/// <summary>
/// Represents the result of an operation.
/// </summary>
[JsonConverter(typeof(OperationResultJsonConverterFactory))]
public interface IOperationResult
{
    /// <summary>
    /// Gets the status code of the operation.
    /// </summary>
    HttpStatusCode StatusCode { get; }

    /// <summary>
    /// Gets the title of the operation result.
    /// </summary>
    [MaybeNull, AllowNull]
    string Title { get; }

    /// <summary>
    /// Gets the detail of the operation result.
    /// </summary>
    [MaybeNull, AllowNull]
    string Detail { get; }

    /// <summary>
    /// Gets the location URI of the operation result.
    /// </summary>
    [MaybeNull, AllowNull]
    Uri Location { get; }

    /// <summary>
    /// Gets the result object of the operation.
    /// </summary>
    [MaybeNull, AllowNull]
    object Result { get; }

    /// <summary>
    /// Gets the collection of errors associated with the operation.
    /// </summary>
    ElementCollection Errors { get; }

    /// <summary>
    /// Gets the collection of headers associated with the operation.
    /// </summary>
    ElementCollection Headers { get; }

    /// <summary>
    /// Gets the collection of extensions associated with the operation.
    /// </summary>
    ElementCollection Extensions { get; }

    /// <summary>
    /// Gets a value indicating whether the operation is generic.
    /// </summary>
    public bool IsGeneric => false;

    /// <summary>
    /// Gets a value indicating whether the operation's status code 
    /// is a success status code.
    /// </summary>
    public bool IsSuccessStatusCode =>
        (int)StatusCode is >= 200 and <= 299;

    /// <summary>
    /// Gets the exception associated with the operation result, if any.
    /// </summary>
    /// <returns>The exception entry if available; otherwise, null.</returns>
    public ElementEntry? GetException() =>
        Errors[OperationResultExtensions.ExceptionKey];
}

/// <summary>
/// Represents the result of an operation with a specific result type.
/// </summary>
/// <typeparam name="TResult">The type of the result object.</typeparam>
[JsonConverter(typeof(OperationResultJsonConverterFactory))]
public interface IOperationResult<TResult> : IOperationResult
{
    /// <summary>
    /// Gets the result object of the operation.
    /// </summary>    
    [MaybeNull, AllowNull]
    new TResult Result { get; }

    [JsonIgnore]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [MaybeNull, AllowNull]
    object IOperationResult.Result => Result;

    /// <summary>
    /// Gets a value indicating whether the operation's status code 
    /// is a success status code.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Result))]
    public new bool IsSuccessStatusCode =>
        (int)StatusCode is >= 200 and <= 299;

    [EditorBrowsable(EditorBrowsableState.Never)]
    bool IOperationResult.IsSuccessStatusCode => IsSuccessStatusCode;

    /// <summary>  
    /// Gets a value indicating whether the operation is generic.  
    /// </summary>  
    public new bool IsGeneric => true;

    [JsonIgnore]
    [EditorBrowsable(EditorBrowsableState.Never)]
    bool IOperationResult.IsGeneric => IsGeneric;
}
