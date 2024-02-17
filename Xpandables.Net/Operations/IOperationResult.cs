
/************************************************************************************************************
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
************************************************************************************************************/
using System.Net;
using System.Text.Json.Serialization;

using Xpandables.Net.Optionals;
using Xpandables.Net.Primitives;

namespace Xpandables.Net.Operations;

/// <summary>
/// Defines a contract that represents the result of an execution process.
/// </summary>
/// <remarks>You can use <see cref="OperationResultAspJsonConverterFactory"/> for Asp.Net 
/// to convert only <see cref="IOperationResult.Result"/> to Json
/// <para>or use <see cref="OperationResultJsonConverterFactory"/> to convert the instance to Json.</para></remarks>
#if DEBUG
[JsonConverter(typeof(OperationResultJsonConverterFactory))]
#endif
public partial interface IOperationResult
{
    /// <summary>
    /// Gets the operation HTTP status code from the execution operation.
    /// </summary>
    [JsonInclude]
    HttpStatusCode StatusCode { get; }

    /// <summary>
    /// Gets the operation summary problem from the execution operation.
    /// <para>Mostly used when building the Asp.Net response in  case of errors.</para>
    /// </summary>
    /// <remarks>If not defined, the default title will be used.</remarks>
    [JsonInclude]
    Optional<string> Title { get; }

    /// <summary>
    /// Gets the operation explanation specific to the execution operation.
    /// <para>Mostly used when building the Asp.Net response in  case of errors.</para>
    /// </summary>
    /// <remarks>If not defined, the default detail will be used.</remarks>
    [JsonInclude]
    Optional<string> Detail { get; }

    /// <summary>
    /// Gets a user-defined object that qualifies or contains information about an operation return if available.
    /// </summary>
    /// <returns>The result value of this <see cref="IOperationResult"/>, which is of the type <see langword="object"/>.</returns>
    [JsonInclude]
    Optional<object> Result { get; }

    /// <summary>
    /// Gets the URL for location header. 
    /// <para>Mostly used with <see cref="HttpStatusCode.Created"/> in Asp.Net.</para>
    /// </summary>
    [JsonInclude]
    Optional<string> LocationUrl { get; }

    /// <summary>
    /// Gets the collection of header values that will be returned with the response.
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
    /// Determines whether or not the current instance is generic.
    /// </summary>
    /// <remarks>Returns <see langword="true"/> if so, otherwise <see langword="false"/>.</remarks>
    [JsonIgnore]
    public bool IsGeneric => false;

    /// <summary>
    /// Determines whether or not the current instance is successful according to the status of the operation.
    /// </summary>
    [JsonIgnore]
    public bool IsSuccess => StatusCode.IsFailureStatusCode();

    /// <summary>
    /// Determines whether or not the current instance is failed according to the status of the operation.
    /// </summary>
    public bool IsFailure => StatusCode.IsFailureStatusCode();

    ///<inheritdoc/>
    public IOperationResult<TResult> ToOperationResult<TResult>()
        => new OperationResult<TResult>(
            StatusCode,
            Result.IsEmpty
                ? Optional.Empty<TResult>()
                : Result.Value is TResult value
                    ? Optional.Some<TResult>(value)
                    : Optional.Empty<TResult>(),
            LocationUrl,
            Errors,
            Headers,
            Title,
            Detail);
}

/// <summary>
/// Defines a contract that represents a generic result of an execution process.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
/// <remarks>You can use <see cref="OperationResultAspJsonConverterFactory"/> for Asp.Net 
/// to convert only <see cref="IOperationResult{TResult}.Result"/> to Json
/// <para>or use <see cref="OperationResultJsonConverterFactory"/> to convert the instance to Json.</para></remarks>
#if DEBUG
[JsonConverter(typeof(OperationResultJsonConverterFactory))]
#endif
public partial interface IOperationResult<TResult> : IOperationResult
{
    /// <summary>
    /// Gets a user-defined object that qualifies or contains information about an operation return.
    /// </summary>
    /// <returns>The result value of this <see cref="IOperationResult{TResult}"/>, which is of the same 
    /// type as the operation result's type parameter.</returns>
    [JsonIgnore]
    new Optional<TResult> Result { get; }

    [JsonIgnore]
    Optional<object> IOperationResult.Result
        => Result.IsNotEmpty ? Optional.Some<object>(Result.Value) : Optional.Empty<object>();

    /// <summary>
    /// Determines whether or not the current instance is generic.
    /// Returns <see langword="true"/> if so, otherwise <see langword="false"/>.
    /// </summary>
    [JsonIgnore]
    public new bool IsGeneric => true;

    [JsonIgnore]
    bool IOperationResult.IsGeneric => true;

    ///<inheritdoc/>
    public IOperationResult ToOperationResult()
        => new OperationResult(
            StatusCode,
            Result.IsNotEmpty ? Optional.Some<object>(Result.Value) : Optional.Empty<object>(),
            LocationUrl,
            Errors,
            Headers,
            Title,
            Detail);
}