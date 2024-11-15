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

namespace Xpandables.Net.Operations;

/// <summary>
/// Provides methods to build success and failure execution results.
/// </summary>
public readonly record struct ExecutionResults
{
    /// <summary>  
    /// Returns an implementation of <see cref="ISuccessBuilder"/> with the   
    /// specified status code to build a success execution result.  
    /// </summary>  
    /// <param name="statusCode">The status code of the execution result.</param>  
    /// <returns>An instance of <see cref="ISuccessBuilder"/>.</returns>  
    public static ISuccessBuilder Success(
        HttpStatusCode statusCode = HttpStatusCode.OK) =>
        new SuccessBuilder(statusCode);

    /// <summary>  
    /// Returns an implementation of <see cref="ISuccessBuilder{TResult}"/> with the   
    /// specified status code to build a success execution result.  
    /// </summary>  
    /// <typeparam name="TResult">The type of the result.</typeparam>  
    /// <param name="statusCode">The status code of the execution result.</param>  
    /// <returns>An instance of <see cref="ISuccessBuilder{TResult}"/>.</returns>  
    public static ISuccessBuilder<TResult> Success<TResult>(
        HttpStatusCode statusCode = HttpStatusCode.OK) =>
        new SuccessBuilder<TResult>(statusCode);

    /// <summary>  
    /// Returns an implementation of <see cref="ISuccessBuilder{TResult}"/> with the   
    /// specified result and status code to build a success execution result.  
    /// </summary>  
    /// <typeparam name="TResult">The type of the result.</typeparam>  
    /// <param name="result">The result of the execution.</param>  
    /// <param name="statusCode">The status code of the execution result.</param>  
    /// <returns>An instance of <see cref="ISuccessBuilder{TResult}"/>.</returns>  
    public static ISuccessBuilder<TResult> Success<TResult>(
        TResult result, HttpStatusCode statusCode = HttpStatusCode.OK) =>
        new SuccessBuilder<TResult>(statusCode).WithResult(result);

    /// <summary>  
    /// Returns an implementation of <see cref="IFailureBuilder"/> with the   
    /// specified status code to build a failure execution result.  
    /// </summary>  
    /// <param name="statusCode">The status code of the execution result.</param>  
    /// <returns>An instance of <see cref="IFailureBuilder"/>.</returns>  
    public static IFailureBuilder Failure(
        HttpStatusCode statusCode = HttpStatusCode.BadRequest) =>
        new FailureBuilder(statusCode);

    /// <summary>  
    /// Returns an implementation of <see cref="IFailureBuilder{TResult}"/> with the   
    /// specified status code to build a failure execution result.  
    /// </summary>  
    /// <typeparam name="TResult">The type of the result.</typeparam>  
    /// <param name="statusCode">The status code of the execution result.</param>  
    /// <returns>An instance of <see cref="IFailureBuilder{TResult}"/>.</returns>  
    public static IFailureBuilder<TResult> Failure<TResult>(
        HttpStatusCode statusCode = HttpStatusCode.BadRequest) =>
        new FailureBuilder<TResult>(statusCode);

    /// <summary>  
    /// Returns an implementation of <see cref="ISuccessBuilder"/> with the   
    /// status code OK to build a success execution result.  
    /// </summary>  
    /// <returns>An instance of <see cref="ISuccessBuilder"/>.</returns>  
    public static ISuccessBuilder Ok() => Success();

    /// <summary>  
    /// Returns an implementation of <see cref="ISuccessBuilder{TResult}"/> with the   
    /// status code OK to build a success execution result.  
    /// </summary>  
    /// <typeparam name="TResult">The type of the result.</typeparam>  
    /// <returns>An instance of <see cref="ISuccessBuilder{TResult}"/>.</returns>  
    public static ISuccessBuilder<TResult> Ok<TResult>() => Success<TResult>();

    /// <summary>  
    /// Returns an implementation of <see cref="ISuccessBuilder{TResult}"/> with the   
    /// specified result and status code OK to build a success execution result.  
    /// </summary>  
    /// <typeparam name="TResult">The type of the result.</typeparam>  
    /// <param name="result">The result of the execution.</param>  
    /// <returns>An instance of <see cref="ISuccessBuilder{TResult}"/>.</returns>  
    public static ISuccessBuilder<TResult> Ok<TResult>(TResult result) =>
        Success(result);

    /// <summary>  
    /// Returns an implementation of <see cref="ISuccessBuilder"/> with the   
    /// status code Created to build a success execution result.  
    /// </summary>  
    /// <returns>An instance of <see cref="ISuccessBuilder"/>.</returns>  
    public static ISuccessBuilder Created() => Success(HttpStatusCode.Created);

    /// <summary>  
    /// Returns an implementation of <see cref="ISuccessBuilder{TResult}"/> with the   
    /// status code Created to build a success execution result.  
    /// </summary>  
    /// <typeparam name="TResult">The type of the result.</typeparam>  
    /// <returns>An instance of <see cref="ISuccessBuilder{TResult}"/>.</returns>  
    public static ISuccessBuilder<TResult> Created<TResult>() =>
        Success<TResult>(HttpStatusCode.Created);

    /// <summary>  
    /// Returns an implementation of <see cref="ISuccessBuilder{TResult}"/> with the   
    /// specified result and status code Created to build a success execution result.  
    /// </summary>  
    /// <typeparam name="TResult">The type of the result.</typeparam>  
    /// <param name="result">The result of the execution.</param>  
    /// <returns>An instance of <see cref="ISuccessBuilder{TResult}"/>.</returns>  
    public static ISuccessBuilder<TResult> Created<TResult>(TResult result) =>
        Success(result, HttpStatusCode.Created);

    /// <summary>  
    /// Returns an implementation of <see cref="ISuccessBuilder"/> with the   
    /// status code NoContent to build a success execution result.  
    /// </summary>  
    /// <returns>An instance of <see cref="ISuccessBuilder"/>.</returns>  
    public static ISuccessBuilder NoContent() => Success(HttpStatusCode.NoContent);

    /// <summary>  
    /// Returns an implementation of <see cref="ISuccessBuilder{TResult}"/> with the   
    /// status code NoContent to build a success execution result.  
    /// </summary>  
    /// <typeparam name="TResult">The type of the result.</typeparam>  
    /// <returns>An instance of <see cref="ISuccessBuilder{TResult}"/>.</returns>  
    public static ISuccessBuilder<TResult> NoContent<TResult>() =>
        Success<TResult>(HttpStatusCode.NoContent);

    /// <summary>  
    /// Returns an implementation of <see cref="IFailureBuilder"/> with the   
    /// status code NotFound to build a failure execution result.  
    /// </summary>  
    /// <returns>An instance of <see cref="IFailureBuilder"/>.</returns>  
    public static IFailureBuilder NotFound() => Failure(HttpStatusCode.NotFound);

    /// <summary>  
    /// Returns an implementation of <see cref="IFailureBuilder{TResult}"/> with the   
    /// status code NotFound to build a failure execution result.  
    /// </summary>  
    /// <typeparam name="TResult">The type of the result.</typeparam>  
    /// <returns>An instance of <see cref="IFailureBuilder{TResult}"/>.</returns>  
    public static IFailureBuilder<TResult> NotFound<TResult>() =>
       Failure<TResult>(HttpStatusCode.NotFound);

    /// <summary>  
    /// Returns an implementation of <see cref="IFailureBuilder"/> with the   
    /// status code BadRequest to build a failure execution result.  
    /// </summary>  
    /// <returns>An instance of <see cref="IFailureBuilder"/>.</returns>  
    public static IFailureBuilder BadRequest() => Failure(HttpStatusCode.BadRequest);

    /// <summary>  
    /// Returns an implementation of <see cref="IFailureBuilder{TResult}"/> with the   
    /// status code BadRequest to build a failure execution result.  
    /// </summary>  
    /// <typeparam name="TResult">The type of the result.</typeparam>  
    /// <returns>An instance of <see cref="IFailureBuilder{TResult}"/>.</returns>  
    public static IFailureBuilder<TResult> BadRequest<TResult>() =>
        Failure<TResult>(HttpStatusCode.BadRequest);

    /// <summary>  
    /// Returns an implementation of <see cref="IFailureBuilder"/> with the   
    /// status code Conflict to build a failure execution result.  
    /// </summary>  
    /// <returns>An instance of <see cref="IFailureBuilder"/>.</returns>  
    public static IFailureBuilder Conflict() => Failure(HttpStatusCode.Conflict);

    /// <summary>  
    /// Returns an implementation of <see cref="IFailureBuilder{TResult}"/> with the   
    /// status code Conflict to build a failure execution result.  
    /// </summary>  
    /// <typeparam name="TResult">The type of the result.</typeparam>  
    /// <returns>An instance of <see cref="IFailureBuilder{TResult}"/>.</returns>  
    public static IFailureBuilder<TResult> Conflict<TResult>() =>
        Failure<TResult>(HttpStatusCode.Conflict);

    /// <summary>  
    /// Returns an implementation of <see cref="IFailureBuilder"/> with the   
    /// status code Unauthorized to build a failure execution result.  
    /// </summary>  
    /// <returns>An instance of <see cref="IFailureBuilder"/>.</returns>  
    public static IFailureBuilder Unauthorized() =>
        Failure(HttpStatusCode.Unauthorized);

    /// <summary>  
    /// Returns an implementation of <see cref="IFailureBuilder{TResult}"/> with the   
    /// status code Unauthorized to build a failure execution result.  
    /// </summary>  
    /// <typeparam name="TResult">The type of the result.</typeparam>  
    /// <returns>An instance of <see cref="IFailureBuilder{TResult}"/>.</returns>  
    public static IFailureBuilder<TResult> Unauthorized<TResult>() =>
       Failure<TResult>(HttpStatusCode.Unauthorized);

    /// <summary>  
    /// Returns an implementation of <see cref="IFailureBuilder"/> with the   
    /// status code InternalServerError to build a failure execution result.  
    /// </summary>  
    /// <returns>An instance of <see cref="IFailureBuilder"/>.</returns>  
    public static IFailureBuilder InternalServerError() =>
        Failure(HttpStatusCode.InternalServerError);

    /// <summary>  
    /// Returns an implementation of <see cref="IFailureBuilder{TResult}"/> with the   
    /// status code InternalServerError to build a failure execution result.  
    /// </summary>  
    /// <typeparam name="TResult">The type of the result.</typeparam>  
    /// <returns>An instance of <see cref="IFailureBuilder{TResult}"/>.</returns>  
    public static IFailureBuilder<TResult> InternalServerError<TResult>() =>
        Failure<TResult>(HttpStatusCode.InternalServerError);

    /// <summary>  
    /// Returns an implementation of <see cref="IFailureBuilder"/> with the   
    /// status code ServiceUnavailable to build a failure execution result.  
    /// </summary>  
    /// <returns>An instance of <see cref="IFailureBuilder"/>.</returns>  
    public static IFailureBuilder ServiceUnavailable() =>
        Failure(HttpStatusCode.ServiceUnavailable);

    /// <summary>  
    /// Returns an implementation of <see cref="IFailureBuilder{TResult}"/> with the   
    /// status code ServiceUnavailable to build a failure execution result.  
    /// </summary>  
    /// <typeparam name="TResult">The type of the result.</typeparam>  
    /// <returns>An instance of <see cref="IFailureBuilder{TResult}"/>.</returns>  
    public static IFailureBuilder<TResult> ServiceUnavailable<TResult>() =>
        Failure<TResult>(HttpStatusCode.ServiceUnavailable);
}
