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
using Xpandables.Net.Operations;

namespace Xpandables.Net.Executions;

/// <summary>
/// Represents the result of an execution.
/// </summary>
public interface IExecutionResult
{
    /// <summary>
    /// Gets the status code of the execution.
    /// </summary>
    HttpStatusCode StatusCode { get; }

    /// <summary>
    /// Gets the title of the execution result.
    /// </summary>
    [MaybeNull, AllowNull]
    string Title { get; }

    /// <summary>
    /// Gets the detail of the execution result.
    /// </summary>
    [MaybeNull, AllowNull]
    string Detail { get; }

    /// <summary>
    /// Gets the location URI of the execution result.
    /// </summary>
    [MaybeNull, AllowNull]
    Uri Location { get; }

    /// <summary>
    /// Gets the result object of the execution.
    /// </summary>
    /// <remarks>May be null if the execution has no result.</remarks>
    [MaybeNull, AllowNull]
    object Result { get; }

    /// <summary>
    /// Gets the collection of errors associated with the execution.
    /// </summary>
    ElementCollection Errors { get; }

    /// <summary>
    /// Gets the collection of headers associated with the execution.
    /// </summary>
    ElementCollection Headers { get; }

    /// <summary>
    /// Gets the collection of extensions associated with the execution.
    /// </summary>
    ElementCollection Extensions { get; }

    /// <summary>
    /// Gets a value indicating whether the execution result is generic.
    /// </summary>
    public bool IsGeneric => false;

    /// <summary>
    /// Gets a value indicating whether the execution's status code 
    /// is a success status code.
    /// </summary>
    public bool IsSuccessStatusCode =>
        (int)StatusCode is >= 200 and <= 299;

    /// <summary>
    /// Ensures that the execution result has a success status code.
    /// </summary>
    /// <exception cref="ExecutionResultException">Thrown if the status code 
    /// is not a success status code.</exception>
    public void EnsureSuccessStatusCode()
    {
        if (!IsSuccessStatusCode)
        {
            throw new ExecutionResultException(this);
        }
    }

    /// <summary>
    /// Gets the exception associated with the execution result, if any.
    /// </summary>
    /// <returns>The exception entry if available; otherwise, null.</returns>
    public ElementEntry? GetException() =>
        Errors[ExecutionResultExtensions.ExceptionKey];
}

/// <summary>
/// Represents the result of an execution with a specific result type.
/// </summary>
/// <typeparam name="TResult">The type of the result object.</typeparam>
public interface IExecutionResult<TResult> : IExecutionResult
{
    /// <summary>
    /// Gets the result object of the execution.
    /// </summary>
    /// <remarks>May be null if the execution has no result.</remarks>
    [MaybeNull, AllowNull]
    new TResult Result { get; }

    [JsonIgnore]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [MaybeNull, AllowNull]
    object IExecutionResult.Result => Result;

    /// <summary>
    /// Gets a value indicating whether the execution's status code 
    /// is a success status code.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Result))]
    public new bool IsSuccessStatusCode =>
        (int)StatusCode is >= 200 and <= 299;

    [EditorBrowsable(EditorBrowsableState.Never)]
    bool IExecutionResult.IsSuccessStatusCode => IsSuccessStatusCode;

    /// <summary>
    /// Ensures that the execution result has a success status code.
    /// </summary>
    /// <exception cref="ExecutionResultException">Thrown if the status code 
    /// is not a success status code.</exception>
    [MemberNotNull([nameof(Result)])]
    public new void EnsureSuccessStatusCode()
    {
        if (!IsSuccessStatusCode)
        {
            throw new ExecutionResultException(this);
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    void IExecutionResult.EnsureSuccessStatusCode() => EnsureSuccessStatusCode();

    /// <summary>  
    /// Gets a value indicating whether the execution is generic.  
    /// </summary>  
    public new bool IsGeneric => true;

    [JsonIgnore]
    [EditorBrowsable(EditorBrowsableState.Never)]
    bool IExecutionResult.IsGeneric => IsGeneric;
}
