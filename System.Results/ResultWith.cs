/*******************************************************************************
 * Copyright (C) 2025-2026 Kamersoft
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
public static class ResultWith
{
	/// <summary>
	/// Creates a successful <see cref="Result"/> instance representing an result that completed without
	/// errors.
	/// </summary>
	/// <remarks>You can further customize the result using the builder methods.</remarks>
	/// <returns>An <see cref="Result"/> indicating a successful outcome with an HTTP status code of 200 (OK).</returns>
	public static SuccessResult Success() => new() { StatusCode = HttpStatusCode.OK };

	/// <summary>
	/// Creates a successful <see cref="Result{TValue}"/> with the specified value and an HTTP status code of OK (200).
	/// </summary>
	/// <remarks>You can further customize the result using the builder methods.</remarks>
	/// <typeparam name="TValue">The type of the value to include in the  result.</typeparam>
	/// <param name="value">The value to include in the successful  result.</param>
	/// <returns>An <see cref="Result{TValue}"/> representing a successful  containing the specified
	/// value.</returns>
	public static SuccessResult<TValue> Success<TValue>(TValue value) => new() { StatusCode = HttpStatusCode.OK, Value = value };

	/// <summary>
	/// Creates a result builder that represents a successful  with an HTTP 201 Created status code.
	/// </summary>
	/// <remarks>You can further customize the result using the builder methods.</remarks>
	/// <returns>An <see cref="Result"/> configured with the HTTP 201 Created status code.</returns>
	public static SuccessResult Created() => new() { StatusCode = HttpStatusCode.Created };

	/// <summary>
	/// Creates a successful  result with an HTTP 201 Created status code.
	/// </summary>
	/// <remarks>You can further customize the result using the builder methods.</remarks>
	/// <typeparam name="TValue">The type of the result value to include in the result.</typeparam>
	/// <returns>An <see cref="Result{TValue}"/> representing a successful result with a 201 Created
	/// status code.</returns>
	public static SuccessResult<TValue> Created<TValue>(TValue value) => new() { StatusCode = HttpStatusCode.Created, Value = value };

	/// <summary>
	/// Creates a result builder that represents a successful  with no content to return.
	/// </summary>
	/// <remarks>You can further customize the result using the builder methods.</remarks>
	/// <returns>An <see cref="Result"/> configured with an HTTP 204 No Content status, indicating
	/// that the request was successful but there is no content in the response.</returns>
	public static SuccessResult NoContent() => new() { StatusCode = HttpStatusCode.NoContent };

	/// <summary>
	/// Creates a successful  result with an HTTP 204 No Content status, indicating that the 
	/// completed successfully but does not return any content.
	/// </summary>
	/// <remarks>You can further customize the result using the builder methods.</remarks>
	/// <typeparam name="TValue">The type of the result associated with the . No content will be returned for this type.</typeparam>
	/// <returns>An <see cref="Result{TValue}"/> representing a successful result with no content.</returns>
	public static SuccessResult<TValue> NoContent<TValue>() => new() { StatusCode = HttpStatusCode.NoContent, Value = default };

	/// <summary>
	/// Creates a result that represents a successful  with an HTTP 202 Accepted status.
	/// </summary>
	/// <remarks>You can further customize the result using the builder methods.</remarks>
	/// <returns>An <see cref="Result"/> indicating that the request has been accepted for processing.</returns>
	public static SuccessResult Accepted() => new() { StatusCode = HttpStatusCode.Accepted };

	/// <summary>
	/// Creates an  result that represents an HTTP 202 Accepted response with no content.
	/// </summary>
	/// <remarks>You can further customize the result using the builder methods.</remarks>
	/// <typeparam name="TValue">The type of the result value associated with the  result.</typeparam>
	/// <returns>An <see cref="Result{TValue}"/> indicating that the request was accepted for processing, but no
	/// result content is provided.</returns>
	public static SuccessResult<TValue> Accepted<TValue>() => new() { StatusCode = HttpStatusCode.Accepted, Value = default };

	/// <summary>
	/// Creates a new builder for a failed result with a default HTTP 400 Bad Request status code.
	/// </summary>
	/// <remarks>You can further customize the result using the builder methods.</remarks>
	/// <returns>A <see cref="Result"/> initialized with an HTTP 400 Bad Request status code.</returns>
	public static FailureResult Failure(HttpStatusCode statusCode = HttpStatusCode.BadRequest) => new() { StatusCode = statusCode };

	/// <summary>
	/// Creates a builder for a failed result with a default HTTP 400 Bad Request status code.
	/// </summary>
	/// <remarks>You can further customize the result using the builder methods.</remarks>
	/// <typeparam name="TValue">The type of the result value associated with the failure.</typeparam>
	/// <returns>A <see cref="Result{TValue}"/> initialized with a Bad Request status code, allowing further
	/// configuration of the failure result.</returns>
	public static FailureResult<TValue> Failure<TValue>(HttpStatusCode statusCode = HttpStatusCode.BadRequest) => new() { StatusCode = statusCode };

	/// <summary>
	/// Creates a result representing a not found error with the specified key and message.
	/// </summary>
	/// <remarks>You can further customize the result using the builder methods.</remarks>
	/// <param name="key">The error key that identifies the specific not found condition. Cannot be <see langword="null"/>.</param>
	/// <param name="message">The error message describing the not found condition. Cannot be <see langword="null"/>.</param>
	/// <returns>An <see cref="Result"/> instance containing the not found error information.</returns>
	public static FailureResult NotFound(string key, string message)
	{
		ArgumentNullException.ThrowIfNull(key);
		ArgumentNullException.ThrowIfNull(message);

		return Failure(HttpStatusCode.NotFound).WithError(key, message);
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
	public static FailureResult<TValue> NotFound<TValue>(string key, string message)
	{
		ArgumentNullException.ThrowIfNull(key);
		ArgumentNullException.ThrowIfNull(message);

		return Failure<TValue>(HttpStatusCode.NotFound).WithError(key, message);
	}

	/// <summary>
	/// Creates an  result that represents a conflict error with a specified key and message.
	/// </summary>
	/// <remarks>You can further customize the result using the builder methods.</remarks>
	/// <param name="key">The error key that identifies the source or type of the conflict. Cannot be null.</param>
	/// <param name="message">The error message that describes the conflict. Cannot be null.</param>
	/// <returns>A Result instance containing the conflict error with the provided key and message.</returns>
	public static FailureResult Conflict(string key, string message)
	{
		ArgumentNullException.ThrowIfNull(key);
		ArgumentNullException.ThrowIfNull(message);

		return Failure(HttpStatusCode.Conflict).WithError(key, message);
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
	public static FailureResult<TValue> Conflict<TValue>(string key, string message)
	{
		ArgumentNullException.ThrowIfNull(key);
		ArgumentNullException.ThrowIfNull(message);

		return Failure<TValue>(HttpStatusCode.Conflict).WithError(key, message);
	}

	/// <summary>
	/// Creates an  result representing an internal server error and associates it with the specified
	/// exception.
	/// </summary>
	/// <remarks>You can further customize the result using the builder methods.</remarks>
	/// <param name="exception">The exception that caused the internal server error. Cannot be null.</param>
	/// <returns>A <see cref="Result"/> instance representing an internal server error, containing details from the
	/// provided exception.</returns>
	public static FailureResult InternalServerError(Exception exception)
	{
		ArgumentNullException.ThrowIfNull(exception);

		return Failure(HttpStatusCode.InternalServerError).WithException(exception);
	}

	/// <summary>
	/// Creates an  result representing an internal server error with the specified error key, message, and
	/// associated exception.
	/// </summary>
	/// <remarks>You can further customize the result using the builder methods.</remarks>
	/// <param name="exception">The exception that caused the internal server error. Cannot be null.</param>
	/// <returns>An OperationResult instance containing details about the internal server error, including the error key,
	/// message, and exception.</returns>
	public static FailureResult<TValue> InternalServerError<TValue>(Exception exception)
	{
		ArgumentNullException.ThrowIfNull(exception);

		return Failure<TValue>(HttpStatusCode.InternalServerError).WithException(exception);
	}

	/// <summary>
	/// Creates a failure result indicating that the  was unauthorized.
	/// </summary>
	/// <remarks>You can further customize the result using the builder methods.</remarks>
	/// <returns>A <see cref="Result"/> representing an unauthorized failure result.</returns>
	public static FailureResult Unauthorized() => new() { StatusCode = HttpStatusCode.Unauthorized };

	/// <summary>
	/// Creates a failure result indicating that the  was unauthorized.
	/// </summary>
	/// <remarks>You can further customize the result using the builder methods.</remarks>
	/// <typeparam name="TValue">The type of the result value associated with the failure.</typeparam>
	/// <returns>A <see cref="Result{TValue}"/> representing an unauthorized failure result.</returns>
	public static FailureResult<TValue> Unauthorized<TValue>() => new() { StatusCode = HttpStatusCode.Unauthorized };

	/// <summary>
	/// Creates a failure result indicating that the  is forbidden.
	/// </summary>
	/// <remarks>You can further customize the result using the builder methods.</remarks>
	/// <returns>A <see cref="Result"/> representing a failure with HTTP status code 403
	/// (Forbidden).</returns>
	public static FailureResult Forbidden() => new() { StatusCode = HttpStatusCode.Forbidden };

	/// <summary>
	/// Creates a failure result indicating that the  is forbidden (HTTP 403).
	/// </summary>
	/// <remarks>You can further customize the result using the builder methods.</remarks>
	/// <typeparam name="TValue">The type of the result value associated with the failure.</typeparam>
	/// <returns>A <see cref="Result{TValue}"/> representing a forbidden failure.</returns>
	public static FailureResult<TValue> Forbidden<TValue>() => new() { StatusCode = HttpStatusCode.Forbidden };

	/// <summary>
	/// Creates a failure result indicating that the request could not be processed due to semantic errors,
	/// corresponding to HTTP status code 422 (Unprocessable Entity).
	/// </summary>
	/// <remarks>You can further customize the result using the builder methods.</remarks>
	/// <returns>A <see cref="Result"/> representing an unprocessable entity failure response.</returns>
	public static FailureResult UnprocessableEntity() => new() { StatusCode = (HttpStatusCode)422 };

	/// <summary>
	/// Creates a failure result indicating that the request could not be processed due to semantic errors, using
	/// HTTP status code 422 (Unprocessable Entity).
	/// </summary>
	/// <remarks>You can further customize the result using the builder methods.</remarks>
	/// <typeparam name="TValue">The type of the result value associated with the failure.</typeparam>
	/// <returns>A <see cref="Result{TValue}"/> representing a failure with status code 422
	/// (Unprocessable Entity).</returns>
	public static FailureResult<TValue> UnprocessableEntity<TValue>() => new() { StatusCode = (HttpStatusCode)422 };

	/// <summary>
	/// Creates a failure result indicating that the service is unavailable.
	/// </summary>
	/// <remarks>You can further customize the result using the builder methods.</remarks>
	/// <returns>A <see cref="Result"/> representing a service unavailable error, typically
	/// corresponding to HTTP status code 503.</returns>
	public static FailureResult ServiceUnavailable() => new() { StatusCode = HttpStatusCode.ServiceUnavailable };

	/// <summary>
	/// Creates a failure result indicating that the service is unavailable.
	/// </summary>
	/// <remarks>Use this method to signal that an operation could not be completed because the
	/// underlying service is temporarily unavailable. The returned failure result uses the HTTP 503 Service
	/// Unavailable status code.</remarks>
	/// <typeparam name="TValue">The type of the result value associated with the failure.</typeparam>
	/// <returns>A <see cref="Result{TValue}"/> representing a failure due to service
	/// unavailability.</returns>
	public static FailureResult<TValue> ServiceUnavailable<TValue>() => new() { StatusCode = HttpStatusCode.ServiceUnavailable };

	/// <summary>
	/// Creates a new ResultException that represents the current result.
	/// </summary>
	/// <returns>An ResultException initialized with the current result.</returns>
	public static ResultException ToResultException(this FailureResult result) => new(result);
}
