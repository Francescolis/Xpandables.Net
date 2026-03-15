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
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace System.Results;

/// <summary>
/// Provides extension methods for converting collections of <see cref="ValidationResult"/> objects to results
/// suitable for API responses.
/// </summary>
/// <remarks>These extension methods facilitate mapping validation errors to standardized result
/// formats, enabling consistent error handling in API endpoints. The methods are intended to be used with collections
/// of <see cref="ValidationResult"/> produced by validation frameworks.</remarks>
public static class ValidationResultExtensions
{
	/// <summary>
	/// Converts a collection of validation results into a structured Result object that represents a bad request.
	/// </summary>
	/// <remarks>Throws an ArgumentNullException if the validations parameter is null and an
	/// ArgumentOutOfRangeException if the collection is empty. This method is intended for use in API scenarios where
	/// validation errors need to be returned in a standardized format.</remarks>
	/// <param name="validations">The collection of validation results to be processed. Cannot be null or empty.</param>
	/// <returns>A Result object that encapsulates the bad request status, including a title, detail message, and any associated
	/// validation errors.</returns>
	public static Result ToResult(this IEnumerable<ValidationResult> validations)
	{
		ArgumentNullException.ThrowIfNull(validations);
		ArgumentOutOfRangeException.ThrowIfZero(validations.Count());

		var elements = validations.ToElementCollection();
		return Result
			.BadRequest()
			.WithTitle("one or more validation errors occurred.")
			.WithDetail(HttpStatusCode.BadRequest.Detail)
			.WithErrors(elements)
			.Build();
	}

	/// <summary>
	/// Converts a ValidationException into a Result object that encapsulates validation error details.
	/// </summary>
	/// <remarks>This method throws an ArgumentNullException if the provided exception is null, and an
	/// ArgumentOutOfRangeException if the validation result indicates success, meaning there are no validation errors to
	/// report.</remarks>
	/// <param name="exception">The ValidationException to convert. Cannot be null and must contain validation errors.</param>
	/// <returns>A Result object representing the failure state, including the HTTP status code, a title indicating validation
	/// errors, and a collection of validation error details.</returns>
	public static Result ToResult(this ValidationException exception)
	{
		ArgumentNullException.ThrowIfNull(exception);
		ArgumentOutOfRangeException.ThrowIfEqual(exception.ValidationResult, ValidationResult.Success);

		HttpStatusCode statusCode = exception.GetHttpStatusCode();
		var elements = exception.ValidationResult.ToElementCollection();

		return Result
			.Failure()
			.WithStatusCode(statusCode)
			.WithTitle("one or more validation errors occurred.")
			.WithDetail(statusCode.Detail)
			.WithErrors(elements)
			.Build();
	}
}
