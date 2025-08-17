
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
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json.Serialization;

using Xpandables.Net.Collections;

namespace Xpandables.Net.Executions;

/// <summary>
/// Abstract base class for defining execution result representations, encompassing
/// essential properties and methods to describe execution outcomes such as
/// status code, title, detail, location, result, errors, headers, and extensions.
/// </summary>
/// <remarks>
/// Provides a foundation for specific result implementations and ensures
/// consistent behavior across various execution result types, such as
/// <see cref="ExecutionResult"/> and <see cref="ExecutionResult{TResult}"/>.
/// </remarks>
public abstract record Result
{
    /// <summary>
    /// Contains the key for the exception in the <see cref="ElementCollection" />.
    /// </summary>
    public const string ExceptionKey = "Exception";

    [JsonConstructor]
    internal Result() { }

    /// <summary>
    /// Represents the HTTP status code associated with the execution result.
    /// </summary>
    public required HttpStatusCode StatusCode { get; init; } = HttpStatusCode.OK;

    /// <summary>
    /// Represents the title associated with the execution result, providing a short text description.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Provides additional information about the execution result, typically used to clarify
    /// or elaborate on the <see cref="Title" />.
    /// </summary>
    public string? Detail { get; init; }

    /// <summary>
    /// Represents the URI that indicates the location of a resource relevant to the execution result.
    /// </summary>
    public Uri? Location { get; init; }

    /// <summary>
    /// Represents the result value of an execution, which can contain an object resulting from the operation.
    /// </summary>
    public object? Value { get; init; }

    /// <summary>
    /// Represents a collection of errors associated with an execution result.
    /// </summary>
    public ElementCollection Errors { get; init; } = [];

    /// <summary>
    /// Represents a collection of header entries associated with the execution result.
    /// The headers can include additional metadata relevant to the execution context.
    /// </summary>
    public ElementCollection Headers { get; init; } = [];

    /// <summary>
    /// Represents a collection of additional information associated with the execution result.
    /// </summary>
    public ElementCollection Extensions { get; init; } = [];

    /// <summary>
    /// Indicates whether the execution result contains a generic type for its result representation.
    /// </summary>
    public abstract bool IsGeneric { get; }

    /// <summary>
    /// Indicates whether the response status code represents a successful status code.
    /// </summary>
    public abstract bool IsSuccessStatusCode { get; }

    /// <summary>
    /// Represents an entry in the <see cref="Errors" /> collection that corresponds
    /// to the key specified by <see cref="ExceptionKey" />.
    /// </summary>
    /// <remarks>
    /// This property provides access to detailed exception information stored in
    /// the execution result. Returns <see langword="null" /> if no exception entry exists.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ElementEntry? Exception => Errors[ExceptionKey];

    /// <summary>
    /// Ensures that the execution result indicates a successful status code.
    /// </summary>
    /// <exception cref="ExecutionResultException">
    /// Thrown if the <see cref="StatusCode"/> does not represent a successful status code.
    /// </exception>
    /// <remarks>
    /// This method is abstract and must be implemented in derived classes to verify the success
    /// status of the execution result. In the event of a failure, it may throw an exception
    /// or handle the error accordingly.
    /// </remarks>
    public abstract void EnsureSuccessStatusCode();

    /// <summary>
    /// Converts an instance of <see cref="Result"/> to its associated
    /// <see cref="HttpStatusCode"/> representation.
    /// </summary>
    /// <param name="result">The execution result to convert.</param>
    /// <returns>The <see cref="HttpStatusCode"/> associated with the execution result.</returns>
    /// <remarks>
    /// Provides implicit conversion for <see cref="Result"/> to <see cref="HttpStatusCode"/>,
    /// allowing simpler usage when dealing with HTTP status codes in response to execution results.
    /// </remarks>
    public static implicit operator HttpStatusCode(Result result) => result.ToHttpStatusCode();

    /// <summary>
    /// Converts the execution result to its associated HTTP status code.
    /// </summary>
    /// <returns>
    /// A <see cref="HttpStatusCode"/> representing the status of the execution result.
    /// </returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public HttpStatusCode ToHttpStatusCode() => StatusCode;
}

/// <summary>
/// Represents an execution result, encapsulating necessary details about an operation's outcome,
/// including its status, headers, errors, and result content.
/// </summary>
/// <remarks>
/// This class is a sealed implementation of the <see cref="Result"/> base class.
/// It provides functionality to evaluate success status, ensure success, and convert results
/// to a generic version for handling specific result types.
/// </remarks>
[Serializable]
public sealed partial record ExecutionResult : Result
{
    [JsonConstructor]
    internal ExecutionResult() { }

    /// <inheritdoc />
    public override bool IsGeneric => false;

    /// <inheritdoc />
    public override bool IsSuccessStatusCode => StatusCode.IsSuccessStatusCode();

    /// <summary>
    /// Ensures that the execution result represents a successful status code.
    /// </summary>
    /// <exception cref="ExecutionResultException">
    /// Thrown if the execution result does not indicate a successful status.
    /// </exception>
    /// <remarks>
    /// This method verifies the success status of the execution result.
    /// If the status is unsuccessful, it throws an exception containing the current execution result.
    /// </remarks>
    public override void EnsureSuccessStatusCode()
    {
        if (!IsSuccessStatusCode)
        {
            throw new ExecutionResultException(this);
        }
    }

    /// <summary>
    /// Implicitly converts an instance of <see cref="ExecutionResult"/> to a generic version of
    /// <see cref="ExecutionResult{TResult}"/> with an object as the generic type.
    /// </summary>
    /// <param name="result">
    /// The instance of <see cref="ExecutionResult"/> to be converted.
    /// </param>
    /// <returns>
    /// A new instance of <see cref="ExecutionResult{TResult}"/> containing the details
    /// of the given <paramref name="result"/>.
    /// </returns>
    /// <remarks>
    /// This operator facilitates seamless conversion from a non-generic execution result to its generic counterpart.
    /// Use this in scenarios where typed execution results are required.
    /// </remarks>
    public static implicit operator ExecutionResult<object>(ExecutionResult result) =>
        result.ToExecutionResult();

    /// <summary>
    /// Converts the current instance of <see cref="ExecutionResult"/> to an instance of
    /// <see cref="ExecutionResult{TResult}"/> with the specified generic result type.
    /// </summary>
    /// <returns>
    /// A new <see cref="ExecutionResult{TResult}"/> object that encapsulates the same values as the current instance.
    /// </returns>
    public ExecutionResult<object> ToExecutionResult() =>
        new()
        {
            Detail = Detail,
            Errors = Errors,
            Extensions = Extensions,
            Headers = Headers,
            Location = Location,
            Value = Value,
            StatusCode = StatusCode,
            Title = Title
        };
}

/// <summary>
/// Represents a concrete execution result containing the outcome of an
/// operation with a strongly typed result object and additional properties.
/// </summary>
/// <typeparam name="TResult">The type of the result object contained in the execution result.</typeparam>
/// <remarks>
/// This class extends the abstract <see cref="Result"/> class and provides
/// specific implementation details, such as the result object, properties for
/// status code evaluation, and methods to ensure successful execution outcomes.
/// It also supports implicit conversions to and from the non-generic <see cref="ExecutionResult"/> class.
/// </remarks>
public sealed record ExecutionResult<TResult> : Result
{
    [JsonConstructor]
    internal ExecutionResult() { }

    /// <summary>
    /// Represents the result value of the operation, which can be of a generic or non-generic type.
    /// This property holds the execution outcome or output when available.
    /// </summary>
    // ReSharper disable once UseNullableAnnotationInsteadOfAttribute
    [MaybeNull]
    // ReSharper disable once UseNullableAnnotationInsteadOfAttribute
    [AllowNull]
    public new TResult Value
    {
        get => (TResult?)base.Value;
        init => base.Value = value;
    }

    /// <inheritdoc />
    [MemberNotNullWhen(true, nameof(Value))]
    public override bool IsSuccessStatusCode => StatusCode.IsSuccessStatusCode();

    /// <inheritdoc />
    public override bool IsGeneric => true;

    /// <summary>
    /// Ensures that the execution result indicates a successful status code.
    /// </summary>
    /// <exception cref="ExecutionResultException">
    /// Thrown if the execution result does not represent a successful status code.
    /// </exception>
    /// <remarks>
    /// This method verifies whether the execution result signifies a successful operation.
    /// In cases where the result is unsuccessful, an exception is thrown to handle the error.
    /// </remarks>
    [MemberNotNull([nameof(Value)])]
    public override void EnsureSuccessStatusCode()
    {
        if (!IsSuccessStatusCode)
        {
            throw new ExecutionResultException(this);
        }
    }

    /// <summary>
    /// Implicitly converts an instance of <see cref="ExecutionResult{TResult}"/> to <see cref="ExecutionResult"/>.
    /// </summary>
    /// <param name="result">
    /// The instance of <see cref="ExecutionResult{TResult}"/> to be converted.
    /// </param>
    /// <returns>
    /// An instance of <see cref="ExecutionResult"/> that represents the converted result.
    /// </returns>
    public static implicit operator ExecutionResult(ExecutionResult<TResult> result) =>
        result.ToExecutionResult();

    /// <summary>
    /// Defines an implicit conversion from <see cref="ExecutionResult{TResult}"/> to
    /// <see cref="ExecutionResult"/>. This allows for seamless transformation of a generic
    /// execution result to its base form.
    /// </summary>
    /// <param name="result">
    /// The <see cref="ExecutionResult{TResult}"/> instance to be converted to <see cref="ExecutionResult"/>.
    /// </param>
    /// <returns>
    /// A new instance of <see cref="ExecutionResult"/> populated with the data from the given
    /// <see cref="ExecutionResult{TResult}"/>.
    /// </returns>
    public static implicit operator ExecutionResult<TResult>(ExecutionResult result) =>
        new()
        {
            StatusCode = result.StatusCode,
            Title = result.Title,
            Detail = result.Detail,
            Location = result.Location,
            Value = result.Value is TResult resultValue ? resultValue : default,
            Errors = result.Errors,
            Headers = result.Headers,
            Extensions = result.Extensions
        };

    /// <summary>
    /// Converts the current generic execution result into a non-generic execution result.
    /// </summary>
    /// <returns>
    /// A new <see cref="ExecutionResult"/> instance containing the data from the current instance.
    /// </returns>
    public ExecutionResult ToExecutionResult() => new()
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