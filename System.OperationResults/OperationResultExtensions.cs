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

namespace System.OperationResults;

/// <summary>
/// Provides extension methods for working with <see cref="OperationResult"/> instances, enabling the creation and
/// configuration of operation results for HTTP operations.
/// </summary>
/// <remarks>These extension methods simplify the process of building operation results that represent successful
/// or failed HTTP operations. They are intended to be used in scenarios where standardized result handling is required,
/// such as in web APIs or service layers.</remarks>
public static class OperationResultExtensions
{
    /// <summary>
    /// Provides extension methods for <see cref="OperationResult"/> instances.
    /// </summary>
    /// <param name="operation">The operation result instance to extend.</param>"
    extension(OperationResult operation)
    {
        /// <summary>
        /// Creates a successful <see cref="OperationResult"/> instance representing an operation that completed without
        /// errors.
        /// </summary>
        /// <remarks>Use this method to indicate that an operation has succeeded. The returned result will
        /// have a status code of <see cref="HttpStatusCode.OK"/> and no error information.</remarks>
        /// <returns>An <see cref="OperationResult"/> indicating a successful outcome with an HTTP status code of 200 (OK).</returns>
        public static OperationResult Success() => SuccessStatus(HttpStatusCode.OK).Build();

        /// <summary>
        /// Creates a successful <see cref="OperationResult{TResult}"/> with the specified value and an HTTP status code of OK (200).
        /// </summary>
        /// <typeparam name="TResult">The type of the value to include in the operation result.</typeparam>
        /// <param name="value">The value to include in the successful operation result.</param>
        /// <returns>An <see cref="OperationResult{TResult}"/> representing a successful operation containing the specified
        /// value.</returns>
        public static OperationResult<TResult> Success<TResult>(TResult value) =>
            SuccessStatus(HttpStatusCode.OK, value).Build();

        /// <summary>
        /// Creates a new ExecutionResultException that represents the current operation result.
        /// </summary>
        /// <returns>An ExecutionResultException initialized with the current operation result.</returns>
        public OperationResultException ToExecutionResultException() => new(operation);

        /// <summary>
        /// Creates a builder for a successful operation result with the OK HTTP status code.
        /// </summary>
        /// <returns>An <see cref="IOperationResultSuccessBuilder"/> instance configured with the provided status code.</returns>
        public static IOperationResultSuccessBuilder SuccessStatus() => new OperationResultSuccessBuilder(HttpStatusCode.OK);

        /// <summary>
        /// Creates a builder for a successful operation result with the specified HTTP status code.
        /// </summary>
        /// <param name="statusCode">The HTTP status code to associate with the successful operation result. Typically indicates the outcome of
        /// the operation, such as 200 (OK) or 201 (Created).</param>
        /// <returns>An <see cref="IOperationResultSuccessBuilder"/> instance configured with the provided status code.</returns>
        public static IOperationResultSuccessBuilder SuccessStatus(HttpStatusCode statusCode) =>
            new OperationResultSuccessBuilder(statusCode);

        /// <summary>
        /// Creates a builder for an operation result that represents a successful operation with the OK HTTP
        /// status code.
        /// </summary>
        /// <typeparam name="TResult">The type of the result value to be included in the operation result.</typeparam>
        /// <returns>An <see cref="IOperationResultBuilder{TResult}"/> instance configured to represent a successful result with
        /// the specified status code.</returns>
        public static IOperationResultSuccessBuilder<TResult> SuccessStatus<TResult>() =>
            new OperationResultSuccessBuilder<TResult>(HttpStatusCode.OK);

        /// <summary>
        /// Creates a builder for an operation result that represents a successful operation with the specified HTTP
        /// status code.
        /// </summary>
        /// <typeparam name="TResult">The type of the result value to be included in the operation result.</typeparam>
        /// <param name="statusCode">The HTTP status code to associate with the successful operation result.</param>
        /// <returns>An <see cref="IOperationResultBuilder{TResult}"/> instance configured to represent a successful result with
        /// the specified status code.</returns>
        public static IOperationResultSuccessBuilder<TResult> SuccessStatus<TResult>(HttpStatusCode statusCode) =>
            new OperationResultSuccessBuilder<TResult>(statusCode);

        /// <summary>
        /// Creates a builder for a successful operation result with the specified HTTP status code and result value.
        /// </summary>
        /// <typeparam name="TResult">The type of the result value to be included in the operation result.</typeparam>
        /// <param name="statusCode">The HTTP status code to associate with the successful operation result.</param>
        /// <param name="value">The value to include in the operation result. Represents the result of the successful operation.</param>
        /// <returns>An <see cref="IOperationResultSuccessBuilder{TResult}"/> instance configured with the specified status code
        /// and result value.</returns>
        public static IOperationResultSuccessBuilder<TResult> SuccessStatus<TResult>(HttpStatusCode statusCode, TResult value) =>
            new OperationResultSuccessBuilder<TResult>(statusCode).WithResult(value);

        /// <summary>
        /// Creates a builder for a successful operation result with an HTTP 200 OK status.
        /// </summary>
        /// <returns>An <see cref="IOperationResultSuccessBuilder"/> representing a successful result with status code 200 (OK).</returns>
        public static IOperationResultSuccessBuilder Ok() => SuccessStatus(HttpStatusCode.OK);

        /// <summary>
        /// Creates a builder for a successful operation result with HTTP status code 200 (OK).
        /// </summary>
        /// <typeparam name="TResult">The type of the result value to be returned in the operation result.</typeparam>
        /// <returns>An <see cref="IOperationResultSuccessBuilder{TResult}"/> instance representing a successful result with
        /// status code 200 (OK).</returns>
        public static IOperationResultSuccessBuilder<TResult> Ok<TResult>() => SuccessStatus<TResult>(HttpStatusCode.OK);

        /// <summary>
        /// Creates a successful operation result with an HTTP 200 (OK) status code and the specified result value.
        /// </summary>
        /// <typeparam name="TResult">The type of the result value to include in the operation result.</typeparam>
        /// <param name="result">The value to include in the successful operation result. This value will be returned to the caller.</param>
        /// <returns>An <see cref="IOperationResultSuccessBuilder{TResult}"/> representing a successful operation result
        /// containing the specified value and an HTTP 200 (OK) status code.</returns>
        public static IOperationResultSuccessBuilder<TResult> Ok<TResult>(TResult result) => SuccessStatus(HttpStatusCode.OK, result);

        /// <summary>
        /// Creates a result builder that represents a successful operation with an HTTP 201 Created status code.
        /// </summary>
        /// <remarks>Use this method to indicate that a resource has been successfully created. The
        /// returned builder can be further customized before producing the final operation result.</remarks>
        /// <returns>An <see cref="IOperationResultSuccessBuilder"/> configured with the HTTP 201 Created status code.</returns>
        public static IOperationResultSuccessBuilder Created() => SuccessStatus(HttpStatusCode.Created);

        /// <summary>
        /// Creates a successful operation result with an HTTP 201 Created status code.
        /// </summary>
        /// <typeparam name="TResult">The type of the result value to include in the operation result.</typeparam>
        /// <returns>An <see cref="IOperationResultSuccessBuilder{TResult}"/> representing a successful result with a 201 Created
        /// status code.</returns>
        public static IOperationResultSuccessBuilder<TResult> Created<TResult>() => SuccessStatus<TResult>(HttpStatusCode.Created);

        /// <summary>
        /// Creates a successful operation result with an HTTP 201 Created status and the specified result value.
        /// </summary>
        /// <typeparam name="TResult">The type of the result value to include in the operation result.</typeparam>
        /// <param name="result">The value to include in the operation result. Represents the resource that was created.</param>
        /// <returns>An <see cref="IOperationResultSuccessBuilder{TResult}"/> containing the specified result and an HTTP 201
        /// Created status.</returns>
        public static IOperationResultSuccessBuilder<TResult> Created<TResult>(TResult result) => SuccessStatus(HttpStatusCode.Created, result);

        /// <summary>
        /// Creates a result builder that represents a successful operation with no content to return.
        /// </summary>
        /// <returns>An <see cref="IOperationResultSuccessBuilder"/> configured with an HTTP 204 No Content status, indicating
        /// that the request was successful but there is no content in the response.</returns>
        public static IOperationResultSuccessBuilder NoContent() => SuccessStatus(HttpStatusCode.NoContent);

        /// <summary>
        /// Creates a successful operation result with an HTTP 204 No Content status, indicating that the operation
        /// completed successfully but does not return any content.
        /// </summary>
        /// <remarks>Use this method when an operation completes successfully and no response body is
        /// required. The HTTP status code will be set to 204 (No Content).</remarks>
        /// <typeparam name="TResult">The type of the result associated with the operation. No content will be returned for this type.</typeparam>
        /// <returns>An <see cref="IOperationResultSuccessBuilder{TResult}"/> representing a successful result with no content.</returns>
        public static IOperationResultSuccessBuilder<TResult> NoContent<TResult>() => SuccessStatus<TResult>(HttpStatusCode.NoContent);

        /// <summary>
        /// Creates an <see cref="OperationResult"/> that represents a successful operation with an HTTP 202 Accepted
        /// status.
        /// </summary>
        /// <remarks>Use this method to signal that the request was valid and has been accepted, but
        /// processing is not yet complete. This is commonly used in asynchronous or deferred processing
        /// scenarios.</remarks>
        /// <returns>An <see cref="OperationResult"/> indicating that the request has been accepted for processing.</returns>
        public static OperationResult Accepted() => SuccessStatus(HttpStatusCode.Accepted).Build();

        /// <summary>
        /// Creates an operation result that represents an HTTP 202 Accepted response with no content.
        /// </summary>
        /// <typeparam name="TResult">The type of the result value associated with the operation result.</typeparam>
        /// <returns>An <see cref="OperationResult{TResult}"/> indicating that the request was accepted for processing, but no
        /// result content is provided.</returns>
        public static OperationResult<TResult> Accepted<TResult>() => SuccessStatus<TResult>(HttpStatusCode.Accepted).Build();

        /// <summary>
        /// Creates an operation result representing a failed operation with a specified error key and message.
        /// </summary>
        /// <remarks>The returned result uses a status code of <see
        /// cref="HttpStatusCode.BadRequest"/>. Use this method to report validation or client-side errors
        /// with a specific error key and message.</remarks>
        /// <param name="key">The key that identifies the type or category of the error. Cannot be null.</param>
        /// <param name="message">The error message describing the reason for the failure. Cannot be null.</param>
        /// <returns>An <see cref="OperationResult"/> instance indicating failure, containing the provided error key and message.</returns>
        public static OperationResult Failure(string key, string message) =>
            FailureStatus(HttpStatusCode.BadRequest)
            .WithError(key, message)
            .Build();

        /// <summary>
        /// Creates an <see cref="OperationResult"/> representing a failed operation, including details from the
        /// specified exception.
        /// </summary>
        /// <param name="exception">The exception that describes the reason for the failure. Cannot be <see langword="null"/>.</param>
        /// <returns>An <see cref="OperationResult"/> instance that encapsulates the failure information and exception details.</returns>
        public static OperationResult Failure(Exception exception)
        {
            ArgumentNullException.ThrowIfNull(exception);

            return FailureStatus(HttpStatusCode.BadRequest)
                .Merge(exception.ToOperationResult())
                .Build();
        }

        /// <summary>
        /// Creates a failed operation result with the specified exception and a default HTTP status code of BadRequest.
        /// </summary>
        /// <remarks>The returned operation result will have its status code set to BadRequest and will
        /// include the specified exception in its error details.</remarks>
        /// <typeparam name="TResult">The type of the result value associated with the operation result.</typeparam>
        /// <param name="exception">The exception that describes the failure. Cannot be null.</param>
        /// <returns>An OperationResult representing a failed operation containing the provided exception.</returns>
        public static OperationResult<TResult> Failure<TResult>(Exception exception)
        {
            ArgumentNullException.ThrowIfNull(exception);

            return FailureStatus<TResult>(HttpStatusCode.BadRequest)
                .Merge(exception.ToOperationResult())
                .Build();
        }

        /// <summary>
        /// Creates an <see cref="OperationResult"/> representing a not found error with the specified key and message.
        /// </summary>
        /// <param name="key">The error key that identifies the specific not found condition. Cannot be <see langword="null"/>.</param>
        /// <param name="message">The error message describing the not found condition. Cannot be <see langword="null"/>.</param>
        /// <returns>An <see cref="OperationResult"/> instance containing the not found error information.</returns>
        public static OperationResult NotFound(string key, string message)
        {
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(message);

            return NotFound()
                .WithError(key, message)
                .Build();
        }

        /// <summary>
        /// Creates an operation result indicating that the requested resource was not found, and attaches an error with
        /// the specified key and message.
        /// </summary>
        /// <typeparam name="TResult">The type of the result value associated with the operation result.</typeparam>
        /// <param name="key">The error key that identifies the type or source of the not found error. Cannot be null.</param>
        /// <param name="message">The error message that describes the not found condition. Cannot be null.</param>
        /// <returns>An <see cref="OperationResult{TResult}"/> representing a not found result, containing the specified error
        /// key and message.</returns>
        public static OperationResult<TResult> NotFound<TResult>(string key, string message)
        {
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(message);

            return NotFound<TResult>()
                .WithError(key, message)
                .Build();
        }

        /// <summary>
        /// Creates an operation result that represents a conflict error with a specified key and message.
        /// </summary>
        /// <param name="key">The error key that identifies the source or type of the conflict. Cannot be null.</param>
        /// <param name="message">The error message that describes the conflict. Cannot be null.</param>
        /// <returns>An OperationResult instance containing the conflict error with the provided key and message.</returns>
        public static OperationResult Conflict(string key, string message)
        {
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(message);

            return Conflict()
                .WithError(key, message)
                .Build();
        }

        /// <summary>
        /// Creates an operation result that represents a conflict error with the specified key and message.
        /// </summary>
        /// <remarks>Use this method to indicate that an operation could not be completed due to a
        /// conflict, such as a resource state or data integrity issue. The returned result will include the provided
        /// error information and can be used to communicate conflict details to the caller.</remarks>
        /// <typeparam name="TResult">The type of the result value associated with the operation result.</typeparam>
        /// <param name="key">A string that identifies the error. Cannot be null.</param>
        /// <param name="message">A descriptive message explaining the nature of the conflict. Cannot be null.</param>
        /// <returns>An <see cref="OperationResult{TResult}"/> representing a conflict error containing the specified key and
        /// message.</returns>
        public static OperationResult<TResult> Conflict<TResult>(string key, string message)
        {
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(message);

            return Conflict<TResult>()
                .WithError(key, message)
                .Build();
        }

        /// <summary>
        /// Creates an operation result representing an internal server error and associates it with the specified
        /// exception.
        /// </summary>
        /// <param name="exception">The exception that caused the internal server error. Cannot be null.</param>
        /// <returns>An <see cref="OperationResult"/> instance representing an internal server error, containing details from the
        /// provided exception.</returns>
        public static OperationResult InternalServerError(Exception exception)
        {
            ArgumentNullException.ThrowIfNull(exception);

            return InternalServerError()
                .WithException(exception)
                .Build();
        }

        /// <summary>
        /// Creates an operation result representing an internal server error with the specified error key, message, and
        /// associated exception.
        /// </summary>
        /// <param name="key">The unique key identifying the error. Cannot be null.</param>
        /// <param name="message">The error message describing the internal server error. Cannot be null.</param>
        /// <param name="exception">The exception that caused the internal server error. Cannot be null.</param>
        /// <returns>An OperationResult instance containing details about the internal server error, including the error key,
        /// message, and exception.</returns>
        public static OperationResult InternalServerError(string key, string message, Exception exception)
        {
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(message);
            ArgumentNullException.ThrowIfNull(exception);

            return InternalServerError()
                .WithError(key, message)
                .WithException(exception)
                .Build();
        }

        /// <summary>
        /// Creates a builder for an operation result that represents a failure with the BadRequest HTTP status code.
        /// </summary>       
        /// <returns>An <see cref="IOperationResultFailureBuilder"/> instance configured with the specified HTTP status code.</returns>
        public static IOperationResultFailureBuilder FailureStatus() =>
            new OperationResultFailureBuilder(HttpStatusCode.BadRequest);

        /// <summary>
        /// Creates a builder for an operation result that represents a failure with the specified HTTP status code.
        /// </summary>
        /// <param name="statusCode">The HTTP status code to associate with the failure result. This value determines the type of failure
        /// reported to the caller.</param>
        /// <returns>An <see cref="IOperationResultFailureBuilder"/> instance configured with the specified HTTP status code.</returns>
        public static IOperationResultFailureBuilder FailureStatus(HttpStatusCode statusCode) =>
            new OperationResultFailureBuilder(statusCode);

        /// <summary>
        /// Creates a builder for an operation result that represents a failure with the BadRequest HTTP status code.
        /// </summary>
        /// <remarks>Use this method to construct a failure result in scenarios where an operation does
        /// not succeed and an HTTP status code should be returned to indicate the nature of the failure.</remarks>
        /// <typeparam name="TResult">The type of the result value associated with the operation result.</typeparam>
        /// <returns>An <see cref="IOperationResultFailureBuilder{TResult}"/> instance configured with the specified status code.</returns>
        public static IOperationResultFailureBuilder<TResult> FailureStatus<TResult>() =>
            new OperationResultFailureBuilder<TResult>(HttpStatusCode.BadRequest);

        /// <summary>
        /// Creates a builder for an operation result that represents a failure with the specified HTTP status code.
        /// </summary>
        /// <remarks>Use this method to construct a failure result in scenarios where an operation does
        /// not succeed and an HTTP status code should be returned to indicate the nature of the failure.</remarks>
        /// <typeparam name="TResult">The type of the result value associated with the operation result.</typeparam>
        /// <param name="statusCode">The HTTP status code to associate with the failure result.</param>
        /// <returns>An <see cref="IOperationResultFailureBuilder{TResult}"/> instance configured with the specified status code.</returns>
        public static IOperationResultFailureBuilder<TResult> FailureStatus<TResult>(HttpStatusCode statusCode) =>
            new OperationResultFailureBuilder<TResult>(statusCode);

        /// <summary>
        /// Creates a failure result indicating that the requested resource was not found (HTTP 404).
        /// </summary>
        /// <returns>An <see cref="IOperationResultFailureBuilder"/> representing a failure with a 404 Not Found status.</returns>
        public static IOperationResultFailureBuilder NotFound() => FailureStatus(HttpStatusCode.NotFound);

        /// <summary>
        /// Creates a failure result indicating that the requested resource was not found.
        /// </summary>
        /// <remarks>Use this method to signal that an operation could not locate the requested resource.
        /// The returned failure builder can be further customized before producing the final result.</remarks>
        /// <typeparam name="TResult">The type of the result value associated with the failure.</typeparam>
        /// <returns>An <see cref="IOperationResultFailureBuilder{TResult}"/> representing a failure with a 404 Not Found status.</returns>
        public static IOperationResultFailureBuilder<TResult> NotFound<TResult>() => FailureStatus<TResult>(HttpStatusCode.NotFound);

        /// <summary>
        /// Creates a failure result indicating that the request was invalid and could not be processed.
        /// </summary>
        /// <returns>An <see cref="IOperationResultFailureBuilder"/> representing a failure with a 400 Bad Request status code.</returns>
        public static IOperationResultFailureBuilder BadRequest() => FailureStatus(HttpStatusCode.BadRequest);

        /// <summary>
        /// Creates a failure result builder representing an HTTP 400 Bad Request error for the specified result type.
        /// </summary>
        /// <typeparam name="TResult">The type of the result associated with the failure.</typeparam>
        /// <returns>An <see cref="IOperationResultFailureBuilder{TResult}"/> configured for a Bad Request (HTTP 400) error.</returns>
        public static IOperationResultFailureBuilder<TResult> BadRequest<TResult>() => FailureStatus<TResult>(HttpStatusCode.BadRequest);

        /// <summary>
        /// Creates a failure result builder representing a conflict error (HTTP 409).
        /// </summary>
        /// <remarks>Use this method to signal that a request could not be completed due to a conflict
        /// with the current state of the resource. This is typically used in scenarios where an operation cannot
        /// proceed because of a versioning or resource state conflict.</remarks>
        /// <returns>An <see cref="IOperationResultFailureBuilder"/> configured to indicate a conflict error.</returns>
        public static IOperationResultFailureBuilder Conflict() => FailureStatus(HttpStatusCode.Conflict);

        /// <summary>
        /// Creates a failure result builder representing a conflict (HTTP 409) error for the specified result type.
        /// </summary>
        /// <remarks>Use this method to indicate that the requested operation could not be completed due
        /// to a conflict, such as a resource state mismatch. The returned builder can be further customized before
        /// producing the final failure result.</remarks>
        /// <typeparam name="TResult">The type of the result associated with the failure.</typeparam>
        /// <returns>An <see cref="IOperationResultFailureBuilder{TResult}"/> configured to represent a conflict error.</returns>
        public static IOperationResultFailureBuilder<TResult> Conflict<TResult>() => FailureStatus<TResult>(HttpStatusCode.Conflict);

        /// <summary>
        /// Creates a failure result indicating that the operation was unauthorized.
        /// </summary>
        /// <returns>An <see cref="IOperationResultFailureBuilder"/> representing an unauthorized failure result.</returns>
        public static IOperationResultFailureBuilder Unauthorized() => FailureStatus(HttpStatusCode.Unauthorized);

        /// <summary>
        /// Creates a failure result indicating that the operation was unauthorized.
        /// </summary>
        /// <remarks>Use this method to signal that the current operation failed due to insufficient
        /// authorization. The returned failure result will have an HTTP status code of 401 (Unauthorized).</remarks>
        /// <typeparam name="TResult">The type of the result value associated with the failure.</typeparam>
        /// <returns>An <see cref="IOperationResultFailureBuilder{TResult}"/> representing an unauthorized failure result.</returns>
        public static IOperationResultFailureBuilder<TResult> Unauthorized<TResult>() => FailureStatus<TResult>(HttpStatusCode.Unauthorized);

        /// <summary>
        /// Creates a failure result indicating that the operation is forbidden.
        /// </summary>
        /// <remarks>Use this method to signal that the current request is not permitted due to
        /// insufficient permissions. The resulting failure can be further customized before being returned to the
        /// client.</remarks>
        /// <returns>An <see cref="IOperationResultFailureBuilder"/> representing a failure with HTTP status code 403
        /// (Forbidden).</returns>
        public static IOperationResultFailureBuilder Forbidden() => FailureStatus(HttpStatusCode.Forbidden);

        /// <summary>
        /// Creates a failure result indicating that the operation is forbidden (HTTP 403).
        /// </summary>
        /// <remarks>Use this method to signal that the requested action is not permitted due to
        /// insufficient permissions or access rights. The returned failure corresponds to the HTTP 403 Forbidden status
        /// code.</remarks>
        /// <typeparam name="TResult">The type of the result value associated with the failure.</typeparam>
        /// <returns>An <see cref="IOperationResultFailureBuilder{TResult}"/> representing a forbidden operation failure.</returns>
        public static IOperationResultFailureBuilder<TResult> Forbidden<TResult>() => FailureStatus<TResult>(HttpStatusCode.Forbidden);

        /// <summary>
        /// Creates a failure result indicating that the request could not be processed due to semantic errors,
        /// corresponding to HTTP status code 422 (Unprocessable Entity).
        /// </summary>
        /// <remarks>Use this method to signal that the server understands the content type and syntax of
        /// the request, but was unable to process the contained instructions. This is typically used for validation
        /// errors or business rule violations that prevent successful processing.</remarks>
        /// <returns>An <see cref="IOperationResultFailureBuilder"/> representing an unprocessable entity failure response.</returns>
        public static IOperationResultFailureBuilder UnprocessableEntity() => FailureStatus((HttpStatusCode)422);

        /// <summary>
        /// Creates a failure result indicating that the request could not be processed due to semantic errors, using
        /// HTTP status code 422 (Unprocessable Entity).
        /// </summary>
        /// <remarks>Use this method to signal that the server understands the request but cannot process
        /// it due to semantic issues, such as validation errors. This is commonly used in APIs to indicate that the
        /// request is well-formed but contains invalid data.</remarks>
        /// <typeparam name="TResult">The type of the result value associated with the failure.</typeparam>
        /// <returns>An <see cref="IOperationResultFailureBuilder{TResult}"/> representing a failure with status code 422
        /// (Unprocessable Entity).</returns>
        public static IOperationResultFailureBuilder<TResult> UnprocessableEntity<TResult>() => FailureStatus<TResult>((HttpStatusCode)422);

        /// <summary>
        /// Creates a failure result builder representing an internal server error (HTTP 500).
        /// </summary>
        /// <remarks>Use this method to indicate that an unexpected error occurred on the server. The
        /// resulting failure builder can be further customized before generating the final operation result.</remarks>
        /// <returns>An <see cref="IOperationResultFailureBuilder"/> configured for an internal server error response.</returns>
        public static IOperationResultFailureBuilder InternalServerError() => FailureStatus(HttpStatusCode.InternalServerError);

        /// <summary>
        /// Creates a failure result builder representing an internal server error (HTTP 500).
        /// </summary>
        /// <remarks>Use this method to indicate that an operation failed due to an unexpected server-side
        /// error. The returned builder can be further customized before producing the final failure result.</remarks>
        /// <typeparam name="TResult">The type of the result value associated with the failure.</typeparam>
        /// <returns>An <see cref="IOperationResultFailureBuilder{TResult}"/> configured for an internal server error response.</returns>
        public static IOperationResultFailureBuilder<TResult> InternalServerError<TResult>() =>
            FailureStatus<TResult>(HttpStatusCode.InternalServerError);

        /// <summary>
        /// Creates a failure result indicating that the service is unavailable.
        /// </summary>
        /// <remarks>Use this method to signal that the requested operation cannot be completed because
        /// the service is temporarily unavailable. This is commonly used in scenarios where maintenance or overload
        /// prevents the service from processing requests.</remarks>
        /// <returns>An <see cref="IOperationResultFailureBuilder"/> representing a service unavailable error, typically
        /// corresponding to HTTP status code 503.</returns>
        public static IOperationResultFailureBuilder ServiceUnavailable() =>
            FailureStatus(HttpStatusCode.ServiceUnavailable);

        /// <summary>
        /// Creates a failure result indicating that the service is unavailable.
        /// </summary>
        /// <remarks>Use this method to signal that an operation could not be completed because the
        /// underlying service is temporarily unavailable. The returned failure result uses the HTTP 503 Service
        /// Unavailable status code.</remarks>
        /// <typeparam name="TResult">The type of the result value associated with the failure.</typeparam>
        /// <returns>An <see cref="IOperationResultFailureBuilder{TResult}"/> representing a failure due to service
        /// unavailability.</returns>
        public static IOperationResultFailureBuilder<TResult> ServiceUnavailable<TResult>() =>
            FailureStatus<TResult>(HttpStatusCode.ServiceUnavailable);
    }
}
