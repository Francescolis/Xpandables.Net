
/*******************************************************************************
 * Copyright (C) 2023 Francis-Black EWANE
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
using System.Net;
using System.Text.Json.Serialization;

using Xpandables.Net.Primitives;

namespace Xpandables.Net.Operations;

/// <summary>
/// Represents a result of an execution process.
/// </summary>
/// <remarks>You can use <see cref="OperationResultAspJsonConverterFactory"/> 
/// for Asp.Net to convert only <see cref="IOperationResult.Result"/> to Json
/// <para>or use <see cref="OperationResultJsonConverterFactory"/> 
/// to convert the instance to Json.</para></remarks>
public partial interface IOperationResult
{
    /// <summary>
    /// Gets the operation HTTP status code from the execution operation.
    /// </summary>
    [JsonInclude]
    HttpStatusCode StatusCode { get; }

    /// <summary>
    /// Gets the operation summary problem from the execution operation.
    /// <para>Mostly used when building the Asp.Net response in case 
    /// of errors.</para>
    /// </summary>
    /// <remarks>If not defined, the default title will be used.</remarks>
    [JsonInclude]
    [AllowNull, MaybeNull]
    string Title { get; }

    /// <summary>
    /// Gets the operation explanation specific to the execution operation.
    /// <para>Mostly used when building the Asp.Net response in 
    /// case of errors.</para>
    /// </summary>
    /// <remarks>If not defined, the default detail will be used.</remarks>
    [JsonInclude]
    [AllowNull, MaybeNull]
    string Detail { get; }

    /// <summary>
    /// Gets a user-defined object that qualifies or contains 
    /// information about an operation return if available.
    /// </summary>
    /// <returns>The result value of this <see cref="IOperationResult"/>, 
    /// which is of the type <see langword="object"/>.</returns>
    [JsonInclude]
    [AllowNull, MaybeNull]
    object Result { get; }

    /// <summary>
    /// Gets the URL for location header. 
    /// <para>Mostly used with <see cref="HttpStatusCode.Created"/> in 
    /// Asp.Net.</para>
    /// </summary>
    [JsonInclude]
    [AllowNull, MaybeNull]
    Uri LocationUrl { get; }

    /// <summary>
    /// Gets the collection of header values that will be returned 
    /// with the response.
    /// </summary>
    /// <remarks>The default value contains an empty collection.</remarks>
    [JsonInclude]
    ElementCollection Headers { get; }

    /// <summary>
    /// Gets the collection of errors from the last execution operation.
    /// </summary>
    /// <remarks>The default value contains an empty collection.</remarks>
    [JsonInclude]
    ElementCollection Errors { get; }

    /// <summary>
    /// Gets the collection of extensions that will be returned 
    /// with an error response.
    /// </summary>
    /// <remarks>The default value contains an empty collection.</remarks>
    [JsonInclude]
    ElementCollection Extensions { get; }

    /// <summary>
    /// Determines whether or not the current instance is generic.
    /// </summary>
    /// <remarks>Returns <see langword="true"/> if so, 
    /// otherwise <see langword="false"/>.</remarks>
    [JsonIgnore]
    public bool IsGeneric => false;

    /// <summary>
    /// Determines whether or not the current instance is successful 
    /// according to the status of the operation.
    /// </summary>
    [JsonIgnore]
    public bool IsSuccess => StatusCode.IsSuccessStatusCode();

    /// <summary>
    /// Determines whether or not the current instance is 
    /// failed according to the status of the operation.
    /// </summary>
    public bool IsFailure => StatusCode.IsFailureStatusCode();

    /// <summary>
    /// Determines whether or not the errors collection contains 
    /// an exception key. If so, returns the error.
    /// </summary>
    /// <param name="elementEntry">the output error if found.</param>
    /// <returns><see langword="true"/> if collection contains key named 
    /// <see cref="OperationResultExtensions.ExceptionKey"/>, 
    /// otherwise <see langword="false"/>.</returns>
    public bool TryGetExceptionEntry(
        [NotNullWhen(true)] out ElementEntry elementEntry)
    {
        elementEntry = Errors.FirstOrDefault(i => i.Key.Equals(
            OperationResultExtensions.ExceptionKey,
            StringComparison.OrdinalIgnoreCase));

        return elementEntry is { Key: not null };
    }

    /// <summary>
    /// Converts the current instance to a generic instance.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <returns>A new instance of <see cref="IOperationResult{TResult}"/>
    /// .</returns>
    public IOperationResult<TResult> ToOperationResult<TResult>()
        => new OperationResult<TResult>(
            StatusCode,
            (TResult?)Result,
            LocationUrl,
            Errors,
            Headers,
            Extensions,
            Title,
            Detail);

    [MemberNotNullWhen(true, nameof(Result))]
    internal bool IsGenericAndIsSuccess => IsGeneric && IsSuccess;
}

/// <summary>
/// Defines a contract that represents a generic result of an execution process.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
/// <remarks>You can use <see cref="OperationResultAspJsonConverterFactory"/> 
/// for Asp.Net to convert only <see cref="IOperationResult{TResult}.Result"/> 
/// to Json
/// <para>or use <see cref="OperationResultJsonConverterFactory"/> 
/// to convert the instance to Json.</para></remarks>
public partial interface IOperationResult<TResult> : IOperationResult
{
    /// <summary>
    /// Gets a user-defined object that qualifies or contains
    /// information about an operation return.
    /// </summary>
    /// <returns>The result value of this 
    /// <see cref="IOperationResult{TResult}"/>, which is of the same 
    /// type as the operation result's type parameter.</returns>
    [JsonIgnore]
    [AllowNull, MaybeNull]
    new TResult Result { get; }

    [JsonIgnore]
    [AllowNull, MaybeNull]
    object IOperationResult.Result => Result is TResult result
        ? result
        : null;

    /// <summary>
    /// Determines whether or not the current instance is generic.
    /// Returns <see langword="true"/> if so, otherwise <see langword="false"/>.
    /// </summary>
    [JsonIgnore]
    public new bool IsGeneric => true;

    [JsonIgnore]
    bool IOperationResult.IsGeneric => true;

    /// <summary>
    /// Converts the current instance to a non-generic instance.
    /// </summary>
    /// <returns>A new instance of <see cref="IOperationResult"/>.</returns>
    public IOperationResult ToOperationResult()
        => new OperationResult(
            StatusCode,
            Result,
            LocationUrl,
            Errors,
            Headers,
            Extensions,
            Title,
            Detail);
}