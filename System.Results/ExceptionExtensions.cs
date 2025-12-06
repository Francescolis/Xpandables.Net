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
    /// Extension methods for mapping exceptions to HTTP status codes.
    /// </summary>
    /// <param name="exception">The exception to map.</param>
    extension(Exception exception)
    {
        /// <summary>
        /// Creates an <see cref="Result"/> representing the outcome of the current exception, optionally using
        /// a specified HTTP status code and reason phrase.
        /// </summary>
        /// <remarks>In development environments, the operation result includes detailed exception
        /// information for easier debugging. In other environments, only generic error details are provided to avoid
        /// exposing sensitive information.</remarks>
        /// <param name="statusCode">The HTTP status code to associate with the operation result. If <see langword="null"/>, the status code is
        /// determined from the exception.</param>
        /// <param name="reason">An optional reason phrase to include in the operation result. If <see langword="null"/>, the exception
        /// message or status code title is used depending on the environment.</param>
        /// <returns>A <see cref="Result"/> describing the failure, including status code, error details, and exception
        /// information.</returns>
        public FailureResult ToFailureResult(HttpStatusCode? statusCode = null, string? reason = default)
        {
            ArgumentNullException.ThrowIfNull(exception);

            bool isDevelopment = (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development") == "Development";

            if (exception is ResultException resultException)
            {
                var executionResult = resultException.Result;
                return Result
                    .Failure()
                    .WithStatusCode(executionResult.StatusCode)
                    .WithTitle(executionResult.StatusCode.Title)
                    .WithDetail(executionResult.StatusCode.Detail)
                    .WithErrors(executionResult.Errors)
                    .WithExtensions(executionResult.Extensions)
                    .WithHeaders(executionResult.Headers)
                    .Build();
            }

            statusCode ??= exception.GetHttpStatusCode();

            var builder = Result
                .Failure()
                .WithStatusCode(statusCode.Value)
                .WithTitle(isDevelopment ? reason ?? exception.Message : statusCode.Value.Title)
                .WithDetail(isDevelopment ? $"{exception}" : statusCode.Value.Detail)
                .WithErrors(exception.GetElementEntries());

            return exception is ValidationException
                ? builder.Build()
                : builder.WithException(exception).Build();
        }

        /// <summary>
        /// Creates a failure result representing the current exception, optionally specifying an HTTP status code and
        /// reason.
        /// </summary>
        /// <remarks>If the exception is a ResultException, its embedded result is used to construct the
        /// failure result. In development environments, additional exception details are included in the result for
        /// debugging purposes.</remarks>
        /// <typeparam name="TValue">The type of the value associated with the failure result.</typeparam>
        /// <param name="statusCode">The HTTP status code to associate with the failure result. If null, a status code is inferred from the
        /// exception.</param>
        /// <param name="reason">An optional reason phrase to include in the failure result. If null, a default reason is used based on the
        /// exception or status code.</param>
        /// <returns>A failure result containing details about the exception, including status code, error information, and
        /// optional reason.</returns>
        public FailureResult<TValue> ToFailureResult<TValue>(HttpStatusCode? statusCode = null, string? reason = default)
        {
            ArgumentNullException.ThrowIfNull(exception);
            bool isDevelopment = (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development") == "Development";
            if (exception is ResultException resultException)
            {
                var executionResult = resultException.Result;
                return Result
                    .Failure<TValue>()
                    .WithStatusCode(executionResult.StatusCode)
                    .WithTitle(executionResult.StatusCode.Title)
                    .WithDetail(executionResult.StatusCode.Detail)
                    .WithErrors(executionResult.Errors)
                    .WithExtensions(executionResult.Extensions)
                    .WithHeaders(executionResult.Headers)
                    .Build();
            }

            statusCode ??= exception.GetHttpStatusCode();

            var builder = Result
                .Failure<TValue>()
                .WithStatusCode(statusCode.Value)
                .WithTitle(isDevelopment ? reason ?? exception.Message : statusCode.Value.Title)
                .WithDetail(isDevelopment ? $"{exception}" : statusCode.Value.Detail)
                .WithErrors(exception.GetElementEntries());

            return exception is ValidationException
                ? builder.Build()
                : builder.WithException(exception).Build();
        }
    }
}
