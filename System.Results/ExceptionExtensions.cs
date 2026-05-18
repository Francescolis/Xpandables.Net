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
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace System.Results;

/// <summary>
/// Provides extension methods for mapping exceptions to HTTP status codes.
/// </summary>
/// <remarks>These methods enable consistent translation of common .NET exceptions to standard HTTP status codes,
/// which can be useful when building web APIs or services that need to communicate error conditions to clients. The
/// mappings are based on typical usage patterns and may be customized as needed for specific application
/// requirements.</remarks>
public static class ExceptionExtensions
{
	/// <summary>
	/// Creates an <see cref="Result"/> representing the outcome of the current exception, optionally using
	/// a specified HTTP status code and reason phrase.
	/// </summary>
	/// <remarks>In development environments, the operation result includes detailed exception
	/// information for easier debugging. In other environments, only generic error details are provided to avoid
	/// exposing sensitive information.</remarks>
	/// <param name="exception">The exception to convert into a result. Cannot be null.</param>
	/// <param name="statusCode">The HTTP status code to associate with the operation result. If <see langword="null"/>, the status code is
	/// determined from the exception.</param>
	/// <param name="reason">An optional reason phrase to include in the operation result. If <see langword="null"/>, the exception
	/// message or status code title is used depending on the environment.</param>
	/// <returns>A <see cref="Result"/> describing the failure, including status code, error details, and exception
	/// information.</returns>
	public static FailureResult ToResult(
		this Exception exception,
		HttpStatusCode? statusCode = null,
		string? reason = default)
	{
		ArgumentNullException.ThrowIfNull(exception);

		bool isDevelopment = (Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
			?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development") == "Development";

		if (exception is ResultException resultException)
		{
			FailureResult executionResult = resultException.Result;
			return ResultWith
				.Failure(executionResult.StatusCode)
				.WithTitle(executionResult.StatusCode.Title)
				.WithDetail(executionResult.StatusCode.Detail)
				.WithErrors(executionResult.Errors)
				.WithExtensions(executionResult.Extensions)
				.WithHeaders(executionResult.Headers);
		}

		statusCode ??= exception.GetHttpStatusCode();

		if (exception is ValidationException validationException)
		{
			statusCode = validationException.GetHttpStatusCode();
			FailureResult result = validationException.ToResult();
			return ResultWith
				.Failure(statusCode.Value)
				.WithTitle("one or more validation errors occurred.")
				.WithDetail(statusCode.Value.Detail)
				.WithErrors(result.Errors);
		}

		return ResultWith
			.Failure(statusCode.Value)
			.WithTitle(isDevelopment ? reason ?? exception.Message : statusCode.Value.Title)
			.WithDetail(statusCode.Value.Detail)
			.WithException(exception)
			.WithErrors(exception.GetElementEntries());
	}
}
