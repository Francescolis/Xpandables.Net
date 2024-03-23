
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
using System.Net;

namespace Xpandables.Net.Operations;

/// <summary>
/// A factory for <see cref="OperationResult"/> and 
/// <see cref="OperationResult{TResult}"/> using fluent interface.
/// </summary>
public sealed class OperationResults
{
    private OperationResults() { }

    /// <summary>
    /// Returns the <see cref="IOperationResult.ISuccessBuilder"/> 
    /// builder with the specified status code.
    /// </summary>
    /// <remarks>Throws an exception if the status code is not a success.</remarks>
    /// <param name="statusCode">The status code for the builder.</param>
    /// <returns>An instance of a type
    /// that implements <see cref="IOperationResult.ISuccessBuilder"/> 
    /// holding commands that can be chained.</returns>
    public static IOperationResult.ISuccessBuilder Success(
        HttpStatusCode statusCode = HttpStatusCode.OK)
        => new SuccessBuilder(statusCode);

    /// <summary>
    /// Returns the <see cref="IOperationResult.ISuccessBuilder{TResult}"/> 
    /// builder with the specified status code.
    /// </summary>
    /// <remarks>Throws an exception if the status code is not a success.</remarks>
    /// <param name="statusCode">The status code for the builder.</param>
    /// <returns>An instance of a type that implements 
    /// <see cref="IOperationResult.ISuccessBuilder{TResult}"/> 
    /// holding commands that can be chained.</returns>
    public static IOperationResult.ISuccessBuilder<TResult> Success<TResult>(
        HttpStatusCode statusCode = HttpStatusCode.OK)
        => new SuccessBuilder<TResult>(statusCode);

    /// <summary>
    /// Returns the <see cref="IOperationResult.IFailureBuilder"/> 
    /// builder with the specified status code.
    /// </summary>
    /// <remarks>Throws an exception if the status code is not a failure.</remarks>
    /// <param name="statusCode">The status code for the builder.</param>
    /// <returns>An instance of a type that implements 
    /// <see cref="IOperationResult.IFailureBuilder"/> 
    /// holding commands that can be chained.</returns>
    public static IOperationResult.IFailureBuilder Failure(
        HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        => new FailureBuilder(statusCode);

    /// <summary>
    /// Returns the <see cref="IOperationResult.IFailureBuilder{TResult}"/> 
    /// builder with the specified status code.
    /// </summary>
    /// <remarks>Throws an exception if the status code is not a failure.</remarks>
    /// <param name="statusCode">The status code for the builder.</param>
    /// <returns>An instance of a type that implements 
    /// <see cref="IOperationResult.IFailureBuilder{TResult}"/> 
    /// holding commands that can be chained.</returns>
    public static IOperationResult.IFailureBuilder<TResult> Failure<TResult>(
        HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        => new FailureBuilder<TResult>(statusCode);

    /// <summary>
    /// Returns the <see cref="IOperationResult.ISuccessBuilder"/> 
    /// builder with the <see cref="HttpStatusCode.OK"/>.
    /// </summary>
    /// <returns>An instance of a type that implements 
    /// <see cref="IOperationResult.ISuccessBuilder"/> 
    /// holding commands that can be chained.</returns>
    public static IOperationResult.ISuccessBuilder Ok()
        => new SuccessBuilder(HttpStatusCode.OK);

    /// <summary>
    /// Returns the <see cref="IOperationResult.ISuccessBuilder{TResult}"/> 
    /// builder with the <see cref="HttpStatusCode.OK"/>..
    /// </summary>
    /// <param name="result">The  result for the operation.</param>
    /// <returns>An instance of a type that implements 
    /// <see cref="IOperationResult.ISuccessBuilder{TResult}"/> 
    /// holding commands that can be chained.</returns>
    public static IOperationResult.ISuccessBuilder<TResult> Ok<TResult>(
        TResult result)
    {
        SuccessBuilder<TResult> successBuilder = new(HttpStatusCode.OK);
        return ((IOperationResult.ISuccessBuilder<TResult>)successBuilder)
            .WithResult(result);
    }

    /// <summary>
    /// Returns the <see cref="IOperationResult.ISuccessBuilder"/> 
    /// builder with the <see cref="HttpStatusCode.Created"/>.
    /// </summary>
    /// <returns>An instance of a type that implements 
    /// <see cref="IOperationResult.ISuccessBuilder"/> 
    /// holding commands that can be chained.</returns>
    public static IOperationResult.ISuccessBuilder Create()
        => new SuccessBuilder(HttpStatusCode.Created);

    /// <summary>
    /// Returns the <see cref="IOperationResult.ISuccessBuilder{TResult}"/> 
    /// builder with the <see cref="HttpStatusCode.Created"/>.
    /// </summary>
    /// <returns>An instance of a type that implements 
    /// <see cref="IOperationResult.ISuccessBuilder{TResult}"/> 
    /// holding commands that can be chained.</returns>
    public static IOperationResult.ISuccessBuilder<TResult> Create<TResult>()
        => new SuccessBuilder<TResult>(HttpStatusCode.Created);

    /// <summary>
    /// Returns the <see cref="IOperationResult.IFailureBuilder"/> 
    /// builder with the <see cref="HttpStatusCode.BadRequest"/>.
    /// </summary>
    /// <returns>An instance of a type that implements 
    /// <see cref="IOperationResult.IFailureBuilder"/> 
    /// holding commands that can be chained.</returns>
    public static IOperationResult.IFailureBuilder BadRequest()
        => new FailureBuilder(HttpStatusCode.BadRequest);

    /// <summary>
    /// Returns the <see cref="IOperationResult.IFailureBuilder{TResult}"/> 
    /// builder with the <see cref="HttpStatusCode.BadRequest"/>.
    /// </summary>
    /// <returns>An instance of a type that implements 
    /// <see cref="IOperationResult.IFailureBuilder{TResult}"/> 
    /// holding commands that can be chained.</returns>
    public static IOperationResult.IFailureBuilder<TResult> BadRequest<TResult>()
        => new FailureBuilder<TResult>(HttpStatusCode.BadRequest);

    /// <summary>
    /// Returns the <see cref="IOperationResult.IFailureBuilder"/> 
    /// builder with the <see cref="HttpStatusCode.Conflict"/>.
    /// </summary>
    /// <returns>An instance of a type that implements 
    /// <see cref="IOperationResult.IFailureBuilder"/> 
    /// holding commands that can be chained.</returns>
    public static IOperationResult.IFailureBuilder Conflict()
        => new FailureBuilder(HttpStatusCode.Conflict);

    /// <summary>
    /// Returns the <see cref="IOperationResult.IFailureBuilder{TResult}"/> 
    /// builder with the <see cref="HttpStatusCode.Conflict"/>.
    /// </summary>
    /// <returns>An instance of a type that implements 
    /// <see cref="IOperationResult.IFailureBuilder{TResult}"/> 
    /// holding commands that can be chained.</returns>
    public static IOperationResult.IFailureBuilder<TResult> Conflict<TResult>()
        => new FailureBuilder<TResult>(HttpStatusCode.BadRequest);

    /// <summary>
    /// Returns the <see cref="IOperationResult.IFailureBuilder"/> 
    /// builder with the <see cref="HttpStatusCode.NotFound"/>.
    /// </summary>
    /// <returns>An instance of a type that implements 
    /// <see cref="IOperationResult.IFailureBuilder"/>
    /// holding commands that can be chained.</returns>
    public static IOperationResult.IFailureBuilder NotFound()
        => new FailureBuilder(HttpStatusCode.NotFound);

    /// <summary>
    /// Returns the <see cref="IOperationResult.IFailureBuilder{TResult}"/> 
    /// builder with the <see cref="HttpStatusCode.NotFound"/>.
    /// </summary>
    /// <returns>An instance of a type that implements 
    /// <see cref="IOperationResult.IFailureBuilder{TResult}"/> 
    /// holding commands that can be chained.</returns>
    public static IOperationResult.IFailureBuilder<TResult> NotFound<TResult>()
        => new FailureBuilder<TResult>(HttpStatusCode.NotFound);

    /// <summary>
    /// Returns the <see cref="IOperationResult.IFailureBuilder"/> 
    /// builder with the <see cref="HttpStatusCode.Unauthorized"/>.
    /// </summary>
    /// <returns>An instance of a type that implements 
    /// <see cref="IOperationResult.IFailureBuilder"/> 
    /// holding commands that can be chained.</returns>
    public static IOperationResult.IFailureBuilder Unauthorized()
        => new FailureBuilder(HttpStatusCode.Unauthorized);

    /// <summary>
    /// Returns the <see cref="IOperationResult.IFailureBuilder{TResult}"/> 
    /// builder with the <see cref="HttpStatusCode.Unauthorized"/>.
    /// </summary>
    /// <returns>An instance of a type that implements 
    /// <see cref="IOperationResult.IFailureBuilder{TResult}"/> 
    /// holding commands that can be chained.</returns>
    public static IOperationResult.IFailureBuilder<TResult> Unauthorized<TResult>()
        => new FailureBuilder<TResult>(HttpStatusCode.Unauthorized);

    /// <summary>
    /// Returns the <see cref="IOperationResult.IFailureBuilder"/> 
    /// builder with the <see cref="HttpStatusCode.InternalServerError"/>.
    /// </summary>
    /// <returns>An instance of a type that implements 
    /// <see cref="IOperationResult.IFailureBuilder"/> 
    /// holding commands that can be chained.</returns>
    public static IOperationResult.IFailureBuilder InternalError()
        => new FailureBuilder(HttpStatusCode.InternalServerError);

    /// <summary>
    /// Returns the <see cref="IOperationResult.IFailureBuilder{TResult}"/> 
    /// builder with the <see cref="HttpStatusCode.InternalServerError"/>.
    /// </summary>
    /// <returns>An instance of a type that implements 
    /// <see cref="IOperationResult.IFailureBuilder{TResult}"/> 
    /// holding commands that can be chained.</returns>
    public static IOperationResult.IFailureBuilder<TResult> InternalError<TResult>()
        => new FailureBuilder<TResult>(HttpStatusCode.InternalServerError);
}
