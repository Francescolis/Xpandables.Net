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
using System.Net;

namespace Xpandables.Net.Executions;

/// <summary>
/// Provides methods to build success and failure execution results.
/// </summary>
public readonly record struct ExecutionResults
{
    /// <summary>
    /// Returns an implementation of <see cref="ExecutionResult"/> with the 
    /// status code OK.
    /// </summary>
    /// <returns>An implementation of <see cref="ExecutionResult"/>.</returns>
    public static ExecutionResult Success() => Success(HttpStatusCode.OK).Build();

    /// <summary>
    /// Returns an implementation of <see cref="ExecutionResult{TResult}"/> with the
    /// status code OK and the specified result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="result">The result of the execution.</param>
    /// <returns>An implementation of <see cref="ExecutionResult{TResult}"/>.</returns>
    public static ExecutionResult<TResult> Success<TResult>(TResult result) =>
        Success(result, HttpStatusCode.OK).Build();

    /// <summary>
    /// Returns an implementation of <see cref="ExecutionResult"/> with the
    /// status code BadRequest and the specified error.
    /// </summary>
    /// <param name="key">The key of the error.</param>
    /// <param name="message">The message of the error.</param>
    /// <returns>An implementation of <see cref="ExecutionResult"/>.</returns>
    public static ExecutionResult Failure(string key, string message) =>
        Failure(HttpStatusCode.BadRequest)
        .WithError(key, message)
        .Build();

    /// <summary>
    /// Returns an implementation of <see cref="ExecutionResult"/> with the
    /// failure status code and the specified exception.
    /// </summary>
    /// <param name="exception">The exception to the error.</param>
    /// <returns>An implementation of <see cref="ExecutionResult"/>.</returns>
    public static ExecutionResult Failure(Exception exception) =>
        Failure(HttpStatusCode.BadRequest)
        .Merge(exception.ToExecutionResult())
        .Build();

    /// <summary>
    /// Returns an implementation of <see cref="ExecutionResult"/> with the
    /// failure status code and the specified exception.
    /// </summary>
    /// <param name="exception">The exception to the error.</param>
    /// <returns>An implementation of <see cref="ExecutionResult"/>.</returns>
    public static ExecutionResult<TResult> Failure<TResult>(Exception exception) =>
        Failure<TResult>(HttpStatusCode.BadRequest)
        .Merge(exception.ToExecutionResult())
        .Build();

    /// <summary>  
    /// Returns an implementation of <see cref="IExecutionResultSuccessBuilder"/> with the   
    /// specified status code to build a success execution result.  
    /// </summary>  
    /// <param name="statusCode">The status code of the execution result.</param>  
    /// <returns>An instance of <see cref="IExecutionResultSuccessBuilder"/>.</returns>  
    public static IExecutionResultSuccessBuilder Success(HttpStatusCode statusCode) =>
        new ExecutionResultSuccessBuilder(statusCode);

    /// <summary>  
    /// Returns an implementation of <see cref="IExecutionResultSuccessBuilder{TResult}"/> with the   
    /// specified status code to build a success execution result.  
    /// </summary>  
    /// <typeparam name="TResult">The type of the result.</typeparam>  
    /// <param name="statusCode">The status code of the execution result.</param>  
    /// <returns>An instance of <see cref="IExecutionResultSuccessBuilder{TResult}"/>.</returns>  
    // ReSharper disable once MemberCanBePrivate.Global
    public static IExecutionResultSuccessBuilder<TResult> Success<TResult>(
        HttpStatusCode statusCode) =>
        new ExecutionResultSuccessBuilder<TResult>(statusCode);

    /// <summary>  
    /// Returns an implementation of <see cref="IExecutionResultSuccessBuilder{TResult}"/> with the   
    /// specified result and status code to build a success execution result.  
    /// </summary>  
    /// <typeparam name="TResult">The type of the result.</typeparam>  
    /// <param name="result">The result of the execution.</param>  
    /// <param name="statusCode">The status code of the execution result.</param>  
    /// <returns>An instance of <see cref="IExecutionResultSuccessBuilder{TResult}"/>.</returns>  
    // ReSharper disable once MemberCanBePrivate.Global
    public static IExecutionResultSuccessBuilder<TResult> Success<TResult>(
        TResult result, HttpStatusCode statusCode) =>
        new ExecutionResultSuccessBuilder<TResult>(statusCode).WithResult(result);

    /// <summary>  
    /// Returns an implementation of <see cref="IExecutionResultFailureBuilder"/> with the   
    /// specified status code to build a failure execution result.  
    /// </summary>  
    /// <param name="statusCode">The status code of the execution result.</param>  
    /// <returns>An instance of <see cref="IExecutionResultFailureBuilder"/>.</returns>  
    public static IExecutionResultFailureBuilder Failure(HttpStatusCode statusCode) =>
        new ExecutionResultFailureBuilder(statusCode);

    /// <summary>  
    /// Returns an implementation of <see cref="IExecutionResultFailureBuilder{TResult}"/> with the   
    /// specified status code to build a failure execution result.  
    /// </summary>  
    /// <typeparam name="TResult">The type of the result.</typeparam>  
    /// <param name="statusCode">The status code of the execution result.</param>  
    /// <returns>An instance of <see cref="IExecutionResultFailureBuilder{TResult}"/>.</returns>  
    // ReSharper disable once MemberCanBePrivate.Global
    public static IExecutionResultFailureBuilder<TResult> Failure<TResult>(
        HttpStatusCode statusCode) =>
        new ExecutionResultFailureBuilder<TResult>(statusCode);

    /// <summary>  
    /// Returns an implementation of <see cref="IExecutionResultSuccessBuilder"/> with the   
    /// status code OK to build a success execution result.  
    /// </summary>  
    /// <returns>An instance of <see cref="IExecutionResultSuccessBuilder"/>.</returns>  
    public static IExecutionResultSuccessBuilder Ok() => Success(HttpStatusCode.OK);

    /// <summary>  
    /// Returns an implementation of <see cref="IExecutionResultSuccessBuilder{TResult}"/> with the   
    /// status code OK to build a success execution result.  
    /// </summary>  
    /// <typeparam name="TResult">The type of the result.</typeparam>  
    /// <returns>An instance of <see cref="IExecutionResultSuccessBuilder{TResult}"/>.</returns>  
    public static IExecutionResultSuccessBuilder<TResult> Ok<TResult>() => Success<TResult>(HttpStatusCode.OK);

    /// <summary>  
    /// Returns an implementation of <see cref="IExecutionResultSuccessBuilder{TResult}"/> with the   
    /// specified result and status code OK to build a success execution result.  
    /// </summary>  
    /// <typeparam name="TResult">The type of the result.</typeparam>  
    /// <param name="result">The result of the execution.</param>  
    /// <returns>An instance of <see cref="IExecutionResultSuccessBuilder{TResult}"/>.</returns>  
    public static IExecutionResultSuccessBuilder<TResult> Ok<TResult>(TResult result) =>
        Success(result, HttpStatusCode.OK);

    /// <summary>  
    /// Returns an implementation of <see cref="IExecutionResultSuccessBuilder"/> with the   
    /// status code Created to build a success execution result.  
    /// </summary>  
    /// <returns>An instance of <see cref="IExecutionResultSuccessBuilder"/>.</returns>  
    public static IExecutionResultSuccessBuilder Created() => Success(HttpStatusCode.Created);

    /// <summary>  
    /// Returns an implementation of <see cref="IExecutionResultSuccessBuilder{TResult}"/> with the   
    /// status code Created to build a success execution result.  
    /// </summary>  
    /// <typeparam name="TResult">The type of the result.</typeparam>  
    /// <returns>An instance of <see cref="IExecutionResultSuccessBuilder{TResult}"/>.</returns>  
    public static IExecutionResultSuccessBuilder<TResult> Created<TResult>() =>
        Success<TResult>(HttpStatusCode.Created);

    /// <summary>  
    /// Returns an implementation of <see cref="IExecutionResultSuccessBuilder{TResult}"/> with the   
    /// specified result and status code Created to build a success execution result.  
    /// </summary>  
    /// <typeparam name="TResult">The type of the result.</typeparam>  
    /// <param name="result">The result of the execution.</param>  
    /// <returns>An instance of <see cref="IExecutionResultSuccessBuilder{TResult}"/>.</returns>  
    public static IExecutionResultSuccessBuilder<TResult> Created<TResult>(TResult result) =>
        Success(result, HttpStatusCode.Created);

    /// <summary>  
    /// Returns an implementation of <see cref="IExecutionResultSuccessBuilder"/> with the   
    /// status code NoContent to build a success execution result.  
    /// </summary>  
    /// <returns>An instance of <see cref="IExecutionResultSuccessBuilder"/>.</returns>  
    public static IExecutionResultSuccessBuilder NoContent() => Success(HttpStatusCode.NoContent);

    /// <summary>  
    /// Returns an implementation of <see cref="IExecutionResultSuccessBuilder{TResult}"/> with the   
    /// status code NoContent to build a success execution result.  
    /// </summary>  
    /// <typeparam name="TResult">The type of the result.</typeparam>  
    /// <returns>An instance of <see cref="IExecutionResultSuccessBuilder{TResult}"/>.</returns>  
    public static IExecutionResultSuccessBuilder<TResult> NoContent<TResult>() =>
        Success<TResult>(HttpStatusCode.NoContent);

    /// <summary>  
    /// Returns an implementation of <see cref="IExecutionResultFailureBuilder"/> with the   
    /// status code NotFound to build a failure execution result.  
    /// </summary>  
    /// <returns>An instance of <see cref="IExecutionResultFailureBuilder"/>.</returns>  
    public static IExecutionResultFailureBuilder NotFound() => Failure(HttpStatusCode.NotFound);

    /// <summary>  
    /// Returns an implementation of <see cref="IExecutionResultFailureBuilder{TResult}"/> with the   
    /// status code NotFound to build a failure execution result.  
    /// </summary>  
    /// <typeparam name="TResult">The type of the result.</typeparam>  
    /// <returns>An instance of <see cref="IExecutionResultFailureBuilder{TResult}"/>.</returns>  
    public static IExecutionResultFailureBuilder<TResult> NotFound<TResult>() =>
       Failure<TResult>(HttpStatusCode.NotFound);

    /// <summary>  
    /// Returns an implementation of <see cref="IExecutionResultFailureBuilder"/> with the   
    /// status code BadRequest to build a failure execution result.  
    /// </summary>  
    /// <returns>An instance of <see cref="IExecutionResultFailureBuilder"/>.</returns>  
    public static IExecutionResultFailureBuilder BadRequest() => Failure(HttpStatusCode.BadRequest);

    /// <summary>  
    /// Returns an implementation of <see cref="IExecutionResultFailureBuilder{TResult}"/> with the   
    /// status code BadRequest to build a failure execution result.  
    /// </summary>  
    /// <typeparam name="TResult">The type of the result.</typeparam>  
    /// <returns>An instance of <see cref="IExecutionResultFailureBuilder{TResult}"/>.</returns>  
    public static IExecutionResultFailureBuilder<TResult> BadRequest<TResult>() =>
        Failure<TResult>(HttpStatusCode.BadRequest);

    /// <summary>  
    /// Returns an implementation of <see cref="IExecutionResultFailureBuilder"/> with the   
    /// status code Conflict to build a failure execution result.  
    /// </summary>  
    /// <returns>An instance of <see cref="IExecutionResultFailureBuilder"/>.</returns>  
    public static IExecutionResultFailureBuilder Conflict() => Failure(HttpStatusCode.Conflict);

    /// <summary>  
    /// Returns an implementation of <see cref="IExecutionResultFailureBuilder{TResult}"/> with the   
    /// status code Conflict to build a failure execution result.  
    /// </summary>  
    /// <typeparam name="TResult">The type of the result.</typeparam>  
    /// <returns>An instance of <see cref="IExecutionResultFailureBuilder{TResult}"/>.</returns>  
    public static IExecutionResultFailureBuilder<TResult> Conflict<TResult>() =>
        Failure<TResult>(HttpStatusCode.Conflict);

    /// <summary>  
    /// Returns an implementation of <see cref="IExecutionResultFailureBuilder"/> with the   
    /// status code Unauthorized to build a failure execution result.  
    /// </summary>  
    /// <returns>An instance of <see cref="IExecutionResultFailureBuilder"/>.</returns>  
    public static IExecutionResultFailureBuilder Unauthorized() =>
        Failure(HttpStatusCode.Unauthorized);

    /// <summary>  
    /// Returns an implementation of <see cref="IExecutionResultFailureBuilder{TResult}"/> with the   
    /// status code Unauthorized to build a failure execution result.  
    /// </summary>  
    /// <typeparam name="TResult">The type of the result.</typeparam>  
    /// <returns>An instance of <see cref="IExecutionResultFailureBuilder{TResult}"/>.</returns>  
    public static IExecutionResultFailureBuilder<TResult> Unauthorized<TResult>() =>
       Failure<TResult>(HttpStatusCode.Unauthorized);

    /// <summary>  
    /// Returns an implementation of <see cref="IExecutionResultFailureBuilder"/> with the   
    /// status code InternalServerError to build a failure execution result.  
    /// </summary>  
    /// <returns>An instance of <see cref="IExecutionResultFailureBuilder"/>.</returns>  
    public static IExecutionResultFailureBuilder InternalServerError() =>
        Failure(HttpStatusCode.InternalServerError);

    /// <summary>  
    /// Returns an implementation of <see cref="IExecutionResultFailureBuilder{TResult}"/> with the   
    /// status code InternalServerError to build a failure execution result.  
    /// </summary>  
    /// <typeparam name="TResult">The type of the result.</typeparam>  
    /// <returns>An instance of <see cref="IExecutionResultFailureBuilder{TResult}"/>.</returns>  
    public static IExecutionResultFailureBuilder<TResult> InternalServerError<TResult>() =>
        Failure<TResult>(HttpStatusCode.InternalServerError);

    /// <summary>  
    /// Returns an implementation of <see cref="IExecutionResultFailureBuilder"/> with the   
    /// status code ServiceUnavailable to build a failure execution result.  
    /// </summary>  
    /// <returns>An instance of <see cref="IExecutionResultFailureBuilder"/>.</returns>  
    public static IExecutionResultFailureBuilder ServiceUnavailable() =>
        Failure(HttpStatusCode.ServiceUnavailable);

    /// <summary>  
    /// Returns an implementation of <see cref="IExecutionResultFailureBuilder{TResult}"/> with the   
    /// status code ServiceUnavailable to build a failure execution result.  
    /// </summary>  
    /// <typeparam name="TResult">The type of the result.</typeparam>  
    /// <returns>An instance of <see cref="IExecutionResultFailureBuilder{TResult}"/>.</returns>  
    public static IExecutionResultFailureBuilder<TResult> ServiceUnavailable<TResult>() =>
        Failure<TResult>(HttpStatusCode.ServiceUnavailable);
}
