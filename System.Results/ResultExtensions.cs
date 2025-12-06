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

using System.Net;

namespace System.Results;

/// <summary>
/// Provides extension methods for working with <see cref="Result"/> instances, enabling the creation and
/// configuration of  results for HTTP operations.
/// </summary>
/// <remarks>These extension methods simplify the process of building  results that represent successful
/// or failed HTTP operations. They are intended to be used in scenarios where standardized result handling is required,
/// such as in web APIs or service layers.</remarks>
public static class ResultExtensions
{
    /// <summary>
    /// Provides extension methods for <see cref="Result"/> instances.
    /// </summary>
    /// <param name="result">The result instance to extend.</param>"
    extension(Result result)
    {
        /// <summary>
        /// Creates a new ResultException that represents the current result.
        /// </summary>
        /// <returns>An ResultException initialized with the current result.</returns>
        public ResultException ToResultException() => new(result);

        /// <summary>
        /// Creates a new <see cref="FailureResult"/> instance that represents the current failure state.
        /// </summary>
        /// <returns>A <see cref="FailureResult"/> containing the status code, title, detail, location, errors, headers,
        /// extensions, and exception from the current result.</returns>
        public FailureResult ToFailureResult()
        {
            return new FailureResult
            {
                StatusCode = result.StatusCode,
                Title = result.Title,
                Detail = result.Detail,
                Location = result.Location,
                Errors = result.Errors,
                Headers = result.Headers,
                Extensions = result.Extensions,
                Exception = result.Exception
            };
        }

        /// <summary>
        /// Creates a new failure result of the specified value type, copying error details from the current result.
        /// </summary>
        /// <typeparam name="TValue">The type of the value associated with the failure result.</typeparam>
        /// <returns>A <see cref="FailureResult{TValue}"/> containing the status code, error details, and metadata from the
        /// current result.</returns>
        public FailureResult<TValue> ToFailureResult<TValue>()
        {
            return new FailureResult<TValue>
            {
                StatusCode = result.StatusCode,
                Title = result.Title,
                Detail = result.Detail,
                Location = result.Location,
                Errors = result.Errors,
                Headers = result.Headers,
                Extensions = result.Extensions,
                Exception = result.Exception
            };
        }

        /// <summary>
        /// Creates a successful <see cref="Result"/> instance representing an result that completed without
        /// errors.
        /// </summary>
        /// <remarks>You can further customize the result using the builder methods.</remarks>
        /// <returns>An <see cref="Result"/> indicating a successful outcome with an HTTP status code of 200 (OK).</returns>
        public static SuccessResultBuilder Success() => new(HttpStatusCode.OK);

        /// <summary>
        /// Creates a successful <see cref="Result{TValue}"/> with the specified value and an HTTP status code of OK (200).
        /// </summary>
        /// <remarks>You can further customize the result using the builder methods.</remarks>
        /// <typeparam name="TValue">The type of the value to include in the  result.</typeparam>
        /// <param name="value">The value to include in the successful  result.</param>
        /// <returns>An <see cref="Result{TValue}"/> representing a successful  containing the specified
        /// value.</returns>
        public static SuccessResultBuilder<TValue> Success<TValue>(TValue value) => new SuccessResultBuilder<TValue>(HttpStatusCode.OK).WithValue(value);

        /// <summary>
        /// Creates a result builder that represents a successful  with an HTTP 201 Created status code.
        /// </summary>
        /// <remarks>You can further customize the result using the builder methods.</remarks>
        /// <returns>An <see cref="Result"/> configured with the HTTP 201 Created status code.</returns>
        public static SuccessResultBuilder Created() => new(HttpStatusCode.Created);

        /// <summary>
        /// Creates a successful  result with an HTTP 201 Created status code.
        /// </summary>
        /// <remarks>You can further customize the result using the builder methods.</remarks>
        /// <typeparam name="TValue">The type of the result value to include in the result.</typeparam>
        /// <returns>An <see cref="Result{TValue}"/> representing a successful result with a 201 Created
        /// status code.</returns>
        public static SuccessResultBuilder<TValue> Created<TValue>() => new(HttpStatusCode.Created);

        /// <summary>
        /// Creates a result builder that represents a successful  with no content to return.
        /// </summary>
        /// <remarks>You can further customize the result using the builder methods.</remarks>
        /// <returns>An <see cref="Result"/> configured with an HTTP 204 No Content status, indicating
        /// that the request was successful but there is no content in the response.</returns>
        public static SuccessResultBuilder NoContent() => new(HttpStatusCode.NoContent);

        /// <summary>
        /// Creates a successful  result with an HTTP 204 No Content status, indicating that the 
        /// completed successfully but does not return any content.
        /// </summary>
        /// <remarks>You can further customize the result using the builder methods.</remarks>
        /// <typeparam name="TValue">The type of the result associated with the . No content will be returned for this type.</typeparam>
        /// <returns>An <see cref="Result{TValue}"/> representing a successful result with no content.</returns>
        public static SuccessResultBuilder<TValue> NoContent<TValue>() => new(HttpStatusCode.NoContent);

        /// <summary>
        /// Creates a result that represents a successful  with an HTTP 202 Accepted status.
        /// </summary>
        /// <remarks>You can further customize the result using the builder methods.</remarks>
        /// <returns>An <see cref="Result"/> indicating that the request has been accepted for processing.</returns>
        public static SuccessResultBuilder Accepted() => new(HttpStatusCode.Accepted);

        /// <summary>
        /// Creates an  result that represents an HTTP 202 Accepted response with no content.
        /// </summary>
        /// <remarks>You can further customize the result using the builder methods.</remarks>
        /// <typeparam name="TValue">The type of the result value associated with the  result.</typeparam>
        /// <returns>An <see cref="Result{TValue}"/> indicating that the request was accepted for processing, but no
        /// result content is provided.</returns>
        public static SuccessResultBuilder<TValue> Accepted<TValue>() => new(HttpStatusCode.Accepted);

        /// <summary>
        /// Creates a new builder for a failed result with a default HTTP 400 Bad Request status code.
        /// </summary>
        /// <remarks>You can further customize the result using the builder methods.</remarks>
        /// <returns>A <see cref="Result"/> initialized with an HTTP 400 Bad Request status code.</returns>
        public static FailureResultBuilder Failure() => new(HttpStatusCode.BadRequest);

        /// <summary>
        /// Creates a builder for a failed result with a default HTTP 400 Bad Request status code.
        /// </summary>
        /// <remarks>You can further customize the result using the builder methods.</remarks>
        /// <typeparam name="TValue">The type of the result value associated with the failure.</typeparam>
        /// <returns>A <see cref="Result{TValue}"/> initialized with a Bad Request status code, allowing further
        /// configuration of the failure result.</returns>
        public static FailureResultBuilder<TValue> Failure<TValue>() => new(HttpStatusCode.BadRequest);

        /// <summary>
        /// Creates an result representing a failed result with a specified error key and message and <see cref="HttpStatusCode.BadRequest"/>.
        /// </summary>
        /// <remarks>You can further customize the result using the builder methods.</remarks>
        /// <param name="key">The key that identifies the type or category of the error. Cannot be null.</param>
        /// <param name="message">The error message describing the reason for the failure. Cannot be null.</param>
        /// <returns>An <see cref="Result"/> instance indicating failure, containing the provided error key and message.</returns>
        public static FailureResultBuilder Failure(string key, string message) => new FailureResultBuilder(HttpStatusCode.BadRequest).WithError(key, message);

        /// <summary>
        /// Creates a result representing a failed result with an exception and <see cref="HttpStatusCode.BadRequest"/>.
        /// </summary>
        /// <remarks>You can further customize the result using the builder methods.</remarks>
        /// <param name="exception">The exception that describes the reason for the failure. Cannot be <see langword="null"/>.</param>
        /// <returns>An <see cref="Result"/> instance that encapsulates the failure information and exception details.</returns>
        public static FailureResultBuilder Failure(Exception exception)
        {
            ArgumentNullException.ThrowIfNull(exception);
            return new FailureResultBuilder(HttpStatusCode.BadRequest).WithException(exception);
        }

        /// <summary>
        /// Creates a failed result with the specified exception and a default HTTP status code of BadRequest.
        /// </summary>
        /// <remarks>You can further customize the result using the builder methods.</remarks>
        /// <typeparam name="TValue">The type of the result value associated with the failed result.</typeparam>
        /// <param name="exception">The exception that describes the failure. Cannot be null.</param>
        /// <returns>An OperationResult representing a failed result containing the provided exception.</returns>
        public static FailureResultBuilder<TValue> Failure<TValue>(Exception exception)
        {
            ArgumentNullException.ThrowIfNull(exception);
            return new FailureResultBuilder<TValue>(HttpStatusCode.BadRequest).WithException(exception);
        }

        /// <summary>
        /// Creates a new builder for a failure result representing a bad HTTP request (HTTP 400).
        /// </summary>
        /// <returns>A <see cref="FailureResultBuilder"/> initialized with <see cref="HttpStatusCode.BadRequest"/>.</returns>
        public static FailureResultBuilder BadRequest() => new(HttpStatusCode.BadRequest);

        /// <summary>
        /// Creates a builder for a failure result representing a bad request (HTTP 400).
        /// </summary>
        /// <typeparam name="TValue">The type of the value associated with the failure result.</typeparam>
        /// <returns>A <see cref="FailureResultBuilder{TValue}"/> initialized with the bad request status code.</returns>
        public static FailureResultBuilder<TValue> BadRequest<TValue>() => new(HttpStatusCode.BadRequest);

        /// <summary>
        /// Creates a result representing a not found error with the specified key and message.
        /// </summary>
        /// <remarks>You can further customize the result using the builder methods.</remarks>
        /// <param name="key">The error key that identifies the specific not found condition. Cannot be <see langword="null"/>.</param>
        /// <param name="message">The error message describing the not found condition. Cannot be <see langword="null"/>.</param>
        /// <returns>An <see cref="Result"/> instance containing the not found error information.</returns>
        public static FailureResultBuilder NotFound(string key, string message)
        {
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(message);

            return new FailureResultBuilder(HttpStatusCode.NotFound)
                .WithError(key, message);
        }

        /// <summary>
        /// Creates an  result indicating that the requested resource was not found, and attaches an error with
        /// the specified key and message.
        /// </summary>
        /// <remarks>You can further customize the result using the builder methods.</remarks>
        /// <typeparam name="TValue">The type of the result value associated with the  result.</typeparam>
        /// <param name="key">The error key that identifies the type or source of the not found error. Cannot be null.</param>
        /// <param name="message">The error message that describes the not found condition. Cannot be null.</param>
        /// <returns>An <see cref="Result{TValue}"/> representing a not found result, containing the specified error
        /// key and message.</returns>
        public static FailureResultBuilder<TValue> NotFound<TValue>(string key, string message)
        {
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(message);

            return new FailureResultBuilder<TValue>(HttpStatusCode.NotFound)
                .WithError(key, message);
        }

        /// <summary>
        /// Creates an  result that represents a conflict error with a specified key and message.
        /// </summary>
        /// <remarks>You can further customize the result using the builder methods.</remarks>
        /// <param name="key">The error key that identifies the source or type of the conflict. Cannot be null.</param>
        /// <param name="message">The error message that describes the conflict. Cannot be null.</param>
        /// <returns>A Result instance containing the conflict error with the provided key and message.</returns>
        public static FailureResultBuilder Conflict(string key, string message)
        {
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(message);

            return new FailureResultBuilder(HttpStatusCode.Conflict)
                .WithError(key, message);
        }

        /// <summary>
        /// Creates an  result that represents a conflict error with the specified key and message.
        /// </summary>
        /// <remarks>You can further customize the result using the builder methods.</remarks>
        /// <typeparam name="TValue">The type of the result value associated with the  result.</typeparam>
        /// <param name="key">A string that identifies the error. Cannot be null.</param>
        /// <param name="message">A descriptive message explaining the nature of the conflict. Cannot be null.</param>
        /// <returns>A <see cref="Result{TValue}"/> representing a conflict error containing the specified key and
        /// message.</returns>
        public static FailureResultBuilder<TValue> Conflict<TValue>(string key, string message)
        {
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(message);

            return new FailureResultBuilder<TValue>(HttpStatusCode.Conflict)
                .WithError(key, message);
        }

        /// <summary>
        /// Creates an  result representing an internal server error and associates it with the specified
        /// exception.
        /// </summary>
        /// <remarks>You can further customize the result using the builder methods.</remarks>
        /// <param name="exception">The exception that caused the internal server error. Cannot be null.</param>
        /// <returns>A <see cref="Result"/> instance representing an internal server error, containing details from the
        /// provided exception.</returns>
        public static FailureResultBuilder InternalServerError(Exception exception)
        {
            ArgumentNullException.ThrowIfNull(exception);

            return new FailureResultBuilder(HttpStatusCode.InternalServerError)
                .WithException(exception);
        }

        /// <summary>
        /// Creates an  result representing an internal server error with the specified error key, message, and
        /// associated exception.
        /// </summary>
        /// <remarks>You can further customize the result using the builder methods.</remarks>
        /// <param name="key">The unique key identifying the error. Cannot be null.</param>
        /// <param name="message">The error message describing the internal server error. Cannot be null.</param>
        /// <param name="exception">The exception that caused the internal server error. Cannot be null.</param>
        /// <returns>An OperationResult instance containing details about the internal server error, including the error key,
        /// message, and exception.</returns>
        public static FailureResultBuilder InternalServerError(string key, string message, Exception exception)
        {
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(message);
            ArgumentNullException.ThrowIfNull(exception);

            return new FailureResultBuilder(HttpStatusCode.InternalServerError)
                .WithError(key, message)
                .WithException(exception);
        }

        /// <summary>
        /// Creates a failure result indicating that the  was unauthorized.
        /// </summary>
        /// <remarks>You can further customize the result using the builder methods.</remarks>
        /// <returns>A <see cref="Result"/> representing an unauthorized failure result.</returns>
        public static FailureResultBuilder Unauthorized() => new(HttpStatusCode.Unauthorized);

        /// <summary>
        /// Creates a failure result indicating that the  was unauthorized.
        /// </summary>
        /// <remarks>You can further customize the result using the builder methods.</remarks>
        /// <typeparam name="TValue">The type of the result value associated with the failure.</typeparam>
        /// <returns>A <see cref="Result{TValue}"/> representing an unauthorized failure result.</returns>
        public static FailureResultBuilder<TValue> Unauthorized<TValue>() => new(HttpStatusCode.Unauthorized);

        /// <summary>
        /// Creates a failure result indicating that the  is forbidden.
        /// </summary>
        /// <remarks>You can further customize the result using the builder methods.</remarks>
        /// <returns>A <see cref="Result"/> representing a failure with HTTP status code 403
        /// (Forbidden).</returns>
        public static FailureResultBuilder Forbidden() => new(HttpStatusCode.Forbidden);

        /// <summary>
        /// Creates a failure result indicating that the  is forbidden (HTTP 403).
        /// </summary>
        /// <remarks>You can further customize the result using the builder methods.</remarks>
        /// <typeparam name="TValue">The type of the result value associated with the failure.</typeparam>
        /// <returns>A <see cref="Result{TValue}"/> representing a forbidden failure.</returns>
        public static FailureResultBuilder<TValue> Forbidden<TValue>() => new(HttpStatusCode.Forbidden);

        /// <summary>
        /// Creates a failure result indicating that the request could not be processed due to semantic errors,
        /// corresponding to HTTP status code 422 (Unprocessable Entity).
        /// </summary>
        /// <remarks>You can further customize the result using the builder methods.</remarks>
        /// <returns>A <see cref="Result"/> representing an unprocessable entity failure response.</returns>
        public static FailureResultBuilder UnprocessableEntity() => new((HttpStatusCode)422);

        /// <summary>
        /// Creates a failure result indicating that the request could not be processed due to semantic errors, using
        /// HTTP status code 422 (Unprocessable Entity).
        /// </summary>
        /// <remarks>You can further customize the result using the builder methods.</remarks>
        /// <typeparam name="TValue">The type of the result value associated with the failure.</typeparam>
        /// <returns>A <see cref="Result{TValue}"/> representing a failure with status code 422
        /// (Unprocessable Entity).</returns>
        public static FailureResultBuilder<TValue> UnprocessableEntity<TValue>() => new((HttpStatusCode)422);

        /// <summary>
        /// Creates a failure result indicating that the service is unavailable.
        /// </summary>
        /// <remarks>You can further customize the result using the builder methods.</remarks>
        /// <returns>A <see cref="Result"/> representing a service unavailable error, typically
        /// corresponding to HTTP status code 503.</returns>
        public static FailureResultBuilder ServiceUnavailable() => new(HttpStatusCode.ServiceUnavailable);

        /// <summary>
        /// Creates a failure result indicating that the service is unavailable.
        /// </summary>
        /// <remarks>Use this method to signal that an operation could not be completed because the
        /// underlying service is temporarily unavailable. The returned failure result uses the HTTP 503 Service
        /// Unavailable status code.</remarks>
        /// <typeparam name="TValue">The type of the result value associated with the failure.</typeparam>
        /// <returns>A <see cref="Result{TValue}"/> representing a failure due to service
        /// unavailability.</returns>
        public static FailureResultBuilder<TValue> ServiceUnavailable<TValue>() => new(HttpStatusCode.ServiceUnavailable);
    }
}
